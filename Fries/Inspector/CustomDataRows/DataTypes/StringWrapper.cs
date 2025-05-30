namespace Fries.Inspector.CustomDataRows {
    public class StringWrapper : Unwrapper {
        public string value = "";
        public object unwrap() {
            return value;
        }
    }
}