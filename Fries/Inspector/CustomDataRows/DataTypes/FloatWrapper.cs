namespace Fries.Inspector.CustomDataRows {
    public class FloatWrapper : Unwrapper {
        public float value = 0f;
        public object unwrap() {
            return value;
        }
    }
}