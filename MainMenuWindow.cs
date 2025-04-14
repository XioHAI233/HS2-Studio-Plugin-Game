using UnityEngine;
using BepInEx.Configuration;
using System;
using BepInEx.Logging;

namespace HS2Game
{
    public class MainMenuWindow : BaseWindow
    {
        private CharaData charaData;
        private bool isInitialized = false;
        private SceneManager sceneManager;

        public ConfigEntry<bool> Enabled { get; private set; }
        public ConfigEntry<KeyCode> ToggleKey { get; private set; }

        public MainMenuWindow() : base(0, "", new Rect(0, 0, 0, 0))
        {
        }

        public void Initialize(int windowID, string windowTitle, Rect initialRect, CharaData charaData)
        {
            this.windowID = windowID;
            this.windowTitle = windowTitle;
            this.windowRect = initialRect;
            this.charaData = charaData;
            Enabled = charaData.Config.Bind("General", "启用插件", true, "启用或禁用插件。");
            ToggleKey = charaData.Config.Bind("General", "切换按键", KeyCode.Keypad9, "切换主菜单的按键。");

            var logger = BepInEx.Logging.Logger.CreateLogSource("HS2 Studio Object Name UI");
            if (logger == null)
            {
                Debug.LogError("无法创建日志记录器，SceneManager 初始化失败");
            }
            else
            {
                sceneManager = new SceneManager(logger);
            }

            isInitialized = true;
            Debug.Log($"MainMenuWindow 初始化完成 - Enabled: {Enabled.Value}, ToggleKey: {ToggleKey.Value}");
        }

        private void Update()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("MainMenuWindow Update: 未完成初始化，跳过更新");
                return;
            }

            if (!Enabled.Value)
            {
                Debug.Log("MainMenuWindow Update: 插件未启用，请检查配置文件的 Enabled 设置");
                return;
            }

            if (Input.GetKeyDown(ToggleKey.Value))
            {
                ToggleVisibility();
                Debug.Log($"主菜单窗口可见性: {IsVisible} (检测按键: {ToggleKey.Value})");
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

                if (GUILayout.Button("保存场景"))
                {
                    charaData.SaveToFile();
                }

                if (GUILayout.Button("加载场景"))
                {
                    charaData.LoadFromFile();
                }

                if (GUILayout.Button("初始化"))
                {
                    charaData.InitializeMgdValues();
                }

                if (GUILayout.Button(charaData.IsCheatMode() ? "关闭作弊" : "开启作弊"))
                {
                    charaData.ToggleCheat();
                }

                if (GUILayout.Button("打开角色状态"))
                {
                    gameObject.GetComponent<CharacterStateWindow>()?.Show();
                }

                if (GUILayout.Button("打开训练"))
                {
                    gameObject.GetComponent<TrainingWindow>()?.Show();
                }

                if (GUILayout.Button("打开角色信息"))
                {
                    gameObject.GetComponent<CharacterInfoWindow>()?.Show();
                }

                if (GUILayout.Button("打开角色标签"))
                {
                    gameObject.GetComponent<CharacterTagWindow>()?.Show();
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
                Debug.LogError($"MainMenuWindow DrawWindow 异常: {e}");
                throw;
            }
            finally
            {
                GUI.enabled = guiEnabled;
                GUI.color = guiColor;
                GUI.backgroundColor = guiBackgroundColor;
            }
        }
    }
}