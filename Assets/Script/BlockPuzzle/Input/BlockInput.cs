using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 模式B 的拖拽输入处理 + 放置预览。
    /// 不含游戏逻辑，只负责：检测点击 → 拖拽 → 释放 → 通知 BlockGame。
    /// </summary>
    public class BlockInput
    {
        private readonly Camera cam;
        private readonly BlockRenderer renderer;
        private readonly BlockBoard board;
        private readonly Transform parent;

        // 拖拽状态
        private int dragIndex = -1;
        private GameObject dragGroup;
        private List<SpriteRenderer> previewCells = new List<SpriteRenderer>();

        // 当前轮的数据（由 BlockGame 设置）
        private int[] shapeIndices;
        private int[] shapeTypes;
        private bool[] shapePlaced;

        /// <summary>放置成功时触发：(形状槽位, 网格行, 网格列)。</summary>
        public event Action<int, int, int> OnPlaced;

        public bool IsDragging => dragIndex >= 0;

        public BlockInput(Camera cam, BlockRenderer renderer,
                          BlockBoard board, Transform parent)
        {
            this.cam = cam;
            this.renderer = renderer;
            this.board = board;
            this.parent = parent;
        }

        /// <summary>每轮开始时，把当前轮的形状数据传进来。</summary>
        public void SetRoundData(int[] indices, int[] types, bool[] placed)
        {
            shapeIndices = indices;
            shapeTypes = types;
            shapePlaced = placed;
        }

        // ==============================================================
        //  每帧调用
        // ==============================================================

        public void Tick()
        {
            if (Input.GetMouseButtonDown(0) && dragIndex < 0)
                OnPointerDown();
            else if (Input.GetMouseButton(0) && dragIndex >= 0)
                OnPointerDrag();
            else if (Input.GetMouseButtonUp(0) && dragIndex >= 0)
                OnPointerUp();
        }

        // ==============================================================
        //  拖拽流程
        // ==============================================================

        private void OnPointerDown()
        {
            Vector2 wp = cam.ScreenToWorldPoint(Input.mousePosition);

            for (int i = 0; i < 3; i++)
            {
                if (shapePlaced[i]) continue;

                Rect bounds = renderer.GetTrayBounds(i, shapeIndices[i]);
                if (bounds.Contains(wp))
                {
                    StartDrag(i);
                    return;
                }
            }
        }

        private void StartDrag(int index)
        {
            dragIndex = index;
            renderer.HideTrayItem(index);

            dragGroup = renderer.CreateShapeVisual(shapeIndices[index],
                                                    shapeTypes[index], 1f);
            dragGroup.name = "DragGroup";
        }

        private void OnPointerDrag()
        {
            Vector2 wp = cam.ScreenToWorldPoint(Input.mousePosition);
            dragGroup.transform.position = new Vector3(wp.x, wp.y, -1f);

            GridPosFromWorld(wp, out int gr, out int gc);
            UpdatePreview(gr, gc);
        }

        private void OnPointerUp()
        {
            Vector2 wp = cam.ScreenToWorldPoint(Input.mousePosition);
            GridPosFromWorld(wp, out int gr, out int gc);

            int si = shapeIndices[dragIndex];

            if (board.CanPlace(si, gr, gc))
            {
                // 通知 BlockGame 处理放置逻辑
                int placedIndex = dragIndex;
                CleanupDrag();
                OnPlaced?.Invoke(placedIndex, gr, gc);
            }
            else
            {
                // 放不下，返回托盘
                renderer.ShowTrayItem(dragIndex);
                CleanupDrag();
            }
        }

        private void CleanupDrag()
        {
            ClearPreview();
            if (dragGroup != null) UnityEngine.Object.Destroy(dragGroup);
            dragIndex = -1;
        }

        // ==============================================================
        //  放置预览
        // ==============================================================

        private void UpdatePreview(int gr, int gc)
        {
            ClearPreview();
            if (dragIndex < 0) return;

            int si = shapeIndices[dragIndex];
            var shape = BlockShapes.All[si];
            int sRows = shape.GetLength(0);
            int sCols = shape.GetLength(1);

            bool canPlace = board.CanPlace(si, gr, gc);
            Color previewColor = canPlace
                ? new Color(0.3f, 0.9f, 0.3f, 0.4f)
                : new Color(0.9f, 0.3f, 0.3f, 0.4f);

            Sprite pixel = CreateTempPixel();

            for (int r = 0; r < sRows; r++)
                for (int c = 0; c < sCols; c++)
                {
                    if (shape[r, c] == 0) continue;
                    int pr = gr + r, pc = gc + c;
                    if (pr < 0 || pr >= BlockBoard.Rows || pc < 0 || pc >= BlockBoard.Cols)
                        continue;

                    var go = new GameObject("Preview");
                    go.transform.SetParent(parent);
                    go.transform.position = renderer.CellCenter(pr, pc);
                    go.transform.localScale = Vector3.one * (renderer.CellSize * 0.88f);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = pixel;
                    sr.color = previewColor;
                    sr.sortingOrder = 4;
                    previewCells.Add(sr);
                }
        }

        private void ClearPreview()
        {
            foreach (var sr in previewCells)
                if (sr != null) UnityEngine.Object.Destroy(sr.gameObject);
            previewCells.Clear();
        }

        // ==============================================================
        //  坐标转换
        // ==============================================================

        private void GridPosFromWorld(Vector2 wp, out int row, out int col)
        {
            var o = renderer.GridOrigin();
            float cs = renderer.CellSize;
            col = Mathf.FloorToInt((wp.x - o.x) / cs);
            row = Mathf.FloorToInt((o.y - wp.y) / cs);

            if (dragIndex >= 0)
            {
                int si = shapeIndices[dragIndex];
                row -= BlockShapes.Rows(si) / 2;
                col -= BlockShapes.Cols(si) / 2;
            }
        }

        // ==============================================================
        //  工具
        // ==============================================================

        private Sprite _tempPixel;
        private Sprite CreateTempPixel()
        {
            if (_tempPixel != null) return _tempPixel;
            var tex = new Texture2D(4, 4);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            _tempPixel = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _tempPixel;
        }
    }
}