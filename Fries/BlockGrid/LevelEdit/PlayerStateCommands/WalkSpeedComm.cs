using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.PlayerStateCommands {
    public class WalkSpeedComm : CommandBase {
        public WalkSpeedComm() : base("walkspeed", "Walkspeed") { }

        protected override void execute(string senderId, string[] args) {
            if (!AssertArgumentLengthEquals(args, 1, out var msg)) {
                LevelEditor.writer.write(msg);
                return;
            }

            string raw = args[0];
            if (raw == "-r") {
                LevelEditor.Inst.MovementController.ResetWalkSpeed();
                LevelEditor.writer.write($"Set walkspeed to {LevelEditor.Inst.MovementController.GetWalkSpeed()}!");
                return;
            }

            if (float.TryParse(raw, out float newSpeed)) {
                LevelEditor.Inst.MovementController.SetWalkSpeed(newSpeed);
                LevelEditor.writer.write($"Set walkspeed to {newSpeed}!");
                return;
            }
            
            LevelEditor.writer.write("Invalid argument, speed must be a number!");
        }
    }
}