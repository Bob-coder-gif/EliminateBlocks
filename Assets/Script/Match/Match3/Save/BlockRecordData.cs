using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 方块模式成绩记录管理。
    /// 存储每次游戏的得分和历史记录。
    /// </summary>
    [Serializable]
    public class BlockRecord
    {
        public int score;
        public string date;
    }

    [Serializable]
    public class BlockSaveWrapper
    {
        public List<BlockRecord> records = new List<BlockRecord>();
        public int highScore = 0;
    }

    public static class BlockRecordData
    {
        private const string SAVE_KEY = "Block_Records";
        private const int MAX_RECORDS = 50;

        /// <summary>保存一条游戏记录。</summary>
        public static void SaveRecord(int score)
        {
            var data = LoadAll();

            data.records.Add(new BlockRecord
            {
                score = score,
                date = DateTime.Now.ToString("MM-dd HH:mm")
            });

            if (score > data.highScore)
                data.highScore = score;

            if (data.records.Count > MAX_RECORDS)
                data.records.RemoveAt(0);

            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        /// <summary>获取历史最高分。</summary>
        public static int GetHighScore()
        {
            return LoadAll().highScore;
        }

        /// <summary>获取所有记录（最新在前）。</summary>
        public static List<BlockRecord> GetAllRecords()
        {
            var data = LoadAll();
            var list = new List<BlockRecord>(data.records);
            list.Reverse();
            return list;
        }

        public static void ClearAll()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
        }

        private static BlockSaveWrapper LoadAll()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(json))
                return new BlockSaveWrapper();
            return JsonUtility.FromJson<BlockSaveWrapper>(json);
        }
    }
}