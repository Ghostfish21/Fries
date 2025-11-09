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
    }
}