using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 棋盘的“数据”本体：只负责存放二维格子和坐标换算，不含任何玩法逻辑。
    /// 所有子系统（生成 / 检测 / 移动 / 消除）都共享同一个 BoardGrid 实例来读写状态。
    /// 注意：类名没叫 Grid，是为了避开 Unity 自带的 UnityEngine.Grid 组件。
    /// </summary>
    public class BoardGrid
    {
        public readonly int Width;
        public readonly int Height;
        public readonly float TileSize;

        private readonly Tile[,] tiles;

        public BoardGrid(int width, int height, float tileSize)
        {
            Width = width;
            Height = height;
            TileSize = tileSize;
            tiles = new Tile[width, height];
        }

        public Tile Get(int x, int y) => tiles[x, y];
        public void Set(int x, int y, Tile t) => tiles[x, y] = t;

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        /// <summary>网格坐标 -> 世界坐标（棋盘整体居中于原点）。</summary>
        public Vector3 GridToWorld(int x, int y)
        {
            float ox = -(Width - 1) * TileSize / 2f;
            float oy = -(Height - 1) * TileSize / 2f;
            return new Vector3(ox + x * TileSize, oy + y * TileSize, 0f);
        }

        /// <summary>遍历所有非空块（等待动画、全盘扫描时用）。</summary>
        public IEnumerable<Tile> AllTiles()
        {
            foreach (var t in tiles)
                if (t != null) yield return t;
        }
    }
}
