using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Mine
{
    /// <summary>
    /// 扫雷模式的棋盘数据。
    /// grid[row, col]：0 = 空，0~8 = 周围雷数，9 = 雷。
    /// row=0 是最顶行
    /// 难度1 9*9 10 个雷
    /// 难度2 16*9 22 个雷
    /// 难度3 16*9 29 个雷
    /// </summary>
    public class MineGrid
    {
        public readonly int width;          // 棋盘列数
        public readonly int height;         // 棋盘行数
        public readonly int mineCount;      // 雷的数量
        public readonly int flagCount;      // 旗子数量
       
        public int[,] grid;        // 棋盘数据

        private int minePlaced = 0;     // 已放置的雷数量

        private readonly Random random = new Random();

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public int GetMineCount()
        {
            return mineCount;
        }

        public int GetCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return -1; // 越界返回 -1
            return grid[y, x];
        }

        public MineGrid(int w, int h, int mines, int flags)
        {
            width = w;
            height = h;
            mineCount = mines;
            flagCount = flags; // 旗子数量默认和雷数相同
            grid = new int[height, width];
        }

        /// <summary>网格坐标 -> 世界坐标（棋盘整体居中于原点）。</summary>
        public Vector3 GridToWorld(int x, int y)
        {
            float ox = -(width - 1) / 2f;
            float oy = -(height - 1) / 2f;
            return new Vector3(ox + x, oy + y , 0f);
        }
        public void createGrid(int safeX, int safeY)
        {
            // ---- 1. 布雷 ----
            while (minePlaced < mineCount)
            {
                for (int y = 0; y < height && minePlaced < mineCount; y++)
                {
                    for (int x = 0; x < width && minePlaced < mineCount; x++)
                    {
                        // 跳过安全格（不是终止循环，是跳过这一格）
                        if (x == safeX && y == safeY) continue;

                        if (random.createRandom() && grid[y, x] != 9)
                        {
                            grid[y, x] = 9;
                            minePlaced++;
                        }
                    }
                }
            }

            // ---- 2. 算数字（全部雷放完后算一次）----
            CalculateNumbers();
        }

        /// <summary>计算每个非雷格子周围的雷数。</summary>
        private void CalculateNumbers()
        {
            for (int y = 0; y < height; y++)          // 从0开始，包含边界
            {
                for (int x = 0; x < width; x++)       // 从0开始，包含边界
                {
                    if (grid[y, x] == 9) continue;

                    int count = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            // 边界检查，防止越界
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (grid[ny, nx] == 9)
                                    count++;
                            }
                        }
                    }
                    grid[y, x] = count;
                }
            }
        }

        public void PlaceMines(int x, int y)
        {
            if (grid[y, x] == 9)       
            {
                grid[y, x] = 0;
                minePlaced--;
                createGrid(x, y);
            }
        }
    }
}