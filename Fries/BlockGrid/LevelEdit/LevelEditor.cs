using Fries.BlockGrid.LevelEdit.EditCommands;
using Fries.BlockGrid.LevelEdit.PlayerStateCommands;
using Fries.Chat;
using Fries.EvtSystem;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class LevelEditor : MonoBehaviour {
        [SerializeField] internal string saveName;
        [SerializeField] internal GameObject levelSave;
        
        internal UndoRedoManager UndoRedoManager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize() {
            writer = null;
        }
        
        [SerializeField] internal EverythingPool EverythingPool;
        [SerializeField] internal PlayerBackpack PlayerBackpack;
        [SerializeField] internal SimpleCameraController CameraController;
        [SerializeField] internal SimpleMovementController MovementController;
        [SerializeField] internal BlockMap BlockMap;
        [SerializeField] internal CrosshairDisplayer CrosshairDisplayer;
        [SerializeField] internal BlockInteractionController BlockInteractionController;

        public bool isValid { get; private set; } = true;
        private void Awake() {
            if (!levelSave && string.IsNullOrEmpty(saveName)) {
                isValid = false;
                Debug.LogError("Please enter the level name, or assign an existing level's save!");
                return;
            }

            if (!Inst) Inst = this;
            else {
                Destroy(Inst);
                return;
            }
            
            new Pos1Comm();
            new Pos2Comm();
            new SetComm();
            new GiveComm();
            new SpeedComm();
            
            BlockMap.everythingPool = EverythingPool;
            UndoRedoManager = new UndoRedoManager(EverythingPool, BlockMap);
            MovementController?.ChangeDefaultSpeed(BlockMap.UnitLength * 10);

            if (!levelSave) return;
            BlockInfoHolder.LoadThroughPrefab = true;
            Instantiate(levelSave);
            BlockInfoHolder.LoadThroughPrefab = false;
            if (!levelSave.name.Contains('-'))
                saveName = UnityEngine.Random.Range(0, 1000000000).ToString();
            else saveName = levelSave.name.Split("-")[0];
        }

        internal static LevelEditor Inst;
        internal static ChatCore.Writer writer;
        [EvtListener(typeof(ChatCore.OnInitiated))]
        private static void initWriter() {
            writer = ChatCore.create("Block Level Editor");
        }

        private bool isSaved = false;
        private void OnApplicationQuit() {
            if (isSaved) return;
            LevelSaver.Save(BlockMap.gameObject, "Level Editor", saveName);
            isSaved = true;
        }
        private void OnDestroy() {
            if (isSaved) return;
            LevelSaver.Save(BlockMap.gameObject, "Level Editor", saveName);
            isSaved = true;
        }
    }
}