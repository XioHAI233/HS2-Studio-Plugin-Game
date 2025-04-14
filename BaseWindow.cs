using UnityEngine;

namespace HS2Game
{
    public abstract class BaseWindow : MonoBehaviour
    {
        protected Rect windowRect;
        protected bool isVisible;
        protected bool isActive;
        protected int windowID;
        protected string windowTitle;

        public BaseWindow(int windowID, string windowTitle, Rect initialRect)
        {
            this.windowID = windowID;
            this.windowTitle = windowTitle;
            this.windowRect = initialRect;
            this.isVisible = false;
            this.isActive = false;
        }

        public bool IsVisible => isVisible;
        public bool IsActive => isActive;
        public Rect GetWindowRect() => windowRect;

        public void ToggleVisibility()
        {
            isVisible = !isVisible;
            if (!isVisible)
            {
                isActive = false; // 窗口隐藏时，重置激活状态
            }
        }

        public void Show()
        {
            isVisible = true;
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        protected void OnGUI()
        {
            if (isVisible)
            {
                // 检测鼠标位置是否在窗口内
                Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                isActive = windowRect.Contains(mousePos);

                // 如果窗口处于激活状态，阻止鼠标事件传递给游戏
                if (isActive)
                {
                    Input.ResetInputAxes();
                }

                windowRect = GUILayout.Window(windowID, windowRect, DrawWindow, windowTitle);
            }
        }

        protected abstract void DrawWindow(int windowID);
    }
}