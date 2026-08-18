using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 模式B 的 9×10 棋盘数据。
    /// grid[row, col]：-1 = 空，0~8 = 水果类型。
    /// row=0 是最顶行，row=9 是最底行。
    /// </summary>
    public class BlockBoard
    {
        public const int Cols = 9;
        public const int Rows = 10;

        private readonly int[,] grid = new int[Rows, Cols];

        public BlockBoard() { Clear(); }

        public void Clear()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    grid[r, c] = -1;
        }

        public int Get(int row, int col) => grid[row, col];
        public void Set(int row, int col, int type) => grid[row, col] = type;
        public bool IsEmpty(int row, int col) => grid[row, col] == -1;

        public bool CanPlace(int shapeIndex, int startRow, int startCol)
        {
            var shape = BlockShapes.All[shapeIndex];
            int sRows = shape.GetLength(0);
            int sCols = shape.GetLength(1);

            for (int r = 0; r < sRows; r++)
                for (int c = 0; c < sCols; c++)
                {
                    if (shape[r, c] == 0) continue;
                    int gr = startRow + r;
                    int gc = startCol + c;
                    if (gr < 0 || gr >= Rows || gc < 0 || gc >= Cols) return false;
                    if (!IsEmpty(gr, gc)) return false;
                }
            return true;
        }

        public void Place(int shapeIndex, int startRow, int startCol, int fruitType)
        {
            var shape = BlockShapes.All[shapeIndex];
            int sRows = shape.GetLength(0);
            int sCols = shape.GetLength(1);

            for (int r = 0; r < sRows; r++)
                for (int c = 0; c < sCols; c++)
                    if (shape[r, c] == 1)
                        grid[startRow + r, startCol + c] = fruitType;
        }

        public bool CanFitAnywhere(int shapeIndex)
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (CanPlace(shapeIndex, r, c)) return true;
            return false;
        }

        /// <summary>
        /// 检查并消除满行满列。
        /// 返回消除条数，并把被消除的格子坐标填入 clearedCells（给特效用）。
        /// </summary>
        public int ClearFullLines(List<Vector2Int> clearedCells)
        {
            clearedCells.Clear();
            bool[] fullRows = new bool[Rows];
            bool[] fullCols = new bool[Cols];
            int cleared = 0;

            for (int r = 0; r < Rows; r++)
            {
                bool full = true;
                for (int c = 0; c < Cols; c++)
                    if (IsEmpty(r, c)) { full = false; break; }
                if (full) { fullRows[r] = true; cleared++; }
            }

            for (int c = 0; c < Cols; c++)
            {
                bool full = true;
                for (int r = 0; r < Rows; r++)
                    if (IsEmpty(r, c)) { full = false; break; }
                if (full) { fullCols[c] = true; cleared++; }
            }

            // 收集被消除的格子 + 执行消除
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (fullRows[r] || fullCols[c])
                    {
                        if (grid[r, c] != -1)
                            clearedCells.Add(new Vector2Int(r, c));
                        grid[r, c] = -1;
                    }

            return cleared;
        }

        public static int ScoreForLines(int lineCount)
        {
            switch (lineCount)
            {
                case 0: return 0;
                case 1: return 100;
                case 2: return 300;
                case 3: return 600;
                case 4: return 1000;
                default: return 1500;
            }
        }

        public int[] Export()
        {
            int[] data = new int[Rows * Cols];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    data[r * Cols + c] = grid[r, c];
            return data;
        }

        public void Import(int[] data)
        {
            if (data == null || data.Length != Rows * Cols) { Clear(); return; }
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    grid[r, c] = data[r * Cols + c];
        }
    }
}