using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.PlayerStateCommands {
    public class SpeedComm : CommandBase {
        public SpeedComm() : base("/speed", "/Speed") { }

        protected override void execute(string senderId, string[] args) {
            if (!AssertArgumentLengthEquals(args, 1, out var msg)) {
                LevelEditor.writer.write(msg);
                return;
            }

            string raw = args[0];
            if (raw == "-r") {
                LevelEditor.Inst.MovementController.ResetSpeed();
                LevelEditor.writer.write($"Set speed to {LevelEditor.Inst.MovementController.GetSpeed()}!");
                return;
            }

            if (float.TryParse(raw, out float newSpeed)) {
                LevelEditor.Inst.MovementController.SetSpeed(newSpeed);
                LevelEditor.writer.write($"Set speed to {newSpeed}!");
                return;
            }
            
            LevelEditor.writer.write("Invalid argument, speed must be a number!");
        }
    }
}