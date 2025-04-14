using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using Studio;
using System;
using System.Collections.Generic;
using System.IO;
using AIChara;
using KKAPI.Studio;
using System.Linq;

namespace HS2Game
{
    [BepInPlugin("com.yourname.hs2studio.objectnameui", "HS2 Studio Object Name UI", "1.0.0")]
    [BepInDependency("com.deathweasel.bepinex.illusionmoddingapi", BepInDependency.DependencyFlags.HardDependency)]
    public class CharaData : BaseUnityPlugin
    {
        private string objectName = "未选择对象";
        private string characterName = "无角色姓名";
        private bool isCheatMode = false;
        private Dictionary<string, CharacterData> characterDataDict = new Dictionary<string, CharacterData>();
        private readonly string dataFilePath = Path.Combine(Paths.PluginPath, "HS2StudioData", "data.txt");
        private string lastObjectName = null;

        public float CurrentMgdValue { get; set; } = 0f;
        public float CurrentHpValue { get; set; } = 0f;
        public float CurrentPainValue { get; set; } = 0f;
        public float CurrentSanValue { get; set; } = 0f;
        public float CurrentTrustValue { get; set; } = 0f;
        public float CurrentHeight { get; set; } = 0f;
        public float CurrentWeight { get; set; } = 0f;
        public float CurrentBust { get; set; } = 0f;
        public float CurrentWaist { get; set; } = 0f;
        public float CurrentHips { get; set; } = 0f;
        public float CurrentWHR => CurrentHips != 0f ? CurrentWaist / CurrentHips : 0f;
        public string CurrentCupSize { get; set; } = "未测量";
        public float CurrentLegLength { get; set; } = 0f;
        public float CurrentLegRatio { get; set; } = 0f;

        public ConfigEntry<bool> Enabled { get; private set; }
        public ConfigEntry<KeyCode> ToggleKey { get; private set; }

        private class CharacterData
        {
            public string ObjectName { get; set; }
            public string CharacterName { get; set; }
            public float Mgd { get; set; }
            public float Hp { get; set; }
            public float Pain { get; set; }
            public float San { get; set; }
            public float Trust { get; set; }
            public float Height { get; set; }
            public float Weight { get; set; }
            public float Bust { get; set; }
            public float Waist { get; set; }
            public float Hips { get; set; }
            public string CupSize { get; set; }
            public float LegLength { get; set; }
            public float LegRatio { get; set; }
        }

        private void Awake()
        {
            Enabled = Config.Bind("General", "启用插件", true, "启用或禁用插件。");
            ToggleKey = Config.Bind("General", "切换按键", KeyCode.Keypad9, "切换主菜单的按键。");
            Logger.LogInfo($"CharaData 初始化 - Enabled: {Enabled.Value}, ToggleKey: {ToggleKey.Value}");

            Directory.CreateDirectory(Path.GetDirectoryName(dataFilePath));
            LoadFromFile();

            var mainMenu = gameObject.AddComponent<MainMenuWindow>();
            if (mainMenu != null)
            {
                mainMenu.Initialize(UnityEngine.Random.Range(30000, 40000), "主菜单", new Rect(500, 200, 200, 250), this);
                Logger.LogInfo("MainMenuWindow 添加并初始化成功");
            }
            else
            {
                Logger.LogError("MainMenuWindow 添加失败");
            }

            var characterStateWindow = gameObject.AddComponent<CharacterStateWindow>();
            if (characterStateWindow != null)
            {
                characterStateWindow.Initialize(UnityEngine.Random.Range(20000, 30000), "角色状态", new Rect(800, 200, 350, 300));
                Logger.LogInfo("CharacterStateWindow 添加并初始化成功");
            }
            else
            {
                Logger.LogError("CharacterStateWindow 添加失败");
            }

            var trainingWindow = gameObject.AddComponent<TrainingWindow>();
            if (trainingWindow != null)
            {
                trainingWindow.Initialize(UnityEngine.Random.Range(40000, 50000), "训练窗口", new Rect(1100, 200, 200, 150));
                Logger.LogInfo("TrainingWindow 添加并初始化成功");
            }
            else
            {
                Logger.LogError("TrainingWindow 添加失败");
            }

            var characterInfoWindow = gameObject.AddComponent<CharacterInfoWindow>();
            if (characterInfoWindow != null)
            {
                characterInfoWindow.Initialize(UnityEngine.Random.Range(50000, 60000), "角色信息", new Rect(1200, 200, 350, 300));
                Logger.LogInfo("CharacterInfoWindow 添加并初始化成功");
            }
            else
            {
                Logger.LogError("CharacterInfoWindow 添加失败");
            }

            var characterTagWindow = gameObject.AddComponent<CharacterTagWindow>();
            if (characterTagWindow != null)
            {
                characterTagWindow.Initialize(UnityEngine.Random.Range(60000, 70000), "角色标签", new Rect(1550, 200, 300, 400), this);
                Logger.LogInfo("CharacterTagWindow 添加并初始化成功");
            }
            else
            {
                Logger.LogError("CharacterTagWindow 添加失败");
            }

            Logger.LogInfo("HS2 Studio Object Name UI 加载成功！");
        }

        private void Update()
        {
            if (!Enabled.Value)
            {
                return;
            }

            string currentObjectName = GetCurrentObjectName();
            if (currentObjectName != lastObjectName)
            {
                Logger.LogInfo("CharaData Update: 检测到选中对象变化，更新角色信息");
                UpdateSelectedObjectInfo();
                lastObjectName = currentObjectName;
            }
        }

        public OCIChar GetSelectedObject()
        {
            try
            {
                var selectedObject = StudioAPI.GetSelectedObjects().FirstOrDefault();
                if (selectedObject is OCIChar ociChar)
                {
                    return ociChar;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取选中对象出错: {ex.Message}");
            }
            return null;
        }

        public string GetCharacterName()
        {
            var selectedChar = GetSelectedObject();
            if (selectedChar != null && selectedChar.charInfo != null)
            {
                return selectedChar.charInfo.chaFile.parameter.fullname ?? "姓名为空";
            }
            return "无角色姓名";
        }

        private string GetCurrentObjectName()
        {
            var selectedChar = GetSelectedObject();
            if (selectedChar != null)
            {
                return selectedChar.guideObject.transformTarget.gameObject.name;
            }
            return "未选择对象";
        }

        private void UpdateSelectedObjectInfo()
        {
            try
            {
                var selectedChar = GetSelectedObject();
                if (selectedChar != null)
                {
                    objectName = selectedChar.guideObject.transformTarget.gameObject.name;
                    characterName = GetCharacterName();

                    if (!characterDataDict.ContainsKey(objectName))
                    {
                        characterDataDict[objectName] = new CharacterData
                        {
                            ObjectName = objectName,
                            CharacterName = characterName,
                            Mgd = CurrentMgdValue,
                            Hp = CurrentHpValue,
                            Pain = CurrentPainValue,
                            San = CurrentSanValue,
                            Trust = CurrentTrustValue,
                            Height = CurrentHeight,
                            Weight = CurrentWeight,
                            Bust = CurrentBust,
                            Waist = CurrentWaist,
                            Hips = CurrentHips,
                            CupSize = CurrentCupSize,
                            LegLength = CurrentLegLength,
                            LegRatio = CurrentLegRatio
                        };
                        Logger.LogInfo($"UpdateSelectedObjectInfo: 新建对象 {objectName}, 参数保持为 Mgd={CurrentMgdValue}, Hp={CurrentHpValue}, Pain={CurrentPainValue}, San={CurrentSanValue}, Trust={CurrentTrustValue}, Height={CurrentHeight}, Weight={CurrentWeight}, Bust={CurrentBust}, Waist={CurrentWaist}, Hips={CurrentHips}, CupSize={CurrentCupSize}, LegLength={CurrentLegLength}, LegRatio={CurrentLegRatio}");
                    }
                    else
                    {
                        var data = characterDataDict[objectName];
                        data.CharacterName = characterName;
                        CurrentMgdValue = data.Mgd;
                        CurrentHpValue = data.Hp;
                        CurrentPainValue = data.Pain;
                        CurrentSanValue = data.San;
                        CurrentTrustValue = data.Trust;
                        CurrentHeight = data.Height;
                        CurrentWeight = data.Weight;
                        CurrentBust = data.Bust;
                        CurrentWaist = data.Waist;
                        CurrentHips = data.Hips;
                        CurrentCupSize = data.CupSize;
                        CurrentLegLength = data.LegLength;
                        CurrentLegRatio = data.LegRatio;
                        Logger.LogInfo($"UpdateSelectedObjectInfo: 更新对象 {objectName}, 参数同步为 Mgd={CurrentMgdValue}, Hp={CurrentHpValue}, Pain={CurrentPainValue}, San={CurrentSanValue}, Trust={CurrentTrustValue}, Height={CurrentHeight}, Weight={CurrentWeight}, Bust={CurrentBust}, Waist={CurrentWaist}, Hips={CurrentHips}, CupSize={CurrentCupSize}, LegLength={CurrentLegLength}, LegRatio={CurrentLegRatio}");
                    }
                }
                else
                {
                    objectName = "未选择对象";
                    characterName = "无角色姓名";
                    if (!characterDataDict.ContainsKey(objectName))
                    {
                        characterDataDict[objectName] = new CharacterData
                        {
                            ObjectName = objectName,
                            CharacterName = characterName,
                            Mgd = CurrentMgdValue,
                            Hp = CurrentHpValue,
                            Pain = CurrentPainValue,
                            San = CurrentSanValue,
                            Trust = CurrentTrustValue,
                            Height = CurrentHeight,
                            Weight = CurrentWeight,
                            Bust = CurrentBust,
                            Waist = CurrentWaist,
                            Hips = CurrentHips,
                            CupSize = CurrentCupSize,
                            LegLength = CurrentLegLength,
                            LegRatio = CurrentLegRatio
                        };
                        Logger.LogInfo($"UpdateSelectedObjectInfo: 未选择对象，新建对象 {objectName}, 参数保持为 Mgd={CurrentMgdValue}, Hp={CurrentHpValue}, Pain={CurrentPainValue}, San={CurrentSanValue}, Trust={CurrentTrustValue}, Height={CurrentHeight}, Weight={CurrentWeight}, Bust={CurrentBust}, Waist={CurrentWaist}, Hips={CurrentHips}, CupSize={CurrentCupSize}, LegLength={CurrentLegLength}, LegRatio={CurrentLegRatio}");
                    }
                }
            }
            catch (Exception ex)
            {
                objectName = "发生错误";
                characterName = $"错误: {ex.Message}";
            }
        }

        public string GetObjectName()
        {
            return objectName;
        }

        public string GetCharacterNamePublic()
        {
            return characterName;
        }

        public bool IsCheatMode()
        {
            return isCheatMode;
        }

        public void ToggleCheat()
        {
            isCheatMode = !isCheatMode;
            Logger.LogInfo($"作弊模式已切换为: {(isCheatMode ? "开启" : "关闭")}");
        }

        public void InitializeMgdValues()
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("chaF"))
                {
                    ChaControl chaControl = obj.GetComponent<ChaControl>();
                    string charName = "未知";
                    if (chaControl != null && chaControl.chaFile != null)
                    {
                        charName = chaControl.chaFile.parameter.fullname ?? "姓名为空";
                    }

                    characterDataDict[obj.name] = new CharacterData
                    {
                        ObjectName = obj.name,
                        CharacterName = charName,
                        Mgd = 0f,
                        Hp = 0f,
                        Pain = 0f,
                        San = 0f,
                        Trust = 0f,
                        Height = 0f,
                        Weight = 0f,
                        Bust = 0f,
                        Waist = 0f,
                        Hips = 0f,
                        CupSize = "未测量",
                        LegLength = 0f,
                        LegRatio = 0f
                    };
                }
            }
            Logger.LogInfo($"初始化了 {characterDataDict.Count} 个包含 'chaF' 的对象，所有身体数据设为未测量状态");

            UpdateSelectedObjectInfo();
        }

        public void UpdateParameter(string paramName, float newValue)
        {
            try
            {
                var selectedChar = GetSelectedObject();
                if (selectedChar != null)
                {
                    string targetObjectName = selectedChar.guideObject.transformTarget.gameObject.name;
                    if (characterDataDict.ContainsKey(targetObjectName))
                    {
                        var data = characterDataDict[targetObjectName];
                        switch (paramName)
                        {
                            case "敏感度: ": data.Mgd = newValue; CurrentMgdValue = newValue; break;
                            case "体力: ": data.Hp = newValue; CurrentHpValue = newValue; break;
                            case "疼痛: ": data.Pain = newValue; CurrentPainValue = newValue; break;
                            case "理智: ": data.San = newValue; CurrentSanValue = newValue; break;
                            case "信赖: ": data.Trust = newValue; CurrentTrustValue = newValue; break;
                            case "身高 (cm): ": data.Height = newValue; CurrentHeight = newValue; break;
                            case "体重 (kg): ": data.Weight = newValue; CurrentWeight = newValue; break;
                            case "胸围 (cm): ": data.Bust = newValue; CurrentBust = newValue; break;
                            case "腰围 (cm): ": data.Waist = newValue; CurrentWaist = newValue; break;
                            case "臀围 (cm): ": data.Hips = newValue; CurrentHips = newValue; break;
                            case "腿长 (cm): ": data.LegLength = newValue; CurrentLegLength = newValue; break;
                            case "腿占比: ": data.LegRatio = newValue; CurrentLegRatio = newValue; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"更新参数 {paramName} 出错: {ex.Message}");
            }
        }

        public void UpdateParameter(string paramName, string newValue)
        {
            try
            {
                var selectedChar = GetSelectedObject();
                if (selectedChar != null)
                {
                    string targetObjectName = selectedChar.guideObject.transformTarget.gameObject.name;
                    if (characterDataDict.ContainsKey(targetObjectName))
                    {
                        var data = characterDataDict[targetObjectName];
                        if (paramName == "罩杯: ")
                        {
                            data.CupSize = newValue;
                            CurrentCupSize = newValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"更新参数 {paramName} 出错: {ex.Message}");
            }
        }

        public void SaveToFile()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (var data in characterDataDict.Values)
                {
                    lines.Add($"ObjectName={data.ObjectName},CharacterName={data.CharacterName},Mgd={data.Mgd},Hp={data.Hp},Pain={data.Pain},San={data.San},Trust={data.Trust},Height={data.Height},Weight={data.Weight},Bust={data.Bust},Waist={data.Waist},Hips={data.Hips},CupSize={data.CupSize},LegLength={data.LegLength},LegRatio={data.LegRatio}");
                }
                File.WriteAllLines(dataFilePath, lines);
                Logger.LogInfo($"数据已保存到 {dataFilePath}，包含角色信息窗口中的身体数据");
            }
            catch (Exception ex)
            {
                Logger.LogError($"保存文件出错: {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string[] lines = File.ReadAllLines(dataFilePath);
                    characterDataDict.Clear();
                    foreach (string line in lines)
                    {
                        var parts = line.Split(',');
                        string objName = parts[0].Split('=')[1];
                        string charName = parts[1].Split('=')[1];
                        float mgd = float.Parse(parts[2].Split('=')[1]);
                        float hp = float.Parse(parts[3].Split('=')[1]);
                        float pain = float.Parse(parts[4].Split('=')[1]);
                        float san = float.Parse(parts[5].Split('=')[1]);
                        float trust = float.Parse(parts[6].Split('=')[1]);
                        float height = float.Parse(parts[7].Split('=')[1]);
                        float weight = float.Parse(parts[8].Split('=')[1]);
                        float bust = float.Parse(parts[9].Split('=')[1]);
                        float waist = float.Parse(parts[10].Split('=')[1]);
                        float hips = float.Parse(parts[11].Split('=')[1]);
                        string cupSize = parts[12].Split('=')[1];
                        float legLength = float.Parse(parts[13].Split('=')[1]);
                        float legRatio = float.Parse(parts[14].Split('=')[1]);

                        characterDataDict[objName] = new CharacterData
                        {
                            ObjectName = objName,
                            CharacterName = charName,
                            Mgd = mgd,
                            Hp = hp,
                            Pain = pain,
                            San = san,
                            Trust = trust,
                            Height = height,
                            Weight = weight,
                            Bust = bust,
                            Waist = waist,
                            Hips = hips,
                            CupSize = cupSize,
                            LegLength = legLength,
                            LegRatio = legRatio
                        };
                    }
                    Logger.LogInfo($"从 {dataFilePath} 加载了 {characterDataDict.Count} 条数据");

                    UpdateSelectedObjectInfo();
                }
                else
                {
                    Logger.LogInfo($"未找到数据文件: {dataFilePath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"加载文件出错: {ex.Message}");
            }
        }
    }
}