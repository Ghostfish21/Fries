using System.Collections.Generic;
using UnityEngine;

namespace Fries.Data.SerializableWrappers {
    public class UnityObjectList : SerializableWrapper {
        public List<Object> v;
        public override T get<T>() {
            return (T)(object)v;
        }
    }
}