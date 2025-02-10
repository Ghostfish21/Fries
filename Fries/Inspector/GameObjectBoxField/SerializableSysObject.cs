using System;

namespace Fries.Inspector.GameObjectBoxField {
    [Serializable]
    public class SerializableSysObject {
        public virtual T get<T>() {
            throw new NotImplementedException();
        }
    }
}