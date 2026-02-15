using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.EditCommands {
    public class UndoComm : CommandBase {
        public UndoComm() : base("/undo", "/Undo") { }
        protected override void execute(string senderId, string[] args) {
            LevelEditor.Inst.MarkAsDirty();
            if (LevelEditor.Inst.UndoRedoManager.UndoChanges())
                LevelEditor.writer.write("Undo completed!");
            else LevelEditor.writer.write("There is nothing to undo!");
        }
    }
}