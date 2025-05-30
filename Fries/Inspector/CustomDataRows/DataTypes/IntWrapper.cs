namespace Fries.Inspector.CustomDataRows {
    public class IntWrapper : Unwrapper {
        public int value = 0;
        public object unwrap() {
            return value;
        }
    }
}