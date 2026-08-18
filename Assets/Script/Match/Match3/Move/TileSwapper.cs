using System;

namespace Match3
{
    /// <summary>
    /// 负责交换两个块（同时更新数据和移动目标），以及判断两个块是否相邻。
    /// </summary>
    public class TileSwapper
    {
        private readonly BoardGrid grid;

        public TileSwapper(BoardGrid grid)
        {
            this.grid = grid;
        }

        public bool AreAdjacent(Tile a, Tile b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) == 1;
        }

        public void Swap(Tile a, Tile b)
        {
            int ax = a.x, ay = a.y, bx = b.x, by = b.y;
            grid.Set(ax, ay, b);
            grid.Set(bx, by, a);
            a.SetCoords(bx, by, grid.GridToWorld(bx, by));
            b.SetCoords(ax, ay, grid.GridToWorld(ax, ay));
        }
    }
}
