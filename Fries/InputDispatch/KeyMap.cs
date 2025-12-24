using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.InputDispatch {
    public static class KeyMap {
        
        private static readonly (int keycodeVal, int keyVal)[] mapping = {
            ((int)UnityEngine.KeyCode.None, (int)UnityEngine.InputSystem.Key.None),
            ((int)UnityEngine.KeyCode.Space, (int)UnityEngine.InputSystem.Key.Space),
            ((int)UnityEngine.KeyCode.Return, (int)UnityEngine.InputSystem.Key.Enter),
            ((int)UnityEngine.KeyCode.Tab, (int)UnityEngine.InputSystem.Key.Tab),
            ((int)UnityEngine.KeyCode.BackQuote, (int)UnityEngine.InputSystem.Key.Backquote),
            ((int)UnityEngine.KeyCode.Quote, (int)UnityEngine.InputSystem.Key.Quote),
            ((int)UnityEngine.KeyCode.Semicolon, (int)UnityEngine.InputSystem.Key.Semicolon),
            ((int)UnityEngine.KeyCode.Comma, (int)UnityEngine.InputSystem.Key.Comma),
            ((int)UnityEngine.KeyCode.Period, (int)UnityEngine.InputSystem.Key.Period),
            ((int)UnityEngine.KeyCode.Slash, (int)UnityEngine.InputSystem.Key.Slash),
            ((int)UnityEngine.KeyCode.Backslash, (int)UnityEngine.InputSystem.Key.Backslash),
            ((int)UnityEngine.KeyCode.LeftBracket, (int)UnityEngine.InputSystem.Key.LeftBracket),
            ((int)UnityEngine.KeyCode.RightBracket, (int)UnityEngine.InputSystem.Key.RightBracket),
            ((int)UnityEngine.KeyCode.Minus, (int)UnityEngine.InputSystem.Key.Minus),
            ((int)UnityEngine.KeyCode.Equals, (int)UnityEngine.InputSystem.Key.Equals),

            ((int)UnityEngine.KeyCode.A, (int)UnityEngine.InputSystem.Key.A),
            ((int)UnityEngine.KeyCode.B, (int)UnityEngine.InputSystem.Key.B),
            ((int)UnityEngine.KeyCode.C, (int)UnityEngine.InputSystem.Key.C),
            ((int)UnityEngine.KeyCode.D, (int)UnityEngine.InputSystem.Key.D),
            ((int)UnityEngine.KeyCode.E, (int)UnityEngine.InputSystem.Key.E),
            ((int)UnityEngine.KeyCode.F, (int)UnityEngine.InputSystem.Key.F),
            ((int)UnityEngine.KeyCode.G, (int)UnityEngine.InputSystem.Key.G),
            ((int)UnityEngine.KeyCode.H, (int)UnityEngine.InputSystem.Key.H),
            ((int)UnityEngine.KeyCode.I, (int)UnityEngine.InputSystem.Key.I),
            ((int)UnityEngine.KeyCode.J, (int)UnityEngine.InputSystem.Key.J),
            ((int)UnityEngine.KeyCode.K, (int)UnityEngine.InputSystem.Key.K),
            ((int)UnityEngine.KeyCode.L, (int)UnityEngine.InputSystem.Key.L),
            ((int)UnityEngine.KeyCode.M, (int)UnityEngine.InputSystem.Key.M),
            ((int)UnityEngine.KeyCode.N, (int)UnityEngine.InputSystem.Key.N),
            ((int)UnityEngine.KeyCode.O, (int)UnityEngine.InputSystem.Key.O),
            ((int)UnityEngine.KeyCode.P, (int)UnityEngine.InputSystem.Key.P),
            ((int)UnityEngine.KeyCode.Q, (int)UnityEngine.InputSystem.Key.Q),
            ((int)UnityEngine.KeyCode.R, (int)UnityEngine.InputSystem.Key.R),
            ((int)UnityEngine.KeyCode.S, (int)UnityEngine.InputSystem.Key.S),
            ((int)UnityEngine.KeyCode.T, (int)UnityEngine.InputSystem.Key.T),
            ((int)UnityEngine.KeyCode.U, (int)UnityEngine.InputSystem.Key.U),
            ((int)UnityEngine.KeyCode.V, (int)UnityEngine.InputSystem.Key.V),
            ((int)UnityEngine.KeyCode.W, (int)UnityEngine.InputSystem.Key.W),
            ((int)UnityEngine.KeyCode.X, (int)UnityEngine.InputSystem.Key.X),
            ((int)UnityEngine.KeyCode.Y, (int)UnityEngine.InputSystem.Key.Y),
            ((int)UnityEngine.KeyCode.Z, (int)UnityEngine.InputSystem.Key.Z),

            ((int)UnityEngine.KeyCode.Alpha0, (int)UnityEngine.InputSystem.Key.Digit0),
            ((int)UnityEngine.KeyCode.Alpha1, (int)UnityEngine.InputSystem.Key.Digit1),
            ((int)UnityEngine.KeyCode.Alpha2, (int)UnityEngine.InputSystem.Key.Digit2),
            ((int)UnityEngine.KeyCode.Alpha3, (int)UnityEngine.InputSystem.Key.Digit3),
            ((int)UnityEngine.KeyCode.Alpha4, (int)UnityEngine.InputSystem.Key.Digit4),
            ((int)UnityEngine.KeyCode.Alpha5, (int)UnityEngine.InputSystem.Key.Digit5),
            ((int)UnityEngine.KeyCode.Alpha6, (int)UnityEngine.InputSystem.Key.Digit6),
            ((int)UnityEngine.KeyCode.Alpha7, (int)UnityEngine.InputSystem.Key.Digit7),
            ((int)UnityEngine.KeyCode.Alpha8, (int)UnityEngine.InputSystem.Key.Digit8),
            ((int)UnityEngine.KeyCode.Alpha9, (int)UnityEngine.InputSystem.Key.Digit9),

            ((int)UnityEngine.KeyCode.LeftShift, (int)UnityEngine.InputSystem.Key.LeftShift),
            ((int)UnityEngine.KeyCode.RightShift, (int)UnityEngine.InputSystem.Key.RightShift),
            ((int)UnityEngine.KeyCode.LeftAlt, (int)UnityEngine.InputSystem.Key.LeftAlt),
            ((int)UnityEngine.KeyCode.RightAlt, (int)UnityEngine.InputSystem.Key.RightAlt),
            ((int)UnityEngine.KeyCode.AltGr, (int)UnityEngine.InputSystem.Key.AltGr),
            ((int)UnityEngine.KeyCode.LeftControl, (int)UnityEngine.InputSystem.Key.LeftCtrl),
            ((int)UnityEngine.KeyCode.RightControl, (int)UnityEngine.InputSystem.Key.RightCtrl),

            ((int)UnityEngine.KeyCode.LeftMeta, (int)UnityEngine.InputSystem.Key.LeftMeta),
            ((int)UnityEngine.KeyCode.RightMeta, (int)UnityEngine.InputSystem.Key.RightMeta),
            ((int)UnityEngine.KeyCode.LeftWindows, (int)UnityEngine.InputSystem.Key.LeftMeta),
            ((int)UnityEngine.KeyCode.RightWindows, (int)UnityEngine.InputSystem.Key.RightMeta),

            ((int)UnityEngine.KeyCode.Menu, (int)UnityEngine.InputSystem.Key.ContextMenu),

            ((int)UnityEngine.KeyCode.Escape, (int)UnityEngine.InputSystem.Key.Escape),
            ((int)UnityEngine.KeyCode.LeftArrow, (int)UnityEngine.InputSystem.Key.LeftArrow),
            ((int)UnityEngine.KeyCode.RightArrow, (int)UnityEngine.InputSystem.Key.RightArrow),
            ((int)UnityEngine.KeyCode.UpArrow, (int)UnityEngine.InputSystem.Key.UpArrow),
            ((int)UnityEngine.KeyCode.DownArrow, (int)UnityEngine.InputSystem.Key.DownArrow),
            ((int)UnityEngine.KeyCode.Backspace, (int)UnityEngine.InputSystem.Key.Backspace),
            ((int)UnityEngine.KeyCode.PageDown, (int)UnityEngine.InputSystem.Key.PageDown),
            ((int)UnityEngine.KeyCode.PageUp, (int)UnityEngine.InputSystem.Key.PageUp),
            ((int)UnityEngine.KeyCode.Home, (int)UnityEngine.InputSystem.Key.Home),
            ((int)UnityEngine.KeyCode.End, (int)UnityEngine.InputSystem.Key.End),
            ((int)UnityEngine.KeyCode.Insert, (int)UnityEngine.InputSystem.Key.Insert),
            ((int)UnityEngine.KeyCode.Delete, (int)UnityEngine.InputSystem.Key.Delete),

            ((int)UnityEngine.KeyCode.CapsLock, (int)UnityEngine.InputSystem.Key.CapsLock),
            ((int)UnityEngine.KeyCode.Numlock, (int)UnityEngine.InputSystem.Key.NumLock),
            ((int)UnityEngine.KeyCode.Print, (int)UnityEngine.InputSystem.Key.PrintScreen),
            ((int)UnityEngine.KeyCode.ScrollLock, (int)UnityEngine.InputSystem.Key.ScrollLock),
            ((int)UnityEngine.KeyCode.Pause, (int)UnityEngine.InputSystem.Key.Pause),

            ((int)UnityEngine.KeyCode.KeypadEnter, (int)UnityEngine.InputSystem.Key.NumpadEnter),
            ((int)UnityEngine.KeyCode.KeypadDivide, (int)UnityEngine.InputSystem.Key.NumpadDivide),
            ((int)UnityEngine.KeyCode.KeypadMultiply, (int)UnityEngine.InputSystem.Key.NumpadMultiply),
            ((int)UnityEngine.KeyCode.KeypadPlus, (int)UnityEngine.InputSystem.Key.NumpadPlus),
            ((int)UnityEngine.KeyCode.KeypadMinus, (int)UnityEngine.InputSystem.Key.NumpadMinus),
            ((int)UnityEngine.KeyCode.KeypadPeriod, (int)UnityEngine.InputSystem.Key.NumpadPeriod),
            ((int)UnityEngine.KeyCode.KeypadEquals, (int)UnityEngine.InputSystem.Key.NumpadEquals),
            ((int)UnityEngine.KeyCode.Keypad0, (int)UnityEngine.InputSystem.Key.Numpad0),
            ((int)UnityEngine.KeyCode.Keypad1, (int)UnityEngine.InputSystem.Key.Numpad1),
            ((int)UnityEngine.KeyCode.Keypad2, (int)UnityEngine.InputSystem.Key.Numpad2),
            ((int)UnityEngine.KeyCode.Keypad3, (int)UnityEngine.InputSystem.Key.Numpad3),
            ((int)UnityEngine.KeyCode.Keypad4, (int)UnityEngine.InputSystem.Key.Numpad4),
            ((int)UnityEngine.KeyCode.Keypad5, (int)UnityEngine.InputSystem.Key.Numpad5),
            ((int)UnityEngine.KeyCode.Keypad6, (int)UnityEngine.InputSystem.Key.Numpad6),
            ((int)UnityEngine.KeyCode.Keypad7, (int)UnityEngine.InputSystem.Key.Numpad7),
            ((int)UnityEngine.KeyCode.Keypad8, (int)UnityEngine.InputSystem.Key.Numpad8),
            ((int)UnityEngine.KeyCode.Keypad9, (int)UnityEngine.InputSystem.Key.Numpad9),

            ((int)UnityEngine.KeyCode.F1, (int)UnityEngine.InputSystem.Key.F1),
            ((int)UnityEngine.KeyCode.F2, (int)UnityEngine.InputSystem.Key.F2),
            ((int)UnityEngine.KeyCode.F3, (int)UnityEngine.InputSystem.Key.F3),
            ((int)UnityEngine.KeyCode.F4, (int)UnityEngine.InputSystem.Key.F4),
            ((int)UnityEngine.KeyCode.F5, (int)UnityEngine.InputSystem.Key.F5),
            ((int)UnityEngine.KeyCode.F6, (int)Key.F6),
            ((int)UnityEngine.KeyCode.F7, (int)UnityEngine.InputSystem.Key.F7),
            ((int)UnityEngine.KeyCode.F8, (int)UnityEngine.InputSystem.Key.F8),
            ((int)UnityEngine.KeyCode.F9, (int)UnityEngine.InputSystem.Key.F9),
            ((int)UnityEngine.KeyCode.F10, (int)UnityEngine.InputSystem.Key.F10),
            ((int)UnityEngine.KeyCode.F11, (int)UnityEngine.InputSystem.Key.F11),
            ((int)UnityEngine.KeyCode.F12, (int)UnityEngine.InputSystem.Key.F12),
        };

        private static Dictionary<int, int> keycode2KeyMap = new();
        private static Dictionary<int, int> key2KeycodeMap = new();
        static KeyMap() {
            foreach (var valueTuple in mapping) {
                keycode2KeyMap[valueTuple.Item1] = valueTuple.Item2;
                key2KeycodeMap[valueTuple.Item2] = valueTuple.Item1;
            }
        }

        public static Key toKey(KeyCode keyCode) => (Key) keycode2KeyMap[(int)keyCode];
        public static KeyCode toKeyCode(Key key) => (KeyCode) key2KeycodeMap[(int)key];
    }
}