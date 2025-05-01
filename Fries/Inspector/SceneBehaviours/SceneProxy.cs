using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace Fries.Inspector.SceneBehaviours {
    public class SceneSelectionProxy : ScriptableObject {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void onGameStart() {
            Debug.Log(1);
            SceneBehaviour[] sbs = Resources.LoadAll<SceneBehaviour>("Scene Data");
            foreach (var sb in sbs) {
                sb.OnEnable();
            }
            SceneSelectionProxy[] ssps = Resources.LoadAll<SceneSelectionProxy>("Scene Data");
            foreach (var ssp in ssps) {
                ssp.OnEnable();
            }
        }

        public int sceneHandle;
        public string scenePath;
        public string sceneName;

        private async void OnEnable() {
            Debug.Log("SP");
            await Task.Delay(TimeSpan.FromSeconds(1));
            registerBehaviours();
            SceneSelectCatcher.registerProxy(sceneName, this);
        }

        private void registerBehaviours() {
            foreach (var id in behaviourTimeIds) {
                var v = SceneBehaviour.getSceneBehaviour(id);
                if (v == null) removeBehaviour(id, true);
                else registerBehaviour(v);
            }
        }

        public SceneSelectionProxy() {
            behaviourTimeIds = new();
        }
        
        public List<long> behaviourTimeIds;
        private readonly Dictionary<long, Type> behavioursType = new();
        // 创建时间 对 实例
        private readonly SortedDictionary<long, SceneBehaviour> behaviourInsts = new();
        public List<SceneBehaviour> getSceneBehaviours() {
            return behaviourInsts.Values.ToList();
        }
        // 类型 对 创建时间 对 实例
        private readonly Dictionary<Type, SortedDictionary<long, SceneBehaviour>> behavioursStructure = new();

        # if UNITY_EDITOR
        public void addBehaviour(Type type) {
            var sceneBehaviour = (SceneBehaviour)ScriptableObject.CreateInstance(type);
            sceneBehaviour.name = $"{scenePath.Replace('\\', '\u00a6').Replace('/', '\u00a6')}_{sceneBehaviour.createTime}";
            string assetPath = $"{SceneSelectCatcher.resourcePath}{SceneSelectCatcher.resourceFolder}/{sceneBehaviour.name}.asset";
            AssetDatabase.CreateAsset(sceneBehaviour, assetPath);
            AssetDatabase.SaveAssets();
            registerBehaviour(sceneBehaviour);
        }
        # endif
        
        private void registerBehaviour(SceneBehaviour sceneBehaviour) {
            if (!behaviourTimeIds.Contains(sceneBehaviour.createTime))
                behaviourTimeIds.Add(sceneBehaviour.createTime);
            
            behavioursType[sceneBehaviour.createTime] = sceneBehaviour.GetType();
            behaviourInsts[sceneBehaviour.createTime] = sceneBehaviour;
            if (!behavioursStructure.ContainsKey(sceneBehaviour.GetType()))
                behavioursStructure[sceneBehaviour.GetType()] = new();
            behavioursStructure[sceneBehaviour.GetType()][sceneBehaviour.createTime] = sceneBehaviour;
        }

        public bool hasBehaviour(SceneBehaviour sceneBehaviour) {
            return hasBehaviour(sceneBehaviour.createTime);
        }
        
        public void removeBehaviour(SceneBehaviour sceneBehaviour) {
            removeBehaviour(sceneBehaviour.createTime);
        }

        private bool hasBehaviour(long sceneBehaviourTimeId) {
            return behaviourTimeIds.Contains(sceneBehaviourTimeId);
        }
        
        private void removeBehaviour(long sceneBehaviourTimeId, bool suppressWarning = false) {
            if (!hasBehaviour(sceneBehaviourTimeId)) {
                if (suppressWarning) return;
                throw new KeyNotFoundException("The given scene behaviour is not found!");
            }
            
            behaviourTimeIds.Remove(sceneBehaviourTimeId);
            behaviourInsts.Remove(sceneBehaviourTimeId);
            behavioursStructure[behavioursType[sceneBehaviourTimeId]].Remove(sceneBehaviourTimeId);
            behavioursType.Remove(sceneBehaviourTimeId);
        }

        public T getBehaviour<T>() {
            if (!behavioursStructure.ContainsKey(typeof(T))) return (T)(object)null;
            if (behavioursStructure[typeof(T)].Count == 0) return (T)(object)null;
            T behave = (T)(object)null;
            List<long> toRemove = new();
            foreach (var sb in behavioursStructure[typeof(T)]) {
                if (sb.Value == null) toRemove.Add(sb.Key);
                else {
                    behave = (T)(object)sb.Value;
                    break;
                }
            }
            foreach (var time in toRemove) removeBehaviour(time);

            return behave;
        }
        
        public T[] getBehaviours<T>() {
            if (!behavioursStructure.ContainsKey(typeof(T))) return Array.Empty<T>();
            if (behavioursStructure[typeof(T)].Count == 0) return Array.Empty<T>();

            List<long> toRemove = new();
            foreach (var sb in behavioursStructure[typeof(T)]) {
                if (sb.Value == null) toRemove.Add(sb.Key);
            }
            foreach (var time in toRemove) removeBehaviour(time);

            T[] array = new T[behavioursStructure[typeof(T)].Count];
            int i = 0;
            foreach (var sb in behavioursStructure[typeof(T)].Values) {
                array[i] = (T)(object)sb;
                i++;
            }
            
            return array;
        }
    }
}