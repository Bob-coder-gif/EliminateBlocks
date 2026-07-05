using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3
{
    /// <summary>
    /// 总指挥（唯一挂到场景 GameObject 上的脚本）。
    /// 负责：Start 里组装所有子系统；每帧把输入交给 InputHandler；
    /// 用协程调度“交换 -> 检测 -> 消除(含特殊消除) -> 下落 -> 补充 -> 连锁”的整体流程；
    /// 计分、步数、胜负结算、死局洗牌、摄像机 / 背景自适应、屏幕 UI。
    /// 用法：新建空 GameObject 挂上本脚本，把糖果 Sprite 拖进 candySprites（建议 5~6 个），运行即可。
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [Header("棋盘尺寸")]
        public int width = 7;
        public int height = 9;
        public float tileSize = 1f;

        [Header("素材（建议先放 5~6 个）")]
        public Sprite[] candySprites;

        [Header("动画速度")]
        public float moveSpeed = 12f;

        [Header("关卡目标")]
        public int targetScore = 5000;
        public int maxSteps = 30;

        [Header("消除特效（把粒子 Prefab 拖进来，留空则无特效）")]
        public GameObject clearEffectPrefab;

        [Header("背景（把背景的 SpriteRenderer 拖进来）")]
        public SpriteRenderer background;

        // 子系统
        private BoardGrid grid;
        private TileFactory factory;
        private MatchFinder matchFinder;
        private ClearResolver clearResolver;
        private TileSwapper swapper;
        private Gravity gravity;
        private Refiller refiller;
        private TileClearer clearer;
        private InputHandler input;
        private ScoreManager score;
        private MoveValidator moveValidator;
        private Shuffler shuffler;

        // 状态
        private bool busy;        // 动画进行中锁输入
        private bool shuffling;   // 正在洗牌（用于提示）
        private bool gameOver;    // 已结算

        // UI 样式
        private GUIStyle labelStyle;
        private GUIStyle boxStyle;
        private GUIStyle buttonStyle;

        private void Start()
        {
            if (candySprites == null || candySprites.Length < 3)
            {
                Debug.LogError("请在 candySprites 里至少放 3 个 Sprite！");
                enabled = false;
                return;
            }

            // ---- 组装 ----
            grid = new BoardGrid(width, height, tileSize);
            factory = new TileFactory(candySprites, transform, grid, moveSpeed, tileSize);
            matchFinder = new MatchFinder(grid);
            clearResolver = new ClearResolver(grid, matchFinder);
            swapper = new TileSwapper(grid);
            gravity = new Gravity(grid);
            refiller = new Refiller(grid, factory);
            clearer = new TileClearer(grid, clearEffectPrefab);
            score = new ScoreManager(targetScore, maxSteps);
            moveValidator = new MoveValidator(grid);
            shuffler = new Shuffler(grid, candySprites, matchFinder, moveValidator);

            input = new InputHandler(Camera.main, swapper);
            input.OnSwapRequested += (a, b) => StartCoroutine(TrySwap(a, b));

            new BoardSetup(grid, factory).Fill();

            // 开局兜底：万一铺出来就是死局，直接洗到可玩
            if (!moveValidator.HasPossibleMove())
                shuffler.ShuffleUntilPlayable();

            CenterCamera();
            FitBackground();
            InitLabelStyle();
        }

        private void Update()
        {
            if (!busy && !gameOver) input.Tick();
        }

        // ---- 流程调度 ----

        private IEnumerator TrySwap(Tile a, Tile b)
        {
            busy = true;
            swapper.Swap(a, b);
            yield return WaitForMoves();

            if (!matchFinder.HasMatch())
            {
                swapper.Swap(a, b);          // 无效交换，换回去，不计步
                yield return WaitForMoves();
            }
            else
            {
                score.UseStep();             // 有效交换，计一步
                yield return ResolveMatches();

                if (score.IsOver)            // 消除结算完，判断是否结束
                {
                    gameOver = true;
                    busy = false;
                    yield break;
                }
            }

            yield return HandleDeadlockIfAny();
            busy = false;
        }

        /// <summary>
        /// 消除 -> 下落 -> 补充，循环处理连锁。
        /// 循环内只累加基础分（不含 combo），连锁结束后一次性加 5*combo。
        /// </summary>
        private IEnumerator ResolveMatches()
        {
            int combo = 0;
            int baseSum = 0;

            while (true)
            {
                var r = clearResolver.Resolve();
                if (r.Tiles.Count == 0)
                {
                    if (combo > 0) score.AddSwapResult(baseSum, combo);
                    yield break;
                }

                combo++;
                baseSum += ScoreManager.BaseGain(r.MatchedCount, r.Tiles.Count);
                //                                检测连数         实际消除总数

                clearer.Clear(r.Tiles);
                yield return new WaitForSeconds(0.08f);

                gravity.Collapse();
                refiller.Refill();
                yield return WaitForMoves();
            }
        }

        /// <summary>盘面若已无可走步，就洗牌到可玩。</summary>
        private IEnumerator HandleDeadlockIfAny()
        {
            if (moveValidator.HasPossibleMove()) yield break;

            shuffling = true;
            yield return new WaitForSeconds(0.4f);
            shuffler.ShuffleUntilPlayable();
            yield return WaitForMoves();
            shuffling = false;
        }

        private IEnumerator WaitForMoves()
        {
            bool moving = true;
            while (moving)
            {
                moving = false;
                foreach (var t in grid.AllTiles())
                {
                    if (!t.AtTarget) { moving = true; break; }
                }
                yield return null;
            }
        }

        // ---- 摄像机 / 背景 ----

        private void CenterCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            // 棋盘一半的宽、高，各留一格边距
            float halfW = (width * tileSize) / 2f + tileSize;
            float halfH = (height * tileSize) / 2f + tileSize;

            // 竖屏时宽度是瓶颈：把需要的宽度换算成 orthographicSize
            float aspect = cam.aspect;
            float sizeByWidth = halfW / Mathf.Max(aspect, 0.01f);   // 防除零
            float sizeByHeight = halfH;

            cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
        }

        private void FitBackground()
        {
            if (background == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            float camH = cam.orthographicSize * 2f;
            float camW = camH * cam.aspect;
            Vector2 size = background.sprite.bounds.size;

            // Max：铺满整个屏幕（可能裁掉图片边缘）。想“整图完整、四周留白”改成 Mathf.Min。
            float scale = Mathf.Max(camW / size.x, camH / size.y);
            background.transform.localScale = new Vector3(scale, scale, 1f);
            background.transform.position = new Vector3(0f, 0f, background.transform.position.z);
        }

        // ---- 重开 ----

        private void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // ---- UI（IMGUI 临时方案，正式发布建议换 UGUI）----

        private void InitLabelStyle()
        {
            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = new Color(0.1f, 0.4f, 1f);          // 蓝色
            labelStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.035f);   // 随分辨率
        }

        private void OnGUI()
        {
            if (labelStyle == null) InitLabelStyle();

            // 安全区（避开圆角 / 刘海 / 状态栏）
            Rect safe = Screen.safeArea;
            float left = safe.x + 12;
            float top = (Screen.height - safe.yMax) + 12;   // safeArea 的 y 从下往上，GUI 从上往下，需换算
            float lineH = labelStyle.fontSize + 8;

            GUI.Label(new Rect(left, top, 500, lineH), "得分: " + score.Score, labelStyle);
            GUI.Label(new Rect(left, top + lineH, 500, lineH), "剩余步数: " + score.Steps + " / " + score.MaxSteps, labelStyle);
            GUI.Label(new Rect(left, top + lineH * 2, 500, lineH), "目标分数: " + score.TargetScore, labelStyle);

            if (score.LastGain > 0)
            {
                float w = 250f;
                GUI.Label(new Rect(safe.xMax - w - 12, top, w, lineH), "+" + score.LastGain, labelStyle);
            }

            if (shuffling)
                GUI.Label(new Rect(left, top + lineH * 3, 500, lineH), "无可消除，正在洗牌…", labelStyle);

            if (gameOver)
            {
                if (boxStyle == null)
                {
                    boxStyle = new GUIStyle(GUI.skin.box);
                    boxStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.045f);
                    boxStyle.fontStyle = FontStyle.Bold;
                    boxStyle.alignment = TextAnchor.UpperCenter;

                    buttonStyle = new GUIStyle(GUI.skin.button);
                    buttonStyle.fontSize = Mathf.RoundToInt(Screen.height * 0.04f);
                }

                float bw = Screen.width * 0.6f;
                float bh = Screen.height * 0.28f;
                float bx = (Screen.width - bw) / 2f;
                float by = (Screen.height - bh) / 2f;

                GUI.Box(new Rect(bx, by, bw, bh), score.IsWin ? "胜利！" : "失败", boxStyle);
                GUI.Label(new Rect(bx, by + bh * 0.35f, bw, bh * 0.2f), "最终得分：" + score.Score, boxStyle);

                float btnW = bw * 0.5f;
                float btnH = bh * 0.25f;
                if (GUI.Button(new Rect(bx + (bw - btnW) / 2f, by + bh * 0.65f, btnW, btnH), "重新开始", buttonStyle))
                    Restart();
            }
        }
    }
}