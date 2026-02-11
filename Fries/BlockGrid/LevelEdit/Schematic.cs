using System.Collections.Generic;
using Fries.Pool;
using UnityEngine;

namespace Fries.BlockGrid.LevelEdit {
    public readonly struct Schematic {
        private readonly Vector3Int pos1;
        private readonly Vector3Int pos2;
        private readonly Dictionary<short, BlockKey> blockLookupTable;
        private readonly EverythingPool pool;
        private readonly string changes;
        
        public Schematic(EverythingPool everythingPool, Vector3Int pos1, Vector3Int pos2) {
            pool = everythingPool;
            this.pos1 = pos1;
            this.pos2 = pos2;
            blockLookupTable = everythingPool.ActivateObject<Dictionary<short, BlockKey>>();
            this.changes = "";
        }
        
        public void Free() => pool.DeactivateObject(blockLookupTable);
    }

    public readonly struct EditRecordGroup {
        internal readonly List<Schematic> records;

        public EditRecordGroup(List<Schematic> records) {
            this.records = records;
        }
        
        public void Free(EverythingPool everythingPool) {
            foreach (var record in records) record.Free();
            everythingPool.DeactivateObject(records);
        }
    }

    public class EditRecordManager {
        private readonly EverythingPool everythingPool;
        public EditRecordManager(EverythingPool everythingPool) {
            this.everythingPool = everythingPool;
        }
        
        // This stack stores the record groups that is ready for undo
        private Stack<EditRecordGroup> undoStack = new();
        // THis stack stores the record groups that is ready for redo
        private Stack<EditRecordGroup> redoStack = new();

        public void RecordChanges((Vector3Int pos1, Vector3Int pos2) selection) {
            var recordList = everythingPool.ActivateObject<List<Schematic>>();
            recordList.Add(new Schematic(LevelEditor.Inst.EverythingPool, selection.pos1, selection.pos2));
            undoStack.Push(new EditRecordGroup(recordList));

            foreach (var redoEditRecordGroup in redoStack) 
                redoEditRecordGroup.Free(everythingPool);
        }
        
        public bool UndoChanges() {
            if (undoStack.Count == 0) return false;
            var recordGroup = undoStack.Pop();
            
            // TODO Actually undo stuff
            
            redoStack.Push(recordGroup);
            return true;
        }

        public bool RedoChanges() {
            if (redoStack.Count == 0) return false;
            var recordGroup = redoStack.Pop();
            
            // TODO Actually redo stuff
            
            undoStack.Push(recordGroup);
            return true;
        }
    }
}