using System;
using Fries.Inspector.CustomDataRows;

namespace Fries.Chat {
    public abstract class CommandBase {
        protected readonly string commandName;
        
        protected CommandBase(string commandName, params string[] aliases) {
            this.commandName = commandName;
            
            ChatCore.inst.commands.tryRegisterCommand(commandName, execute);
            foreach (var alias in aliases) ChatCore.inst.commands.tryRegisterCommand(alias, execute);
        }

        protected abstract void execute(string senderId, string[] args);

        public static bool AssertArgumentLengthAtLeast(string[] args, int length, out string errMsg) {
            errMsg = "";
            if (args.Length < length) {
                errMsg = $"Not enough arguments! Expected {length}, actual {args.Length}";
                return false;
            }
            return true;
        }

        public static bool AssertArgumentLengthEquals(string[] args, int length, out string errMsg) {
            errMsg = "";
            if (args.Length != length) {
                errMsg = $"Arguments length incorrect! Expected {length}, actual {args.Length}";
                return false;
            }
            return true;
        }
    }
}