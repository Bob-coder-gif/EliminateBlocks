namespace Match3
{
    /// <summary>
    /// 20 关的配置表。
    /// 难度梯度设计思路：
    ///   - 水果种类越多，随机性越大，越难凑出三连 → 逐渐增加
    ///   - 棋盘越大，需要消除的面积越大 → 逐渐增大
    ///   - 目标分越高、步数越少 → 逐渐收紧
    ///
    /// 可以调这张表来平衡难度，改数字就行，不用动逻辑代码。
    /// </summary>
    public static class LevelDatabase
    {
        //                                    编号  宽  高  种类  目标分  步数
        public static readonly LevelConfig[] Levels =
        {
            new LevelConfig( 1,  5, 5, 3,   500,  30),  // ── 新手入门 ──
            new LevelConfig( 2,  5, 5, 3,   800,  30),
            new LevelConfig( 3,  5, 6, 4,  1000,  30),
            new LevelConfig( 4,  5, 6, 4,  1200,  30),
            new LevelConfig( 5,  6, 6, 4,  1400,  30),

            new LevelConfig( 6,  6, 7, 5,  900,  25),  // ── 中等难度 ──
            new LevelConfig( 7,  6, 7, 5,  900,  25),
            new LevelConfig( 8,  6, 8, 5,  900,  24),
            new LevelConfig( 9,  7, 7, 5,  950,  24),
            new LevelConfig(10,  7, 8, 6,  800,  24),

            new LevelConfig(11,  7, 8, 6,  820,  24),  // ── 较难 ──
            new LevelConfig(12,  7, 8, 6,  840,  24),
            new LevelConfig(13,  7, 8, 6,  870,  23),
            new LevelConfig(14,  7, 8, 7,  800,  29),
            new LevelConfig(15,  7, 8, 7,  800,  28),

            new LevelConfig(16,  7, 8, 7,  700,  30),  // ── 高难度 ──
            new LevelConfig(17,  7, 8, 7,  720,  30),
            new LevelConfig(18,  7, 8, 7,  750,  30),
            new LevelConfig(19,  7, 8, 7,  770,  30),
            new LevelConfig(20,  7, 8, 7,  790,  30),  
        };

        public static int Count => Levels.Length;

        /// <summary>按关卡编号获取（1-based）。</summary>
        public static LevelConfig Get(int levelNumber)
        {
            return Levels[levelNumber - 1];
        }
    }
}