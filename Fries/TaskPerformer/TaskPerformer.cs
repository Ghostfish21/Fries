using System.Collections;
using System.Collections.Concurrent;
using Fries.OrderedCode;
using UnityEngine;

namespace Fries.TaskPerformer {
    
    /// <summary>
    /// <para>用于在其他线程向主线程发送任务并执行</para>
    /// </summary>
    public class TaskPerformer : OrderedScript {

        private static TaskPerformer tp;
        public static TaskPerformer inst() => tp;

        public override void construct() {
            tp = this;
        }
        
        /// <summary>
        /// <para>存储所有待执行的方法和需要的参数</para>
        /// </summary>
        private ConcurrentQueue<ParamedAction> tasks = new();

        public override void update() {
            if (tasks.IsEmpty) return;

            tasks.TryDequeue(out var paramedAction);
            paramedAction.action.Invoke(paramedAction.param);
        }

        private ParamedAction wrapParamedAction(ParamedAction paramedAction) {
            ParamedAction wrapper = new ParamedAction {
                action = objs => {
                    if (paramedAction.taskHandle.isCancelled) return;
                    paramedAction.action(objs);
                    paramedAction.taskHandle.executedTime++;
                    paramedAction.taskHandle.isExecuted = true;
                },
                param = paramedAction.param,
                taskHandle = new TaskHandle()
            };
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
    }
}