namespace Match3
{
    /// <summary>
    /// 模式B 的 31 种方块形状。
    /// 每个形状用 int[,] 表示，shape[row, col]，1=有格子，0=没有。
    /// row=0 是形状的最顶行。
    /// </summary>
    public static class BlockShapes
    {
        public static readonly int[][,] All = new int[][,]
        {
            // ---- 基础方块 (5) ----
            new int[,] { {1} },                                                     // #0  1×1
            new int[,] { {1,1}, {1,1} },                                            // #1  2×2
            new int[,] { {1,1,1}, {1,1,1}, {1,1,1} },                              // #2  3×3
            new int[,] { {1,1,1,1} },                                               // #3  横1×4
            new int[,] { {1,1,1,1,1} },                                             // #4  横1×5
            new int[,] { {1}, {1}, {1}, {1} },                                      // #5  竖4×1
            new int[,] { {1}, {1}, {1}, {1}, {1} },                                 // #6  竖5×1

            // ---- 2格拐角 (4) ----
            new int[,] { {1,0}, {1,1} },                                            // #7
            new int[,] { {1,1}, {0,1} },                                            // #8
            new int[,] { {0,1}, {1,1} },                                            // #9
            new int[,] { {1,1}, {1,0} },                                            // #10

            // ---- T形 (4) ----
            new int[,] { {0,1,0}, {1,1,1} },                                       // #11
            new int[,] { {1,1,1}, {0,1,0} },                                       // #12
            new int[,] { {1,0}, {1,1}, {1,0} },                                    // #13
            new int[,] { {0,1}, {1,1}, {0,1} },                                    // #14

            // ---- L形 3×2 (4) ----
            new int[,] { {1,1,1}, {0,0,1} },                                       // #15
            new int[,] { {1,0,0}, {1,1,1} },                                       // #16
            new int[,] { {1,1,1}, {1,0,0} },                                       // #17
            new int[,] { {0,0,1}, {1,1,1} },                                       // #18

            // ---- L形 2×3 (4) ----
            new int[,] { {1,1}, {1,0}, {1,0} },                                    // #19
            new int[,] { {1,1}, {0,1}, {0,1} },                                    // #20
            new int[,] { {1,0}, {1,0}, {1,1} },                                    // #21
            new int[,] { {0,1}, {0,1}, {1,1} },                                    // #22

            // ---- 大L形 3×3 (4) ----
            new int[,] { {1,0,0}, {1,0,0}, {1,1,1} },                              // #23
            new int[,] { {1,1,1}, {0,0,1}, {0,0,1} },                              // #24
            new int[,] { {1,1,1}, {1,0,0}, {1,0,0} },                              // #25
            new int[,] { {0,0,1}, {0,0,1}, {1,1,1} },                              // #26

            // ---- S/Z形 (4) ----
            new int[,] { {1,0}, {1,1}, {0,1} },                                    // #27
            new int[,] { {0,1}, {1,1}, {1,0} },                                    // #28
            new int[,] { {1,1,0}, {0,1,1} },                                       // #29
            new int[,] { {0,1,1}, {1,1,0} },                                       // #30
        };

        /// <summary>获取形状的行数。</summary>
        public static int Rows(int shapeIndex) => All[shapeIndex].GetLength(0);

        /// <summary>获取形状的列数。</summary>
        public static int Cols(int shapeIndex) => All[shapeIndex].GetLength(1);

        /// <summary>获取形状包含的格子总数（用于统计）。</summary>
        public static int CellCount(int shapeIndex)
        {
            var s = All[shapeIndex];
            int count = 0;
            for (int r = 0; r < s.GetLength(0); r++)
                for (int c = 0; c < s.GetLength(1); c++)
                    if (s[r, c] == 1) count++;
            return count;
        }

        public static int Count => All.Length;
    }
}