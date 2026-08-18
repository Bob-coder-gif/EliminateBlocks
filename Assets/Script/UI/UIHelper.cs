using System;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    /// <summary>
    /// 共享的 UI 创建工具方法。
    /// UIManager / MatchUI / MineUI 都调用这些静态方法来创建面板、文字、按钮，
    /// 避免每个文件里都写一遍重复代码。
    /// </summary>
    public static class UIHelper
    {
        // ---- 公共颜色常量 ----
        public static readonly Color ColorPrimary = new Color(0.20f, 0.60f, 0.95f);
        public static readonly Color ColorSuccess = new Color(0.30f, 0.78f, 0.30f);
        public static readonly Color ColorDanger  = new Color(0.90f, 0.30f, 0.30f);
        public static readonly Color ColorLocked  = new Color(0.55f, 0.55f, 0.55f);
        public static readonly Color ColorCleared = new Color(1.00f, 0.75f, 0.20f);
        public static readonly Color ColorBg      = new Color(0.12f, 0.12f, 0.18f, 0.92f);
        public static readonly Color ColorOrange  = new Color(0.95f, 0.55f, 0.15f);
        public static readonly Color ColorGray    = new Color(0.40f, 0.40f, 0.40f);

        /// <summary>创建一个铺满父节点的面板。</summary>
        public static GameObject CreatePanel(Transform parent, string name, bool withBackground = true)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (withBackground)
            {
                var img = go.AddComponent<Image>();
                img.color = ColorBg;
                img.raycastTarget = true;
            }
            return go;
        }

        /// <summary>创建一个 Text（居中定位）。</summary>
        public static Text CreateText(Transform parent, string content, int fontSize,
                                      TextAnchor anchor, Vector2 pos, Vector2 size,
                                      Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.alignment = anchor;
            txt.color = color;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        /// <summary>创建一个 Text（锚点定位，适合 HUD 贴边布局）。</summary>
        public static Text CreateTextAnchored(Transform parent, string content, int fontSize,
                                              float anchorX, float anchorY,
                                              Vector2 pos, Vector2 size,
                                              Color color, FontStyle style = FontStyle.Normal)
        {
            var txt = CreateText(parent, content, fontSize, TextAnchor.MiddleLeft,
                                 Vector2.zero, size, color, style);
            var rt = txt.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(anchorX, anchorY);
            rt.anchorMax = new Vector2(anchorX, anchorY);
            rt.pivot     = new Vector2(anchorX, anchorY);
            rt.anchoredPosition = pos;
            return txt;
        }

        /// <summary>创建一个按钮。</summary>
        public static Button CreateButton(Transform parent, string label, int fontSize,
                                          Vector2 pos, Vector2 size, Color bgColor, Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.1f, bgColor.g + 0.1f, bgColor.b + 0.1f);
            colors.pressedColor     = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
            colors.disabledColor    = ColorLocked;
            btn.colors = colors;

            if (onClick != null) btn.onClick.AddListener(() => onClick());

            CreateText(go.transform, label, fontSize, TextAnchor.MiddleCenter,
                       Vector2.zero, size, Color.white, FontStyle.Bold);
            return btn;
        }
    }
}