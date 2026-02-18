using Fries.BlockGrid.LevelEdit.EditCommands;
using Fries.BlockGrid.LevelEdit.PlayerStateCommands;
using Fries.Chat;
using Fries.CompCache;
using Fries.Data;
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
        [SerializeField] internal LockDisplayer LockDisplayer;
        [SerializeField] internal BlockInteractionController BlockInteractionController;
        
        internal PartModelCache PartModelCache { get; private set; } = new();
        
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
            new UndoComm();
            new RedoComm();
            new MoveComm();
            
            BlockMap.everythingPool = EverythingPool;
            UndoRedoManager = new UndoRedoManager(EverythingPool, BlockMap);
            MovementController?.ChangeDefaultSpeed(BlockMap.UnitLength * 6.5f);
            BlockInteractionController?.SetArmReachLength(BlockMap.UnitLength * 5.5f);

            if (!levelSave) return;
            PartInfoHolder.LoadThroughPrefab = true;
            BlockInfoHolder.LoadThroughPrefab = true;
            Instantiate(levelSave);
            BlockInfoHolder.LoadThroughPrefab = false;
            PartInfoHolder.LoadThroughPrefab = false;
            if (!levelSave.name.Contains('-'))
                saveName = Random.Range(0, 1000000000).ToString();
            else saveName = levelSave.name.Split("-")[0];
        }

        internal static LevelEditor Inst;
        internal static ChatCore.Writer writer;
        [EvtListener(typeof(ChatCore.OnInitiated))]
        private static void initWriter() {
            writer = ChatCore.create("Block Level Editor");
        }

        private bool isSaved = false;
        private bool forceSavedOnExit = false;
        public void MarkAsDirty() => isSaved = false;
        private void OnApplicationQuit() {
            if (forceSavedOnExit) return;
            Save(true);
            forceSavedOnExit = true;
        }
        private void OnDestroy() {
            if (forceSavedOnExit) return;
            Save(true);
            forceSavedOnExit = true;
        }
        
        public static void OnBlockCreation(GameObject gobj, BlockKey blkKey) {
            var bih = gobj.GetTaggedObject<BlockInfoHolder>();
            if (!bih) bih = gobj.AddComponent<BlockInfoHolder>();
            bih.blockKey = blkKey;
        }

        public void Save(bool forceSave = false) {
            if (!forceSave && isSaved) return;
            LevelSaver.Save(BlockMap.gameObject, "Level Editor", saveName, forceSave);
            isSaved = true;
        }
        
        public void SetPart(Vector3 at, string partIdInGiveComm) {
            GameObject part = PartModelCache.Activate(
                PlayerBackpack.GetItemOnHand(),
                out GameObject prefab, out int partId);
            
            Transform camT = CameraController.transform;
            Quaternion playerYaw = Quaternion.Euler(0f, camT.eulerAngles.y, 0f);
            Quaternion prefabRot = prefab ? prefab.transform.localRotation : Quaternion.identity;
            Quaternion finalRot = playerYaw * prefabRot;

            Facing playerFacing = camT.GetFacing(out _);

            PartBounds pb = part.GetTaggedObject<PartBounds>();
            Vector3 localAnchor = Vector3.zero;
            localAnchor = pb.GetFaceCenterLocal(playerFacing);

            part.transform.SetPositionAndRotation(at, finalRot);

            Vector3 worldAnchor = part.transform.TransformPoint(localAnchor);
            Vector3 offset = at - worldAnchor;
            part.transform.position += offset;
            part.transform.SetParent(BlockMap.transform);

            var pih = part.GetTaggedObject<PartInfoHolder>();
            pih.partId = partId;
            pih.partIdInGiveComm = partIdInGiveComm;
            pih.blockMapLocalPos = part.transform.position - BlockMap.transform.position;
            
            MarkAsDirty();
        }
    }
}