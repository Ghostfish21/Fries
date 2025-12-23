# if InputSys
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Fries.InputDispatch {
    public enum MouseButton : byte {
        Left = 0, 
        Right = 1, 
        Middle = 2, 
        Forward = 3, 
        Back = 4
    }
    public class MouseInputModule : InputModule {
        public Type deviceType { get; } = typeof(Mouse);
        
        public InputControl[] controlsToListenTo {
            get {
                if (Mouse.current == null) return null;
                InputControl[] ret = new InputControl[5];
                ret[(int)MouseButton.Left] = Mouse.current.leftButton;
                ret[(int)MouseButton.Right] = Mouse.current.rightButton;
                ret[(int)MouseButton.Middle] = Mouse.current.middleButton;
                ret[(int)MouseButton.Forward] = Mouse.current.forwardButton;
                ret[(int)MouseButton.Back] = Mouse.current.backButton;
                return ret;
            }
        }

        public InputKind kind { get; } = InputKind.Of<MouseInputModule>();

        private bool[] rawMouseButtonStates;
        private ulong[] consumedMouseButtons;
        public void setup() {
            reset();
        }

        public void reset() {
            rawMouseButtonStates = new bool[5];
            consumedMouseButtons = new ulong[rawMouseButtonStates.Length];
        }

        public void catchInput(InputControl control, InputEventPtr eventPtr) {
            if (control.device is not Mouse) return;
            if (Mouse.current == null) return;
            ButtonControl buttonControl = control as ButtonControl;
            bool down = InputDispatcher.f2b(buttonControl.ReadValueFromEvent(eventPtr));
            if (buttonControl == Mouse.current.leftButton) 
                rawMouseButtonStates[(int)MouseButton.Left] = down;
            else if (buttonControl == Mouse.current.rightButton) 
                rawMouseButtonStates[(int)MouseButton.Right] = down;
            else if (buttonControl == Mouse.current.middleButton) 
                rawMouseButtonStates[(int)MouseButton.Middle] = down;
            else if (buttonControl == Mouse.current.forwardButton) 
                rawMouseButtonStates[(int)MouseButton.Forward] = down;
            else if (buttonControl == Mouse.current.backButton) 
                rawMouseButtonStates[(int)MouseButton.Back] = down;
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
        public void consume(int code) => consumedMouseButtons[code] = tickVersion;
        private bool isConsumed(int code) => consumedMouseButtons[code] == tickVersion;

        public void requestStates(List<int> codes, Dictionary<InputId, float> heldInputs) {
            foreach (var code in codes) {
                InputId inputId = new(kind, code);
                if (isConsumed(code)) {
                    Debug.LogWarning($"Write default for {inputId} due to consumed key");
                    heldInputs[inputId] = 0;
                }
                else {
                    heldInputs[inputId] = rawMouseButtonStates[code] ? 1 : 0;
                    Debug.LogWarning("Write raw key state for " + inputId + " : " + heldInputs[inputId] + "");
                }
            }
        }
    }
}
# endif