using System;
using Fries.CompCache;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    [TypeTag]
    public class PartInfoHolder : MonoBehaviour {
        public static bool IsPartPrefabImmutable = false;
        public static bool LoadThroughPrefab = false;
        public static bool WriteIntoPartMap = false;

        [SerializeField] internal int partId;
        [SerializeField] internal string partIdInGiveComm;
        [SerializeField] internal Vector3 blockMapLocalPos;
        internal int boundsId;

        private void Awake() {
            if (LoadThroughPrefab) {
                gameObject.SetActive(false);
                TaskPerformer.TaskPerformer.callOnConstruct(() => {
                    TaskPerformer.TaskPerformer.inst().scheduleTaskWhen((Action)(() => {
                        if (IsPartPrefabImmutable) {
                            transform.position = LevelEditor.Inst.BlockMap.transform.position + blockMapLocalPos;
                            gameObject.transform.SetParent(LevelEditor.Inst.BlockMap.transform);
                            gameObject.SetActive(true);

                            if (WriteIntoPartMap)
                                boundsId = LevelEditor.Inst.BlockMap.partMap.AddBounds(gameObject
                                    .GetTaggedObject<PartBounds>().CalcWorldAabb());
                        }
                        else {
                            LevelEditor.Inst.SetPart(LevelEditor.Inst.BlockMap.transform.position + blockMapLocalPos, transform.eulerAngles, partIdInGiveComm, true);
                            Destroy(gameObject);
                        }
                    }), () => LevelEditor.Inst.BlockMap.HasEverythingPool);
                });
            }
        }
        private void OnDestroy() { }
    }
}