using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.PlayerStateCommands {
    public class GamemodeComm : CommandBase {
        public GamemodeComm() : base("gamemode", "Gamemode") {
        }

        protected override void execute(string senderId, string[] args) {
            if (!AssertArgumentLengthEquals(args, 1, out var errMsg)) {
                LevelEditor.writer.write(errMsg);
                return;
            }

            if (!int.TryParse(args[0], out int gamemode)) {
                LevelEditor.writer.write("Gamemode must be 0 (Walk) / 1 (Fly) / 3 (Spectator)!");
                return;
            }
            
            if (gamemode != 0 && gamemode != 1 && gamemode != 3) {
                LevelEditor.writer.write("Gamemode must be 0 (Walk) / 1 (Fly) / 3 (Spectator)!");
                return;
            }
            
            if (gamemode == 0) 
                LevelEditor.Inst.MovementController.SetGamemode(SimpleMovementController.SURVIVAL);
            else if (gamemode == 1)
                LevelEditor.Inst.MovementController.SetGamemode(SimpleMovementController.CREATIVE);
            else if (gamemode == 3)
                LevelEditor.Inst.MovementController.SetGamemode(SimpleMovementController.SPECTATOR);
        }
    }
}