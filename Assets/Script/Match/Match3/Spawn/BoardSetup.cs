using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 开局把棋盘铺满，并保证不会一上来就有三连。
    /// </summary>
    public class BoardSetup
    {
        private readonly BoardGrid grid;
        private readonly TileFactory factory;

        public BoardSetup(BoardGrid grid, TileFactory factory)
        {
            this.grid = grid;
            this.factory = factory;
        }

        public void Fill()
        {
            for (int x = 0; x < grid.Width; x++)
                for (int y = 0; y < grid.Height; y++)
                {
                    int type = RandomTypeNoMatch(x, y);
                    grid.Set(x, y, factory.Create(x, y, type, grid.GridToWorld(x, y)));
                }
        }

        /// <summary>随机一个不会与左边两个 / 下边两个凑成三连的类型。</summary>
        private int RandomTypeNoMatch(int x, int y)
        {
            var options = new List<int>();
            for (int t = 0; t < factory.TypeCount; t++)
            {
                bool h = x >= 2 && grid.Get(x - 1, y).type == t && grid.Get(x - 2, y).type == t;
                bool v = y >= 2 && grid.Get(x, y - 1).type == t && grid.Get(x, y - 2).type == t;
                if (!h && !v) options.Add(t);
            }
            if (options.Count == 0) return Random.Range(0, factory.TypeCount);
            return options[Random.Range(0, options.Count)];
        }
    }
}
