using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 模式B 的所有视觉渲染：网格、黄色分割线、托盘形状、消除特效。
    /// 纯渲染层，不含游戏逻辑。
    /// </summary>
    public class BlockRenderer
    {
        private readonly Transform parent;
        private readonly Sprite[] candySprites;
        private readonly Sprite pixelSprite;
        private readonly GameObject effectPrefab;
        private readonly float cellSize;

        private SpriteRenderer[,] cellRenderers;
        private GameObject[] trayGroups = new GameObject[3];

        public float CellSize => cellSize;

        public BlockRenderer(Transform parent, Sprite[] candySprites,
                             GameObject effectPrefab, float cellSize)
        {
            this.parent = parent;
            this.candySprites = candySprites;
            this.effectPrefab = effectPrefab;
            this.cellSize = cellSize;
            this.pixelSprite = CreatePixelSprite();
        }

        // ==============================================================
        //  坐标转换（公共方法，供 BlockInput 使用）
        // ==============================================================

        /// <summary>网格左上角的世界坐标。</summary>
        public Vector3 GridOrigin()
        {
            float ox = -(BlockBoard.Cols * cellSize) / 2f;
            float oy = (BlockBoard.Rows * cellSize) / 2f + cellSize;
            return new Vector3(ox, oy, 0f);
        }

        /// <summary>某个格子的中心世界坐标。</summary>
        public Vector3 CellCenter(int row, int col)
        {
            var o = GridOrigin();
            return new Vector3(
                o.x + col * cellSize + cellSize / 2f,
                o.y - row * cellSize - cellSize / 2f, 0f);
        }

        // ==============================================================
        //  网格渲染
        // ==============================================================

        public void CreateGrid()
        {
            var gridParent = new GameObject("Grid").transform;
            gridParent.SetParent(parent);

            // 格子背景
            cellRenderers = new SpriteRenderer[BlockBoard.Rows, BlockBoard.Cols];
            for (int r = 0; r < BlockBoard.Rows; r++)
                for (int c = 0; c < BlockBoard.Cols; c++)
                {
                    var go = new GameObject($"Cell_{r}_{c}");
                    go.transform.SetParent(gridParent);
                    go.transform.position = CellCenter(r, c);
                    go.transform.localScale = Vector3.one * (cellSize * 0.92f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = pixelSprite;
                    sr.color = new Color(0.2f, 0.2f, 0.25f, 0.6f);
                    sr.sortingOrder = 0;
                    cellRenderers[r, c] = sr;
                }

            // 黄色网格线
            var o = GridOrigin();
            Color yellow = new Color(1f, 0.85f, 0f, 0.7f);
            float lineW = 0.04f;

            for (int c = 0; c <= BlockBoard.Cols; c++)
            {
                float x = o.x + c * cellSize;
                CreateLine(gridParent, x, o.y, x, o.y - BlockBoard.Rows * cellSize, yellow, lineW);
            }
            for (int r = 0; r <= BlockBoard.Rows; r++)
            {
                float y = o.y - r * cellSize;
                CreateLine(gridParent, o.x, y, o.x + BlockBoard.Cols * cellSize, y, yellow, lineW);
            }
        }

        public void RefreshGrid(BlockBoard board)
        {
            for (int r = 0; r < BlockBoard.Rows; r++)
                for (int c = 0; c < BlockBoard.Cols; c++)
                {
                    int type = board.Get(r, c);
                    var sr = cellRenderers[r, c];
                    if (type == -1)
                    {
                        sr.sprite = pixelSprite;
                        sr.color = new Color(0.2f, 0.2f, 0.25f, 0.6f);
                        sr.transform.localScale = Vector3.one * (cellSize * 0.92f);
                    }
                    else
                    {
                        sr.sprite = candySprites[type];
                        sr.color = Color.white;
                        sr.transform.localScale = Vector3.one * cellSize;
                    }
                }
        }

        // ==============================================================
        //  托盘（3个待放置形状）
        // ==============================================================

        public void RefreshTray(int[] shapeIndices, int[] shapeTypes, bool[] shapePlaced)
        {
            float trayScale = 0.55f;
            for (int i = 0; i < 3; i++)
            {
                if (trayGroups[i] != null) Object.Destroy(trayGroups[i]);

                if (shapePlaced[i])
                {
                    trayGroups[i] = null;
                }
                else
                {
                    trayGroups[i] = CreateShapeVisual(shapeIndices[i], shapeTypes[i], trayScale);
                    PositionTray(i);
                }
            }
        }

        public void HideTrayItem(int index)
        {
            if (trayGroups[index] != null) trayGroups[index].SetActive(false);
        }

        public void ShowTrayItem(int index)
        {
            if (trayGroups[index] != null) trayGroups[index].SetActive(true);
        }

        /// <summary>获取托盘中某个形状的包围盒（用于点击检测）。</summary>
        public Rect GetTrayBounds(int index, int shapeIndex)
        {
            if (trayGroups[index] == null) return Rect.zero;
            float trayScale = 0.55f;
            Vector3 pos = trayGroups[index].transform.position;
            float w = BlockShapes.Cols(shapeIndex) * cellSize * trayScale;
            float h = BlockShapes.Rows(shapeIndex) * cellSize * trayScale;
            return new Rect(pos.x - 0.5f, pos.y - h - 0.3f, w + 1f, h + 0.6f);
        }

        private void PositionTray(int index)
        {
            if (trayGroups[index] == null) return;
            var o = GridOrigin();
            float gridBottom = o.y - BlockBoard.Rows * cellSize;
            float trayY = gridBottom - cellSize * 1.5f;
            float totalW = BlockBoard.Cols * cellSize;
            float spacing = totalW / 3f;
            float x = o.x + spacing * (index + 0.5f);
            trayGroups[index].transform.position = new Vector3(x, trayY, 0f);
        }

        // ==============================================================
        //  形状视觉 + 拖拽副本
        // ==============================================================

        public GameObject CreateShapeVisual(int shapeIndex, int fruitType, float scale)
        {
            var shape = BlockShapes.All[shapeIndex];
            int sRows = shape.GetLength(0);
            int sCols = shape.GetLength(1);

            var group = new GameObject("Shape");
            group.transform.SetParent(parent);

            for (int r = 0; r < sRows; r++)
                for (int c = 0; c < sCols; c++)
                {
                    if (shape[r, c] == 0) continue;
                    var go = new GameObject("Block");
                    go.transform.SetParent(group.transform);
                    go.transform.localPosition = new Vector3(
                        c * cellSize * scale, -r * cellSize * scale, 0f);
                    go.transform.localScale = Vector3.one * cellSize * scale * 0.92f;

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = candySprites[fruitType];
                    sr.sortingOrder = 3;
                }
            return group;
        }

        // ==============================================================
        //  消除特效
        // ==============================================================

        public void SpawnClearEffects(List<Vector2Int> clearedCells)
        {
            if (effectPrefab == null) return;
            foreach (var cell in clearedCells)
            {
                Vector3 pos = CellCenter(cell.x, cell.y);
                Object.Instantiate(effectPrefab, pos, Quaternion.identity);
            }
        }

        // ==============================================================
        //  工具
        // ==============================================================

        private void CreateLine(Transform lineParent, float x1, float y1,
                                float x2, float y2, Color color, float width)
        {
            var go = new GameObject("Line");
            go.transform.SetParent(lineParent);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(x1, y1, -0.1f));
            lr.SetPosition(1, new Vector3(x2, y2, -0.1f));
            lr.startWidth = width; lr.endWidth = width;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color; lr.endColor = color;
            lr.sortingOrder = 2;
        }

        private Sprite CreatePixelSprite()
        {
            var tex = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}