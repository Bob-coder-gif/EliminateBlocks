using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 单个糖果块。由 TileFactory 在运行时挂载，你不需要手动加。
    /// 只负责：记住自己的网格坐标 / 类型，并平滑移动到目标世界坐标。
    /// </summary>
    public class Tile : MonoBehaviour
    {
        public int x;        // 列
        public int y;        // 行（0 在最下方）
        public int type;     // 糖果种类，对应 candySprites 下标
        public SpriteRenderer sr;

        public Vector3 target;   // 要移动到的世界坐标
        private float speed;

        public void Init(int x, int y, int type, SpriteRenderer sr, float speed)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.sr = sr;
            this.speed = speed;
        }

        /// <summary>更新逻辑坐标 + 设定新的移动目标（交换 / 下落时用）。</summary>
        public void SetCoords(int nx, int ny, Vector3 worldTarget)
        {
            x = nx;
            y = ny;
            target = worldTarget;
        }

        /// <summary>换一种糖果（洗牌时用：位置不动，只改类型和外观）。</summary>
        public void SetType(int newType, Sprite sprite)
        {
            type = newType;
            sr.sprite = sprite;
        }

        /// <summary>选中时放大并置顶。</summary>
        public void SetHighlight(bool on)
        {
            transform.localScale = on ? Vector3.one * 1.15f : Vector3.one;
            sr.sortingOrder = on ? 5 : 1;
        }

        /// <summary>是否已到达目标位置。</summary>
        public bool AtTarget => (transform.position - target).sqrMagnitude <= 0.0001f;

        private void Update()
        {
            if (!AtTarget)
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            else
                transform.position = target;
        }
    }
}
