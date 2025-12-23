using System;
using System.Collections.Generic;
using Fries.EvtSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Fries.InputDispatch {
    public class KeyboardAxisInputModule : InputModule {
        public Type deviceType { get; } = typeof(Keyboard);

        public InputControl[] controlsToListenTo => Array.Empty<InputControl>();

        public InputKind kind { get; } = InputKind.Of<KeyboardAxisInputModule>();
        
        private readonly Dictionary<int, string> inputId2AxisName = new();
        private readonly Dictionary<string, int> inputAxisName2Id = new();
        private bool isSealed = false;
        public bool addAxis(int code, string axisName) {
            if (isSealed) throw new InvalidOperationException("Can't add axis after setup! Please register to event BeforeKeyboardAxisSetup!");
            if (!inputId2AxisName.TryAdd(code, axisName)) return false;
            inputAxisName2Id[axisName] = code;
            return true;
        }
        public int getAxisCode(string axisName) {
            if (inputAxisName2Id.TryGetValue(axisName, out var code)) return code;
            throw new InvalidOperationException("Axis " + axisName + " is not registered!");
        }
        
        private readonly Dictionary<string, float> rawInputs = new(); 
        private bool isSettedup = false;
        private ulong[] consumedInputs;

        public void setup() {
            if (isSettedup) return; 
            isSettedup = true;
            addAxis(0, "Horizontal");
            addAxis(1, "Vertical");
            Evt.TriggerNonAlloc<InputEvents.BeforeKeyboardAxisSetup>(this);
            isSealed = true;
            
            rawInputs.Clear();
            consumedInputs = new ulong[inputId2AxisName.Count];
        }

        public void reset() => rawInputs.Clear();
        public void catchInput(InputControl control, InputEventPtr eventPtr) { }
        public void onDeviceChange(InputDevice device, InputDeviceChange change) {
            if (change is InputDeviceChange.Removed or InputDeviceChange.Disconnected or InputDeviceChange.Disabled 
                or InputDeviceChange.Added or InputDeviceChange.Reconnected or InputDeviceChange.Enabled or 
                InputDeviceChange.HardReset or InputDeviceChange.UsageChanged) {
                reset();
            }
        }

        private ulong tickVersion = 0;
        public void beginUpdate(ulong tickVersion) {
            this.tickVersion = tickVersion;
            foreach (var (code, axisName) in inputId2AxisName) 
                rawInputs[axisName] = Input.GetAxis(axisName);
        }

        public void consume(int code) => consumedInputs[code] = tickVersion;
        private bool isConsumed(int code) => consumedInputs[code] == tickVersion;

        public void requestStates(List<int> codes, Dictionary<InputId, float> heldInputs) {
            foreach (var code in codes) {
                InputId inputId = new(kind, code);
                if (isConsumed(code)) {
                    Debug.LogWarning($"Write default for {inputId} due to consumed key");
                    heldInputs[inputId] = 0;
                }
                else {
                    string axisName = inputId2AxisName[code];
                    heldInputs[inputId] = rawInputs[axisName];
                    Debug.LogWarning("Write raw key state for " + inputId + " : " + heldInputs[inputId] + "");
                }
            }
        }
        
        public static InputId get(int id) => new(InputKind.Of<KeyboardAxisInputModule>(), id);
    }
}