using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mine
{
    /// <summary>
    /// 扫雷成绩记录管理。
    /// 存储每个难度的最佳用时和历史记录。
    /// </summary>
    [Serializable]
    public class MineRecord
    {
        public int difficulty;     // 1/2/3
        public int seconds;        // 用时
        public string date;        // 日期
        public bool isWin;         // 是否胜利
    }

    [Serializable]
    public class MineSaveWrapper
    {
        public List<MineRecord> records = new List<MineRecord>();
    }

    public static class MineSaveData
    {
        private const string SAVE_KEY = "Mine_Records";
        private const int MAX_RECORDS = 50;    // 最多保存50条历史

        /// <summary>保存一条游戏记录。</summary>
        public static void SaveRecord(int difficulty, int seconds, bool isWin)
        {
            var data = LoadAll();
            data.records.Add(new MineRecord
            {
                difficulty = difficulty,
                seconds = seconds,
                date = DateTime.Now.ToString("MM-dd HH:mm"),
                isWin = isWin
            });

            // 限制历史数量
            if (data.records.Count > MAX_RECORDS)
                data.records.RemoveAt(0);

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>获取某个难度的最佳用时（秒），没有记录返回 -1。</summary>
        public static int GetBestTime(int difficulty)
        {
            var data = LoadAll();
            int best = -1;
            foreach (var r in data.records)
            {
                if (r.difficulty == difficulty && r.isWin)
                {
                    if (best < 0 || r.seconds < best)
                        best = r.seconds;
                }
            }
            return best;
        }

        /// <summary>获取某个难度的所有记录（最新的在前面）。</summary>
        public static List<MineRecord> GetRecords(int difficulty)
        {
            var data = LoadAll();
            var result = new List<MineRecord>();
            foreach (var r in data.records)
                if (r.difficulty == difficulty)
                    result.Add(r);
            result.Reverse();   // 最新的在前
            return result;
        }

        /// <summary>获取所有记录。</summary>
        public static List<MineRecord> GetAllRecords()
        {
            var data = LoadAll();
            var list = new List<MineRecord>(data.records);
            list.Reverse();
            return list;
        }

        /// <summary>清除所有记录。</summary>
        public static void ClearAll()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
        }

        private static MineSaveWrapper LoadAll()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(json))
                return new MineSaveWrapper();
            return JsonUtility.FromJson<MineSaveWrapper>(json);
        }
    }
}