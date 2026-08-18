using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 关卡进度存档，用 PlayerPrefs 做持久化（最简单的方案）。
    ///
    /// 每关有三种状态：
    ///   0 = 锁定（Locked）
    ///   1 = 已解锁但未通关（Unlocked）
    ///   2 = 已通关（Cleared）
    ///
    /// 第 1 关默认解锁。通关第 N 关时自动解锁第 N+1 关。
    ///
    /// 小知识：PlayerPrefs 在 Windows 上存到注册表，macOS 存到 plist，
    /// Android/iOS 存到各自的沙盒。够用但不安全（玩家可以手动改），
    /// 正式上线的游戏一般用加密文件存档。学习阶段用这个完全没问题。
    /// </summary>
    public static class SaveManager
    {
        private const string KEY_PREFIX = "Match3_Level_";
        private const string KEY_BEST   = "Match3_Best_";

        /// <summary>获取关卡状态：0=锁定, 1=已解锁, 2=已通关。</summary>
        public static int GetLevelState(int level)
        {
            if (level == 1)
                return Mathf.Max(1, PlayerPrefs.GetInt(KEY_PREFIX + level, 1));
            return PlayerPrefs.GetInt(KEY_PREFIX + level, 0);
        }

        /// <summary>该关是否可以玩（已解锁或已通关）。</summary>
        public static bool IsPlayable(int level) => GetLevelState(level) >= 1;

        /// <summary>该关是否已通关。</summary>
        public static bool IsCleared(int level) => GetLevelState(level) >= 2;

        /// <summary>获取某关最高分（用于显示，0 表示没玩过）。</summary>
        public static int GetBestScore(int level) => PlayerPrefs.GetInt(KEY_BEST + level, 0);

        /// <summary>
        /// 通关结算：标记当前关为已通关，解锁下一关，记录最高分。
        /// </summary>
        public static void SetLevelCleared(int level, int score)
        {
            // 标记已通关
            PlayerPrefs.SetInt(KEY_PREFIX + level, 2);

            // 更新最高分
            int best = GetBestScore(level);
            if (score > best)
                PlayerPrefs.SetInt(KEY_BEST + level, score);

            // 解锁下一关（如果还没解锁）
            if (level < LevelDatabase.Count)
            {
                int nextLevel = level + 1;
                if (GetLevelState(nextLevel) == 0)
                    PlayerPrefs.SetInt(KEY_PREFIX + nextLevel, 1);
            }

            PlayerPrefs.Save();
        }

        /// <summary>清除所有存档（调试用）。</summary>
        public static void ClearAll()
        {
            for (int i = 1; i <= LevelDatabase.Count; i++)
            {
                PlayerPrefs.DeleteKey(KEY_PREFIX + i);
                PlayerPrefs.DeleteKey(KEY_BEST + i);
            }
            PlayerPrefs.Save();
            Debug.Log("所有存档已清除。");
        }
    }
}