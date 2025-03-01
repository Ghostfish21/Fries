using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.Inspector.GameObjectBoxField {
    [Serializable]
    public class GameObjectBoxes : SerializableSysObject {
        
    }
    
    [Serializable]
    public class GameObjectBoxes<T> : GameObjectBoxes {
        [SerializeField] [HideInInspector] public List<GameObjectBox<T>> list;
    }
}