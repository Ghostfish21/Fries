using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fries.Fries.EditorDoc.Commands {
    public class CommandMap {
        private Dictionary<string, Action<string[]>> commandMap = new();

        static CommandMap() {
            
        }
        
        private void registerCommand(string commandName, Action<string[]> action) {
            commandMap.Add(commandName, action);
        }
    }
}