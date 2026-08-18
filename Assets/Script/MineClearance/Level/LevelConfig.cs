namespace Mine
{
    /// <summary>
    /// 扫雷模式的关卡配置数据。
    /// </summary>
    [System.Serializable]
    public class LevelConfig
    {
        public int levelNumber;   // 关卡编号（1-based）
        public int width;         // 棋盘列数
        public int height;        // 棋盘行数
        public int mineCount;     // 本关雷的数量
        public int flagCount;     // 本关旗子数量

        public LevelConfig(int num, int w, int h, int mines)
        {
            levelNumber = num;
            width = w;
            height = h;
            mineCount = mines;
            flagCount = mines; // 旗子数量默认和雷数相同
        }
    }
    
}