using System;
using UnityEngine;

namespace HS2Game
{
    public class CharacterStateWindow : BaseWindow
    {
        private CharaData charaData;
        private string mgdInputString = "0";
        private string hpInputString = "0";
        private string painInputString = "0";
        private string sanInputString = "0";
        private string trustInputString = "0";
        private bool isInitialized = false;

        public CharacterStateWindow() : base(0, "", new Rect(0, 0, 0, 0))
        {
        }

        public void Initialize(int windowID, string windowTitle, Rect initialRect)
        {
            this.windowID = windowID;
            this.windowTitle = windowTitle;
            this.windowRect = initialRect;
            isInitialized = true;
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("CharacterStateWindow Start: 未完成初始化，跳过 Start");
                return;
            }

            charaData = FindObjectOfType<CharaData>(); // 使用 FindObjectOfType 获取 CharaData
            if (charaData == null)
            {
                Debug.LogError("CharaData 未在 CharacterStateWindow 中找到。");
            }
            else
            {
                SyncInputStrings(); // 初始化时同步输入字符串
            }
        }

        protected override void DrawWindow(int id)
        {
            bool guiEnabled = GUI.enabled;
            Color guiColor = GUI.color;
            Color guiBackgroundColor = GUI.backgroundColor;

            try
            {
                GUILayout.BeginVertical();

                if (charaData != null)
                {
                    // 动态同步输入字符串，确保显示最新数据
                    SyncInputStrings();

                    GUILayout.Label($"目标序号: {charaData.GetObjectName()}", GUILayout.Height(20));
                    GUILayout.Label($"姓名: {charaData.GetCharacterName()}", GUILayout.Height(20));

                    DrawParameter("敏感度: ", ref mgdInputString, charaData.CurrentMgdValue);
                    DrawParameter("体力: ", ref hpInputString, charaData.CurrentHpValue);
                    DrawParameter("疼痛: ", ref painInputString, charaData.CurrentPainValue);
                    DrawParameter("理智: ", ref sanInputString, charaData.CurrentSanValue);
                    DrawParameter("信赖: ", ref trustInputString, charaData.CurrentTrustValue);
                }
                else
                {
                    GUILayout.Label("目标序号: 未找到数据", GUILayout.Height(20));
                    GUILayout.Label("姓名: 未找到数据", GUILayout.Height(20));
                }

                if (GUILayout.Button("关闭"))
                {
                    ToggleVisibility();
                }

                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, windowRect.width, windowRect.height));
            }
            catch (Exception e)
            {
                Debug.LogError($"CharacterStateWindow DrawWindow 异常: {e}");
            }
            finally
            {
                GUI.enabled = guiEnabled;
                GUI.color = guiColor;
                GUI.backgroundColor = guiBackgroundColor;
            }
        }

        private void DrawParameter(string label, ref string inputString, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(50));
            if (charaData != null && charaData.IsCheatMode())
            {
                GUI.SetNextControlName(label);
                string newInput = GUILayout.TextField(inputString, GUILayout.Width(100));
                if (newInput != inputString) // 仅在输入变化时尝试解析
                {
                    if (float.TryParse(newInput, out float newValue))
                    {
                        charaData.UpdateParameter(label, newValue);
                        inputString = newValue.ToString(); // 更新输入字符串
                    }
                    else
                    {
                        Debug.LogWarning($"无效输入: {newInput}，无法解析为浮点数");
                    }
                }
            }
            else
            {
                GUILayout.Label(value.ToString("F1"), GUILayout.Width(100)); // 显示格式化值
            }
            GUILayout.EndHorizontal();
        }

        private void SyncInputStrings()
        {
            if (charaData == null) return;
            mgdInputString = charaData.CurrentMgdValue.ToString("F1");
            hpInputString = charaData.CurrentHpValue.ToString("F1");
            painInputString = charaData.CurrentPainValue.ToString("F1");
            sanInputString = charaData.CurrentSanValue.ToString("F1");
            trustInputString = charaData.CurrentTrustValue.ToString("F1");
        }

        public new void Show()
        {
            base.Show();
            SyncInputStrings();
        }
    }
}