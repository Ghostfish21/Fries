using System.Collections.Generic;
using Fries.Data;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public class UndoRedoManager {
            private readonly EverythingPool everythingPool;
            private readonly BlockMap blockMap;
            public UndoRedoManager(EverythingPool everythingPool, BlockMap blockMap) {
                this.everythingPool = everythingPool;
                this.blockMap = blockMap;
            }
            
            // This stack stores the record groups that is ready for undo
            private Stack<Schematic> undoStack = new();
            // THis stack stores the record groups that is ready for redo
            private Stack<Schematic> redoStack = new();
    
            public void RecordChanges((Vector3Int pos1, Vector3Int pos2) selection, ISet<BlockKey> blockDataOfTheRegion, bool isSorted = true) {
                var s = new Schematic(LevelEditor.Inst.EverythingPool, selection.pos1, selection.pos2, blockDataOfTheRegion, isSorted);
                undoStack.Push(s);
                redoStack.Clear();
            }
            
            public bool UndoChanges() {
                if (undoStack.Count == 0) return false;
                var schematic = undoStack.Pop();

                var original = new ListSet<BlockKey>();
                blockMap.OverwriteSetBlock(schematic, original);
                var s = new Schematic(LevelEditor.Inst.EverythingPool, schematic.pos1, schematic.pos2, original);

                redoStack.Push(s);
                return true;
            }
    
            public bool RedoChanges() {
                if (redoStack.Count == 0) return false;
                var schematic = redoStack.Pop();
                
                var original = new ListSet<BlockKey>();
                blockMap.OverwriteSetBlock(schematic, original);
                var s = new Schematic(LevelEditor.Inst.EverythingPool, schematic.pos1, schematic.pos2, original);
                
                undoStack.Push(s);
                return true;
            }
        }
}