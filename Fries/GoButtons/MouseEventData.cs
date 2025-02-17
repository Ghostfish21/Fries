using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fries.GoButtons {
    [Flags]
    public enum ButtonType {
        None   = 0,
        Left   = 1 << 0,  // 1
        Middle = 1 << 1,  // 2
        Right  = 1 << 2   // 4
    }
    
    public struct MouseEventData {
        public bool isInvalid;
        public Vector2 position;
        public ButtonType button;
        public int index;
        
        public MouseEventData(MouseEventData med) {
            this.position = med.position;
            this.button = med.button;
            this.index = med.index;
            this.isInvalid = false;
        }

        public static implicit operator MouseEventData(PointerEventData ped) {
            if (ped == null)
                return new MouseEventData {isInvalid = true};
            
            return new MouseEventData {
                position = ped.position,
                button = convert(ped.button)
            };
        }
        
        public bool hasButton(ButtonType obj, ButtonType subject) {
            return (obj & subject) == subject;
        }

        private static ButtonType convert(PointerEventData.InputButton ib) {
            return ib switch {
                PointerEventData.InputButton.Left => ButtonType.Left,
                PointerEventData.InputButton.Right => ButtonType.Right,
                PointerEventData.InputButton.Middle => ButtonType.Middle,
                _ => ButtonType.None
            };
        }
    }
}