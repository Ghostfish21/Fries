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
            private Stack<List<Schematic>> undoStack = new();
            // THis stack stores the record groups that is ready for redo
            private Stack<List<Schematic>> redoStack = new();

            private Schematic last;
            public Schematic CopyOfLast => last.Clone();
            
            public void RecordSingleChanges((Vector3Int pos1, Vector3Int pos2) selection, ISet<BlockKey> blockDataOfTheRegion, bool isSorted = true) {
                var s = new Schematic(LevelEditor.Inst.EverythingPool, selection.pos1, selection.pos2, blockDataOfTheRegion, isSorted);
                last = s;
                
                var list = everythingPool.ActivateObject<List<Schematic>>();
                list.Add(s);
                undoStack.Push(list);

                foreach (var redoSchematicList in redoStack) 
                    everythingPool.DeactivateObject(redoSchematicList);
                redoStack.Clear();
            }

            private List<Schematic> multipleChangesBuffer = new();
            public void ResetMultipleChangesBuffer() => multipleChangesBuffer.Clear();
            public void RecordMultipleChanges((Vector3Int pos1, Vector3Int pos2) selection, ISet<BlockKey> blockDataOfTheRegion, bool isSorted = true) {
                var s = new Schematic(LevelEditor.Inst.EverythingPool, selection.pos1, selection.pos2, blockDataOfTheRegion, isSorted);
                last = s;
                multipleChangesBuffer.Add(s);
            }
            public void FlushMultipleChangesBuffer() {
                var list = everythingPool.ActivateObject<List<Schematic>>();
                list.AddRange(multipleChangesBuffer);
                multipleChangesBuffer.Clear();
                undoStack.Push(list);

                foreach (var redoSchematicList in redoStack) 
                    everythingPool.DeactivateObject(redoSchematicList);
                redoStack.Clear();
            }
            
            public bool UndoChanges() {
                if (undoStack.Count == 0) return false;
                var schematicList = undoStack.Pop();

                var redoList = everythingPool.ActivateObject<List<Schematic>>();
                foreach (var undoSchem in schematicList) {
                    var original = new ListSet<BlockKey>();
                    blockMap.OverwriteSetBlock(undoSchem, original, onBlockCreation:LevelEditor.OnBlockCreation);
                    var s = new Schematic(LevelEditor.Inst.EverythingPool, undoSchem.pos1, undoSchem.pos2, original);
                    redoList.Add(s);
                }
                everythingPool.DeactivateObject(schematicList);
                
                redoStack.Push(redoList);
                return true;
            }
    
            public bool RedoChanges() {
                if (redoStack.Count == 0) return false;
                var schematicList = redoStack.Pop();
                
                var undoList = everythingPool.ActivateObject<List<Schematic>>();
                foreach (var redoSchem in schematicList) {
                    var original = new ListSet<BlockKey>();
                    blockMap.OverwriteSetBlock(redoSchem, original, onBlockCreation:LevelEditor.OnBlockCreation);
                    var s = new Schematic(LevelEditor.Inst.EverythingPool, redoSchem.pos1, redoSchem.pos2, original);
                    undoList.Add(s);
                }
                everythingPool.DeactivateObject(schematicList);
                
                undoStack.Push(undoList);
                return true;
            }
        }
}