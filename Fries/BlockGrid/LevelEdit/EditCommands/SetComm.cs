using System.Collections.Generic;
using Fries.Chat;
using Fries.CompCache;
using Fries.Data;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit.EditCommands {
    public class SetComm : CommandBase {
        private BlockMapQuerier querier;
        private HashSet<BlockKey> queryResult = new();

        public SetComm() : base("/set", "/Set") { }
        protected override void execute(string senderId, string[] args) {
            // TODO Make chanceStr set in the next version
            if (!LevelEditor.Inst.isValid) return;

            querier ??= new BlockMapQuerier(LevelEditor.Inst.EverythingPool);
            
            bool overwriteSet = false;
            foreach (string arg in args) {
                if (arg == "-os") overwriteSet = true;
            }
            bool removeBlock = false;
            foreach (string arg in args) {
                if (arg == "-0") removeBlock = true;
            }
            
            if (!BlockSelection.AssertSelectionIsComplete(out var pos1, out var pos2))
                return;
            
            querier.ResetParameters();
            querier.SetPositionRange(pos1, pos2);
            querier.Query(LevelEditor.Inst.BlockMap, queryResult);
            if (removeBlock) {
                LevelEditor.Inst.MarkAsDirty();
                LevelEditor.Inst.UndoRedoManager.RecordSingleChanges((pos1, pos2), queryResult, false);
                int affected = removeBlocks();
                LevelEditor.writer.write($"Operation completed ({affected})");
                return;
            }
            
            object blockType = LevelEditor.Inst.PlayerBackpack.GetBlockOnHand();
            if (!LevelEditor.Inst.PlayerBackpack.IsItemABlock(blockType)) {
                LevelEditor.writer.write("Cannot set a non-block item to the selection!");
                return;
            }
            
            LevelEditor.Inst.MarkAsDirty();
            if (overwriteSet) removeBlocks();
            
            LevelEditor.Inst.CameraController.transform.GetFacing(out Facing horizontal);
            
            LevelEditor.Inst.UndoRedoManager.RecordSingleChanges((pos1, pos2), queryResult, false);
            LevelEditor.Inst.BlockMap.SetBlock(pos1, pos2, blockType, direction:horizontal,
                onBlockCreation:LevelEditor.OnBlockCreation);
            
            LevelEditor.writer.write($"Operation completed ({BlockSelection.GetSelectionSize(pos1, pos2, out _, out _, out _)})");
        }

        private int removeBlocks() {
            return LevelEditor.Inst.BlockMap.RemoveBlocks(queryResult, null);
        }
    }
}