using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fries {
    public static class ComponentCache {
        private static Dictionary<GameObject, Dictionary<Type, Component[]>> cache = new();
        
        public static T getComponent<T>(this GameObject gameObject) where T : Component {
            if (!cache.ContainsKey(gameObject)) cache[gameObject] = new Dictionary<Type, Component[]>();
            if (cache[gameObject].ContainsKey(typeof(T)) && !cache[gameObject][typeof(T)][0]) 
                cache[gameObject].Remove(typeof(T));
            if (!cache[gameObject].ContainsKey(typeof(T))) {
                T[] comps = gameObject.GetComponents<T>();
                if (comps == null || comps.Length == 0) return null;
                Component[] compsRaw = comps.Select(element => (Component)element).ToArray();
                cache[gameObject][typeof(T)] = compsRaw;
            }

            return (T)cache[gameObject][typeof(T)][0];
        }
        
        public static T getComponent<T>(this Transform transform) where T : Component {
            return getComponent<T>(transform.gameObject);
        }
        
        public static T getComponent<T>(this Component component) where T : Component {
            return getComponent<T>(component.gameObject);
        }
        
        public static T[] getComponents<T>(this GameObject gameObject, bool refresh = false) where T : Component {
            if (refresh) free(gameObject, typeof(T));
            
            T comp = getComponent<T>(gameObject);
            if (comp == null) return null;
            return (T[])cache[gameObject][typeof(T)];
        }
        
        public static T[] getComponents<T>(this Transform transform, bool refresh = false) where T : Component {
            return getComponents<T>(transform.gameObject, refresh);
        }
        
        public static T[] getComponents<T>(this Component component, bool refresh = false) where T : Component {
            return getComponents<T>(component.gameObject, refresh);
        }

        public static void free(GameObject gameObject, Type type = null) {
            if (type == null) {
                if (cache.ContainsKey(gameObject)) {
                    cache.Remove(gameObject);
                }
            }
            else {
                if (cache.ContainsKey(gameObject) && cache[gameObject].ContainsKey(type)) {
                    cache[gameObject].Remove(type);
                }
            }
        }
    }
}