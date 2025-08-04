namespace Fries.Data.SerializableWrappers {
    public class String : SerializableWrapper {
        public string v;
        
        public override T get<T>() {
            return (T)(object)v;
        }

        public override object get() {
            return v;
        }
    }
}