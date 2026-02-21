using System.Collections.Generic;
using Fries.Chat;
using Fries.Data;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit.EditCommands {
    public class MoveComm : CommandBase {
        private BlockMapQuerier querier;
        private HashSet<BlockKey> queryResult1 = new();
        private HashSet<BlockKey> queryResult2 = new();
        
        public MoveComm() : base("/move", "/Move") { }
        
        protected override void execute(string senderId, string[] args) {
            if (!LevelEditor.Inst.isValid) return;
            
            querier ??= new BlockMapQuerier(LevelEditor.Inst.EverythingPool);

            bool dontCopyAir = false;
            bool moveSelection = false;
            int offset = 0;
            foreach (var argument in args) {
                if (argument == "-s") {
                    moveSelection = true;
                    continue;
                }

                if (argument == "-a") {
                    dontCopyAir = true;
                    continue;
                }
                if (int.TryParse(argument, out int offset1)) 
                    offset = offset1;
            }

            if (offset == 0) {
                LevelEditor.writer.write("Offset must be present and is a non-zero integer!");
                return;
            }

            if (!BlockSelection.AssertSelectionIsComplete(out var pos1, out var pos2))
                return;
            
            LevelEditor.Inst.UndoRedoManager.ResetMultipleChangesBuffer();
            
            LevelEditor.Inst.MarkAsDirty();
            
            queryResult1.Clear();
            queryResult2.Clear();
            
            querier.ResetParameters();
            querier.SetPositionRange(pos1, pos2);
            querier.Query(LevelEditor.Inst.BlockMap, queryResult1);
            LevelEditor.Inst.UndoRedoManager.RecordMultipleChanges((pos1, pos2), queryResult1, false);
            Schematic shouldBeMoved = LevelEditor.Inst.UndoRedoManager.CopyOfLast;

            Facing playerFacing = LevelEditor.Inst.CameraController.GetFacing();
            Vector3Int offsetVector = playerFacing.ToUnitVector() * offset;
            Vector3Int targetPos1 = pos1 + offsetVector;
            Vector3Int targetPos2 = pos2 + offsetVector;
            querier.SetPositionRange(targetPos1, targetPos2);
            querier.Query(LevelEditor.Inst.BlockMap, queryResult2);
            LevelEditor.Inst.UndoRedoManager.RecordMultipleChanges((targetPos1, targetPos2), queryResult2, false);

            LevelEditor.Inst.UndoRedoManager.FlushMultipleChangesBuffer();
            
            LevelEditor.Inst.BlockMap.RemoveBlocks(queryResult1, null);

            // 更改操作目标的起点与终点
            shouldBeMoved.pos1 = targetPos1;
            shouldBeMoved.pos2 = targetPos2;
            LevelEditor.Inst.BlockMap.OverwriteSetBlock(shouldBeMoved, null, dontCopyAir:dontCopyAir, onBlockCreation:LevelEditor.OnBlockCreation);
            
            LevelEditor.writer.write($"Successfully moved {shouldBeMoved.GetBlockCount(dontCopyAir)} blocks!");

            if (moveSelection) {
                BlockSelection.pos1 = targetPos1;
                BlockSelection.pos2 = targetPos2;
            }
        }
    }
}