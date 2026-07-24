using System;
using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class UIManager
    {
        // ---- 面板 ----
        private GameObject mainMenuPanel;
        private GameObject levelSelectPanel;
        private GameObject gameHudPanel;
        private GameObject gameOverPanel;
        private GameObject blockHudPanel;      // 模式B HUD
        private GameObject blockOverPanel;     // 模式B 结算

        // ---- 模式A HUD 控件 ----
        private Text scoreText, stepsText, targetText, lastGainText, shuffleText, levelNameText;

        // ---- 模式A 结算控件 ----
        private Text resultTitleText, resultScoreText, resultBestText;

        // ---- 模式B HUD 控件 ----
        private Text blockScoreText, blockHighText;

        // ---- 模式B 结算控件 ----
        private Text blockOverScoreText, blockOverHighText;

        // ---- 选关按钮 ----
        private Button[] levelButtons;
        private Text[] levelButtonTexts;
        private Image[] levelButtonImages;

        // ---- 事件：模式A ----
        public event Action OnModeAClicked;
        public event Action<int> OnLevelSelected;
        public event Action OnBackToMenu;
        public event Action OnBackToLevelSelect;
        public event Action OnRetryLevel;
        public event Action OnClearSave;

        // ---- 事件：模式B ----
        public event Action OnModeBClicked;
        public event Action OnBackToMenuFromB;
        public event Action OnBlockNewGame;

        // ---- 颜色 ----
        private readonly Color colorPrimary  = new Color(0.20f, 0.60f, 0.95f);
        private readonly Color colorSuccess  = new Color(0.30f, 0.78f, 0.30f);
        private readonly Color colorDanger   = new Color(0.90f, 0.30f, 0.30f);
        private readonly Color colorLocked   = new Color(0.55f, 0.55f, 0.55f);
        private readonly Color colorCleared  = new Color(1.00f, 0.75f, 0.20f);
        private readonly Color colorBg       = new Color(0.12f, 0.12f, 0.18f, 0.92f);
        private readonly Color colorOrange   = new Color(0.95f, 0.55f, 0.15f);

        private Canvas canvas;

        public void Build()
        {
            CreateCanvas();
            BuildMainMenu();
            BuildLevelSelect();
            BuildGameHUD();
            BuildGameOver();
            BuildBlockHUD();
            BuildBlockOver();
            ShowMainMenu();
        }

        // ==============================================================
        //  显示/隐藏
        // ==============================================================

        private void HideAll()
        {
            mainMenuPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
            gameHudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            blockHudPanel.SetActive(false);
            blockOverPanel.SetActive(false);
        }

        public void ShowMainMenu()        { HideAll(); mainMenuPanel.SetActive(true); }
        public void ShowLevelSelect()     { HideAll(); levelSelectPanel.SetActive(true); RefreshLevelButtons(); }
        public void ShowGameHUD(int level){ HideAll(); gameHudPanel.SetActive(true); levelNameText.text = "第 " + level + " 关"; shuffleText.text = ""; lastGainText.text = ""; }
        public void ShowBlockHUD()        { HideAll(); blockHudPanel.SetActive(true); }

        public void ShowGameOver(bool isWin, int score, int levelNumber)
        {
            gameOverPanel.SetActive(true);
            resultTitleText.text  = isWin ? "通关成功！" : "挑战失败";
            resultTitleText.color = isWin ? colorSuccess : colorDanger;
            resultScoreText.text  = "得分：" + score;
            int best = SaveManager.GetBestScore(levelNumber);
            resultBestText.text = best > 0 ? "最高分：" + best : "";
        }

        public void ShowBlockGameOver()
        {
            blockOverPanel.SetActive(true);
        }

        // ==============================================================
        //  模式A HUD 更新
        // ==============================================================

        public void UpdateScore(int s)            => scoreText.text  = "得分: " + s;
        public void UpdateSteps(int used, int max) => stepsText.text  = "步数: " + used + " / " + max;
        public void UpdateTarget(int t)           => targetText.text = "目标: " + t;
        public void ShowLastGain(int g)           => lastGainText.text = g > 0 ? "+" + g : "";
        public void ShowShuffleHint(bool b)       => shuffleText.text = b ? "无可消除，正在洗牌…" : "";

        public void ApplyHudStyle(float anchorX, float padLeft, float padTop,
                                  float lineHeight, int fontSize, Color color)
        {
            if (scoreText == null) return;
            Text[] texts = { levelNameText, scoreText, stepsText, targetText, shuffleText };
            for (int i = 0; i < texts.Length; i++)
            {
                var rt = texts[i].GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(anchorX, 1f);
                rt.anchorMax = new Vector2(anchorX, 1f);
                rt.pivot = new Vector2(anchorX, 1f);
                rt.anchoredPosition = new Vector2(padLeft, -padTop - lineHeight * i);
                texts[i].fontSize = fontSize;
            }
            scoreText.color = color; stepsText.color = color; targetText.color = color;
        }

        // ==============================================================
        //  模式B HUD 更新
        // ==============================================================

        public void UpdateBlockScore(int s)
        {
            if (blockScoreText != null) blockScoreText.text = "得分: " + s;
            if (blockOverScoreText != null) blockOverScoreText.text = "最终得分：" + s;
        }

        public void UpdateBlockHighScore(int s)
        {
            if (blockHighText != null) blockHighText.text = "最高分: " + s;
            if (blockOverHighText != null) blockOverHighText.text = "最高分：" + s;
        }

        // ==============================================================
        //  选关刷新
        // ==============================================================

        public void RefreshLevelButtons()
        {
            for (int i = 0; i < LevelDatabase.Count; i++)
            {
                int level = i + 1;
                int state = SaveManager.GetLevelState(level);
                levelButtons[i].interactable = state >= 1;
                if (state == 0)      { levelButtonImages[i].color = colorLocked; levelButtonTexts[i].text = "🔒"; }
                else if (state == 1) { levelButtonImages[i].color = colorPrimary; levelButtonTexts[i].text = level.ToString(); }
                else                 { levelButtonImages[i].color = colorCleared; levelButtonTexts[i].text = level + "\n★"; }
                levelButtonTexts[i].color = Color.white;
            }
        }

        // ==============================================================
        //  构建面板
        // ==============================================================

        private void CreateCanvas()
        {
            var go = new GameObject("UICanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void BuildMainMenu()
        {
            mainMenuPanel = CreatePanel("MainMenuPanel");
            CreateText(mainMenuPanel.transform, "消消乐", 72, TextAnchor.MiddleCenter,
                       new Vector2(0, 200), new Vector2(600, 100), Color.white, FontStyle.Bold);
            CreateButton(mainMenuPanel.transform, "闯关模式", 38,
                         new Vector2(0, 20), new Vector2(400, 90),
                         colorPrimary, () => OnModeAClicked?.Invoke());
            CreateButton(mainMenuPanel.transform, "方块模式", 38,
                         new Vector2(0, -100), new Vector2(400, 90),
                         colorOrange, () => OnModeBClicked?.Invoke());
            CreateButton(mainMenuPanel.transform, "清除存档", 24,
                         new Vector2(0, -320), new Vector2(220, 55),
                         new Color(0.4f, 0.4f, 0.4f), () => OnClearSave?.Invoke());
        }

        private void BuildLevelSelect()
        {
            levelSelectPanel = CreatePanel("LevelSelectPanel");
            CreateText(levelSelectPanel.transform, "选择关卡", 52, TextAnchor.MiddleCenter,
                       new Vector2(0, 380), new Vector2(500, 80), Color.white, FontStyle.Bold);
            CreateButton(levelSelectPanel.transform, "← 返回", 30,
                         new Vector2(-380, 380), new Vector2(160, 60),
                         new Color(0.4f, 0.4f, 0.5f), () => OnBackToMenu?.Invoke());

            int cols = 4; float btnSize = 120f; float gap = 20f;
            float totalW = cols * btnSize + (cols - 1) * gap;
            float startX = -totalW / 2f + btnSize / 2f;
            float startY = 240f;

            levelButtons = new Button[LevelDatabase.Count];
            levelButtonTexts = new Text[LevelDatabase.Count];
            levelButtonImages = new Image[LevelDatabase.Count];

            for (int i = 0; i < LevelDatabase.Count; i++)
            {
                int col = i % cols; int row = i / cols;
                float px = startX + col * (btnSize + gap);
                float py = startY - row * (btnSize + gap);
                int level = i + 1;
                var btn = CreateButton(levelSelectPanel.transform, level.ToString(), 32,
                                       new Vector2(px, py), new Vector2(btnSize, btnSize),
                                       colorPrimary, () => OnLevelSelected?.Invoke(level));
                levelButtons[i] = btn;
                levelButtonTexts[i] = btn.GetComponentInChildren<Text>();
                levelButtonImages[i] = btn.GetComponent<Image>();
            }
        }

        private void BuildGameHUD()
        {
            gameHudPanel = CreatePanel("GameHudPanel", false);
            float padL = 20f; float padT = -20f; float lineH = 50f;
            Color textBlue = new Color(0.2f, 0.6f, 1f);

            levelNameText = CreateTextAnchored(gameHudPanel.transform, "", 36, 0f, 1f,
                            new Vector2(padL, padT), new Vector2(500, lineH), colorCleared, FontStyle.Bold);
            scoreText = CreateTextAnchored(gameHudPanel.transform, "得分: 0", 34, 0f, 1f,
                        new Vector2(padL, padT - lineH), new Vector2(500, lineH), textBlue, FontStyle.Bold);
            stepsText = CreateTextAnchored(gameHudPanel.transform, "步数: 0 / 30", 34, 0f, 1f,
                        new Vector2(padL, padT - lineH * 2), new Vector2(500, lineH), textBlue, FontStyle.Bold);
            targetText = CreateTextAnchored(gameHudPanel.transform, "目标: 5000", 34, 0f, 1f,
                         new Vector2(padL, padT - lineH * 3), new Vector2(500, lineH), textBlue, FontStyle.Bold);
            shuffleText = CreateTextAnchored(gameHudPanel.transform, "", 30, 0f, 1f,
                          new Vector2(padL, padT - lineH * 4), new Vector2(500, lineH), colorCleared, FontStyle.Normal);
            lastGainText = CreateTextAnchored(gameHudPanel.transform, "", 38, 1f, 1f,
                           new Vector2(-20f, padT), new Vector2(250, lineH), colorSuccess, FontStyle.Bold);
            lastGainText.alignment = TextAnchor.MiddleRight;

            var exitBtn = CreateButton(gameHudPanel.transform, "退出", 26,
                          Vector2.zero, new Vector2(110, 50),
                          colorDanger, () => OnBackToLevelSelect?.Invoke());
            var exitRt = exitBtn.GetComponent<RectTransform>();
            exitRt.anchorMin = exitRt.anchorMax = exitRt.pivot = new Vector2(1f, 1f);
            exitRt.anchoredPosition = new Vector2(-20f, padT - lineH);
        }

        private void BuildGameOver()
        {
            gameOverPanel = CreatePanel("GameOverPanel");
            resultTitleText = CreateText(gameOverPanel.transform, "", 64, TextAnchor.MiddleCenter,
                               new Vector2(0, 150), new Vector2(600, 90), Color.white, FontStyle.Bold);
            resultScoreText = CreateText(gameOverPanel.transform, "", 40, TextAnchor.MiddleCenter,
                               new Vector2(0, 50), new Vector2(600, 60), Color.white);
            resultBestText = CreateText(gameOverPanel.transform, "", 32, TextAnchor.MiddleCenter,
                              new Vector2(0, -10), new Vector2(600, 50), colorCleared);
            CreateButton(gameOverPanel.transform, "重新挑战", 34,
                         new Vector2(-140, -120), new Vector2(240, 70),
                         colorPrimary, () => OnRetryLevel?.Invoke());
            CreateButton(gameOverPanel.transform, "返回选关", 34,
                         new Vector2(140, -120), new Vector2(240, 70),
                         colorSuccess, () => OnBackToLevelSelect?.Invoke());
        }

        // ---- 模式B 面板 ----

        private void BuildBlockHUD()
        {
            blockHudPanel = CreatePanel("BlockHudPanel", false);
            Color blue = new Color(0.2f, 0.6f, 1f);

            // 分数（居中偏上，大字号，醒目）
            blockScoreText = CreateTextAnchored(blockHudPanel.transform, "得分: 0", 48,
                             0.5f, 1f, new Vector2(0, -20), new Vector2(500, 60),
                             blue, FontStyle.Bold);
            blockScoreText.alignment = TextAnchor.MiddleCenter;

            // 最高分（分数下方，稍小）
            blockHighText = CreateTextAnchored(blockHudPanel.transform, "最高分: 0", 30,
                            0.5f, 1f, new Vector2(0, -70), new Vector2(500, 40),
                            colorCleared, FontStyle.Normal);
            blockHighText.alignment = TextAnchor.MiddleCenter;

            // 退出按钮（右上角）
            var exitBtn = CreateButton(blockHudPanel.transform, "退出", 26,
                          Vector2.zero, new Vector2(110, 50),
                          colorDanger, () => OnBackToMenuFromB?.Invoke());
            var rt = exitBtn.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
        }

        private void BuildBlockOver()
        {
            blockOverPanel = CreatePanel("BlockOverPanel");

            CreateText(blockOverPanel.transform, "游戏结束", 64, TextAnchor.MiddleCenter,
                       new Vector2(0, 150), new Vector2(600, 90), colorDanger, FontStyle.Bold);
            blockOverScoreText = CreateText(blockOverPanel.transform, "", 40, TextAnchor.MiddleCenter,
                                  new Vector2(0, 50), new Vector2(600, 60), Color.white);
            blockOverHighText = CreateText(blockOverPanel.transform, "", 32, TextAnchor.MiddleCenter,
                                 new Vector2(0, -10), new Vector2(600, 50), colorCleared);

            CreateButton(blockOverPanel.transform, "重新开始", 34,
                         new Vector2(-140, -120), new Vector2(240, 70),
                         colorPrimary, () => OnBlockNewGame?.Invoke());
            CreateButton(blockOverPanel.transform, "返回主菜单", 30,
                         new Vector2(140, -120), new Vector2(240, 70),
                         colorSuccess, () => OnBackToMenuFromB?.Invoke());
        }

        // ==============================================================
        //  UI 创建辅助
        // ==============================================================

        private GameObject CreatePanel(string name, bool withBackground = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            if (withBackground)
            {
                var img = go.AddComponent<Image>();
                img.color = colorBg;
                img.raycastTarget = true;
            }
            return go;
        }

        private Text CreateText(Transform parent, string content, int fontSize,
                                TextAnchor anchor, Vector2 pos, Vector2 size,
                                Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize; txt.fontStyle = style;
            txt.alignment = anchor; txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private Text CreateTextAnchored(Transform parent, string content, int fontSize,
                                        float anchorX, float anchorY,
                                        Vector2 pos, Vector2 size,
                                        Color color, FontStyle style = FontStyle.Normal)
        {
            var txt = CreateText(parent, content, fontSize, TextAnchor.MiddleLeft,
                                 Vector2.zero, size, color, style);
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorX, anchorY);
            rt.anchorMax = new Vector2(anchorX, anchorY);
            rt.pivot = new Vector2(anchorX, anchorY);
            rt.anchoredPosition = pos;
            return txt;
        }

        private Button CreateButton(Transform parent, string label, int fontSize,
                                    Vector2 pos, Vector2 size, Color bgColor, Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
            colors.disabledColor = colorLocked;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            CreateText(go.transform, label, fontSize, TextAnchor.MiddleCenter,
                       Vector2.zero, size, Color.white, FontStyle.Bold);
            return btn;
        }

        public void Destroy()
        {
            if (canvas != null) UnityEngine.Object.Destroy(canvas.gameObject);
        }
    }
}