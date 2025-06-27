# if DOTWEEN
using System;
using DG.Tweening;

namespace Fries.Inspector.CustomDataRows {
    public class TweenCustomData : CustomDataType {
        
        public string getDisplayName() {
            return "Tween";
        }

        public Type getType() {
            return typeof(Tween);
        }

        public object getDefaultValue() {
            return DOVirtual.DelayedCall(0f, () => { });
        }
    }
}
# endif