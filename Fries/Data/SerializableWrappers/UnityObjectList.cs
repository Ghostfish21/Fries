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
        public static implicit operator List<Object>(UnityObjectList wrapper) => wrapper?.v;
    }
}