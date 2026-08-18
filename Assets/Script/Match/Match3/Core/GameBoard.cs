using System;
using System.Collections;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 总指挥（由 GameManager 在运行时动态创建，不再需要手动挂到场景）。
    /// 
    /// 相比原版的改动：
    ///   1. 去掉了 OnGUI（IMGUI），改为通过事件通知 UIManager 更新界面
    ///   2. 去掉了 Restart（不再用 SceneManager 重载场景，由 GameManager 控制）
    ///   3. width/height/targetScore/maxSteps 等参数由 GameManager 在创建后赋值
    ///   4. candySprites 由 GameManager 根据关卡配置裁剪后传入
    ///
    /// 核心游戏流程不变：交换 → 检测 → 消除 → 下落 → 补充 → 连锁。
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [Header("棋盘尺寸")]
        public int width = 7;
        public int height = 9;
        public float tileSize = 1f;

        [Header("素材")]
        public Sprite[] candySprites;

        [Header("动画速度")]
        public float moveSpeed = 12f;

        [Header("关卡目标")]
        public int targetScore = 5000;
        public int maxSteps = 30;

        [Header("消除特效")]
        public GameObject clearEffectPrefab;

        [Header("背景")]
        public SpriteRenderer background;

        // ---- UI 事件（由 GameManager 注册监听） ----
        public event Action<int> OnScoreChanged;         // 分数变化
        public event Action<int, int> OnStepsChanged;    // (已用步数, 最大步数)
        public event Action<int> OnLastGain;             // 上一次得分增量
        public event Action<bool> OnShuffling;           // 是否正在洗牌
        public event Action<bool, int> OnGameOver;       // (是否胜利, 最终分数)

        // 子系统
        private BoardGrid grid;
        private TileFactory factory;
        private MatchFinder matchFinder;
        private ClearResolver clearResolver;
        private TileSwapper swapper;
        private Gravity gravity;
        private Refiller refiller;
        private TileClearer clearer;
        private InputHandler input;
        private ScoreManager score;
        private MoveValidator moveValidator;
        private Shuffler shuffler;

        // 状态
        private bool busy;
        private bool gameOver;

        private void Start()
        {
            if (candySprites == null || candySprites.Length < 3)
            {
                Debug.LogError("candySprites 数量不足！");
                enabled = false;
                return;
            }

            // ---- 组装子系统 ----
            grid = new BoardGrid(width, height, tileSize);
            factory = new TileFactory(candySprites, transform, grid, moveSpeed, tileSize);
            matchFinder = new MatchFinder(grid);
            clearResolver = new ClearResolver(grid, matchFinder);
            swapper = new TileSwapper(grid);
            gravity = new Gravity(grid);
            refiller = new Refiller(grid, factory);
            clearer = new TileClearer(grid, clearEffectPrefab);
            score = new ScoreManager(targetScore, maxSteps);
            moveValidator = new MoveValidator(grid);
            shuffler = new Shuffler(grid, candySprites, matchFinder, moveValidator);

            input = new InputHandler(Camera.main, swapper);
            input.OnSwapRequested += (a, b) => StartCoroutine(TrySwap(a, b));

            new BoardSetup(grid, factory).Fill();

            if (!moveValidator.HasPossibleMove())
                shuffler.ShuffleUntilPlayable();

            CenterCamera();
            FitBackground();

            // 通知 UI 初始状态
            NotifyScoreUI();
        }

        private void Update()
        {
            if (!busy && !gameOver) input.Tick();
        }

        // ---- 流程调度（和原版一样） ----

        private IEnumerator TrySwap(Tile a, Tile b)
        {
            busy = true;
            swapper.Swap(a, b);
            yield return WaitForMoves();

            if (!matchFinder.HasMatch())
            {
                swapper.Swap(a, b);
                yield return WaitForMoves();
            }
            else
            {
                score.UseStep();
                NotifyScoreUI();
                yield return ResolveMatches();

                if (score.IsOver)
                {
                    gameOver = true;
                    busy = false;
                    OnGameOver?.Invoke(score.IsWin, score.Score);
                    yield break;
                }
            }

            yield return HandleDeadlockIfAny();
            busy = false;
        }

        private IEnumerator ResolveMatches()
        {
            int combo = 0;
            int baseSum = 0;

            while (true)
            {
                var r = clearResolver.Resolve();
                if (r.Tiles.Count == 0)
                {
                    if (combo > 0)
                    {
                        score.AddSwapResult(baseSum, combo);
                        NotifyScoreUI();
                        OnLastGain?.Invoke(score.LastGain);
                    }
                    yield break;
                }

                combo++;
                baseSum += ScoreManager.BaseGain(r.MatchedCount, r.Tiles.Count);

                clearer.Clear(r.Tiles);
                yield return new WaitForSeconds(0.08f);

                gravity.Collapse();
                refiller.Refill();
                yield return WaitForMoves();
            }
        }

        private IEnumerator HandleDeadlockIfAny()
        {
            if (moveValidator.HasPossibleMove()) yield break;

            OnShuffling?.Invoke(true);
            yield return new WaitForSeconds(0.4f);
            shuffler.ShuffleUntilPlayable();
            yield return WaitForMoves();
            OnShuffling?.Invoke(false);
        }

        private IEnumerator WaitForMoves()
        {
            bool moving = true;
            while (moving)
            {
                moving = false;
                foreach (var t in grid.AllTiles())
                {
                    if (!t.AtTarget) { moving = true; break; }
                }
                yield return null;
            }
        }

        private void NotifyScoreUI()
        {
            OnScoreChanged?.Invoke(score.Score);
            OnStepsChanged?.Invoke(score.Steps, score.MaxSteps);
        }

        // ---- 摄像机 / 背景 ----

        private void CenterCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            float halfW = (width * tileSize) / 2f + tileSize;
            float halfH = (height * tileSize) / 2f + tileSize;

            float aspect = cam.aspect;
            float sizeByWidth = halfW / Mathf.Max(aspect, 0.01f);
            float sizeByHeight = halfH;

            cam.orthographicSize = Mathf.Max(sizeByWidth, sizeByHeight);
        }

        private void FitBackground()
        {
            if (background == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            float camH = cam.orthographicSize * 2f;
            float camW = camH * cam.aspect;
            Vector2 size = background.sprite.bounds.size;
            float scale = Mathf.Max(camW / size.x, camH / size.y);
            background.transform.localScale = new Vector3(scale, scale, 1f);
            background.transform.position = new Vector3(0f, 0f, background.transform.position.z);
        }
    }
}