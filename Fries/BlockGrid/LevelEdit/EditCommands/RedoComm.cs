using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.EditCommands {
    public class RedoComm : CommandBase {
        public RedoComm() : base("/redo", "/Redo") { }
        protected override void execute(string senderId, string[] args) {
            if (LevelEditor.Inst.UndoRedoManager.RedoChanges())
                LevelEditor.writer.write("Redo completed!");
            else LevelEditor.writer.write("There is nothing to redo!");
        }
    }
}