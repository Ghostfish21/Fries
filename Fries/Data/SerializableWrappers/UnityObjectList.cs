using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Fries.Data.SerializableWrappers {
    [Serializable]
    public class UnityObjectList : SerializableWrapper {
        public List<Object> v;
        public override T get<T>() {
            return (T)(object)v;
        }
    }
}