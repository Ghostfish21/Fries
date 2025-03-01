using UnityEngine;
using System.Collections.Generic;
using System;

namespace Fries.Inspector {
    
    [Serializable]
    public class RefUnion {
        public List<string> inputTypes;
        public MonoBehaviour target;
        public List<SerializableSysObject> inputFieldAnchorInsts;
        
        public RefUnion(MonoBehaviour target, params (string, SerializableSysObject)[] fieldAnchorTypeAndInsts) {
            this.target = target;
            inputTypes = new();
            inputFieldAnchorInsts = new();
            foreach (var fai in fieldAnchorTypeAndInsts) {
                if (fai.Item2 == null)
                    throw new ArgumentException("Field Anchor Inst can not be null!");
                if (string.IsNullOrEmpty(fai.Item1))
                    throw new ArgumentException("Type of Field Anchor Inst can not be null or empty");
                
                fai.Item2.createId();
                inputTypes.Add(fai.Item1);
                inputFieldAnchorInsts.Add(fai.Item2);
            }
        }

        public int selectedIndex;

        public string getSelectedType() {
            return inputTypes[selectedIndex];
        }

        public object getValue() {
            return inputFieldAnchorInsts[selectedIndex];
        }
    }
}