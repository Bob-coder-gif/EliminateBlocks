using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

namespace Mine
{
    public class MineUI : MonoBehaviour
    {
        // ---- 面板 ----
        private GameObject difficultyPanel;
        private GameObject gameHudPanel;
        private GameObject gameOverPanel;
        private GameObject historyPanel;

        // ---- HUD 控件 ----
        private Text mineCountText, flagCountText, timerText, difficultyLabel, bestTimeText;

        // ---- 结算控件 ----
        private Text overTitleText, overTimeText, overInfoText, overBestText;

        // ---- 难度选择页面的最佳记录 ----
        private Text bestEasyText, bestMediumText, bestHardText;

        // ---- 历史记录 ----
        private Transform historyListParent;
        private Text pageInfoText;              // "第 1/3 页"
        private Button prevPageBtn, nextPageBtn;
        private int currentPage;
        private const int PER_PAGE = 8;

        // ---- 事件 ----
        public event Action<int> OnDifficultySelected;
        public event Action OnBackToMenu;
        public event Action OnNewGame;
        public event Action OnBackToDifficulty;

        private Canvas canvas;

        public void Build()
        {
            canvas = UIManager.Instance.SharedCanvas;
            BuildDifficultyPanel();
            BuildGameHUD();
            BuildGameOver();
            BuildHistoryPanel();
            HideAll();
        }

        // ==============================================================
        //  显示 / 隐藏
        // ==============================================================

        public void HideAll()
        {
            difficultyPanel.SetActive(false);
            gameHudPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            historyPanel.SetActive(false);
        }

        public void ShowDifficultySelect()
        {
            HideAll();
            difficultyPanel.SetActive(true);
            difficultyPanel.transform.SetAsLastSibling();
            RefreshBestTimes();
        }

        public void ShowGameHUD(int levelNumber)
        {
            HideAll();
            gameHudPanel.SetActive(true);
            gameHudPanel.transform.SetAsLastSibling();

            var cfg = LevelDatabase.Get(levelNumber);
            mineCountText.text = "雷 " + cfg.mineCount;
            flagCountText.text = "旗 " + cfg.mineCount;
            timerText.text = "0s";
            bestTimeText.text = "";

            string[] names = { "简单", "中等", "困难" };
            int idx = Mathf.Clamp(levelNumber - 1, 0, names.Length - 1);
            difficultyLabel.text = names[idx] + "  " + cfg.width + "×" + cfg.height;
        }

        public void ShowGameOver(bool isWin, int seconds, int levelNumber, int bestTime)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();

            overTitleText.text  = isWin ? "排雷成功！" : "踩到雷了！";
            overTitleText.color = isWin ? UIHelper.ColorSuccess : UIHelper.ColorDanger;
            overTimeText.text   = "用时：" + seconds + " 秒";

            var cfg = LevelDatabase.Get(levelNumber);
            overInfoText.text = cfg.width + "×" + cfg.height + "  " + cfg.mineCount + "颗雷";
            overBestText.text = bestTime >= 0 ? "最佳：" + bestTime + "秒" : "";
        }

        private void ShowHistory()
        {
            HideAll();
            historyPanel.SetActive(true);
            historyPanel.transform.SetAsLastSibling();
            currentPage = 0;
            RefreshHistoryList();
        }

        // ==============================================================
        //  HUD 更新
        // ==============================================================

        public void UpdateMineCount(int n)  { if (mineCountText != null) mineCountText.text = "雷 " + n; }
        public void UpdateFlagCount(int n)  { if (flagCountText != null) flagCountText.text = "旗 " + n; }
        public void UpdateTimer(int s)      { if (timerText != null) timerText.text = "" + s + "s"; }
        public void UpdateBestTime(int s)   { if (bestTimeText != null) bestTimeText.text = "最佳 " + s + "s"; }

        // ==============================================================
        //  刷新最佳记录
        // ==============================================================

        private void RefreshBestTimes()
        {
            bestEasyText.text   = FormatBest(MineSaveData.GetBestTime(1));
            bestMediumText.text = FormatBest(MineSaveData.GetBestTime(2));
            bestHardText.text   = FormatBest(MineSaveData.GetBestTime(3));
        }

        private string FormatBest(int seconds)
        {
            return seconds >= 0 ? "最佳 " + seconds + "s" : "暂无记录";
        }

        // ==============================================================
        //  构建面板
        // ==============================================================

        private void BuildDifficultyPanel()
        {
            difficultyPanel = UIHelper.CreatePanel(canvas.transform, "MineDifficultyPanel");

            UIHelper.CreateText(difficultyPanel.transform, "扫雷模式", 60, TextAnchor.MiddleCenter,
                                new Vector2(0, 350), new Vector2(600, 90),
                                Color.white, FontStyle.Bold);

            // ---- 三个难度的最佳记录 ----
            UIHelper.CreateText(difficultyPanel.transform, "-- 最佳记录 --", 28, TextAnchor.MiddleCenter,
                                new Vector2(0, 260), new Vector2(500, 40),
                                new Color(1f, 1f, 1f, 0.5f));

            bestEasyText = UIHelper.CreateText(difficultyPanel.transform, "", 26, TextAnchor.MiddleCenter,
                           new Vector2(-200, 220), new Vector2(200, 35),
                           UIHelper.ColorSuccess);
            bestMediumText = UIHelper.CreateText(difficultyPanel.transform, "", 26, TextAnchor.MiddleCenter,
                             new Vector2(0, 220), new Vector2(200, 35),
                             UIHelper.ColorPrimary);
            bestHardText = UIHelper.CreateText(difficultyPanel.transform, "", 26, TextAnchor.MiddleCenter,
                           new Vector2(200, 220), new Vector2(200, 35),
                           UIHelper.ColorDanger);

            // 难度标签
            UIHelper.CreateText(difficultyPanel.transform, "简单", 22, TextAnchor.MiddleCenter,
                                new Vector2(-200, 195), new Vector2(100, 25),
                                new Color(1f, 1f, 1f, 0.4f));
            UIHelper.CreateText(difficultyPanel.transform, "中等", 22, TextAnchor.MiddleCenter,
                                new Vector2(0, 195), new Vector2(100, 25),
                                new Color(1f, 1f, 1f, 0.4f));
            UIHelper.CreateText(difficultyPanel.transform, "困难", 22, TextAnchor.MiddleCenter,
                                new Vector2(200, 195), new Vector2(100, 25),
                                new Color(1f, 1f, 1f, 0.4f));

            // ---- 难度选择按钮 ----
            UIHelper.CreateText(difficultyPanel.transform, "选择难度", 36, TextAnchor.MiddleCenter,
                                new Vector2(0, 145), new Vector2(400, 50),
                                UIHelper.ColorCleared, FontStyle.Normal);

            UIHelper.CreateButton(difficultyPanel.transform, "简单\n9×9 · 10雷", 30,
                                  new Vector2(0, 40), new Vector2(420, 90),
                                  UIHelper.ColorSuccess,
                                  () => OnDifficultySelected?.Invoke(1));
            UIHelper.CreateButton(difficultyPanel.transform, "中等\n9×16 · 22雷", 30,
                                  new Vector2(0, -65), new Vector2(420, 90),
                                  UIHelper.ColorPrimary,
                                  () => OnDifficultySelected?.Invoke(2));
            UIHelper.CreateButton(difficultyPanel.transform, "困难\n9×16 · 29雷", 30,
                                  new Vector2(0, -170), new Vector2(420, 90),
                                  UIHelper.ColorDanger,
                                  () => OnDifficultySelected?.Invoke(3));

            // ---- 底部按钮 ----
            UIHelper.CreateButton(difficultyPanel.transform, "历史记录", 28,
                                  new Vector2(0, -300), new Vector2(300, 70),
                                  new Color(0.35f, 0.35f, 0.50f),
                                  ShowHistory);

            UIHelper.CreateButton(difficultyPanel.transform, "← 返回", 30,
                                  new Vector2(-380, 380), new Vector2(160, 60),
                                  UIHelper.ColorGray,
                                  () => OnBackToMenu?.Invoke());
        }

        private void BuildGameHUD()
        {
            gameHudPanel = UIHelper.CreatePanel(canvas.transform, "MineGameHudPanel", false);
            float padT = -30f;

            difficultyLabel = UIHelper.CreateTextAnchored(gameHudPanel.transform, "", 32,
                              0.5f, 1f, new Vector2(0, padT), new Vector2(400, 40),
                              UIHelper.ColorCleared, FontStyle.Bold);
            difficultyLabel.alignment = TextAnchor.MiddleCenter;

            mineCountText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "雷 0", 32,
                            0f, 1f, new Vector2(20f, padT - 50f), new Vector2(200, 35),
                            Color.white, FontStyle.Bold);
            flagCountText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "旗 0", 32,
                            0f, 1f, new Vector2(20f, padT - 90f), new Vector2(200, 35),
                            Color.white, FontStyle.Bold);

            timerText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "0s", 32,
                        1f, 1f, new Vector2(-20f, padT - 50f), new Vector2(200, 35),
                        Color.white, FontStyle.Bold);
            timerText.alignment = TextAnchor.MiddleRight;

            bestTimeText = UIHelper.CreateTextAnchored(gameHudPanel.transform, "", 28,
                           1f, 1f, new Vector2(-20f, padT - 90f), new Vector2(200, 35),
                           UIHelper.ColorCleared, FontStyle.Normal);
            bestTimeText.alignment = TextAnchor.MiddleRight;

            UIHelper.CreateTextAnchored(gameHudPanel.transform, "短按翻开  长按插旗", 22,
                        0.5f, 0f, new Vector2(0, 60f), new Vector2(400, 30),
                        new Color(1f, 1f, 1f, 0.5f), FontStyle.Normal)
                        .alignment = TextAnchor.MiddleCenter;

            var exitBtn = UIHelper.CreateButton(gameHudPanel.transform, "退出", 26,
                          Vector2.zero, new Vector2(110, 50),
                          UIHelper.ColorDanger,
                          () => OnBackToMenu?.Invoke());
            var exitRt = exitBtn.GetComponent<RectTransform>();
            exitRt.anchorMin = exitRt.anchorMax = exitRt.pivot = new Vector2(1f, 0f);
            exitRt.anchoredPosition = new Vector2(-20f, 10f);
        }

        private void BuildGameOver()
        {
            gameOverPanel = UIHelper.CreatePanel(canvas.transform, "MineGameOverPanel");

            overTitleText = UIHelper.CreateText(gameOverPanel.transform, "", 64, TextAnchor.MiddleCenter,
                             new Vector2(0, 180), new Vector2(600, 90), Color.white, FontStyle.Bold);
            overTimeText = UIHelper.CreateText(gameOverPanel.transform, "", 40, TextAnchor.MiddleCenter,
                            new Vector2(0, 90), new Vector2(600, 60), Color.white);
            overInfoText = UIHelper.CreateText(gameOverPanel.transform, "", 30, TextAnchor.MiddleCenter,
                            new Vector2(0, 30), new Vector2(600, 50), UIHelper.ColorCleared);
            overBestText = UIHelper.CreateText(gameOverPanel.transform, "", 34, TextAnchor.MiddleCenter,
                            new Vector2(0, -30), new Vector2(600, 50),
                            UIHelper.ColorCleared, FontStyle.Bold);

            UIHelper.CreateButton(gameOverPanel.transform, "再来一局", 34,
                                  new Vector2(-140, -140), new Vector2(240, 70),
                                  UIHelper.ColorPrimary, () => OnNewGame?.Invoke());
            UIHelper.CreateButton(gameOverPanel.transform, "换难度", 34,
                                  new Vector2(140, -140), new Vector2(240, 70),
                                  UIHelper.ColorSuccess, () => OnBackToDifficulty?.Invoke());
        }

        // ==============================================================
        //  历史记录面板（分页版）
        // ==============================================================

        private void BuildHistoryPanel()
        {
            historyPanel = UIHelper.CreatePanel(canvas.transform, "MineHistoryPanel");

            UIHelper.CreateText(historyPanel.transform, "历史记录", 52, TextAnchor.MiddleCenter,
                                new Vector2(0, 380), new Vector2(500, 80),
                                Color.white, FontStyle.Bold);

            UIHelper.CreateButton(historyPanel.transform, "← 返回", 30,
                                  new Vector2(-380, 380), new Vector2(160, 60),
                                  UIHelper.ColorGray, ShowDifficultySelect);

            UIHelper.CreateButton(historyPanel.transform, "清除", 24,
                                  new Vector2(380, 380), new Vector2(120, 60),
                                  UIHelper.ColorDanger,
                                  () => { MineSaveData.ClearAll(); currentPage = 0; RefreshHistoryList(); });

            // 表头
            UIHelper.CreateText(historyPanel.transform,
                                "难度        用时        结果        日期", 24,
                                TextAnchor.MiddleCenter,
                                new Vector2(0, 310), new Vector2(900, 35),
                                new Color(1f, 1f, 1f, 0.5f));

            // 分页按钮
            prevPageBtn = UIHelper.CreateButton(historyPanel.transform, "上一页", 26,
                          new Vector2(-150, -380), new Vector2(200, 60),
                          UIHelper.ColorPrimary,
                          () => { currentPage--; RefreshHistoryList(); });

            nextPageBtn = UIHelper.CreateButton(historyPanel.transform, "下一页", 26,
                          new Vector2(150, -380), new Vector2(200, 60),
                          UIHelper.ColorPrimary,
                          () => { currentPage++; RefreshHistoryList(); });

            pageInfoText = UIHelper.CreateText(historyPanel.transform, "", 24,
                           TextAnchor.MiddleCenter,
                           new Vector2(0, -380), new Vector2(100, 60),
                           Color.white);

            // 记录条目容器
            var listGo = new GameObject("HistoryList", typeof(RectTransform));
            listGo.transform.SetParent(historyPanel.transform, false);
            var listRt = listGo.GetComponent<RectTransform>();
            listRt.anchorMin = Vector2.zero;
            listRt.anchorMax = Vector2.one;
            listRt.offsetMin = Vector2.zero;
            listRt.offsetMax = Vector2.zero;
            historyListParent = listGo.transform;
        }

        private void RefreshHistoryList()
        {
            // 清空旧条目
            for (int i = historyListParent.childCount - 1; i >= 0; i--)
                Destroy(historyListParent.GetChild(i).gameObject);

            List<MineRecord> records = MineSaveData.GetAllRecords();
            int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)records.Count / PER_PAGE));

            // 边界检查
            currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

            // 分页按钮状态
            prevPageBtn.interactable = currentPage > 0;
            nextPageBtn.interactable = currentPage < totalPages - 1;
            pageInfoText.text = records.Count > 0
                ? (currentPage + 1) + "/" + totalPages
                : "";

            // 隐藏分页按钮（如果记录太少）
            bool showPaging = records.Count > PER_PAGE;
            prevPageBtn.gameObject.SetActive(showPaging);
            nextPageBtn.gameObject.SetActive(showPaging);
            pageInfoText.gameObject.SetActive(showPaging);

            if (records.Count == 0)
            {
                UIHelper.CreateText(historyListParent, "暂无记录，去玩一局吧！", 30,
                                    TextAnchor.MiddleCenter,
                                    new Vector2(0, 100), new Vector2(600, 50),
                                    new Color(1f, 1f, 1f, 0.5f));
                return;
            }

            // 当前页的数据
            string[] diffNames = { "", "简单", "中等", "困难" };
            float rowH = 55f;
            float startY = 265f;
            int startIdx = currentPage * PER_PAGE;
            int endIdx = Mathf.Min(startIdx + PER_PAGE, records.Count);

            for (int i = startIdx; i < endIdx; i++)
            {
                var r = records[i];
                int row = i - startIdx;
                float py = startY - row * rowH;

                string diff = (r.difficulty >= 1 && r.difficulty <= 3) ? diffNames[r.difficulty] : "?";
                string result = r.isWin ? "胜利" : "失败";
                string line = $"{diff}        {r.seconds}s        {result}        {r.date}";

                Color color = r.isWin
                    ? new Color(0.4f, 0.9f, 0.4f)
                    : new Color(0.9f, 0.5f, 0.5f);

                UIHelper.CreateText(historyListParent, line, 26,
                                    TextAnchor.MiddleCenter,
                                    new Vector2(0, py), new Vector2(900, rowH),
                                    color);
            }
        }
    }
}