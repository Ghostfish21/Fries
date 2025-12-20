using System;
using System.Collections.Generic;
using System.Linq;
using Fries.EvtSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Fries.InputDispatch {
    public class InputDispatcher : MonoBehaviour, IInputStateChangeMonitor {
        # region 初始化
        // =============================================================================================================
        // 输入模块
        private Dictionary<Type, List<InputModule>> modulesByDevice = new(); // DeviceType => List<InputModule>
        private Dictionary<InputKind, InputModule> modules = new();
        public void addModule(InputModule module) {
            if (isModulesSealed) throw new InvalidOperationException("Cannot add modules after Awake!");
            if (!modules.TryAdd(module.kind, module)) return;
            Type deviceType = module.deviceType;
            modulesByDevice.TryAdd(deviceType, new List<InputModule>());
            modulesByDevice[deviceType].Add(module);
        }
        public void clearModules() {
            if (isModulesSealed) throw new InvalidOperationException("Cannot remove modules after Awake!");
            modules.Clear();
            modulesByDevice.Clear();
        }
        public void addCommonModules() {
            addModule(new KeyInputModule());
            addModule(new MouseInputModule());
        }
        private bool isModulesSealed = false;
        
        // =============================================================================================================
        // 输入层级
        private readonly List<InputLayer> layers = new();
        internal void sort() {
            layers.Clear();
            foreach (Transform child in transform) {
                InputLayer il = child.getComponent<InputLayer>();
                if (il) layers.Add(il);
            }
        }
        
        // =============================================================================================================
        // 单例
        private static InputDispatcher _inst;
        public static InputDispatcher inst => _inst;
        
        // =============================================================================================================
        private void Awake() {
            if (_inst) {
                Destroy(gameObject);
                return;
            }
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            _inst = this;

            addCommonModules();
            Evt.TriggerNonAlloc<InputEvents.BeforeInputDispatcherSetup>(this);
            isModulesSealed = true;

            foreach (var module in modules.Values) {
                inactiveInputModules.TryAdd(module.deviceType, new HashSet<InputModule>());
                inactiveInputModules[module.deviceType].Add(module);
            }

            sort();
        }
        # endregion
        
        # region 帧状态更新
        // =============================================================================================================
        // 全局消耗
        private bool isGloballyConsumed = false;
        internal void blockAll() => isGloballyConsumed = true;
        
        // 指定消耗
        internal void consume(InputId input) {
            if (modules.TryGetValue(input.kind, out var module)) module.consume(input.code);
            else Debug.LogError($"InputModule {input.kind} is not registered!");
        }

        // 帧戳
        private ulong tickVersion = 0;
        private void Update() {
            // 更新帧戳
            tickVersion++;
            if (!enabled) return;

            // 模块帧首初始化，通常是清理内部缓存等
            isGloballyConsumed = false;
            foreach (var inputModule in modules.Values) {
                try { inputModule.beginUpdate(tickVersion); }
                catch (Exception ex) {
                    Debug.LogError($"Encountered error while module {inputModule.kind.ToString()} beginUpdate: " + ex.Message + "\n" + ex.StackTrace);
                }
            }

            // 按层序遍历所有输入层
            foreach (var inputLayer in layers) {
                try {
                    if (!inputLayer.enabled) continue;
                    // 重置输入层状态 // TODO
                    inputLayer.reset();
                    // 输入层重新 Fetch 最新的输入
                    inputLayer.fetchUpdate(this);
                    // 输入层消耗输入
                    inputLayer.consume(this);
                } catch (Exception ex) {
                    Debug.LogError($"Encountered error while input layer {inputLayer.name} processing: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        private void writeDefaultStates(InputKind kind, List<int> codes, Dictionary<InputId, float> heldInputs) {
            foreach (int code in codes) {
                InputId id = new(kind, code);
                heldInputs[id] = 0;
            }
        }
        internal void requestStates(InputKind kind, List<int> codes, Dictionary<InputId, float> heldInputs) {
            // 如果获取不到，就写入虚假状态并结束
            if (!modules.TryGetValue(kind, out InputModule module)) {
                Debug.LogWarning($"Write Default due to missing module {kind.ToString()}");
                writeDefaultStates(kind, codes, heldInputs);
                return;
            }
            // 如果全局被消耗，就写入虚假状态并结束
            if (isGloballyConsumed) {
                Debug.LogWarning($"Write Default due to globally consumed");
                writeDefaultStates(kind, codes, heldInputs);
                return;
            }
            module.requestStates(codes, heldInputs);
        }
        # endregion
        
        # region 原始输入获取 与 底层输入回调
        private readonly Dictionary<InputControl, HashSet<InputModule>> control2ModuleMap = new();
        private readonly HashSet<InputModule> activeInputModules = new();
        private readonly Dictionary<Type, HashSet<InputModule>> inactiveInputModules = new();
        private void OnEnable() {
            InputSystem.onDeviceChange += onDeviceChange;

            foreach (var moduleListOrigin in inactiveInputModules.Values.ToList()) {
                foreach (var module in moduleListOrigin.ToList()) {
                    bool boostResult = tryBoostInputModule(module);

                    // 如果启动成功，将该模组加入 Active列表
                    if (boostResult) {
                        activeInputModules.Add(module);
                        moduleListOrigin.Remove(module);
                        continue;
                    }
                    
                    // 如果两者有失败的， 将其加入 Inactive列表
                    inactiveInputModules.TryAdd(module.deviceType, new HashSet<InputModule>());
                    inactiveInputModules[module.deviceType].Add(module);
                }
            }
        }

        private bool tryBoostInputModule(InputModule module) {
            if (activeInputModules.Contains(module)) 
                throw new InvalidOperationException($"Cannot boost a module twice, module {module.kind.ToString()} is already active!");
            
            // 尝试 注册Monitor 与 Setup
            bool registerFlag = registerInputMonitors(module);
            bool setupFlag = true;
            if (registerFlag) {
                try { module.setup(); }
                catch (Exception ex) {
                    Debug.LogError($"Encountered error while module {module.kind.ToString()} setup: " +
                                   ex.Message + "\n" + ex.StackTrace);
                    // 如果 Setup 不成功，反注册 Monitor
                    unregisterInputMonitors(module);
                    setupFlag = false;
                }
            }
            
            return registerFlag && setupFlag;
        }

        // ControlsToListenTo 可以在 OnEnable 时无法获取，这里需要储存一个因此而转入 Inactive 的 Modules 列表，
        // 它在每次 Device 更新时介入检查，如果存在它需要的设备，就重试注册
        private readonly Dictionary<InputModule, InputControl[]> inputControlCache = new();
        private bool registerInputMonitors(InputModule module) {
            InputControl[] controlsToListenTo = module.controlsToListenTo;
            if (controlsToListenTo == null) return false;
            inputControlCache[module] = controlsToListenTo;
            foreach (var inputControl in controlsToListenTo) {
                InputState.AddChangeMonitor(inputControl, this);
                control2ModuleMap.TryAdd(inputControl, new HashSet<InputModule>());
                control2ModuleMap[inputControl].Add(module);
            }
            return true;
        }
        private void unregisterInputMonitors(InputModule module) {
            foreach (var inputControl in inputControlCache[module]) {
                InputState.RemoveChangeMonitor(inputControl, this);
                control2ModuleMap[inputControl].Remove(module);
            }
            inputControlCache.Remove(module);
        }
        
        private void OnDisable() {
            InputSystem.onDeviceChange -= onDeviceChange;
            foreach (var module in activeInputModules.ToList()) {
                unregisterInputMonitors(module);
                try { module.reset(); }
                catch (Exception ex) {
                    Debug.LogError($"Encountered error while module {module.kind.ToString()} resetting: " + ex.Message + "\n" + ex.StackTrace);
                }
                // 将其从 Active列表 中移除，并加入 Inactive 列表
                activeInputModules.Remove(module);
                inactiveInputModules.TryAdd(module.deviceType, new HashSet<InputModule>());
                inactiveInputModules[module.deviceType].Add(module);
            }
        }

        private readonly Dictionary<Type, Type> typeCache = new();
        private Type getRegisteredType(Type lookupType) {
            if (typeCache.TryGetValue(lookupType, out var registeredType)) return registeredType;
            
            Type registeredType1 = null;
            for (Type t = lookupType; t != null && t != typeof(InputDevice); t = t.BaseType) {
                if (!modulesByDevice.ContainsKey(t)) continue;
                registeredType1 = t;
                break;
            }
            typeCache[lookupType] = registeredType1;
            return registeredType1;
        }
        private void onDeviceChange(InputDevice device, InputDeviceChange change) {
            Type registeredType = getRegisteredType(device.GetType());
            if (registeredType == null) return;
            if (!modulesByDevice.TryGetValue(registeredType, out var moduleList)) return;
            
            if (inactiveInputModules.TryGetValue(registeredType, out var inactiveModuleList)) {
                foreach (var module in inactiveModuleList.ToList()) {
                    bool boostResult = tryBoostInputModule(module);
                    if (!boostResult) continue;
                    activeInputModules.Add(module);
                    inactiveModuleList.Remove(module);
                }
            }
            
            foreach (InputModule module in moduleList) {
                if (!activeInputModules.Contains(module)) continue;
                try { module.onDeviceChange(device, change); }
                catch (Exception ex) {
                    Debug.LogError($"Encountered error while module {module.kind.ToString()} receiving device change event: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        
        public void NotifyControlStateChanged(InputControl control, double time, InputEventPtr eventPtr, long monitorIndex) {
            if (!Application.isFocused) return;
            
            if (!control2ModuleMap.TryGetValue(control, out var hashset)) return;
            foreach (var module in hashset) {
                if (!activeInputModules.Contains(module)) continue;
                try { module.catchInput(control, eventPtr); }
                catch (Exception ex) {
                    Debug.LogError($"Encountered error while module {module.kind.ToString()} receiving input event: " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        public void NotifyTimerExpired(InputControl control, double time, long monitorIndex, int timerIndex) { }
        # endregion
        
        # region 通用方法
        public static bool f2b(float f) => f > 0.5f;
        # endregion
    }
}