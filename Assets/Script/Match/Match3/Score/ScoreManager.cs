namespace Match3
{
    /// <summary>
    /// 计分与关卡状态：分数、上一轮得分、步数、目标分 / 步数上限，以及胜负判断。
    /// 计分公式集中在这里，改规则只动这一个类。
    /// </summary>
    public class ScoreManager
    {
        public int Score { get; private set; }
        public int LastGain { get; private set; }   // 上一次交换的总得分（用于显示）
        public int Steps { get; private set; }       // 已用有效步数

        public readonly int TargetScore;
        public readonly int MaxSteps;

        public ScoreManager(int targetScore, int maxSteps)
        {
            TargetScore = targetScore;
            MaxSteps = maxSteps;
        }

        /// <summary>
        /// 单轮基础分（不含连击）：
        /// (检测到的连数 - 1) * 5 + 本轮实际消除总数。
        /// 连击分不在这里加，避免每轮重复累计 combo。
        /// </summary>
        public static int BaseGain(int matchedCount, int clearedCount)
            => (matchedCount - 1) * 5 + clearedCount;

        /// <summary>一次交换结算：所有轮的基础分之和 + 一次性的连击分 5 * combo。</summary>
        public void AddSwapResult(int baseSum, int combo)
        {
            int gain = baseSum + 5 * combo;
            Score += gain;
            LastGain = gain;
        }

        public void UseStep() => Steps++;

        public void Add(int amount) => Score += amount;
        public void Reset() { Score = 0; LastGain = 0; Steps = 0; }

        // 结算判断
        public bool ReachedTarget => Score >= TargetScore;
        public bool OutOfSteps => Steps >= MaxSteps;
        public bool IsOver => ReachedTarget || OutOfSteps;
        public bool IsWin => ReachedTarget;   // 达到目标分即胜
    }
}