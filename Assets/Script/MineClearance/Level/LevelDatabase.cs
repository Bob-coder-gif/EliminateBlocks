namespace Mine
{
    /// <summary>
    /// 扫雷模式的关卡配置表。
    /// 可以调这张表来平衡难度，改数字就行，不用动逻辑代码。
    /// 难度1 9*9 10 个雷
    /// 难度2 16*9 22 个雷
    /// 难度3 16*9 29 个雷
    /// </summary>
    public static class LevelDatabase
    {
        //                                    编号  宽  高  雷数
        public static readonly LevelConfig[] Levels =
        {
            new LevelConfig( 1,  9, 9, 10),  // ── 新手入门 ──
            new LevelConfig( 2,  9, 16, 22),  // ── 中等难度 ──
            new LevelConfig( 3,  9, 16, 29),  // ── 高难度 ──
        };

        public static int Count => Levels.Length;

        /// <summary>按关卡编号获取（1-based）。</summary>
        public static LevelConfig Get(int levelNumber)
        {
            int index = levelNumber - 1;
            if (index < 0 || index >= Levels.Length)
                return Levels[0]; // 非法编号就回退到第一关
            return Levels[index];
        }
    }
}