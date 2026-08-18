using System;
using UnityEngine;

namespace Mine
{
    public class MineBoard : MonoBehaviour
    {
        // ---- 精灵图（由 GameManager 传入）----
        public Sprite coveredSprite;
        public Sprite mineSprite;
        public Sprite flagSprite;

        // ---- 对外事件 ----
        public event Action<int> OnFlagCountChanged;
        public event Action<bool> OnGameOver;
        public event Action OnFirstClick;

        // ---- 数据 ----
        public MineGrid grid;
        private int width, height;
        private int totalMines;
        private int flagsUsed;
        private bool gameEnded;
        private bool hasFirstClick;

        // ---- 渲染组件 ----
        private SpriteRenderer[,] renderers;      // 底层：盖子/翻开底色/地雷
        private SpriteRenderer[,] flagRenderers;   // 中层：旗子（叠在盖子上方）
        private TextMesh[,] textMeshes;            // 上层：数字
        private bool[,] revealed;
        private bool[,] flagged;

        // ---- 输入 ----
        private InputHandler inputHandler;

        // ---- 格子尺寸 ----
        private float cellSize;
        private float cellGap = 0f;

        // ---- 代码生成的底色精灵图 ----
        private Sprite revealedSprite;

        // ---- 经典扫雷配色 ----
        private static readonly Color[] numColors =
        {
            Color.clear,
            new Color(0.0f, 0.0f, 1.0f),       // 1 蓝
            new Color(0.0f, 0.5f, 0.0f),       // 2 绿
            new Color(1.0f, 0.0f, 0.0f),       // 3 红
            new Color(0.0f, 0.0f, 0.5f),       // 4 深蓝
            new Color(0.5f, 0.0f, 0.0f),       // 5 深红
            new Color(0.0f, 0.5f, 0.5f),       // 6 青
            new Color(0.0f, 0.0f, 0.0f),       // 7 黑
            new Color(0.5f, 0.5f, 0.5f),       // 8 灰
        };

        // ==============================================================
        //  初始化
        // ==============================================================

        public void Init(MineGrid grid)
        {
            this.grid = grid;
            this.width = grid.width;
            this.height = grid.height;
            this.totalMines = grid.mineCount;

            flagsUsed = 0;
            gameEnded = false;
            hasFirstClick = false;

            revealed     = new bool[width, height];
            flagged      = new bool[width, height];
            renderers    = new SpriteRenderer[width, height];
            flagRenderers = new SpriteRenderer[width, height];
            textMeshes   = new TextMesh[width, height];

            revealedSprite = CreateColorSprite(new Color(0.82f, 0.82f, 0.82f));

            // 不在这里布雷，等第一次点击时再调 grid.createGrid(x, y)

            CreateCells();
            CenterCamera();
            SetupInput();
        }

        // ==============================================================
        //  输入绑定
        // ==============================================================

        private void SetupInput()
        {
            inputHandler = gameObject.AddComponent<InputHandler>();
            inputHandler.Init(width, height);

            inputHandler.OnCellTapped      += OnTap;
            inputHandler.OnCellLongPressed  += OnLongPress;
        }

        /// <summary>短按 → 翻开格子。</summary>
        private void OnTap(int x, int y)
        {
            if (gameEnded) return;
            RevealCell(x, y);
        }

        /// <summary>长按 → 插旗/取消旗。</summary>
        private void OnLongPress(int x, int y)
        {
            if (gameEnded) return;
            ToggleFlag(x, y);
        }

        // ==============================================================
        //  创建格子
        // ==============================================================

        private void CreateCells()
        {
            var cam = Camera.main;
            float screenHeight = cam.orthographicSize * 2f;
            float screenWidth  = screenHeight * cam.aspect;

            // 棋盘占屏幕宽度 92%，上下留给 HUD 和按钮
            float boardTargetWidth  = screenWidth  * 0.92f;
            float boardTargetHeight = screenHeight * 0.72f;

            float cellByWidth  = (boardTargetWidth  - cellGap * (width  - 1)) / width;
            float cellByHeight = (boardTargetHeight - cellGap * (height - 1)) / height;
            cellSize = Mathf.Min(cellByWidth, cellByHeight);

            float spriteWorldSize = coveredSprite.bounds.size.x;
            float scale = cellSize / spriteWorldSize;

            float step = cellSize + cellGap;
            float boardWidth  = step * width  - cellGap;
            float boardHeight = step * height - cellGap;
            float offsetX = -boardWidth  / 2f + cellSize / 2f;
            // 整体略微偏下，给顶部 HUD 让出空间
            float offsetY = -boardHeight / 2f + cellSize / 2f - screenHeight * 0.04f;

            // 数字大小 ≈ 格子的 85%
            float textCharSize = cellSize * 0.12f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new GameObject($"Cell_{x}_{y}");
                    cell.transform.SetParent(transform);
                    cell.transform.localPosition = new Vector3(
                        offsetX + x * step,
                        offsetY + y * step,
                        0
                    );
                    cell.transform.localScale = new Vector3(scale, scale, 1f);

                    // 底层：精灵图
                    var sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = coveredSprite;
                    sr.sortingOrder = 0;
                    renderers[x, y] = sr;

                    // 中层：旗子（叠在盖子上方，90% 大小）
                    var flagGo = new GameObject("Flag");
                    flagGo.transform.SetParent(cell.transform);
                    flagGo.transform.localPosition = new Vector3(0, 0, -0.05f);
                    flagGo.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
                    var fsr = flagGo.AddComponent<SpriteRenderer>();
                    fsr.sprite = flagSprite;
                    fsr.sortingOrder = 1;
                    fsr.enabled = false;
                    flagRenderers[x, y] = fsr;

                    // 上层：数字文字
                    var textGo = new GameObject("Num");
                    textGo.transform.SetParent(cell.transform);
                    textGo.transform.localPosition = new Vector3(0, 0, -0.1f);
                    var tm = textGo.AddComponent<TextMesh>();
                    tm.text = "";
                    tm.fontSize = 48;
                    tm.characterSize = textCharSize;
                    tm.anchor = TextAnchor.MiddleCenter;
                    tm.alignment = TextAlignment.Center;
                    tm.color = Color.clear;
                    textMeshes[x, y] = tm;

                    // 碰撞体
                    var col = cell.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(spriteWorldSize, spriteWorldSize);
                }
            }
        }

        private void CenterCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
        }

        // ==============================================================
        //  插旗（长按触发）
        // ==============================================================

        private void ToggleFlag(int x, int y)
        {
            if (revealed[x, y]) return;

            if (flagged[x, y])
            {
                flagged[x, y] = false;
                flagsUsed--;
            }
            else
            {
                if (flagsUsed >= totalMines) return;
                flagged[x, y] = true;
                flagsUsed++;
            }

            RenderCell(x, y);
            OnFlagCountChanged?.Invoke(totalMines - flagsUsed);
            CheckWinByFlags();
        }

        // ==============================================================
        //  翻开格子
        // ==============================================================

        private void RevealCell(int x, int y)
        {
            if (revealed[x, y] || flagged[x, y]) return;

            if (!hasFirstClick)
            {
                hasFirstClick = true;
                // 第一次点击时布雷，createGrid 会跳过 (x,y) 确保安全
                grid.createGrid(x, y);
                OnFirstClick?.Invoke();
            }

            int value = grid.GetCell(x, y);

            if (value == 9)
            {
                revealed[x, y] = true;
                RenderCell(x, y);
                GameLost();
                return;
            }

            FloodReveal(x, y);
            CheckWinByReveal();
        }

        private void FloodReveal(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            if (revealed[x, y] || flagged[x, y]) return;

            int value = grid.GetCell(x, y);
            if (value == 9) return;

            revealed[x, y] = true;
            RenderCell(x, y);

            if (value == 0)
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (dx != 0 || dy != 0)
                            FloodReveal(x + dx, y + dy);
            }
        }

        // ==============================================================
        //  渲染
        // ==============================================================

        private void RenderCell(int x, int y)
        {
            var sr  = renderers[x, y];
            var fsr = flagRenderers[x, y];
            var tm  = textMeshes[x, y];

            if (!revealed[x, y])
            {
                // ── 没翻开 ──
                sr.sprite  = coveredSprite;
                sr.enabled = true;
                sr.color   = Color.white;
                fsr.enabled = flagged[x, y];     // 旗子叠在盖子上方
                tm.text = "";
            }
            else
            {
                fsr.enabled = false;              // 翻开后旗子消失
                int value = grid.GetCell(x, y);

                if (value == 9)
                {
                    // ── 地雷 ──
                    sr.sprite  = mineSprite;
                    sr.enabled = true;
                    sr.color   = Color.white;
                    tm.text = "";
                }
                else if (value == 0)
                {
                    // ── 空白：直接消失，露出背景 ──
                    sr.enabled = false;
                    tm.text = "";
                }
                else
                {
                    // ── 数字 1~8 ──
                    sr.sprite  = revealedSprite;
                    sr.enabled = true;
                    sr.color   = Color.white;
                    tm.text  = value.ToString();
                    tm.color = numColors[value];
                }
            }
        }

        // ==============================================================
        //  胜负判定
        // ==============================================================

        private void GameLost()
        {
            gameEnded = true;
            RevealAllMines();
            OnGameOver?.Invoke(false);
        }

        private void RevealAllMines()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (grid.GetCell(x, y) == 9 && !revealed[x, y])
                    {
                        revealed[x, y] = true;
                        RenderCell(x, y);
                    }
        }

        private void CheckWinByReveal()
        {
            int unrevealed = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (!revealed[x, y]) unrevealed++;

            if (unrevealed == totalMines)
            {
                gameEnded = true;
                OnGameOver?.Invoke(true);
            }
        }

        private void CheckWinByFlags()
        {
            if (flagsUsed != totalMines) return;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (flagged[x, y] && grid.GetCell(x, y) != 9) return;
            gameEnded = true;
            OnGameOver?.Invoke(true);
        }

        // ==============================================================
        //  工具
        // ==============================================================

        private Sprite CreateColorSprite(Color color)
        {
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}