using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fries.Chat;

namespace Fries.BlockGrid.LevelEdit.PlayerStateCommands {
    public class GiveComm : CommandBase {
        private Dictionary<int, object> tools = new();

        public GiveComm() : base("/give", "/Give") {
            tools[0] = new WoodenAxe();
        }
        
        protected override void execute(string senderId, string[] args) {
            if (!AssertArgumentLengthAtLeast(args, 1, out var errMsg)) {
                LevelEditor.writer.write(errMsg);
                return;
            }
            
            bool shouldPrint = true;
            if (AssertArgumentLengthEquals(args, 2, out _)) {
                string shouldPrintRaw = args[1];
                shouldPrintRaw = shouldPrintRaw.Replace("1", "true").Replace("0", "false");
                shouldPrintRaw = shouldPrintRaw.ToLower();
                if (!bool.TryParse(shouldPrintRaw, out shouldPrint)) 
                    LevelEditor.writer.write($"Second parameter {shouldPrintRaw} is invalid: Must be 'true'/'1' or 'false'/'0'!");
            }
            
            string raw = args[0];

            if (raw.StartsWith('t') || raw.StartsWith('T')) {
                raw = raw.Substring(1);
                if (int.TryParse(raw, out int toolId)) {
                    if (tools.TryGetValue(toolId, out var tool)) 
                        LevelEditor.Inst.PlayerBackpack.GiveItem($"t{raw}", tool, shouldPrint);
                    else LevelEditor.writer.write($"Given tool id {toolId} is invalid: Tool not found!!");
                }
                else LevelEditor.writer.write($"Given tool id {toolId} is not formatted correctly: Must start with 't' or 'T' and followed by a number!");
                return;
            }

            if (raw.StartsWith('p') || raw.StartsWith('P')) {
                raw = raw.Substring(1);
                if (int.TryParse(raw, out int partId)) {
                    object part = BlockRegistry.GetEnum(partId);
                    if (part != null) 
                        LevelEditor.Inst.PlayerBackpack.GiveItem($"p{raw}", part, shouldPrint);
                    else LevelEditor.writer.write($"Given part id {partId} is invalid: Part not found!!");
                }
                else LevelEditor.writer.write($"Given part id {partId} is not formatted correctly: Must start with 'p' or 'P' and followed by a number!");
                return;
            }

            if (int.TryParse(raw, out int id)) {
                object enumObj = BlockRegistry.GetEnum(id);
                if (enumObj == null) {
                    LevelEditor.writer.write($"Given id {id} is invalid!");
                    return;
                }

                LevelEditor.Inst.PlayerBackpack.GiveItem(raw, enumObj, shouldPrint);
                return;
            }

            string[] typeAndName = raw.Split('.');
            if (typeAndName.Length != 2) {
                LevelEditor.writer.write($"Given type {raw} is invalid: Format not matched!");
                return;
            }

            Type enumType = findEnumTypeByName(typeAndName[0]);
            if (enumType == null) {
                LevelEditor.writer.write($"Given type {raw} is invalid: Enum class not found!");
                return;
            }

            if (!Enum.TryParse(enumType, typeAndName[1], ignoreCase: false, out object value)) {
                LevelEditor.writer.write(
                    $"Given type {raw} is invalid: Value '{typeAndName[1]}' not in enum '{typeAndName[0]}'!");
                return;
            }

            LevelEditor.Inst.PlayerBackpack.GiveItem(raw, value, shouldPrint);
        }

        private static Type findEnumTypeByName(string enumTypeName) {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Type[] types;
                try {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e) {
                    List<Type> list = new List<Type>();
                    foreach (var t1 in e.Types) {
                        if (t1 != null) list.Add(t1);
                    }

                    types = list.ToArray();
                }

                var t = types.FirstOrDefault(t => t.IsEnum && t.Name == enumTypeName);
                if (t != null) return t;
            }

            return null;
        }
    }
}