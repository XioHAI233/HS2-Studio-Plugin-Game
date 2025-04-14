using BepInEx.Logging;
using UnityEngine;
using System;
using System.Reflection;
using Microsoft.Scripting.Hosting;

namespace HS2Game
{
    public class SceneManager
    {
        private ManualLogSource Logger;
        private int targetScene1 = 1; // 切换场景 1 的目标场景（默认 Scene 1）
        private int targetScene2 = 2; // 切换场景 2 的目标场景（默认 Scene 2）

        public SceneManager(ManualLogSource logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void JumpToScene(int sceneNumber)
        {
            try
            {
                // 加载 Unity.Console 程序集
                Assembly unityConsole = Assembly.Load("Unity.Console");
                if (unityConsole == null)
                {
                    Logger.LogError("无法加载 Unity.Console.dll");
                    return;
                }

                // 获取 Unity.Console.Program 类型
                Type programType = unityConsole.GetType("Unity.Console.Program");
                if (programType == null)
                {
                    Logger.LogError("无法找到 Unity.Console.Program 类型");
                    return;
                }

                // 获取 get_MainEngine 方法
                MethodInfo getMainEngineMethod = programType.GetMethod("get_MainEngine", BindingFlags.NonPublic | BindingFlags.Static);
                if (getMainEngineMethod == null)
                {
                    Logger.LogError("无法找到 get_MainEngine 方法");
                    return;
                }

                // 调用 get_MainEngine 获取 ScriptEngine
                object mainEngine = getMainEngineMethod.Invoke(null, new object[] { });
                if (mainEngine == null)
                {
                    Logger.LogError("MainEngine 为空");
                    return;
                }

                ScriptEngine scriptEngine = mainEngine as ScriptEngine;
                if (scriptEngine == null)
                {
                    Logger.LogError("MainEngine 无法转换为 ScriptEngine");
                    return;
                }

                // 创建脚本作用域
                ScriptScope scope = scriptEngine.CreateScope();
                if (scope == null)
                {
                    Logger.LogError("无法创建 ScriptScope");
                    return;
                }

                // 初始化 SSS（仅确保 _sc 存在）
                string initCode =
                    "from vngameengine import vnge_game\n" +
                    "from scenesavestate import _sc, SceneConsole\n" +
                    "\n" +
                    "try:\n" +
                    "    if _sc is None:\n" +
                    "        _sc = SceneConsole(vnge_game)\n" +
                    "        print('SSS 首次初始化，_sc 已创建')\n" +
                    "    print('SSS 当前场景数量:', len(_sc.block))\n" +
                    "except Exception as e:\n" +
                    "    print('SSS 初始化异常:', str(e))\n" +
                    "    raise\n";

                scriptEngine.Execute(initCode, scope);
                Logger.LogInfo("SSS 初始化检查完成");

                // 计算 SSS 的场景索引（0-based）
                int sceneIndex = sceneNumber - 1;
                Logger.LogInfo($"准备跳转到 Scene {sceneNumber} (SSS 索引 {sceneIndex})");

                // 执行场景跳转并加载状态
                string jumpCode =
                    "from vngameengine import vnge_game\n" +
                    "from scenesavestate import _sc\n" +
                    "\n" +
                    "try:\n" +
                    "    if len(_sc.block) == 0:\n" +
                    "        raise Exception('没有可用的场景数据')\n" +
                    "    if " + sceneIndex + " >= len(_sc.block):\n" +
                    "        raise Exception('场景索引超出范围: ' + str(" + sceneIndex + "))\n" +
                    "    _sc.cur_index = " + sceneIndex + "\n" +
                    "    _sc.loadCurrentScene()\n" +
                    "    print('成功跳转到 Scene " + sceneNumber + "，当前索引:', _sc.cur_index)\n" +
                    "except Exception as e:\n" +
                    "    print('跳转异常:', str(e))\n" +
                    "    raise\n";

                scriptEngine.Execute(jumpCode, scope);
                Logger.LogInfo($"成功跳转到 Scene {sceneNumber}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"场景跳转失败: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void SetTargetScene1(int sceneNumber)
        {
            if (sceneNumber >= 1)
            {
                targetScene1 = sceneNumber;
                Logger.LogInfo($"切换场景 1 的目标设置为 Scene {targetScene1}");
            }
            else
            {
                Logger.LogWarning("场景编号必须大于等于 1");
            }
        }

        public void SetTargetScene2(int sceneNumber)
        {
            if (sceneNumber >= 1)
            {
                targetScene2 = sceneNumber;
                Logger.LogInfo($"切换场景 2 的目标设置为 Scene {targetScene2}");
            }
            else
            {
                Logger.LogWarning("场景编号必须大于等于 1");
            }
        }

        public int GetTargetScene1()
        {
            return targetScene1;
        }

        public int GetTargetScene2()
        {
            return targetScene2;
        }
    }
}