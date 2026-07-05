using System;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 输入：处理鼠标点击、选中 / 取消选中，检测到一次合法的相邻交换时，
    /// 通过 OnSwapRequested 事件通知 GameBoard 去执行（输入层不关心交换后的玩法流程）。
    /// </summary>
    public class InputHandler
    {
        private readonly Camera cam;
        private readonly TileSwapper swapper;
        private Tile selected;

        /// <summary>玩家请求交换两个相邻块时触发。</summary>
        public event Action<Tile, Tile> OnSwapRequested;

        public InputHandler(Camera cam, TileSwapper swapper)
        {
            this.cam = cam;
            this.swapper = swapper;
        }

        /// <summary>每帧由 GameBoard 调用一次。</summary>
        public void Tick()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            Vector2 p = cam.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(p, Vector2.zero);
            if (hit.collider == null) return;

            var t = hit.collider.GetComponent<Tile>();
            if (t != null) Handle(t);
        }

        private void Handle(Tile t)
        {
            if (selected == null)
            {
                Select(t);
            }
            else if (selected == t)
            {
                Deselect();
            }
            else if (swapper.AreAdjacent(selected, t))
            {
                var a = selected;
                Deselect();
                OnSwapRequested?.Invoke(a, t);
            }
            else
            {
                Deselect();
                Select(t);
            }
        }

        private void Select(Tile t)
        {
            selected = t;
            t.SetHighlight(true);
        }

        private void Deselect()
        {
            if (selected != null) selected.SetHighlight(false);
            selected = null;
        }
    }
}
