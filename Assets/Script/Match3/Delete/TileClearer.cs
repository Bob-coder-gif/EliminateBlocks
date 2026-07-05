using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 消除：把匹配到的块从数据中清空，销毁其 GameObject，并在其位置生成消除特效。
    /// </summary>
    public class TileClearer
    {
        private readonly BoardGrid grid;
        private readonly GameObject effectPrefab;   // 消除特效预制体（可为空）

        public TileClearer(BoardGrid grid, GameObject effectPrefab)
        {
            this.grid = grid;
            this.effectPrefab = effectPrefab;
        }

        public void Clear(HashSet<Tile> tiles)
        {
            foreach (var t in tiles)
            {
                grid.Set(t.x, t.y, null);
                SpawnEffect(t.transform.position);
                Object.Destroy(t.gameObject);
            }
        }

        private void SpawnEffect(Vector3 pos)
        {
            if (effectPrefab != null)
                Object.Instantiate(effectPrefab, pos, Quaternion.identity);
        }
    }
}