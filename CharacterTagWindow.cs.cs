using System;
using System.Collections.Generic;
using UnityEngine;
using AIChara;

namespace HS2Game
{
    public class CharacterTagWindow : BaseWindow
    {
        private CharaData charaData;
        private List<string> characterTags = new List<string>();
        private bool isInitialized = false;

        public CharacterTagWindow() : base(0, "", new Rect(0, 0, 0, 0))
        {
        }

        public void Initialize(int windowID, string windowTitle, Rect initialRect, CharaData charaData)
        {
            this.windowID = windowID;
            this.windowTitle = windowTitle;
            this.windowRect = initialRect;
            this.charaData = charaData;
            isInitialized = true;
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("CharacterTagWindow Start: 未完成初始化，跳过 Start");
                return;
            }

            if (charaData == null)
            {
                charaData = FindObjectOfType<CharaData>();
                if (charaData == null)
                {
                    Debug.LogError("CharaData 未在 CharacterTagWindow 中找到。");
                    return;
                }
            }

            UpdateTags();
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
                    GUILayout.Label($"姓名: {charaData.GetCharacterName()}", GUILayout.Height(20));
                    GUILayout.Label("角色标签:", GUILayout.Height(20));

                    if (characterTags.Count > 0)
                    {
                        foreach (string tag in characterTags)
                        {
                            GUILayout.Label($"- {tag}", GUILayout.Height(20));
                        }
                    }
                    else
                    {
                        GUILayout.Label("无标签", GUILayout.Height(20));
                    }

                    if (GUILayout.Button("刷新标签"))
                    {
                        UpdateTags();
                    }

                    if (GUILayout.Button("关闭"))
                    {
                        ToggleVisibility();
                    }
                }
                else
                {
                    GUILayout.Label("姓名: 未找到数据", GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUI.DragWindow(new Rect(0, 0, windowRect.width, windowRect.height));
            }
            catch (Exception e)
            {
                Debug.LogError($"CharacterTagWindow DrawWindow 异常: {e}");
            }
            finally
            {
                GUI.enabled = guiEnabled;
                GUI.color = guiColor;
                GUI.backgroundColor = guiBackgroundColor;
            }
        }

        private void UpdateTags()
        {
            characterTags.Clear();

            if (charaData == null)
            {
                Debug.LogWarning("CharaData 未初始化，无法更新标签");
                return;
            }

            // 身高标签
            float height = charaData.CurrentHeight;
            if (height > 170f)
            {
                characterTags.Add("身材高挑");
            }
            else if (height < 160f && height > 0f)
            {
                characterTags.Add("小巧玲珑");
            }

            // 罩杯标签
            string cupSize = charaData.CurrentCupSize;
            if (!string.IsNullOrEmpty(cupSize))
            {
                if (cupSize.EndsWith("E"))
                {
                    characterTags.Add("大杯");
                }
                else if (cupSize.EndsWith("F"))
                {
                    characterTags.Add("超大杯");
                }
                else if (cupSize.EndsWith("G+"))
                {
                    characterTags.Add("圣杯");
                }
            }

            // 体重标签
            float weight = charaData.CurrentWeight;
            if (weight < 45f && weight > 0f)
            {
                characterTags.Add("轻盈体态");
            }

            // 综合身材标签
            float bust = charaData.CurrentBust;
            float waist = charaData.CurrentWaist;
            float hips = charaData.CurrentHips;
            if (bust > 90f && waist < 65f && hips > 90f)
            {
                characterTags.Add("完美曲线");
            }
            if (height > 165f && weight < 50f && weight > 0f)
            {
                characterTags.Add("模特身材");
            }

            // 腿占比标签
            float legRatio = charaData.CurrentLegRatio;
            if (legRatio > 0.5f && legRatio > 0f)
            {
                characterTags.Add("长腿");
            }

            // 翘臀标签
            float whr = charaData.CurrentWHR;
            if (hips > 95f && whr < 0.75f && whr > 0f)
            {
                characterTags.Add("翘臀");
            }

            Debug.Log($"已为角色 {charaData.GetCharacterName()} 更新标签: {string.Join(", ", characterTags)}");
        }

        public new void Show()
        {
            base.Show();
            UpdateTags(); // 打开窗口时刷新标签
        }
    }
}