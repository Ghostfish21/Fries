using Fries.Chat;
using Fries.CompCache;
using Fries.Data;

namespace Fries.BlockGrid.LevelEdit {
    public class SetComm : CommandBase {
        public SetComm() : base("/set", "/Set") { }
        protected override void execute(string senderId, string[] args) {
            // TODO Make chanceStr set in the next version
            if (!LevelEditor.Inst.isValid) return;
            
            object blockType = LevelEditor.Inst.PlayerBackpack.GetBlockOnHand();
            if (!BlockSelection.AssertSelectionIsComplete(out var pos1, out var pos2))
                return;

            LevelEditor.Inst.CameraController.transform.GetFacing(out Facing horizontal);
            LevelEditor.Inst.BlockMap.SetBlock(pos1, pos2, blockType, direction:horizontal,
                onBlockCreation:static (gobj, blkKey) => {
                    var bih = gobj.GetTaggedObject<BlockInfoHolder>();
                    if (!bih) bih = gobj.AddComponent<BlockInfoHolder>();
                    bih.blockKey = blkKey;
                });
            LevelEditor.EditRecordManager.RecordChanges((pos1, pos2));
        }
    }
}