using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

namespace Match3
{
    public class MatchUI : MonoBehaviour
    {
        // ---- 面板 ----
        private GameObject levelSelectPanel;
        private GameObject gameHudPanel;
        private GameObject gameOverPanel;
        private GameObject blockHudPanel;
        private GameObject blockOverPanel;
        private GameObject blockHistoryPanel;       // ★ 新增

        // ---- 模式A HUD 控件 ----
        private Text scoreText, stepsText, targetText, lastGainText, shuffleText, levelNameText;

        // ---- 模式A 结算控件 ----
        private Text resultTitleText, resultScoreText, resultBestText;

        // ---- 模式B HUD 控件 ----
        private Text blockScoreText, blockHighText;

        // ---- 模式B 结算控件 ----
        private Text blockOverScoreText, blockOverHighText;

        // ---- 模式B 历史记录 ----
        private Transform blockHistoryListParent;
        private Text blockPageInfoText;
        private Button blockPrevBtn, blockNextBtn;
        private int blockCurrentPage;
        private const int PER_PAGE = 8;

        // ---- 选关按钮 ----
        private Button[] levelButtons;
        private Text[]   levelButtonTexts;
        private Image[]  levelButtonImages;

        // ---- 事件：模式A ----
        public event Action<int> OnLevelSelected;
        public event Action OnBackToMenu;
        public event Action OnBackToLevelSelect;
        public event Action OnRetryLevel;

        // ---- 事件：模式B ----
        public event Action OnBackToMenuFromB;
        public event Action OnBlockNewGame;
        private Button undoButton;
        public event Action OnBlockUndo;

        private Canvas canvas;

        public void Build()
        {
            canvas = UIManager.Instance.SharedCanvas;

            BuildLevelSelect();
            BuildGameHUD();
            BuildGameOver();
            BuildBlockHUD();
            BuildBlockOver();
            BuildBlockHistory();

            HideAll();
        }

        // ==============================================================
        //  显示 / 隐藏
        // ==============================================================

        public void HideAll()
        {
            levelSelectPanel.SetActive(false);
            gameHudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            blockHudPanel.SetActive(false);
            blockOverPanel.SetActive(false);
            blockHistoryPanel.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            HideAll();
            levelSelectPanel.SetActive(true);
            levelSelectPanel.transform.SetAsLastSibling();
            RefreshLevelButtons();
        }

        public void ShowGameHUD(int level)
        {
            HideAll();
            gameHudPanel.SetActive(true);
            gameHudPanel.transform.SetAsLastSibling();
            levelNameText.text = "第 " + level + " 关";
            shuffleText.text = "";
            lastGainText.text = "";
        }

        public void ShowBlockHUD()
        {
            HideAll();
            blockHudPanel.SetActive(true);
            blockHudPanel.transform.SetAsLastSibling();
        }

        public void ShowGameOver(bool isWin, int score, int levelNumber)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
            resultTitleText.text  = isWin ? "通关成功！" : "挑战失败";
            resultTitleText.color = isWin ? UIHelper.ColorSuccess : UIHelper.ColorDanger;
            resultScoreText.text  = "得分：" + score;
            int best = SaveManager.GetBestScore(levelNumber);
            resultBestText.text = best > 0 ? "最高分：" + best : "";
        }

        public void ShowBlockGameOver()
        {
            blockOverPanel.SetActive(true);
            blockOverPanel.transform.SetAsLastSibling();

            // 刷新最高分显示
            int best = BlockRecordData.GetHighScore();
            if (blockOverHighText != null && best > 0)
                blockOverHighText.text = "历史最高：" + best;
        }

        private void ShowBlockHistory()
        {
            HideAll();
            blockHistoryPanel.SetActive(true);
            blockHistoryPanel.transform.SetAsLastSibling();
            blockCurrentPage = 0;
            RefreshBlockHistoryList();
        }

        // ==============================================================
        //  模式A HUD 更新
        // ==============================================================

        public void UpdateScore(int s)             => scoreText.text  = "得分: " + s;
        public void UpdateSteps(int used, int max) => stepsText.text  = "步数: " + used + " / " + max;
        public void UpdateTarget(int t)            => targetText.text = "目标: " + t;
        public void ShowLastGain(int g)            => lastGainText.text = g > 0 ? "+" + g : "";
        public void ShowShuffleHint(bool b)        => shuffleText.text = b ? "无可消除，正在洗牌…" : "";

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
                rt.pivot     = new Vector2(anchorX, 1f);
                rt.anchoredPosition = new Vector2(padLeft, -padTop - lineHeight * i);
                texts[i].fontSize = fontSize;
            }
            scoreText.color = color;
            stepsText.color = color;
            targetText.color = color;
        }

        // ==============================================================
        //  模式B HUD 更新
        // ==============================================================

        public void UpdateBlockScore(int s)
        {
            if (blockScoreText != null)     blockScoreText.text     = "得分: " + s;
            if (blockOverScoreText != null)  blockOverScoreText.text  = "最终得分：" + s;
        }

        public void UpdateBlockHighScore(int s)
        {
            if (blockHighText != null)      blockHighText.text      = "最高分: " + s;
        }

        public void SetUndoInteractable(bool v)
        {
            if (undoButton != null) undoButton.interactable = v;
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

                if (state == 0)
                {
                    levelButtonImages[i].color = UIHelper.ColorLocked;
                    levelButtonTexts[i].text = "locked";
                }
                else if (state == 1)
                {
                    levelButtonImages[i].color = UIHelper.ColorPrimary;
                    levelButtonTexts[i].text = level.ToString();
                }
                else
                {
                    levelButtonImages[i].color = UIHelper.ColorCleared;
                    levelButtonTexts[i].text = level + "\n*";
                }
                levelButtonTexts[i].color = Color.white;
            }
        }

        // ==============================================================
        //  构建面板
        // ==============================================================

        private void BuildLevelSelect()
        {
            levelSelectPanel = UIHelper.CreatePanel(canvas.transform, "LevelSelectPanel");

            UIHelper.CreateText(levelSelectPanel.transform, "选择关卡", 52, TextAnchor.MiddleCenter,
                                new Vector2(0, 380), new Vector2(500, 80),
                                Color.white, FontStyle.Bold);

            UIHelper.CreateButton(levelSelectPanel.transform, "< 返回", 30,
                                  new Vector2(-380, 380), new Vector2(160, 60),
                                  UIHelper.ColorGray,
                                  () => OnBackToMenu?.Invoke());

            int cols = 4;
            float btnSize = 120f;
            float gap = 20f;
            float totalW = cols * btnSize + (cols - 1) * gap;
            float startX = -totalW / 2f + btnSize / 2f;
            float startY = 240f;

            levelButtons      = new Button[LevelDatabase.Count];
            levelButtonTexts  = new Text[LevelDatabase.Count];
            levelButtonImages = new Image[LevelDatabase.Count];

            for (int i = 0; i < LevelDatabase.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float px = startX + col * (btnSize + gap);
                float py = startY - row * (btnSize + gap);
                int level = i + 1;

                var btn = UIHelper.CreateButton(levelSelectPanel.transform, level.ToString(), 32,
                                                new Vector2(px, py), new Vector2(btnSize, btnSize),
                                                UIHelper.ColorPrimary,
                                                () => OnLevelSelected?.Invoke(level));
                levelButtons[i]      = btn;
                levelButtonTexts[i]  = btn.GetComponentInChildren<Text>();
                levelButtonImages[i] = btn.GetComponent<Image>();
            }
        }

        private void BuildGameHUD()
        {
            gameHudPanel = UIHelper.CreatePanel(canvas.transform, "GameHudPanel", false);

            float padL = 20f;
            float padT = -70f;
            float lineH = 50f;
            Color textBlue = new Color(0.2f, 0.6f, 1f);

            levelNameText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "", 36, 0f, 1f,
                            new Vector2(padL, padT), new Vector2(500, lineH),
                            UIHelper.ColorCleared, FontStyle.Bold);
            scoreText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "得分: 0", 34, 0f, 1f,
                        new Vector2(padL, padT - lineH), new Vector2(500, lineH),
                        textBlue, FontStyle.Bold);
            stepsText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "步数: 0 / 30", 34, 0f, 1f,
                        new Vector2(padL, padT - lineH * 2), new Vector2(500, lineH),
                        textBlue, FontStyle.Bold);
            targetText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "目标: 5000", 34, 0f, 1f,
                         new Vector2(padL, padT - lineH * 3), new Vector2(500, lineH),
                         textBlue, FontStyle.Bold);
            shuffleText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "", 30, 0f, 1f,
                          new Vector2(padL, padT - lineH * 4), new Vector2(500, lineH),
                          UIHelper.ColorCleared, FontStyle.Normal);
            lastGainText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "", 38, 1f, 1f,
                           new Vector2(-20f, padT), new Vector2(250, lineH),
                           UIHelper.ColorSuccess, FontStyle.Bold);
            lastGainText.alignment = TextAnchor.MiddleRight;

            var exitBtn = UIHelper.CreateButton(gameHudPanel.transform, "退出", 26,
                          Vector2.zero, new Vector2(110, 50),
                          UIHelper.ColorDanger,
                          () => OnBackToLevelSelect?.Invoke());
            var exitRt = exitBtn.GetComponent<RectTransform>();
            exitRt.anchorMin = exitRt.anchorMax = exitRt.pivot = new Vector2(1f, 1f);
            exitRt.anchoredPosition = new Vector2(-20f, padT - lineH);
        }

        private void BuildGameOver()
        {
            gameOverPanel = UIHelper.CreatePanel(canvas.transform, "GameOverPanel");

            resultTitleText = UIHelper.CreateText(gameOverPanel.transform, "", 64, TextAnchor.MiddleCenter,
                               new Vector2(0, 150), new Vector2(600, 90),
                               Color.white, FontStyle.Bold);
            resultScoreText = UIHelper.CreateText(gameOverPanel.transform, "", 40, TextAnchor.MiddleCenter,
                               new Vector2(0, 50), new Vector2(600, 60), Color.white);
            resultBestText = UIHelper.CreateText(gameOverPanel.transform, "", 32, TextAnchor.MiddleCenter,
                              new Vector2(0, -10), new Vector2(600, 50), UIHelper.ColorCleared);

            UIHelper.CreateButton(gameOverPanel.transform, "重新挑战", 34,
                                  new Vector2(-140, -120), new Vector2(240, 70),
                                  UIHelper.ColorPrimary,
                                  () => OnRetryLevel?.Invoke());
            UIHelper.CreateButton(gameOverPanel.transform, "返回选关", 34,
                                  new Vector2(140, -120), new Vector2(240, 70),
                                  UIHelper.ColorSuccess,
                                  () => OnBackToLevelSelect?.Invoke());
        }

        // ---- 模式B 面板 ----

        private void BuildBlockHUD()
        {
            blockHudPanel = UIHelper.CreatePanel(canvas.transform, "BlockHudPanel", false);
            Color blue = new Color(0.2f, 0.6f, 1f);

            blockScoreText = UIHelper.CreateTextAnchored(blockHudPanel.transform, "得分: 0", 54,
                             0.5f, 1f, new Vector2(0, -70), new Vector2(500, 60),
                             blue, FontStyle.Bold);
            blockScoreText.alignment = TextAnchor.MiddleCenter;

            blockHighText = UIHelper.CreateTextAnchored(blockHudPanel.transform, "最高分: 0", 42,
                            0.5f, 1f, new Vector2(0, -130), new Vector2(500, 40),
                            UIHelper.ColorDanger, FontStyle.Normal);
            blockHighText.alignment = TextAnchor.MiddleCenter;

            var exitBtn = UIHelper.CreateButton(blockHudPanel.transform, "退出", 26,
                          Vector2.zero, new Vector2(110, 50),
                          UIHelper.ColorDanger,
                          () => OnBackToMenuFromB?.Invoke());
            var rt = exitBtn.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);

            undoButton = UIHelper.CreateButton(blockHudPanel.transform, "撤回", 26,
                         Vector2.zero, new Vector2(110, 50),
                         UIHelper.ColorOrange,
                         () => OnBlockUndo?.Invoke());
            var undoRt = undoButton.GetComponent<RectTransform>();
            undoRt.anchorMin = undoRt.anchorMax = undoRt.pivot = new Vector2(0f, 1f);
            undoRt.anchoredPosition = new Vector2(20f, -20f);
            undoButton.interactable = false;
        }

        private void BuildBlockOver()
        {
            blockOverPanel = UIHelper.CreatePanel(canvas.transform, "BlockOverPanel");

            UIHelper.CreateText(blockOverPanel.transform, "游戏结束", 64, TextAnchor.MiddleCenter,
                                new Vector2(0, 200), new Vector2(600, 90),
                                UIHelper.ColorDanger, FontStyle.Bold);
            blockOverScoreText = UIHelper.CreateText(blockOverPanel.transform, "", 40, TextAnchor.MiddleCenter,
                                  new Vector2(0, 100), new Vector2(600, 60), Color.white);
            blockOverHighText = UIHelper.CreateText(blockOverPanel.transform, "", 32, TextAnchor.MiddleCenter,
                                 new Vector2(0, 40), new Vector2(600, 50), UIHelper.ColorCleared);

            UIHelper.CreateButton(blockOverPanel.transform, "重新开始", 34,
                                  new Vector2(-140, -60), new Vector2(240, 70),
                                  UIHelper.ColorPrimary,
                                  () => OnBlockNewGame?.Invoke());
            UIHelper.CreateButton(blockOverPanel.transform, "返回主菜单", 30,
                                  new Vector2(140, -60), new Vector2(240, 70),
                                  UIHelper.ColorSuccess,
                                  () => OnBackToMenuFromB?.Invoke());

            // ★ 历史记录按钮
            UIHelper.CreateButton(blockOverPanel.transform, "历史记录", 28,
                                  new Vector2(0, -170), new Vector2(260, 65),
                                  new Color(0.35f, 0.35f, 0.50f),
                                  ShowBlockHistory);
        }

        // ==============================================================
        //  ★ 方块模式历史记录面板
        // ==============================================================

        private void BuildBlockHistory()
        {
            blockHistoryPanel = UIHelper.CreatePanel(canvas.transform, "BlockHistoryPanel");

            UIHelper.CreateText(blockHistoryPanel.transform, "方块模式 - 历史记录", 46, TextAnchor.MiddleCenter,
                                new Vector2(0, 380), new Vector2(700, 80),
                                Color.white, FontStyle.Bold);

            // 最高分
            int best = BlockRecordData.GetHighScore();
            UIHelper.CreateText(blockHistoryPanel.transform,
                                best > 0 ? "历史最高: " + best : "暂无最高分",
                                32, TextAnchor.MiddleCenter,
                                new Vector2(0, 310), new Vector2(500, 45),
                                UIHelper.ColorCleared, FontStyle.Bold);

            UIHelper.CreateButton(blockHistoryPanel.transform, "< 返回", 30,
                                  new Vector2(-380, 380), new Vector2(160, 60),
                                  UIHelper.ColorGray,
                                  () => { HideAll(); blockOverPanel.SetActive(true); blockOverPanel.transform.SetAsLastSibling(); });

            UIHelper.CreateButton(blockHistoryPanel.transform, "清除", 24,
                                  new Vector2(380, 380), new Vector2(120, 60),
                                  UIHelper.ColorDanger,
                                  () => { BlockRecordData.ClearAll(); blockCurrentPage = 0; RefreshBlockHistoryList(); });

            // 表头
            UIHelper.CreateText(blockHistoryPanel.transform,
                                "得分                    日期", 24,
                                TextAnchor.MiddleCenter,
                                new Vector2(0, 270), new Vector2(900, 35),
                                new Color(1f, 1f, 1f, 0.5f));

            // 分页按钮
            blockPrevBtn = UIHelper.CreateButton(blockHistoryPanel.transform, "上一页", 26,
                           new Vector2(-150, -380), new Vector2(200, 60),
                           UIHelper.ColorPrimary,
                           () => { blockCurrentPage--; RefreshBlockHistoryList(); });
            blockNextBtn = UIHelper.CreateButton(blockHistoryPanel.transform, "下一页", 26,
                           new Vector2(150, -380), new Vector2(200, 60),
                           UIHelper.ColorPrimary,
                           () => { blockCurrentPage++; RefreshBlockHistoryList(); });
            blockPageInfoText = UIHelper.CreateText(blockHistoryPanel.transform, "", 24,
                                TextAnchor.MiddleCenter,
                                new Vector2(0, -380), new Vector2(100, 60),
                                Color.white);

            // 列表容器
            var listGo = new GameObject("BlockHistoryList", typeof(RectTransform));
            listGo.transform.SetParent(blockHistoryPanel.transform, false);
            var listRt = listGo.GetComponent<RectTransform>();
            listRt.anchorMin = Vector2.zero;
            listRt.anchorMax = Vector2.one;
            listRt.offsetMin = Vector2.zero;
            listRt.offsetMax = Vector2.zero;
            blockHistoryListParent = listGo.transform;
        }

        private void RefreshBlockHistoryList()
        {
            for (int i = blockHistoryListParent.childCount - 1; i >= 0; i--)
                Destroy(blockHistoryListParent.GetChild(i).gameObject);

            List<BlockRecord> records = BlockRecordData.GetAllRecords();
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)records.Count / PER_PAGE));
            blockCurrentPage = Mathf.Clamp(blockCurrentPage, 0, totalPages - 1);

            blockPrevBtn.interactable = blockCurrentPage > 0;
            blockNextBtn.interactable = blockCurrentPage < totalPages - 1;

            bool showPaging = records.Count > PER_PAGE;
            blockPrevBtn.gameObject.SetActive(showPaging);
            blockNextBtn.gameObject.SetActive(showPaging);
            blockPageInfoText.gameObject.SetActive(showPaging);
            blockPageInfoText.text = (blockCurrentPage + 1) + "/" + totalPages;

            if (records.Count == 0)
            {
                UIHelper.CreateText(blockHistoryListParent, "暂无记录，去玩一局吧！", 30,
                                    TextAnchor.MiddleCenter,
                                    new Vector2(0, 100), new Vector2(600, 50),
                                    new Color(1f, 1f, 1f, 0.5f));
                return;
            }

            float rowH = 55f;
            float startY = 225f;
            int startIdx = blockCurrentPage * PER_PAGE;
            int endIdx = Mathf.Min(startIdx + PER_PAGE, records.Count);

            for (int i = startIdx; i < endIdx; i++)
            {
                var r = records[i];
                int row = i - startIdx;
                float py = startY - row * rowH;

                string line = r.score + " 分                    " + r.date;

                UIHelper.CreateText(blockHistoryListParent, line, 28,
                                    TextAnchor.MiddleCenter,
                                    new Vector2(0, py), new Vector2(900, rowH),
                                    new Color(0.4f, 0.85f, 1f));
            }
        }
    }
}