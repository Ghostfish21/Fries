using System;

namespace Fries.Inspector {
    [Serializable]
    public class SerializableSysObject {
        public string guid;

        public void createId() {
            if (string.IsNullOrEmpty(guid))
                guid = Guid.NewGuid().ToString();
        }
        
        public virtual T get<T>() {
            throw new NotImplementedException();
        }
        
        public virtual void set<T>(T value) {
            throw new NotImplementedException();
        }
    }
}