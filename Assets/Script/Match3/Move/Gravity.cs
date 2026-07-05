namespace Match3
{
    /// <summary>
    /// 重力 / 下落：每一列把上方的块往下压，填补被消除留下的空位。
    /// </summary>
    public class Gravity
    {
        private readonly BoardGrid grid;

        public Gravity(BoardGrid grid)
        {
            this.grid = grid;
        }

        public void Collapse()
        {
            for (int x = 0; x < grid.Width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < grid.Height; y++)
                {
                    var t = grid.Get(x, y);
                    if (t != null)
                    {
                        if (y != writeY)
                        {
                            grid.Set(x, writeY, t);
                            grid.Set(x, y, null);
                            t.SetCoords(x, writeY, grid.GridToWorld(x, writeY));
                        }
                        writeY++;
                    }
                }
            }
        }
    }
}
