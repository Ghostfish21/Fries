using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Fries.InputDispatch {
    public class KeyInputModule : InputModule {
        public Type deviceType { get; } = typeof(Keyboard);

        public InputControl[] controlsToListenTo {
            get {
                if (Keyboard.current == null) return null;
                InputControl[] ret = new InputControl[Keyboard.KeyCount];
                for (int j = 0; j < ret.Length; j++) {
                    var currentKey = Keyboard.current.allKeys[j];
                    ret[j] = currentKey;
                }
                return ret;
            }
        }

        public InputKind kind { get; } = InputKind.Of<KeyInputModule>();

        private bool[] rawKeyStates;
        private ulong[] consumedKeys;
        public void setup() {
            reset();
        }

        public void reset() {
            rawKeyStates = new bool[Keyboard.KeyCount];
            consumedKeys = new ulong[rawKeyStates.Length];
        }

        public void catchInput(InputControl control, InputEventPtr eventPtr) {
            if (control is not KeyControl key) return;
            bool down = InputDispatcher.f2b(key.ReadValueFromEvent(eventPtr));
            rawKeyStates[(int)key.keyCode] = down;
        }
        public void onDeviceChange(InputDevice device, InputDeviceChange change) {
            if (change is InputDeviceChange.Removed or InputDeviceChange.Disconnected or InputDeviceChange.Disabled 
                or InputDeviceChange.Added or InputDeviceChange.Reconnected or InputDeviceChange.Enabled or 
                InputDeviceChange.HardReset or InputDeviceChange.UsageChanged) {
                reset();
            }
        }

        private ulong tickVersion = 0;
        public void beginUpdate(ulong tickVersion) => this.tickVersion = tickVersion;
        public void consume(int code) => consumedKeys[code] = tickVersion;
        private bool isConsumed(int code) => consumedKeys[code] == tickVersion;

        public void requestStates(List<int> codes, Dictionary<InputId, float> heldInputs) {
            foreach (var code in codes) {
                InputId inputId = new(kind, code);
                if (isConsumed(code)) {
                    Debug.LogWarning($"Write default for {inputId} due to consumed key");
                    heldInputs[inputId] = 0;
                }
                else {
                    heldInputs[inputId] = rawKeyStates[code] ? 1 : 0;
                    Debug.LogWarning("Write raw key state for " + inputId + " : " + heldInputs[inputId] + "");
                }
            }
        }
    }
}