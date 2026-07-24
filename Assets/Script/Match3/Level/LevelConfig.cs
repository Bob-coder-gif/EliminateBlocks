namespace Match3
{
    /// <summary>
    /// 单个关卡的配置数据。
    /// 不继承 MonoBehaviour，纯数据结构，方便在代码里直接 new。
    /// </summary>
    [System.Serializable]
    public class LevelConfig
    {
        public int levelNumber;   // 关卡编号（1-based）
        public int width;         // 棋盘列数
        public int height;        // 棋盘行数
        public int typeCount;     // 本关使用几种水果（3~9）
        public int targetScore;   // 通关目标分
        public int maxSteps;      // 最大步数

        public LevelConfig(int num, int w, int h, int types, int target, int steps)
        {
            levelNumber = num;
            width = w;
            height = h;
            typeCount = types;
            targetScore = target;
            maxSteps = steps;
        }
    }
}