using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class LevelProperty : MonoBehaviour {
        [SerializeField] internal bool writePartsIntoPartMap;
        [SerializeField] internal bool writeBlocksIntoPartMap;
    }
}