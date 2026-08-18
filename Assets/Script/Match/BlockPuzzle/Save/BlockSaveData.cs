using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 模式B 的存档数据结构 + 读写工具。
    /// 用 PlayerPrefs + JsonUtility 做持久化。
    /// </summary>
    [System.Serializable]
    public class BlockSaveData
    {
        public int[] grid;
        public int[] shapeIndices;
        public int[] shapeTypes;
        public bool[] shapePlaced;
        public int score;

        private const string SaveKey = "Block_Save";
        private const string HighKey = "Block_HighScore";

        public static void Save(BlockBoard board, int[] shapeIndices,
                                int[] shapeTypes, bool[] shapePlaced, int score)
        {
            var data = new BlockSaveData
            {
                grid = board.Export(),
                shapeIndices = (int[])shapeIndices.Clone(),
                shapeTypes   = (int[])shapeTypes.Clone(),
                shapePlaced  = (bool[])shapePlaced.Clone(),
                score = score
            };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public static BlockSaveData Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, "");
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<BlockSaveData>(json);
        }

        public static bool HasSave() => PlayerPrefs.HasKey(SaveKey);
        public static void ClearSave() => PlayerPrefs.DeleteKey(SaveKey);

        public static int GetHighScore() => PlayerPrefs.GetInt(HighKey, 0);
        public static void SetHighScore(int score)
        {
            PlayerPrefs.SetInt(HighKey, score);
            PlayerPrefs.Save();
        }
    }
}