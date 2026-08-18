using UnityEngine;
using Common;

namespace Match3
{
    public class GameManager : MonoBehaviour
    {
        [Header("消消乐模式")]

        [Header("素材（把 9 个水果 Sprite 全拖进来）")]
        public Sprite[] candySprites;

        [Header("消除特效（可选）")]
        public GameObject clearEffectPrefab;

        [Header("背景（可选）")]
        public SpriteRenderer background;

        [Header("动画速度")]
        public float moveSpeed = 12f;

        [Header("===== HUD 调节 =====")]
        [Range(0f, 0.5f)] public float hudAnchorX = 0.1f;
        [Range(0f, 100f)] public float hudPadTop = 80f;
        [Range(0f, 200f)] public float hudPadLeft = 100f;
        [Range(30f, 80f)] public float hudLineHeight = 50f;
        [Range(20, 60)]   public int hudFontSize = 34;
        public Color hudTextColor = new Color(0.2f, 0.6f, 1f);

        [Header("===== 难度调节（模式A） =====")]
        public bool useCustomDifficulty = false;
        public int debugLevel = 0;
        [Range(4, 12)] public int debugWidth = 7;
        [Range(4, 12)] public int debugHeight = 9;
        [Range(3, 9)]  public int debugTypeCount = 5;
        public int debugTargetScore = 5000;
        [Range(5, 50)] public int debugMaxSteps = 30;

        private MatchUI ui;
        private GameBoard board;
        private BlockGame blockGame;
        private GameObject boardObject;
        private int currentLevel;
        

        private void Start()
        {
            if (candySprites == null || candySprites.Length < 9)
            {
                Debug.LogError("请在 candySprites 里放入 9 个水果 Sprite！");
                enabled = false;
                return;
            }

            // ============ 1. 初始化 UIManager（只需一次）============
            if (UIManager.Instance == null)
            {
                var uiGo = new GameObject("UIManager");
                uiGo.AddComponent<UIManager>();
                UIManager.Instance.Build();
            }

            // ============ 2. 用 AddComponent 创建 MatchUI ============
            //  MonoBehaviour 不能用 new，必须用 AddComponent
            var matchGo = new GameObject("MatchUI");
            ui = matchGo.AddComponent<MatchUI>();
            ui.Build();

            // ============ 3. 订阅 UIManager 事件 ============
            UIManager.Instance.OnMatchLevelClicked += () => ui.ShowLevelSelect();
            UIManager.Instance.OnBlockModeClicked  += StartBlockMode;
            UIManager.Instance.OnClearSaveClicked  += () =>
            {
                SaveManager.ClearAll();
                ui.RefreshLevelButtons();
            };

            // ============ 4. 订阅 MatchUI 事件 ============
            ui.OnBackToMenu        += () => { CleanupAll(); ui.HideAll(); UIManager.Instance.ShowMainMenu(); };
            ui.OnBackToLevelSelect += () => { CleanupAll(); ui.ShowLevelSelect(); };
            ui.OnRetryLevel        += () => StartLevel(currentLevel);
            ui.OnLevelSelected     += OnLevelSelected;

            // 模式B
            ui.OnBackToMenuFromB   += () => { CleanupAll(); ui.HideAll(); UIManager.Instance.ShowMainMenu(); };
            ui.OnBlockNewGame      += () => { PlayerPrefs.DeleteKey("Block_Save"); StartBlockMode(); };
            ui.OnBlockUndo         += () => { if (blockGame != null) blockGame.Undo(); };
        }

        // ==============================================================
        //  模式A：闯关
        // ==============================================================

        private void OnLevelSelected(int level)
        {
            if (!SaveManager.IsPlayable(level)) return;
            StartLevel(level);
        }

        private void StartLevel(int level)
        {
            currentLevel = level;
            var config = LevelDatabase.Get(level);

            int w, h, types, target, steps;

            if (useCustomDifficulty)
            {
                w = debugWidth; h = debugHeight; types = debugTypeCount;
                target = debugTargetScore; steps = debugMaxSteps;
            }
            else
            {
                w = config.width; h = config.height; types = config.typeCount;
                target = config.targetScore; steps = config.maxSteps;
                debugLevel = level; debugWidth = w; debugHeight = h;
                debugTypeCount = types; debugTargetScore = target; debugMaxSteps = steps;
            }

            CleanupAll();

            boardObject = new GameObject("GameBoard");
            board = boardObject.AddComponent<GameBoard>();
            board.width = w; board.height = h; board.tileSize = 1f;
            board.moveSpeed = moveSpeed; board.targetScore = target;
            board.maxSteps = steps; board.clearEffectPrefab = clearEffectPrefab;
            board.background = background;

            board.candySprites = new Sprite[types];
            for (int i = 0; i < types; i++)
                board.candySprites[i] = candySprites[i];

            board.OnScoreChanged += (s) => ui.UpdateScore(s);
            board.OnStepsChanged += (used, max) => ui.UpdateSteps(used, max);
            board.OnLastGain     += (g) => ui.ShowLastGain(g);
            board.OnShuffling    += (b) => ui.ShowShuffleHint(b);
            board.OnGameOver     += OnModeAGameOver;

            ui.ShowGameHUD(level);
            ui.UpdateTarget(target); ui.UpdateScore(0); ui.UpdateSteps(0, steps);
        }

        private void OnModeAGameOver(bool isWin, int score)
        {
            if (isWin) SaveManager.SetLevelCleared(currentLevel, score);
            ui.ShowGameOver(isWin, score, currentLevel);
        }

        // ==============================================================
        //  模式B：方块拼图
        // ==============================================================

        private void StartBlockMode()
        {
            CleanupAll();

            boardObject = new GameObject("BlockGame");
            blockGame = boardObject.AddComponent<BlockGame>();
            blockGame.candySprites = candySprites;
            blockGame.background = background;
            blockGame.clearEffectPrefab = clearEffectPrefab;

            blockGame.OnScoreChanged     += (s) => ui.UpdateBlockScore(s);
            blockGame.OnHighScoreChanged += (s) => ui.UpdateBlockHighScore(s);
            blockGame.OnGameOver         += OnBlockGameOver;
            blockGame.OnUndoAvailableChanged += (available) => ui.SetUndoInteractable(available);

            ui.ShowBlockHUD();
        }

        // 在 OnBlockGameOver 里保存记录
   // GameManager.cs 第162行左右，把 BlockSaveData 改成 BlockRecordData
        private void OnBlockGameOver()
        {
            if (blockGame != null)
                BlockRecordData.SaveRecord(blockGame.CurrentScore);  // ← 改这里

            ui.ShowBlockGameOver();
        }
        
        // ==============================================================
        //  清理
        // ==============================================================

        private void CleanupAll()
        {
            if (boardObject != null)
            {
                Destroy(boardObject);
                boardObject = null;
                board = null;
                blockGame = null;
            }
        }

        private void Update()
        {
            if (ui != null)
                ui.ApplyHudStyle(hudAnchorX, hudPadLeft, hudPadTop, hudLineHeight,
                                 hudFontSize, hudTextColor);
        }
    }
}