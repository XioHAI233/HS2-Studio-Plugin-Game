using System;
using UnityEngine;
using Studio;
using BepInEx.Logging;
using AIChara;

namespace HS2Game
{
    public class CharacterInfoWindow : BaseWindow
    {
        private CharaData charaData;
        private string heightInputString = "未测量";
        private string weightInputString = "未测量";
        private string bustInputString = "未测量";
        private string waistInputString = "未测量";
        private string hipsInputString = "未测量";
        private string whrInputString = "未测量";
        private string cupSizeInputString = "未测量";
        private string legLengthInputString = "未测量"; // 新增：腿长输入
        private string legRatioInputString = "未测量"; // 新增：腿占比输入
        private bool isInitialized = false;
        private SceneManager sceneManager;

        public CharacterInfoWindow() : base(0, "", new Rect(0, 0, 0, 0))
        {
        }

        public void Initialize(int windowID, string windowTitle, Rect initialRect)
        {
            this.windowID = windowID;
            this.windowTitle = windowTitle;
            this.windowRect = initialRect;
            isInitialized = true;

            var logger = BepInEx.Logging.Logger.CreateLogSource("HS2 Character Info UI");
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
                Debug.LogWarning("CharacterInfoWindow Start: 未完成初始化，跳过 Start");
                return;
            }

            charaData = FindObjectOfType<CharaData>();
            if (charaData == null)
            {
                Debug.LogError("CharaData 未在 CharacterInfoWindow 中找到。");
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
                    GUILayout.Label($"姓名: {charaData.GetCharacterName()}", GUILayout.Height(20));

                    DrawParameter("身高 (cm): ", ref heightInputString, charaData.CurrentHeight);
                    DrawParameter("体重 (kg): ", ref weightInputString, charaData.CurrentWeight);
                    DrawParameter("胸围 (cm): ", ref bustInputString, charaData.CurrentBust);
                    DrawParameter("腰围 (cm): ", ref waistInputString, charaData.CurrentWaist);
                    DrawParameter("臀围 (cm): ", ref hipsInputString, charaData.CurrentHips);
                    DrawParameter("腰臀比: ", ref whrInputString, charaData.CurrentWHR, readOnly: true);
                    DrawParameter("罩杯: ", ref cupSizeInputString, charaData.CurrentCupSize, isString: true);
                    DrawParameter("腿长 (cm): ", ref legLengthInputString, charaData.CurrentLegLength); // 新增：腿长
                    DrawParameter("腿占比: ", ref legRatioInputString, charaData.CurrentLegRatio, readOnly: true); // 新增：腿占比

                    if (GUILayout.Button("角色测量"))
                    {
                        sceneManager.JumpToScene(6);
                    }

                    if (GUILayout.Button("同步测量数据"))
                    {
                        SyncMeasurements();
                    }
                }
                else
                {
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
                Debug.LogError($"CharacterInfoWindow DrawWindow 异常: {e}");
            }
            finally
            {
                GUI.enabled = guiEnabled;
                GUI.color = guiColor;
                GUI.backgroundColor = guiBackgroundColor;
            }
        }

        private void DrawParameter(string label, ref string inputString, float value, bool readOnly = false, bool isString = false)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(80));
            if (charaData != null && charaData.IsCheatMode() && !readOnly && inputString != "未测量")
            {
                GUI.SetNextControlName(label);
                inputString = GUILayout.TextField(inputString, GUILayout.Width(100));
                if (float.TryParse(inputString, out float newValue))
                {
                    charaData.UpdateParameter(label, newValue);
                }
            }
            else
            {
                string displayValue = (value == 0f || inputString == "未测量")
                    ? "未测量"
                    : (label == "腰臀比: " || label == "腿占比: " ? value.ToString("F3") : value.ToString("F1"));
                GUILayout.Label(displayValue, GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawParameter(string label, ref string inputString, string value, bool readOnly = false, bool isString = false)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(80));
            if (charaData != null && charaData.IsCheatMode() && !readOnly && inputString != "未测量")
            {
                GUI.SetNextControlName(label);
                inputString = GUILayout.TextField(inputString, GUILayout.Width(100));
                charaData.UpdateParameter(label, inputString);
            }
            else
            {
                GUILayout.Label(inputString != "未测量" ? value : "未测量", GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
        }

        public new void Show()
        {
            base.Show();
        }

        private void SyncMeasurements()
        {
            GameObject character = GetSelectedCharacter();
            if (character != null && charaData != null)
            {
                float height, bust, waist, hips, underbust, weight, legLength, legRatio;
                CalculateMeasurements(character, out height, out bust, out waist, out hips, out underbust, out weight, out legLength, out legRatio);

                string cupSize = CalculateCupSize(bust, underbust);

                charaData.UpdateParameter("身高 (cm): ", height);
                charaData.UpdateParameter("体重 (kg): ", weight);
                charaData.UpdateParameter("胸围 (cm): ", bust);
                charaData.UpdateParameter("腰围 (cm): ", waist);
                charaData.UpdateParameter("臀围 (cm): ", hips);
                charaData.UpdateParameter("罩杯: ", cupSize);
                charaData.UpdateParameter("腿长 (cm): ", legLength); // 新增：更新腿长
                charaData.UpdateParameter("腿占比: ", legRatio); // 新增：更新腿占比

                SyncInputStrings();
                Debug.Log($"同步选中角色 {character.name} 数据: Height={height:F1}, Weight={weight:F1}, Bust={bust:F1}, Waist={waist:F1}, Hips={hips:F1}, Underbust={underbust:F1}, CupSize={cupSize}, WHR={(waist / hips):F3}, LegLength={legLength:F1}, LegRatio={legRatio:F3}");
            }
            else
            {
                Debug.LogWarning("同步失败：未选中角色或 CharaData 未初始化");
            }
        }

        private void SyncInputStrings()
        {
            if (charaData == null) return;
            heightInputString = (charaData.CurrentHeight == 0f) ? "未测量" : charaData.CurrentHeight.ToString("F1");
            weightInputString = (charaData.CurrentWeight == 0f) ? "未测量" : charaData.CurrentWeight.ToString("F1");
            bustInputString = (charaData.CurrentBust == 0f) ? "未测量" : charaData.CurrentBust.ToString("F1");
            waistInputString = (charaData.CurrentWaist == 0f) ? "未测量" : charaData.CurrentWaist.ToString("F1");
            hipsInputString = (charaData.CurrentHips == 0f) ? "未测量" : charaData.CurrentHips.ToString("F1");
            whrInputString = (charaData.CurrentHips == 0f || charaData.CurrentWaist == 0f) ? "未测量" : charaData.CurrentWHR.ToString("F3");
            cupSizeInputString = charaData.CurrentCupSize;
            legLengthInputString = (charaData.CurrentLegLength == 0f) ? "未测量" : charaData.CurrentLegLength.ToString("F1"); // 新增：腿长
            legRatioInputString = (charaData.CurrentLegRatio == 0f) ? "未测量" : charaData.CurrentLegRatio.ToString("F3"); // 新增：腿占比
        }

        private GameObject GetSelectedCharacter()
        {
            if (charaData == null)
            {
                Debug.LogError("CharaData 未初始化，无法获取选中角色");
                return null;
            }

            try
            {
                OCIChar selectedChar = charaData.GetSelectedObject();
                if (selectedChar != null && selectedChar.guideObject != null && selectedChar.guideObject.transformTarget != null)
                {
                    GameObject targetObject = selectedChar.guideObject.transformTarget.gameObject;
                    Debug.Log($"成功获取选中角色: {targetObject.name}");
                    return targetObject;
                }
                else
                {
                    Debug.LogWarning("未选中任何角色或 guideObject/transformTarget 为 null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"获取选中角色失败: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private Transform FindTransformByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            if (root.name == name)
                return root;

            foreach (Transform child in root)
            {
                Transform found = FindTransformByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void CalculateMeasurements(GameObject character, out float height, out float bust, out float waist, out float hips, out float underbust, out float weight, out float legLength, out float legRatio)
        {
            height = bust = waist = hips = underbust = weight = legLength = legRatio = 0f;
            if (character == null)
            {
                Debug.LogError("CalculateMeasurements: character 为 null");
                return;
            }

            Debug.Log($"CalculateMeasurements: character name = {character.name}");

            Transform bodyTop = character.transform.Find("BodyTop");
            if (bodyTop == null)
            {
                Debug.LogWarning("未找到 BodyTop");
                return;
            }

            const float scaleBody = 0.1f;
            const float scaleWaist = 0.15f;
            const float scaleHeight = 0.0335f; // 身高和腿长共用此缩放比例
            const float scaleUnderbust = 0.1f;
            const float scaleHips = 0.095f;

            // 计算身高
            Transform headTop = FindTransformByName(bodyTop, "N_Head_top");
            Transform rootBone = FindTransformByName(bodyTop, "N_Foot_R");
            if (headTop != null && rootBone != null)
            {
                if (IsChildOf(headTop, character.transform) && IsChildOf(rootBone, character.transform))
                {
                    float yDistance = headTop.position.y - rootBone.position.y;
                    float baseHeight = yDistance * 100f * scaleHeight;
                    float t = 3f;
                    height = baseHeight * t + 5f;
                    Debug.Log($"身高计算成功: headTop = {headTop.name}, rootBone = {rootBone.name}, yDistance = {yDistance:F2}, height = {height:F1} cm");
                }
            }

            // 计算腿长（与身高使用相同的缩放比例和逻辑）
            Transform legUpR = FindTransformByName(bodyTop, "cf_J_LegUp00_R");
            if (rootBone != null && legUpR != null)
            {
                if (IsChildOf(rootBone, character.transform) && IsChildOf(legUpR, character.transform))
                {
                    // 使用 Y 轴差值以匹配身高计算
                    float legYDistance = legUpR.position.y - rootBone.position.y;
                    float baseLegLength = legYDistance * 100f * scaleHeight; // 使用 scaleHeight
                    float t = 3f; // 与身高相同的乘数
                    legLength = baseLegLength * t + 5f; // 与身高相同的偏移
                    Debug.Log($"腿长计算成功: rootBone = {rootBone.name}, legUpR = {legUpR.name}, legYDistance = {legYDistance:F2}, legLength = {legLength:F1} cm");
                }
            }

            // 计算腿占比
            if (height > 0f && legLength > 0f)
            {
                legRatio = legLength / height;
                Debug.Log($"腿占比计算成功: legLength = {legLength:F1}, height = {height:F1}, legRatio = {legRatio:F3}");
            }
            // 计算下胸围（underbust）
            Transform muneL = FindTransformByName(bodyTop, "cf_J_Mune01_L");
            Transform muneR = FindTransformByName(bodyTop, "cf_J_Mune01_R");
            Transform shoulderL = FindTransformByName(bodyTop, "cf_J_Shoulder_L");
            float underbustWidth = 0f;
            float underbustDepth = 0f;
            if (muneL != null && muneR != null && shoulderL != null)
            {
                if (IsChildOf(muneL, character.transform) && IsChildOf(muneR, character.transform) && IsChildOf(shoulderL, character.transform))
                {
                    underbustWidth = Vector3.Distance(muneL.position, muneR.position) * 2f;
                    Vector3 shoulderToMune = muneL.position - shoulderL.position;
                    Vector3 zAxis = shoulderL.forward;
                    float zDistance = Mathf.Abs(Vector3.Dot(shoulderToMune, zAxis));
                    underbustDepth = zDistance * 2f;
                    underbust = 2f * (underbustWidth + underbustDepth) * 100f * scaleUnderbust;
                    Debug.Log($"下胸围计算: width={underbustWidth:F2}, depth={underbustDepth:F2}, underbust={underbust:F1}");
                }
            }

            // 计算胸围（bust）
            Transform nipL = FindTransformByName(bodyTop, "cf_J_Mune_Nip02_L");
            Transform nipR = FindTransformByName(bodyTop, "cf_J_Mune_Nip02_R");
            if (nipL != null && nipR != null && muneL != null && muneR != null && shoulderL != null)
            {
                if (IsChildOf(nipL, character.transform) && IsChildOf(nipR, character.transform) &&
                    IsChildOf(muneL, character.transform) && IsChildOf(muneR, character.transform))
                {
                    float muneDistance = Vector3.Distance(muneL.position, muneR.position);
                    Vector3 AP = nipL.position - muneL.position;
                    Vector3 AB = muneR.position - muneL.position;
                    float abMagnitudeSqr = AB.sqrMagnitude;
                    float t = abMagnitudeSqr > 0.0001f ? Vector3.Dot(AP, AB) / abMagnitudeSqr : 0f;
                    float footToMuneL = Mathf.Abs(t) * muneDistance;
                    float bustWidth = underbustWidth + footToMuneL * 2f;
                    float nippleToMuneLine = PerpendicularDistance(nipL.position, muneL.position, muneR.position);
                    float bustDepth = underbustDepth + nippleToMuneLine;
                    bust = 2f * (bustWidth + bustDepth) * 100f * scaleBody;
                    Debug.Log($"胸围计算: width={bustWidth:F2}, depth={bustDepth:F2}, bust={bust:F1}");
                }
            }

            // 计算腰围
            Transform waistL = FindTransformByName(bodyTop, "N_Waist_L");
            Transform waistR = FindTransformByName(bodyTop, "N_Waist_R");
            if (waistL != null && waistR != null)
            {
                if (IsChildOf(waistL, character.transform) && IsChildOf(waistR, character.transform))
                {
                    float width = Vector3.Distance(waistL.position, waistR.position) * 0.8f;
                    float depth = width * 0.7f;
                    waist = Mathf.PI * Mathf.Sqrt(width * width + depth * depth) * 0.5f * 100f * scaleWaist;
                }
            }

            // 计算臀围
            Transform hipL = FindTransformByName(bodyTop, "cf_J_LegUp00_L");
            Transform hipR = FindTransformByName(bodyTop, "cf_J_LegUp00_R");
            if (hipL != null && hipR != null)
            {
                if (IsChildOf(hipL, character.transform) && IsChildOf(hipR, character.transform))
                {
                    float hipDistance = Vector3.Distance(hipL.position, hipR.position);
                    float width = hipDistance * 2.0f;
                    float depth = width * 0.6f;
                    hips = 2f * (width + depth) * 100f * scaleHips;
                }
            }

            // 计算体重
            if (height > 0 && waist > 0 && hips > 0 && bust > 0 && underbust > 0)
            {
                float bmi;
                if (waist < 60f) bmi = 18f;
                else if (waist < 70f) bmi = 19f;
                else if (waist < 80f) bmi = 20f;
                else if (waist < 90f) bmi = 22f;
                else bmi = 24f;

                float baseWeight = bmi * Mathf.Pow(height / 100f, 2);
                string cupSize = CalculateCupSize(bust, underbust);
                float cupWeight;
                switch (cupSize.Substring(cupSize.Length - 1))
                {
                    case "A": cupWeight = 0.15f; break;
                    case "B": cupWeight = 0.4f; break;
                    case "C": cupWeight = 0.6f; break;
                    case "D": cupWeight = 0.9f; break;
                    case "E": cupWeight = 1.5f; break;
                    case "F": cupWeight = 2.0f; break;
                    default: cupWeight = 2.75f; break;
                }

                float hipExcess = Mathf.Max(hips - 87f, 0f);
                float hipWeight = (hipExcess / 5f) * 0.6f;
                weight = baseWeight + cupWeight + hipWeight;
                Debug.Log($"体重计算: BMI={bmi:F1}, baseWeight={baseWeight:F2}, cupSize={cupSize}, cupWeight={cupWeight:F2}, hipWeight={hipWeight:F2}, totalWeight={weight:F2}");
            }
        }

        private bool IsChildOf(Transform child, Transform parent)
        {
            if (child == null || parent == null)
                return false;

            Transform current = child;
            while (current != null)
            {
                if (current == parent)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private string CalculateCupSize(float bust, float underbust)
        {
            int underbustRounded = (int)Mathf.Round(underbust / 5f) * 5;
            float diff = bust - underbust;
            float scaleFactor = 0.65f;
            diff *= scaleFactor;

            string cup;
            if (diff <= 12f) cup = "A";
            else if (diff <= 14f) cup = "B";
            else if (diff <= 16) cup = "C";
            else if (diff <= 18) cup = "D";
            else if (diff <= 20) cup = "E";
            else if (diff <= 22f) cup = "F";
            else cup = "G+";

            return $"{underbustRounded}{cup}";
        }

        private float PerpendicularDistance(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 AP = point - lineStart;
            Vector3 AB = lineEnd - lineStart;
            float abMagnitudeSqr = AB.sqrMagnitude;
            if (abMagnitudeSqr < 0.0001f)
            {
                Debug.LogWarning("线段长度过短，使用点到起点的距离");
                return Vector3.Distance(point, lineStart);
            }
            float t = Vector3.Dot(AP, AB) / abMagnitudeSqr;
            Vector3 projection = lineStart + t * AB;
            return Vector3.Distance(point, projection);
        }
    }
}