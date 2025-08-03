namespace Fries.Data.SerializableWrappers {
    public class Float : SerializableWrapper {
        public float v;
        public override T get<T>() {
            return (T)(object)v;
        }

        public override object get() {
            return v;
        }
    }
}