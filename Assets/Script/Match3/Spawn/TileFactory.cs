using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 负责“生产”一个块：创建 GameObject，挂上 SpriteRenderer + BoxCollider2D + Tile。
    /// 只做生成这一件事，被 BoardSetup（开局铺满）和 Refiller（补充新块）共用。
    /// </summary>
    public class TileFactory
    {
        private readonly Sprite[] sprites;
        private readonly Transform parent;
        private readonly BoardGrid grid;
        private readonly float moveSpeed;
        private readonly float tileSize;

        public int TypeCount => sprites.Length;

        public TileFactory(Sprite[] sprites, Transform parent, BoardGrid grid, float moveSpeed, float tileSize)
        {
            this.sprites = sprites;
            this.parent = parent;
            this.grid = grid;
            this.moveSpeed = moveSpeed;
            this.tileSize = tileSize;
        }

        /// <summary>在 spawnPos 生成一个块，其目标格是 (x, y)。</summary>
        public Tile Create(int x, int y, int type, Vector3 spawnPos)
        {
            var go = new GameObject($"Tile_{x}_{y}");
            go.transform.parent = parent;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[type];
            sr.sortingOrder = 1;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * tileSize;   // 点击范围 = 一格

            var t = go.AddComponent<Tile>();
            t.Init(x, y, type, sr, moveSpeed);
            go.transform.position = spawnPos;
            t.target = grid.GridToWorld(x, y);
            return t;
        }
    }
}
