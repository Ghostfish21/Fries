using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.PlayerStateCommands {
    public class FlySpeedComm : CommandBase {
        public FlySpeedComm() : base("flyspeed", "Flyspeed") { }

        protected override void execute(string senderId, string[] args) {
            if (!AssertArgumentLengthEquals(args, 1, out var msg)) {
                LevelEditor.writer.write(msg);
                return;
            }

            string raw = args[0];
            if (raw == "-r") {
                LevelEditor.Inst.MovementController.ResetFlySpeed();
                LevelEditor.writer.write($"Set flyspeed to {LevelEditor.Inst.MovementController.GetFlySpeed()}!");
                return;
            }

            if (float.TryParse(raw, out float newSpeed)) {
                LevelEditor.Inst.MovementController.SetFlySpeed(newSpeed);
                LevelEditor.writer.write($"Set flyspeed to {newSpeed}!");
                return;
            }
            
            LevelEditor.writer.write("Invalid argument, speed must be a number!");
        }
    }
}