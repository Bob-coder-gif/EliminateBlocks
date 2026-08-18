using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 洗牌：当检测到死局时，把盘上现有块的“类型”重新打乱（块的位置不动，只换外观），
    /// 反复打乱直到盘面满足两个条件：没有现成三连、并且至少存在一步可走。
    /// 用的是 Fisher–Yates 洗牌算法：从后往前，每个位置和它之前的随机一个位置交换，
    /// 能保证每种排列出现概率相等，是标准且无偏的打乱方法。
    /// </summary>
    public class Shuffler
    {
        private readonly BoardGrid grid;
        private readonly Sprite[] sprites;
        private readonly MatchFinder matchFinder;
        private readonly MoveValidator moveValidator;

        public Shuffler(BoardGrid grid, Sprite[] sprites, MatchFinder matchFinder, MoveValidator moveValidator)
        {
            this.grid = grid;
            this.sprites = sprites;
            this.matchFinder = matchFinder;
            this.moveValidator = moveValidator;
        }

        /// <summary>反复洗牌，直到得到一个“无三连且有可走步”的盘面。</summary>
        public void ShuffleUntilPlayable()
        {
            const int maxTries = 100;   // 安全上限，正常情况下几次就成功
            for (int i = 0; i < maxTries; i++)
            {
                ShuffleOnce();
                if (!matchFinder.HasMatch() && moveValidator.HasPossibleMove())
                    return;
            }
            Debug.LogWarning("洗牌多次仍未得到理想布局，已使用当前结果。");
        }

        private void ShuffleOnce()
        {
            // 收集所有块，以及它们当前的类型
            var tiles = new List<Tile>();
            foreach (var t in grid.AllTiles()) tiles.Add(t);

            var types = new List<int>(tiles.Count);
            foreach (var t in tiles) types.Add(t.type);

            // Fisher–Yates 打乱类型列表
            for (int i = types.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (types[i], types[j]) = (types[j], types[i]);
            }

            // 把打乱后的类型重新贴回每个块（位置不变，只换类型和图）
            for (int i = 0; i < tiles.Count; i++)
            {
                int nt = types[i];
                tiles[i].SetType(nt, sprites[nt]);
            }
        }
    }
}
