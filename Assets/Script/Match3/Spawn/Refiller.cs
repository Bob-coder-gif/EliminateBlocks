using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 下落之后，顶部剩下的空位由它生成新块，并让新块从棋盘上方掉落进来。
    /// </summary>
    public class Refiller
    {
        private readonly BoardGrid grid;
        private readonly TileFactory factory;

        public Refiller(BoardGrid grid, TileFactory factory)
        {
            this.grid = grid;
            this.factory = factory;
        }

        public void Refill()
        {
            for (int x = 0; x < grid.Width; x++)
            {
                int spawnRow = 0;
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.Get(x, y) == null)
                    {
                        int type = Random.Range(0, factory.TypeCount);
                        Vector3 spawn = grid.GridToWorld(x, grid.Height + spawnRow); // 从上方生成
                        var t = factory.Create(x, y, type, spawn);
                        grid.Set(x, y, t);
                        t.SetCoords(x, y, grid.GridToWorld(x, y));                    // 掉到目标格
                        spawnRow++;
                    }
                }
            }
        }
    }
}
