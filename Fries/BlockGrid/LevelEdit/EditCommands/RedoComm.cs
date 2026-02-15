using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.EditCommands {
    public class RedoComm : CommandBase {
        public RedoComm() : base("/redo", "/Redo") { }
        protected override void execute(string senderId, string[] args) {
            LevelEditor.Inst.UndoRedoManager.RedoChanges();
        }
    }
}