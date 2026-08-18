using UnityEngine;
using Common;

namespace Mine
{
    public class GameManager : MonoBehaviour
    {
        [Header("精灵图（只需3张）")]
        public Sprite coveredSprite;
        public Sprite mineSprite;
        public Sprite flagSprite;

        private MineUI ui;
        private MineBoard mineBoard;
        private GameObject boardObject;

        private int currentLevel;
        private float timer;
        private bool isPlaying;     // 计时器是否在跑

        private void Start()
        {
            if (UIManager.Instance == null)
            {
                var uiGo = new GameObject("UIManager");
                uiGo.AddComponent<UIManager>();
                UIManager.Instance.Build();
            }

            var mineUiGo = new GameObject("MineUI");
            ui = mineUiGo.AddComponent<MineUI>();
            ui.Build();

            UIManager.Instance.OnMineModeClicked += () => ui.ShowDifficultySelect();

            ui.OnDifficultySelected += (level) => StartMineGame(level);
            ui.OnBackToMenu += () =>
            {
                CleanupAll();
                ui.HideAll();
                UIManager.Instance.ShowMainMenu();
            };
            ui.OnBackToDifficulty += () =>
            {
                CleanupAll();
                ui.ShowDifficultySelect();
            };
            ui.OnNewGame += () => StartMineGame(currentLevel);
        }

        private void StartMineGame(int level)
        {
            currentLevel = level;
            CleanupAll();

            var config = LevelDatabase.Get(level);

            boardObject = new GameObject("MineBoard");
            mineBoard = boardObject.AddComponent<MineBoard>();
            mineBoard.coveredSprite = coveredSprite;
            mineBoard.mineSprite    = mineSprite;
            mineBoard.flagSprite    = flagSprite;
            mineBoard.Init(new MineGrid(config.width, config.height, config.mineCount, config.mineCount));

            // 订阅事件
            mineBoard.OnFlagCountChanged += (remaining) => ui.UpdateFlagCount(remaining);
            mineBoard.OnGameOver         += OnMineGameOver;

            // 第一次点击才开始计时
            mineBoard.OnFirstClick += () => { isPlaying = true; };

            // HUD
            ui.ShowGameHUD(level);
            ui.UpdateMineCount(config.mineCount);
            ui.UpdateFlagCount(config.mineCount);

            // 显示最佳记录
            int best = MineSaveData.GetBestTime(level);
            if (best >= 0)
                ui.UpdateBestTime(best);

            timer = 0f;
            isPlaying = false;    // 不立即开始计时，等第一次点击
        }

        private void OnMineGameOver(bool isWin)
        {
            isPlaying = false;
            int seconds = Mathf.FloorToInt(timer);

            // 保存记录
            MineSaveData.SaveRecord(currentLevel, seconds, isWin);

            int bestTime = MineSaveData.GetBestTime(currentLevel);
            ui.ShowGameOver(isWin, seconds, currentLevel, bestTime);
        }

        private void Update()
        {
            if (isPlaying)
            {
                timer += Time.deltaTime;
                ui.UpdateTimer(Mathf.FloorToInt(timer));
            }
        }

        private void CleanupAll()
        {
            isPlaying = false;
            if (boardObject != null)
            {
                Destroy(boardObject);
                boardObject = null;
                mineBoard = null;
            }
        }
    }
}