using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
# if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Callbacks;
# endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fries.Inspector.TypeDrawer {
    [Serializable]
    public class TypeWrapper {
        # if UNITY_EDITOR
        [DidReloadScripts]
        private static void onScriptsReloaded() {
            PlayerPrefs.SetInt("ScriptCompileId", PlayerPrefs.GetInt("ScriptCompileId", 1) + 1);
            sourceFilesCache = new Dictionary<Assembly, HashSet<string>>();
        }
        # endif
        
        // 仅 Editor 中有效
        public string scriptPath;
        public string assemblyName;
        public string nameSpace;
        public List<string> typeNames;
        private List<Type> types;

        private int errorCode = -6;
        private int lastLoadCompileId;
        
        public string scriptContentCache;

        # if UNITY_EDITOR
        private static Dictionary<Assembly, HashSet<string>> sourceFilesCache = new();
        # endif

        public bool forceLoad = false;
        public void load() {
            if (string.IsNullOrEmpty(scriptPath)) {
                typeNames = new();
                types = new();
                nameSpace = "";
                assemblyName = "";
                scriptContentCache = "";
                return;
            }

            if (!forceLoad) {
                int currentCompileId = PlayerPrefs.GetInt("ScriptCompileId", 1);
                if (currentCompileId == lastLoadCompileId) return;
                lastLoadCompileId = currentCompileId;
            }
            else forceLoad = false;

            typeNames = new List<string>();
            
            try {
                tryLoadEditor();
                if (errorCode > 0) return;
                tryLoad();
            }
            catch (Exception e) {
                Debug.LogException(e);
                errorCode = -5;
            }
        }
        private void tryLoadEditor() {
# if UNITY_EDITOR
            if (string.IsNullOrEmpty(scriptPath)) return;
            
            scriptPath = scriptPath.Replace("\\", "/").Trim();
            var assemblies = CompilationPipeline.GetAssemblies();
            foreach (var asm in assemblies) {
                if (!sourceFilesCache.TryGetValue(asm, out var files)) {
                    files = new HashSet<string>();
                    foreach (var asmSourceFile in asm.sourceFiles) 
                        files.Add(asmSourceFile.Replace("\\", "/").Trim());
                    sourceFilesCache[asm] = files;
                }
                if (files.Contains(scriptPath)) assemblyName = asm.name;
            }

            if (string.IsNullOrEmpty(assemblyName)) {
                Debug.LogError($"Script {scriptPath} not found in any assembly!");
                errorCode = -3;
                return;
            }
            
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (!script && string.IsNullOrEmpty(scriptContentCache)) {
                Debug.LogError($"Script {scriptPath} not found!");
                errorCode = -2;
                return;
            }
            scriptContentCache = script.text;
# endif
        }
        
        private void tryLoad() {
            string content = scriptContentCache;

            // 移除块注释 /* ... */
            string noBlockComments = Regex.Replace(content, @"/\*[\s\S]*?\*/", "", RegexOptions.Multiline);
            // 移除行注释 // ...
            string cleanContent = Regex.Replace(noBlockComments, @"//.*", "", RegexOptions.Multiline);

            cleanContent = cleanContent.Replace(';', '\n');
            
            // 匹配 namespace
            var nsMatch = Regex.Match(cleanContent, @"(?m)^[ \t]*namespace\s+([A-Za-z_][\w\.]*)");
            if (nsMatch.Success) nameSpace = nsMatch.Groups[1].Value;
            else {
                Debug.LogError($"Target script {scriptPath} has no namespace!");
                errorCode = -1;
                return;
            }
            
            string typePattern = @"\b(?:class|struct|enum|interface|record)\s+([A-Za-z_]\w*)(\s*<[^>]+>)?";
        
            var matches = Regex.Matches(cleanContent, typePattern);
            typeNames = new List<string>();
            foreach (Match match in matches) {
                string typeName = match.Groups[1].Value;
                var genericArgs = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(genericArgs)) {
                    var arity = genericArgs.Count(ch => ch == ',') + 1;
                    typeName = $"{typeName}`{arity}";
                }
                if (!typeNames.Contains(typeName)) typeNames.Add(typeName);
            }

            types = new List<Type>();
            foreach (var typeName in typeNames.ToArray()) {
                Type type = findType($"{nameSpace}.{typeName}");
                if (type != null) types.Add(type);
                else typeNames.Remove(typeName);
            }

            errorCode = 0;
        }
        
        private Type findType(string typeNameIncludeNamespace) {
            Type type1 = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                if (asm.GetName().Name != assemblyName) continue;
                type1 = asm.GetType(typeNameIncludeNamespace);
                break;
            }
            return type1;
        }
        
        private List<Type> prevTypes = new();
        public List<Type> getTypes(out bool isValueChanged) {
            isValueChanged = false;
            types ??= new();
            load();
            if (prevTypes.Count != types.Count) isValueChanged = true;
            else {
                for (int i = 0; i < types.Count; i++) {
                    if (prevTypes[i] == types[i]) continue;
                    isValueChanged = true;
                    break;
                }
            }
            prevTypes = types.ToList();
            
            if (errorCode >= 0) return types;
            if (errorCode != -6) 
                Debug.LogError($"Error analyzing script during Type Analysis {scriptPath}: {errorCode}");
            return null;
        }
    }
}