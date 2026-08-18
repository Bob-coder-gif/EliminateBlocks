using System;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    /// <summary>
    /// 全局 UI 管理器（单例）。
    /// 
    /// 菜单结构：
    ///   主菜单 ─┬─ 消消乐 → 子菜单 ─┬─ 闯关模式
    ///           │                     └─ 方块模式
    ///           └─ 扫雷   → MineUI 难度选择
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        /// <summary>共享 Canvas，所有 UI 面板都挂在这下面。</summary>
        public Canvas SharedCanvas { get; private set; }

        private GameObject mainMenuPanel;
        private GameObject matchSubMenuPanel;   // 消消乐子菜单

        // ---- 事件 ----
        public event Action OnMatchLevelClicked;  // 闯关模式
        public event Action OnBlockModeClicked;   // 方块模式
        public event Action OnMineModeClicked;    // 扫雷模式
        public event Action OnClearSaveClicked;   // 清除存档

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Build()
        {
            CreateCanvas();
            BuildMainMenu();
            BuildMatchSubMenu();
            ShowMainMenu();
        }

        // ==============================================================
        //  显示 / 隐藏
        // ==============================================================

        private void HideAllOwn()
        {
            mainMenuPanel.SetActive(false);
            matchSubMenuPanel.SetActive(false);
        }

        /// <summary>显示主菜单（从任何地方返回时调用）。</summary>
        public void ShowMainMenu()
        {
            HideAllOwn();
            mainMenuPanel.SetActive(true);
            // 移到 Canvas 最后一个子节点 → 渲染在最上层
            mainMenuPanel.transform.SetAsLastSibling();
        }

        private void ShowMatchSubMenu()
        {
            HideAllOwn();
            matchSubMenuPanel.SetActive(true);
            matchSubMenuPanel.transform.SetAsLastSibling();
        }

        /// <summary>仅隐藏 UIManager 自己的面板（进入游戏模式时调用）。</summary>
        public void HideMainMenu()
        {
            HideAllOwn();
        }

        // ==============================================================
        //  构建
        // ==============================================================

        private void CreateCanvas()
        {
            var go = new GameObject("UICanvas");
            go.transform.SetParent(transform);
            SharedCanvas = go.AddComponent<Canvas>();
            SharedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            SharedCanvas.sortingOrder = 100;

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
            mainMenuPanel = UIHelper.CreatePanel(SharedCanvas.transform, "MainMenuPanel");

            UIHelper.CreateText(mainMenuPanel.transform, "消消乐", 72, TextAnchor.MiddleCenter,
                                new Vector2(0, 280), new Vector2(600, 100),
                                Color.white, FontStyle.Bold);

            // ── 消消乐（进入子菜单）──
            UIHelper.CreateButton(mainMenuPanel.transform, "消消乐", 38,
                                  new Vector2(0, 60), new Vector2(400, 90),
                                  UIHelper.ColorPrimary,
                                  ShowMatchSubMenu);

            // ── 扫雷 ──
            UIHelper.CreateButton(mainMenuPanel.transform, "扫雷", 38,
                                  new Vector2(0, -60), new Vector2(400, 90),
                                  UIHelper.ColorSuccess,
                                  () => { HideAllOwn(); OnMineModeClicked?.Invoke(); });

            // ── 清除存档 ──
            UIHelper.CreateButton(mainMenuPanel.transform, "清除存档", 24,
                                  new Vector2(0, -300), new Vector2(220, 55),
                                  UIHelper.ColorGray,
                                  () => OnClearSaveClicked?.Invoke());
        }

        private void BuildMatchSubMenu()
        {
            matchSubMenuPanel = UIHelper.CreatePanel(SharedCanvas.transform, "MatchSubMenuPanel");

            UIHelper.CreateText(matchSubMenuPanel.transform, "消消乐", 60, TextAnchor.MiddleCenter,
                                new Vector2(0, 280), new Vector2(600, 90),
                                Color.white, FontStyle.Bold);

            UIHelper.CreateText(matchSubMenuPanel.transform, "选择模式", 36, TextAnchor.MiddleCenter,
                                new Vector2(0, 200), new Vector2(400, 50),
                                UIHelper.ColorCleared, FontStyle.Normal);

            // ── 闯关模式 ──
            UIHelper.CreateButton(matchSubMenuPanel.transform, "闯关模式", 38,
                                  new Vector2(0, 60), new Vector2(400, 90),
                                  UIHelper.ColorPrimary,
                                  () => { HideAllOwn(); OnMatchLevelClicked?.Invoke(); });

            // ── 方块模式 ──
            UIHelper.CreateButton(matchSubMenuPanel.transform, "方块模式", 38,
                                  new Vector2(0, -60), new Vector2(400, 90),
                                  UIHelper.ColorOrange,
                                  () => { HideAllOwn(); OnBlockModeClicked?.Invoke(); });

            // ── 返回 ──
            UIHelper.CreateButton(matchSubMenuPanel.transform, "← 返回", 30,
                                  new Vector2(-380, 380), new Vector2(160, 60),
                                  UIHelper.ColorGray,
                                  ShowMainMenu);

            matchSubMenuPanel.SetActive(false);
        }
    }
}