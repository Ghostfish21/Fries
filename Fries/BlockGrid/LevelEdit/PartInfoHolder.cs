using System;
using Fries.CompCache;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    [TypeTag]
    public class PartInfoHolder : MonoBehaviour {
        public static bool LoadThroughPrefab = false;

        [SerializeField] internal int partId;
        [SerializeField] internal string partIdInGiveComm;
        [SerializeField] internal Vector3 blockMapLocalPos;

        private void Awake() {
            if (LoadThroughPrefab) {
                gameObject.SetActive(false);
                TaskPerformer.TaskPerformer.callOnConstruct(() => {
                    TaskPerformer.TaskPerformer.inst().scheduleTaskWhen((Action)(() => {
                        transform.position = LevelEditor.Inst.BlockMap.transform.position + blockMapLocalPos;
                        gameObject.transform.SetParent(LevelEditor.Inst.BlockMap.transform);
                        gameObject.SetActive(true);
                    }), () => LevelEditor.Inst.BlockMap.HasEverythingPool);
                });
            }
        }
        private void OnDestroy() { }
    }
}