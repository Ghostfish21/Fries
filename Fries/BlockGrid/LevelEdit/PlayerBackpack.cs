using System.Collections.Generic;
using Fries.InputDispatch;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.BlockGrid.LevelEdit {
    public class PlayerBackpack : MonoBehaviour {
        private List<object> enums = new();
        [SerializeField] private List<string> items = new();

        private int cursor = 0;

        private void changeCursor(int newCursor) {
            int old = cursor;
            cursor = newCursor;
            items[old] = getItemTitle(old);
            items[cursor] = ">>> " + getItemTitle(cursor);
        }

        public bool IsItemABlock(object item) => item is not ITool;

        public bool IsItemOnHandAPart(out string stringPartId) {
            stringPartId = null;
            if (!PlayerPrefs.HasKey(cursor + "")) return false;
            string s = PlayerPrefs.GetString(cursor + "");
            if (!(s.StartsWith("p") || s.StartsWith("P"))) return false;
            s = s.Substring(1);
            
            if (int.TryParse(s, out _)) {
                stringPartId = s;
                return true;
            }
            return false;
        }
        public object GetItemOnHand() => enums[cursor];
        public object GetBlock(int index) => enums[index];

        public bool IsHandEmpty() => enums[cursor] == null;
        public bool FindNextAvailableSlot(out int slotIndex) {
            slotIndex = -1;
            int i = -1;
            foreach (object obj in enums) {
                i++;
                if (obj != null) continue;
                slotIndex = i;
                return true;
            }
            return false;
        }
        
        private InputLayer gameplay;
        private InputId n1;
        private InputId n2;
        private InputId n3;
        private InputId n4;
        private InputId n5;
        private InputId n6;
        private InputId n7;
        private InputId n8;
        private InputId n9;
        private InputId Q;
        private InputId Control;
        private InputId S;
        
        private void Awake() {
            gameplay = InputLayer.get("Gameplay");
            n1 = Key.Digit1;
            n2 = Key.Digit2;
            n3 = Key.Digit3;
            n4 = Key.Digit4;
            n5 = Key.Digit5;
            n6 = Key.Digit6;
            n7 = Key.Digit7;
            n8 = Key.Digit8;
            n9 = Key.Digit9;
            Q = Key.J;
            Control = Key.LeftShift;
            S = Key.S;
            
            while (enums.Count < 9) enums.Add(null);
            while (items.Count < 9) items.Add("");
        }

        private void Start() {
            for (int i = 0; i < 9; i++) {
                if (!PlayerPrefs.HasKey(i + "")) continue;
                string id = PlayerPrefs.GetString(i + "");
                PlayerPrefs.DeleteKey(i + "");
                LevelEditor.writer.write($"//give {id} 0");
            }
        }

        private void Reset() => OnValidate();

        private void OnValidate() {
            while (enums.Count < 9) enums.Add(null);
            while (items.Count < 9) items.Add("");
            for (int i = 0; i < 9; i++) {
                if (i == cursor) items[i] = ">>> " + getItemTitle(i);
                else items[i] = getItemTitle(i);
            }
        }

        private void Update() {
            if (gameplay.isDown(n1))
                changeCursor(0);
            else if (gameplay.isDown(n2))
                changeCursor(1);
            else if (gameplay.isDown(n3))
                changeCursor(2);
            else if (gameplay.isDown(n4))
                changeCursor(3);
            else if (gameplay.isDown(n5))
                changeCursor(4);
            else if (gameplay.isDown(n6))
                changeCursor(5);
            else if (gameplay.isDown(n7))
                changeCursor(6);
            else if (gameplay.isDown(n8))
                changeCursor(7);
            else if (gameplay.isDown(n9))
                changeCursor(8);

            if (gameplay.isDown(Q)) {
                setItem(cursor, null);
                PlayerPrefs.DeleteKey(cursor + "");
            }

            if (gameplay.isHeld(Control) && gameplay.isDown(S)) 
                LevelEditor.Inst.Save();
        }

        private string getItemTitle(int at) {
            if (enums[at] == null) return "";
            return enums[at].ToString();
        }
        
        private void setItem(int at, object item) {
            enums[at] = item;
            if (at == cursor) 
                items[at] = ">>> " + getItemTitle(at);
            else items[at] = getItemTitle(at);
        }
        public void GiveItem(string id, object enumObj, bool shouldPrint) {
            if (IsHandEmpty()) {
                setItem(cursor, enumObj);
                PlayerPrefs.SetString(cursor + "", id);
                if (shouldPrint) LevelEditor.writer.write($"Gave item {enumObj} to backpack!");
            }
            else if (FindNextAvailableSlot(out int cursor1)) {
                setItem(cursor1, enumObj);
                PlayerPrefs.SetString(cursor1 + "", id);
                changeCursor(cursor1);
                if (shouldPrint) LevelEditor.writer.write($"Gave item {enumObj} to backpack!");
            }
            else LevelEditor.writer.write("Cannot receive item! Backpack is full!");
        }
    }
}