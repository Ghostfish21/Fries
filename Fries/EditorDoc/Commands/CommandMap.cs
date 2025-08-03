# if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fries.EditorDoc.Commands {
    public static class CommandMap {
        private static Dictionary<string, Action<string[]>> commandMap = new();

        static CommandMap() {
            registerCommand("InspectorHighlight", HighlightInspectorCmd.highlightInspector);
        }
        
        private static void registerCommand(string commandName, Action<string[]> action) {
            commandMap.Add(commandName, action);
        }

        public static void execute(string[] cmdSegs) {
            string cmdName = cmdSegs[0];
            if (commandMap.ContainsKey(cmdName)) 
                commandMap[cmdName](cmdSegs.Skip(1).ToArray());
            else Debug.LogWarning("Command not found: " + cmdName);
        }
    }
}
# endif