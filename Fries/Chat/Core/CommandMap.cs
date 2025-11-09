using System;
using System.Collections.Generic;
using System.Linq;

namespace Fries.Chat {
    public class CommandMap {
        private static ChatCore.Writer writer;
        public CommandMap() {
            writer ??= ChatCore.create("CommandMap");
        }
        
        private Dictionary<string, Action<string, string[]>> commandMap = new();

        public void tryRegisterCommand(string command, Action<string, string[]> action) {
            if (string.IsNullOrEmpty(command)) return;
            if (action == null) return;
            
            commandMap.TryAdd(command, action);
        }
        
        public void tryExecuteCommand(Message message) {
            if (string.IsNullOrEmpty(message.content)) return;
            
            string[] args = message.content.Split(' ');
            if (args.Length == 0) return;
            if (args[0] == "") return;
            
            string commandName = args[0];

            if (!commandMap.TryGetValue(commandName, out var action)) {
                writer.write($"No command with name {commandName} can be found!");
                return;
            }
            string[] commandArgs = args.Skip(1).ToArray();
            action(message.senderId, commandArgs);
        }
    }
}