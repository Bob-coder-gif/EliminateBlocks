using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mine
{
    /// <summary>
    /// 扫雷输入处理器。
    /// 短按 = 翻开格子，长按 = 插旗/取消旗。
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        // ---- 事件 ----
        public event Action<int, int> OnCellTapped;        // 短按 → 翻开
        public event Action<int, int> OnCellLongPressed;   // 长按 → 插旗

        private int width, height;

        // ---- 长按检测 ----
        private bool isPressing;
        private float pressStartTime;
        private Vector3 pressStartScreenPos;
        private bool longPressHandled;             // 避免一次长按触发多次

        private const float LONG_PRESS_TIME = 0.35f;  // 长按阈值（秒）
        private const float MOVE_CANCEL_PX  = 30f;    // 手指移动超过这个距离取消

        public void Init(int w, int h)
        {
            width = w;
            height = h;
        }

        private void Update()
        {
            // ---- 按下 ----
            if (GetPressDown() && !IsPointerOverUI())
            {
                isPressing = true;
                longPressHandled = false;
                pressStartTime = Time.time;
                pressStartScreenPos = GetPointerPosition();
            }

            // ---- 按住中：检测长按 ----
            if (isPressing && !longPressHandled && !GetPressUp())
            {
                // 手指移动太远 → 取消
                if (Vector3.Distance(GetPointerPosition(), pressStartScreenPos) > MOVE_CANCEL_PX)
                {
                    isPressing = false;
                    return;
                }

                // 达到长按时间 → 触发长按事件
                if (Time.time - pressStartTime >= LONG_PRESS_TIME)
                {
                    longPressHandled = true;
                    FireEvent(pressStartScreenPos, isLongPress: true);
                }
            }

            // ---- 松开 ----
            if (isPressing && GetPressUp())
            {
                // 没触发过长按 → 算短按
                if (!longPressHandled)
                    FireEvent(pressStartScreenPos, isLongPress: false);

                isPressing = false;
            }
        }

        // ==============================================================
        //  射线检测 + 发射事件
        // ==============================================================

        private void FireEvent(Vector3 screenPos, bool isLongPress)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider == null) return;

            string name = hit.collider.gameObject.name;
            if (!name.StartsWith("Cell_")) return;

            string[] parts = name.Split('_');
            if (parts.Length != 3) return;

            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (isLongPress)
                        OnCellLongPressed?.Invoke(x, y);
                    else
                        OnCellTapped?.Invoke(x, y);
                }
            }
        }

        // ==============================================================
        //  输入兼容（鼠标 + 触屏）
        // ==============================================================

        private bool GetPressDown()
        {
            if (Input.touchCount > 0) return Input.GetTouch(0).phase == TouchPhase.Began;
            return Input.GetMouseButtonDown(0);
        }

        private bool GetPressUp()
        {
            if (Input.touchCount > 0) return Input.GetTouch(0).phase == TouchPhase.Ended;
            return Input.GetMouseButtonUp(0);
        }

        private Vector3 GetPointerPosition()
        {
            if (Input.touchCount > 0) return Input.GetTouch(0).position;
            return Input.mousePosition;
        }

        private bool IsPointerOverUI()
        {
            if (Input.touchCount > 0)
                return EventSystem.current != null
                    && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }
    }
}