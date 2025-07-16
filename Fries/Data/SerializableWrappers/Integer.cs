namespace Fries.Data.SerializableWrappers {
    public class Integer : SerializableWrapper {
        public int v;
        public override T get<T>() {
            return (T)(object)v;
        }
    }
}