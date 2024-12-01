using System.Collections.Generic;
using System.Linq;

namespace Fries.OrderedCode {
    public class ScriptManager {
        private static ScriptManager moduleManager;
        public static ScriptManager inst() {
            if (moduleManager == null) moduleManager = new ScriptManager();
            return moduleManager;
        }
        
        // #################### 无需改动的集合变量 ####################
        // Private collection variables that shouldn't be changed from outside
        private readonly Dictionary<string, List<OrderedScript>> modules = new(); // 模块集合, Keep track of all the modules
        private readonly Dictionary<string, List<OrderedScript>> awaitModulesToConstruct = new(); // 等待构造的模块集合, Keep track of all the modules that are waiting to be constructed
        
        #region Modules 集合 暴露的公共方法

        // 检查某个模块是否可以被构造，也就是说，这个模块所需要的所有依赖模块是否都已经被构造
        // Check if a module is good to construct, which means all the dependencies of this module have been constructed
        private bool isModuleGoodToConstruct(OrderedScript module) {
            List<string> dependencies = module.getDependencies();
            foreach (string dependency in dependencies) 
                if (!modules.ContainsKey(dependency)) return false;
            return true;
        }
        
        // 尝试构造某个模块，如果这个模块已经被构造，则直接停止
        // 不然的话（即这个模块第一次调用这个方法），检查它是否满足构造条件，如果所有依赖都被构造则构造这个模块，否则将其加入等待构造的模块集合
        // 如果成功构造，构造完毕后会检查所有待构造的模块，如果有满足构造条件的模块则构造它们
        // Try to construct a module, if this module has been constructed, stop directly
        // Otherwise (i.e. the first time this module calls this method), check if it meets the condition to construct, if all dependencies have been constructed, construct this module, otherwise add it to the collection of modules that are waiting to be constructed
        // If successfully constructed, after the construction, check all the modules that are waiting to be constructed, if there are modules that meet the condition to construct, construct them
        public void tryConstructModule(OrderedScript script) {
            // 如果这个模块满足构造条件，则构造这个模块
            if (isModuleGoodToConstruct(script)) {
                script.construct();
                if (!modules.ContainsKey(script.getName())) modules[script.getName()] = new List<OrderedScript>();
                modules[script.getName()].Add(script);
                List<List<OrderedScript>> awaitModules = awaitModulesToConstruct.Values.ToList();
                // 构造完毕后检查所有待构造的模块，如果有满足构造条件的模块则构造它们
                foreach (List<OrderedScript> awaitModuleList in awaitModules) {
                    foreach (var awaitModule in awaitModuleList.ToList()) {
                        // 如果这个模块的依赖条件不满足，则跳过
                        if (!isModuleGoodToConstruct(awaitModule)) continue;
                        // 如果满足，则构造这个模块，并从待构造集合中移除。请注意上一行中 
                        // isModuleGoodToConstruct 会被调用两次，上一行一次，在下一行中又一次
                        awaitModuleList.Remove(awaitModule);
                        tryConstructModule(awaitModule);
                        if (awaitModuleList.Count == 0)
                            awaitModulesToConstruct.Remove(awaitModule.getName());
                    }
                }
            }
            else {
                if (!awaitModulesToConstruct.ContainsKey(script.getName()))
                    awaitModulesToConstruct[script.getName()] = new List<OrderedScript>();
                awaitModulesToConstruct[script.getName()].Add(script);
            }
        }

        public bool isConstructed(OrderedScript script) {
            return modules.ContainsKey(script.getName());
        }

        #endregion
        
    }
}