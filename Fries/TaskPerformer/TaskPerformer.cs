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

        /// <summary>
        /// <para>安排一个由主线程立刻执行的任务</para>
        /// </summary>
        public void scheduleTask(ParamedAction paramedAction) {
            tasks.Enqueue(paramedAction);
        }

        public void scheduleTask(ParamedAction paramedAction, float delayInSeconds) {
            StartCoroutine(scheduleDelayedTask(delayInSeconds, paramedAction));
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
                    // 等待指定的秒数
                    yield return new WaitForSeconds(delay);
                    // 延迟执行的方法
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