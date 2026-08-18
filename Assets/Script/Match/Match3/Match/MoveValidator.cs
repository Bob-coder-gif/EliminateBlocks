namespace Match3
{
    /// <summary>
    /// 死局检测：判断当前盘面是否还存在“至少一步能凑成三连”的合法交换。
    /// 做法：把每个块与它右边、上边的邻居做“假设交换”，检查是否产生三连，测完再换回来。
    /// 只测右和上，是因为左 / 下的组合已经被前面的块测过了，避免重复。
    /// 整个过程在一份 int 类型副本上进行，不碰真正的 Tile 对象，也不改动棋盘。
    /// </summary>
    public class MoveValidator
    {
        private readonly BoardGrid grid;

        public MoveValidator(BoardGrid grid)
        {
            this.grid = grid;
        }

        /// <summary>盘面上是否还有可走的一步。</summary>
        public bool HasPossibleMove()
        {
            int W = grid.Width, H = grid.Height;

            // 拷一份类型快照（-1 表示空）
            int[,] t = new int[W, H];
            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                    t[x, y] = grid.Get(x, y) != null ? grid.Get(x, y).type : -1;

            for (int x = 0; x < W; x++)
                for (int y = 0; y < H; y++)
                {
                    if (x + 1 < W && SwapMakesMatch(t, x, y, x + 1, y)) return true; // 和右边换
                    if (y + 1 < H && SwapMakesMatch(t, x, y, x, y + 1)) return true; // 和上边换
                }
            return false;
        }

        /// <summary>假设交换 (ax,ay) 和 (bx,by)，看是否有一方成三连，测完还原。</summary>
        private bool SwapMakesMatch(int[,] t, int ax, int ay, int bx, int by)
        {
            Swap(t, ax, ay, bx, by);
            bool matched = HasRunAt(t, ax, ay) || HasRunAt(t, bx, by);
            Swap(t, ax, ay, bx, by); // 还原
            return matched;
        }

        private void Swap(int[,] t, int ax, int ay, int bx, int by)
        {
            int tmp = t[ax, ay];
            t[ax, ay] = t[bx, by];
            t[bx, by] = tmp;
        }

        /// <summary>(x,y) 处的类型是否横向或纵向连成了 3 个及以上。</summary>
        private bool HasRunAt(int[,] t, int x, int y)
        {
            int v = t[x, y];
            if (v < 0) return false;
            int W = t.GetLength(0), H = t.GetLength(1);

            // 横向：向左 + 向右数同色
            int count = 1;
            for (int i = x - 1; i >= 0 && t[i, y] == v; i--) count++;
            for (int i = x + 1; i < W && t[i, y] == v; i++) count++;
            if (count >= 3) return true;

            // 纵向：向下 + 向上数同色
            count = 1;
            for (int j = y - 1; j >= 0 && t[x, j] == v; j--) count++;
            for (int j = y + 1; j < H && t[x, j] == v; j++) count++;
            return count >= 3;
        }
    }
}
