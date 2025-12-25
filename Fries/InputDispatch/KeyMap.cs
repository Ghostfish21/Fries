# if InputSys
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.InputDispatch {
    public static class KeyMap {
        private static readonly (int keycodeVal, int keyVal)[] mapping = {
            ((int)KeyCode.None, (int)Key.None),
            ((int)KeyCode.Space, (int)Key.Space),
            ((int)KeyCode.Return, (int)Key.Enter),
            ((int)KeyCode.Tab, (int)Key.Tab),
            ((int)KeyCode.BackQuote, (int)Key.Backquote),
            ((int)KeyCode.Quote, (int)Key.Quote),
            ((int)KeyCode.Semicolon, (int)Key.Semicolon),
            ((int)KeyCode.Comma, (int)Key.Comma),
            ((int)KeyCode.Period, (int)Key.Period),
            ((int)KeyCode.Slash, (int)Key.Slash),
            ((int)KeyCode.Backslash, (int)Key.Backslash),
            ((int)KeyCode.LeftBracket, (int)Key.LeftBracket),
            ((int)KeyCode.RightBracket, (int)Key.RightBracket),
            ((int)KeyCode.Minus, (int)Key.Minus),
            ((int)KeyCode.Equals, (int)Key.Equals),

            ((int)KeyCode.A, (int)Key.A),
            ((int)KeyCode.B, (int)Key.B),
            ((int)KeyCode.C, (int)Key.C),
            ((int)KeyCode.D, (int)Key.D),
            ((int)KeyCode.E, (int)Key.E),
            ((int)KeyCode.F, (int)Key.F),
            ((int)KeyCode.G, (int)Key.G),
            ((int)KeyCode.H, (int)Key.H),
            ((int)KeyCode.I, (int)Key.I),
            ((int)KeyCode.J, (int)Key.J),
            ((int)KeyCode.K, (int)Key.K),
            ((int)KeyCode.L, (int)Key.L),
            ((int)KeyCode.M, (int)Key.M),
            ((int)KeyCode.N, (int)Key.N),
            ((int)KeyCode.O, (int)Key.O),
            ((int)KeyCode.P, (int)Key.P),
            ((int)KeyCode.Q, (int)Key.Q),
            ((int)KeyCode.R, (int)Key.R),
            ((int)KeyCode.S, (int)Key.S),
            ((int)KeyCode.T, (int)Key.T),
            ((int)KeyCode.U, (int)Key.U),
            ((int)KeyCode.V, (int)Key.V),
            ((int)KeyCode.W, (int)Key.W),
            ((int)KeyCode.X, (int)Key.X),
            ((int)KeyCode.Y, (int)Key.Y),
            ((int)KeyCode.Z, (int)Key.Z),

            ((int)KeyCode.Alpha0, (int)Key.Digit0),
            ((int)KeyCode.Alpha1, (int)Key.Digit1),
            ((int)KeyCode.Alpha2, (int)Key.Digit2),
            ((int)KeyCode.Alpha3, (int)Key.Digit3),
            ((int)KeyCode.Alpha4, (int)Key.Digit4),
            ((int)KeyCode.Alpha5, (int)Key.Digit5),
            ((int)KeyCode.Alpha6, (int)Key.Digit6),
            ((int)KeyCode.Alpha7, (int)Key.Digit7),
            ((int)KeyCode.Alpha8, (int)Key.Digit8),
            ((int)KeyCode.Alpha9, (int)Key.Digit9),

            ((int)KeyCode.LeftShift, (int)Key.LeftShift),
            ((int)KeyCode.RightShift, (int)Key.RightShift),
            ((int)KeyCode.LeftAlt, (int)Key.LeftAlt),
            ((int)KeyCode.RightAlt, (int)Key.RightAlt),
            ((int)KeyCode.AltGr, (int)Key.AltGr),
            ((int)KeyCode.LeftControl, (int)Key.LeftCtrl),
            ((int)KeyCode.RightControl, (int)Key.RightCtrl),

            ((int)KeyCode.LeftMeta, (int)Key.LeftMeta),
            ((int)KeyCode.RightMeta, (int)Key.RightMeta),
            ((int)KeyCode.LeftWindows, (int)Key.LeftMeta),
            ((int)KeyCode.RightWindows, (int)Key.RightMeta),

            ((int)KeyCode.Menu, (int)Key.ContextMenu),

            ((int)KeyCode.Escape, (int)Key.Escape),
            ((int)KeyCode.LeftArrow, (int)Key.LeftArrow),
            ((int)KeyCode.RightArrow, (int)Key.RightArrow),
            ((int)KeyCode.UpArrow, (int)Key.UpArrow),
            ((int)KeyCode.DownArrow, (int)Key.DownArrow),
            ((int)KeyCode.Backspace, (int)Key.Backspace),
            ((int)KeyCode.PageDown, (int)Key.PageDown),
            ((int)KeyCode.PageUp, (int)Key.PageUp),
            ((int)KeyCode.Home, (int)Key.Home),
            ((int)KeyCode.End, (int)Key.End),
            ((int)KeyCode.Insert, (int)Key.Insert),
            ((int)KeyCode.Delete, (int)Key.Delete),

            ((int)KeyCode.CapsLock, (int)Key.CapsLock),
            ((int)KeyCode.Numlock, (int)Key.NumLock),
            ((int)KeyCode.Print, (int)Key.PrintScreen),
            ((int)KeyCode.ScrollLock, (int)Key.ScrollLock),
            ((int)KeyCode.Pause, (int)Key.Pause),

            ((int)KeyCode.KeypadEnter, (int)Key.NumpadEnter),
            ((int)KeyCode.KeypadDivide, (int)Key.NumpadDivide),
            ((int)KeyCode.KeypadMultiply, (int)Key.NumpadMultiply),
            ((int)KeyCode.KeypadPlus, (int)Key.NumpadPlus),
            ((int)KeyCode.KeypadMinus, (int)Key.NumpadMinus),
            ((int)KeyCode.KeypadPeriod, (int)Key.NumpadPeriod),
            ((int)KeyCode.KeypadEquals, (int)Key.NumpadEquals),
            ((int)KeyCode.Keypad0, (int)Key.Numpad0),
            ((int)KeyCode.Keypad1, (int)Key.Numpad1),
            ((int)KeyCode.Keypad2, (int)Key.Numpad2),
            ((int)KeyCode.Keypad3, (int)Key.Numpad3),
            ((int)KeyCode.Keypad4, (int)Key.Numpad4),
            ((int)KeyCode.Keypad5, (int)Key.Numpad5),
            ((int)KeyCode.Keypad6, (int)Key.Numpad6),
            ((int)KeyCode.Keypad7, (int)Key.Numpad7),
            ((int)KeyCode.Keypad8, (int)Key.Numpad8),
            ((int)KeyCode.Keypad9, (int)Key.Numpad9),

            ((int)KeyCode.F1, (int)Key.F1),
            ((int)KeyCode.F2, (int)Key.F2),
            ((int)KeyCode.F3, (int)Key.F3),
            ((int)KeyCode.F4, (int)Key.F4),
            ((int)KeyCode.F5, (int)Key.F5),
            ((int)KeyCode.F6, (int)Key.F6),
            ((int)KeyCode.F7, (int)Key.F7),
            ((int)KeyCode.F8, (int)Key.F8),
            ((int)KeyCode.F9, (int)Key.F9),
            ((int)KeyCode.F10, (int)Key.F10),
            ((int)KeyCode.F11, (int)Key.F11),
            ((int)KeyCode.F12, (int)Key.F12),
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
# endif