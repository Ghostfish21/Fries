using System;

namespace Fries.Data.SerializableWrappers {
    [Serializable]
    public abstract class SerializableWrapper {
        public abstract T get<T>();
        public abstract object get();
    }
}