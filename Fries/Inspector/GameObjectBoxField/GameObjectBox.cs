using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Fries.Inspector.GameObjectBoxField {

    [Serializable]
    public class GameObjectBox : SerializableSysObject {
        // 这里仅作为基类，实际使用的是 GameObjectBox<T>
        public Object unityObj;
        [SerializeReference] public SerializableSysObject sysObj;
    }

    [Serializable]
    public class GameObjectBox<T> : GameObjectBox {
        
    }

}