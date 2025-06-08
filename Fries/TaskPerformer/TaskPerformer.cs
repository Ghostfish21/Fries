using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Fries.OrderedCode;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Fries.TaskPerformer {
    
    /// <summary>
    /// <para>用于在其他线程向主线程发送任务并执行</para>
    /// </summary>
    public class TaskPerformer : OrderedScript {

        private static TaskPerformer tp;
        public static TaskPerformer inst() => tp;
        private static List<Action> onConstruct = new();
        public static void callOnConstruct(Action action) {
            onConstruct.Add(action);
        }

        public override void construct() {
            if (tp != null) {
                Destroy(gameObject);
                return;
            }
            tp = this;
            DontDestroyOnLoad(tp);

            foreach (var action in onConstruct) action();
            onConstruct.Clear();
        }
        
        /// <summary>
        /// <para>存储所有待执行的方法和需要的参数</para>
        /// </summary>
        private ConcurrentQueue<ParamedAction> tasks = new();
        private ConcurrentDictionary<Func<bool>, ParamedAction> whenTasks = new();
        private ConcurrentDictionary<Func<bool>, ParamedAction> repeatingWhenTasks = new();

        public override void update() {
            if (whenTasks.Count != 0) {
                foreach (var condition in whenTasks.Keys) {
                    if (!condition()) continue;
                    ParamedAction pa = whenTasks[condition];
                    pa.action.Invoke(pa.param);
                    whenTasks.Remove(condition, out _);
                }
            }

            if (repeatingWhenTasks.Count != 0) {
                foreach (var condition in repeatingWhenTasks.Keys) {
                    if (!condition()) continue;
                    ParamedAction pa = repeatingWhenTasks[condition];
                    pa.action.Invoke(pa.param);
                    int currentExeTime = pa.taskHandle.executedTime;
                    int maxExeTime = (int)pa.taskHandle.data["MaxExecuteTime"];
                    if (maxExeTime > 0 && currentExeTime >= maxExeTime) 
                        repeatingWhenTasks.Remove(condition, out _);
                }
            }
            
            if (tasks.IsEmpty) return;
            tasks.TryDequeue(out var paramedAction);
            paramedAction.action.Invoke(paramedAction.param);
        }

        private ParamedAction wrapParamedAction(ParamedAction paramedAction) {
            ParamedAction wrapper = new ParamedAction();
            wrapper.action = objs => {
                if (wrapper.taskHandle.isCancelled) return;
                paramedAction.action(objs);
                wrapper.taskHandle.executedTime++;
                wrapper.taskHandle.onComplete?.Invoke();
                wrapper.taskHandle.isExecuted = true;
            };
            wrapper.param = paramedAction.param;
            wrapper.taskHandle = new TaskHandle();
            return wrapper;
        }

        /// <summary>
        /// <para>安排一个由主线程立刻执行的任务</para>
        /// </summary>
        public TaskHandle scheduleTask(ParamedAction paramedAction) {
            ParamedAction wrapper = wrapParamedAction(paramedAction);
            tasks.Enqueue(wrapper);
            return wrapper.taskHandle;
        }
        
        public TaskHandle scheduleTaskWhen(ParamedAction paramedAction, Func<bool> condition) {
            ParamedAction wrapper = wrapParamedAction(paramedAction);
            whenTasks[condition] = wrapper;
            return wrapper.taskHandle;
        }
        
        public TaskHandle scheduleRepeatingTaskWhen(ParamedAction paramedAction, Func<bool> condition, int executeTime = -1) {
            ParamedAction wrapper = wrapParamedAction(paramedAction);
            wrapper.taskHandle.data["MaxExecuteTime"] = executeTime;
            repeatingWhenTasks[condition] = wrapper;
            return wrapper.taskHandle;
        }

        public TaskHandle scheduleTask(ParamedAction paramedAction, float delayInSeconds) {
            ParamedAction wrapper = wrapParamedAction(paramedAction);
            StartCoroutine(scheduleDelayedTask(delayInSeconds, wrapper));
            return wrapper.taskHandle;
        }
        
        public TaskHandle scheduleRepeatingTask(ParamedAction paramedAction, float delayInSeconds, int executeTime = -1) {
            ParamedAction wrapper = wrapParamedAction(paramedAction);
            StartCoroutine(scheduleRepeatingTask(delayInSeconds, wrapper, executeTime));
            return wrapper.taskHandle;
        }

        public void executeIEnumerator(IEnumerator iEnumerator) {
            StartCoroutine(iEnumerator);
        }
        
        private static IEnumerator scheduleDelayedTask(float delay, ParamedAction paramedAction) {
            // 等待指定的秒数
            yield return new WaitForSeconds(delay);
        
            // 延迟执行的方法
            paramedAction.action(paramedAction.param);
        }
        
        private static IEnumerator scheduleRepeatingTask(float delay, ParamedAction paramedAction, int executeTime = -1) {
            if (executeTime < 0) {
                while (true) {
                    if (paramedAction.taskHandle.isCancelled) break; 
                    // 等待指定的秒数
                    yield return new WaitForSeconds(delay);
                    // 延迟执行的方法
                    if (paramedAction.taskHandle.isCancelled) break; 
                    paramedAction.action(paramedAction.param);
                }
            }

            for (int i = 0; i < executeTime; i++) {
                // 等待指定的秒数
                yield return new WaitForSeconds(delay);
                // 延迟执行的方法
                paramedAction.action(paramedAction.param);
            }
        }

        public static T executeLabeledAction<T>(string label, object[] param, object target = null) {
            return (T)LabeledActionAttribute.execute(label, param, target);
        }
        
        public static void executeLabeledAction(string label, object[] param, object target = null) {
            LabeledActionAttribute.execute(label, param, target);
        }

        public static async void executeExe(string pathToExe, string[] args, bool useShallExe = false, bool createNoWindow = true, Action onComplete = null) {
            // 将参数数组合并为一个字符串，各参数之间以空格隔开
            string arguments = string.Join(" ", args);

            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = pathToExe,
                Arguments = arguments,
                UseShellExecute = useShallExe,  // 禁止使用操作系统外壳启动
                CreateNoWindow = createNoWindow     // 不显示新窗口（根据需求可设置为 false）
            };

            try {
                using Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                // 等待进程退出
                await Task.Run(() => process.WaitForExit());
                onComplete?.Invoke();

                Debug.Log($"Program finished with code: {process.ExitCode}");
            }
            catch (Exception ex) {
                Debug.Log($"Program throws exception: {ex.Message}");
            }
        }
    }
}