using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.EditCommands {
    public class UndoComm : CommandBase {
        public UndoComm() : base("/undo", "/Undo") { }
        protected override void execute(string senderId, string[] args) {
            LevelEditor.Inst.UndoRedoManager.UndoChanges();
        }
    }
}