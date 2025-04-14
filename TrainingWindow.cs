using System;
using UnityEngine;
using BepInEx.Logging;

namespace HS2Game
{
    public class TrainingWindow : BaseWindow
    {
        private CharaData charaData;
        private SceneManager sceneManager;
        private bool isInitialized = false;
        private string sceneInput1 = "1"; // 切换场景 1 的目标场景编号
        private string sceneInput2 = "2"; // 切换场景 2 的目标场景编号

        public TrainingWindow() : base(0, "", new Rect(0, 0, 0, 0))
        {
        }

        public void Initialize(int windowID, string windowTitle, Rect initialRect)
        {
            this.windowID = windowID;
            this.windowTitle = windowTitle;
            this.windowRect = initialRect;
            isInitialized = true;

            var logger = BepInEx.Logging.Logger.CreateLogSource("HS2 Studio Object Name UI");
            if (logger == null)
            {
                Debug.LogError("无法创建日志记录器，SceneManager 初始化失败");
            }
            else
            {
                sceneManager = new SceneManager(logger);
            }
        }

        private void Start()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("TrainingWindow Start: 未完成初始化，跳过 Start");
                return;
            }

            charaData = GetComponent<CharaData>();
            if (charaData == null)
            {
                Debug.LogError("CharaData 未在 TrainingWindow 中找到。");
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

                if (charaData != null && charaData.IsCheatMode())
                {
                    // 切换场景 1
                    GUILayout.Label($"切换场景 1 目标（当前: Scene {sceneManager.GetTargetScene1()}）：");
                    sceneInput1 = GUILayout.TextField(sceneInput1, 5);
                    if (GUILayout.Button("设置切换场景 1 目标"))
                    {
                        if (int.TryParse(sceneInput1, out int sceneNumber))
                        {
                            sceneManager.SetTargetScene1(sceneNumber);
                        }
                        else
                        {
                            Debug.LogWarning("请输入有效的场景编号（整数）");
                        }
                    }
                    if (GUILayout.Button("切换到场景 1"))
                    {
                        sceneManager.JumpToScene(sceneManager.GetTargetScene1());
                    }

                    // 切换场景 2
                    GUILayout.Label($"切换场景 2 目标（当前: Scene {sceneManager.GetTargetScene2()}）：");
                    sceneInput2 = GUILayout.TextField(sceneInput2, 5);
                    if (GUILayout.Button("设置切换场景 2 目标"))
                    {
                        if (int.TryParse(sceneInput2, out int sceneNumber))
                        {
                            sceneManager.SetTargetScene2(sceneNumber);
                        }
                        else
                        {
                            Debug.LogWarning("请输入有效的场景编号（整数）");
                        }
                    }
                    if (GUILayout.Button("切换到场景 2"))
                    {
                        sceneManager.JumpToScene(sceneManager.GetTargetScene2());
                    }
                }
                else
                {
                    if (GUILayout.Button("切换到场景 1"))
                    {
                        sceneManager.JumpToScene(1); // 默认跳转到 Scene 1
                    }

                    if (GUILayout.Button("切换到场景 2"))
                    {
                        sceneManager.JumpToScene(2); // 默认跳转到 Scene 2
                    }
                }

                if (GUILayout.Button("关闭"))
                {
                    ToggleVisibility();
                }

                GUILayout.EndVertical();

                // 整体拖拽
                GUI.DragWindow(new Rect(0, 0, windowRect.width, windowRect.height));
            }
            catch (Exception e)
            {
                Debug.LogError($"TrainingWindow DrawWindow 异常: {e}");
            }
            finally
            {
                GUI.enabled = guiEnabled;
                GUI.color = guiColor;
                GUI.backgroundColor = guiBackgroundColor;
            }
        }

        public new void Show()
        {
            base.Show();
        }
    }
}