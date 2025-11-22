using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fries.Inspector;
using UnityEngine;

namespace Fries.InsertionEventSys {
    [Serializable]
    public class EvtInfo {
        public Type insertedClass;
        public string eventName;
        public Type[] argsTypes;
        public List<EvtListenerInfo> listeners;
    }
    
    [DefaultExecutionOrder(-10000)]
    public class EvtSys : MonoBehaviour {
        private static EvtSys ies;
        public static EvtSys inst => ies;
        
        // 事件参数类型列表 - 事件声明类 -> 事件名 -> 参数类型列表
        private Dictionary<Type, Dictionary<string, Type[]>> eventParamList = new();
        // 存储事件信息 - 事件完整路径名 -> 事件信息
        private Dictionary<string, EvtInfo> eventInfoDict = new();
        // 所有事件的列表
        public List<EvtInfo> events = new();

        public void declareEvent(Type type, string eventName, Type[] parameters = null) {
            // 记录该事件 的参数类型列表
            if (!eventParamList.ContainsKey(type)) eventParamList[type] = new();
            eventParamList[type][eventName] = parameters.Nullable();
            
            // 将 Attribute 的数据转存入 本类的缓存 中
            var insertionEventInfo = new EvtInfo {
                insertedClass = type,
                eventName = eventName,
                argsTypes = parameters.Nullable(),
                listeners = new List<EvtListenerInfo>()
            };
            eventInfoDict[type.FullName + ": " + eventName] = insertionEventInfo;
            
            // 注册事件
            events.Add(insertionEventInfo);
        }

        private Type[] getTypes(MulticastDelegate delegatee) {
            Type delegateType = delegatee.GetType();
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            Type[] parameterTypes = invokeMethod
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();
            return parameterTypes;
        }

        private Dictionary<Type, Dictionary<string, SortedDictionary<EvtListenerInfo, MulticastDelegate>>> typeEvent = new();

        public void registerListener(Type type, string eventName, string listenerName, float priority, MulticastDelegate listener) {
            // 检查该类是否声明过任何事件。如果没有的话爆出错误
            if (!eventParamList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }

            // 检查该类是否声明过该事件。如果没有的话爆出错误
            if (!eventParamList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }

            // 检查该事件监听器的参数是否和注册中留下的记录一致。如果不一致的话爆出错误
            Type[] types = getTypes(listener);
            if (types.Length != eventParamList[type][eventName].Length) {
                Debug.LogError(
                    $"Event {eventName} of type {type.ToString()} has {eventParamList[type][eventName].Length} parameters, but the listener has {types.Length} parameters!");
                return;
            }
            
            // 检查该事件监听器的参数是否和注册中留下的记录一致。如果不一致的话爆出错误
            for (int i = 0; i < types.Length; i++) {
                if (eventParamList[type][eventName][i] == null)
                    Debug.LogError($"Event {eventName} of type {type.ToString()} has Null parameter {i}");
                if (types[i] == null)
                    Debug.LogError($"Argument of Event {eventName} of type {type.ToString()} has Null parameter {i}");
                if (eventParamList[type][eventName][i] == null ||
                    types[i].IsAssignableFrom(eventParamList[type][eventName][i])) continue;
                Debug.LogError(
                    $"Event {eventName} of type {type.ToString()} has parameter {i} of type {eventParamList[type][eventName][i].ToString()}, but the listener has parameter {i} of type {types[i].ToString()}!");
                return;
            }

            // 创建对应缓存目录
            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new(new ListenerComparer());
            
            // 检查是否该监听器已经被注册。这需要相同的类有相同名字和相同参数的监听器被注册两次。
            // 如果发生这样的情况则抛出异常。通常这是由于用户错误的调用内部方法导致的
            var listenerInfo = new EvtListenerInfo(listenerName, priority);
            if (typeEvent[type][eventName].ContainsKey(listenerInfo))
                throw new ArgumentException(
                    $"Event {eventName} of type {type} already has a listener called {listenerName} with priority {priority}!");
            
            // 记录该监听器
            typeEvent[type][eventName][listenerInfo] = listener;
            eventInfoDict[type.FullName + ": " + eventName].listeners.Add(listenerInfo);
        }
        
        
        public void unregisterListener(Type type, string eventName, float priority, string listenerName) {
            if (!eventParamList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }

            if (!eventParamList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }

            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new();
            var listenerInfo = new EvtListenerInfo(listenerName, priority);
            if (typeEvent[type][eventName].ContainsKey(listenerInfo))
                typeEvent[type][eventName].Remove(listenerInfo);
            eventInfoDict[type.FullName + ": " + eventName].listeners.Remove(listenerInfo);
        }

        private static Action<object[]> createInvoker(MulticastDelegate listener, Type[] parameterTypes) {
            if (!listener.Method.IsStatic) return null;

            var argsParam = Expression.Parameter(typeof(object[]), "args");
            var methodParameters = parameterTypes
                .Select((type, index) =>
                    Expression.Convert(Expression.ArrayIndex(argsParam, Expression.Constant(index)), type)
                ).Cast<Expression>().ToArray();

            Expression body = Expression.Call(listener.Method, methodParameters);
            return Expression.Lambda<Action<object[]>>(body, argsParam).Compile();
        }
        
        // 完整类型名 -> 事件监听器信息 -> 缓存过的事件触发器 Action
        private Dictionary<string, Dictionary<EvtListenerInfo, Action<object[]>>> runnableCache = new();
        private Action<object[]> getRunnable(Type type, string eventName, EvtListenerInfo info, MulticastDelegate listener) {
            string fullName = type.FullName + ": " + eventName;
            Type[] types = eventParamList[type][eventName];
            
            runnableCache.TryAdd(fullName, new Dictionary<EvtListenerInfo, Action<object[]>>());
            if (runnableCache[fullName].TryGetValue(info, out var runnable)) return runnable;
            
            Action<object[]> runnable1 = createInvoker(listener, types);
            return runnable1;
        }
        
        public static EvtHandle evtHandle;
        public void triggerListener(Type type, string eventName, params object[] objects) {
            if (!eventParamList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }

            if (!eventParamList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }

            Type[] expectedParams = eventParamList[type][eventName];
            if (objects.Length != expectedParams.Length) {
                Debug.LogError(
                    $"Event {eventName} of type {type} expects {expectedParams.Length} parameters, " +
                    $"but received {objects.Length}!"
                );
                return;
            }

            for (int i = 0; i < objects.Length; i++) {
                var arg = objects[i];
                var expected = expectedParams[i];
                if (!expected.IsInstanceOfType(arg)) {
                    Debug.LogError(
                        $"Event {eventName} parameter {i} expects {expected}, " +
                        $"but got {arg.GetType()}!"
                    );
                    return;
                }
            }
            
            evtHandle = new EvtHandle {
                eventInfo = eventInfoDict[type.FullName + ": " + eventName],
                nextListener = null
            };
            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new();
            foreach (var listenerKvp in typeEvent[type][eventName]) {
                try {
                    evtHandle.nextListener = listenerKvp.Key;
                    if (!evtHandle.shouldProcess(evtHandle)) continue;
                    Action<object[]> runnable = getRunnable(type, eventName, listenerKvp.Key, listenerKvp.Value);
                    runnable.Invoke(objects);
                }
                catch (Exception e) {
                    Debug.LogError($"Catch error in one of the Event Listener: {e}");
                }
            }
        }

        private bool hasStarted = false;
        public string[] loadAssemblies;

        private void Awake() {
            if (ies) {
                Destroy(gameObject);
                return;
            }

            ies = this;
            DontDestroyOnLoad(this);

            ReflectionUtils.forType(ty => {
                var attrs = ty.GetCustomAttributes(typeof(EvtDeclarer));
                foreach (var attr in attrs) {
                    var declarer = (EvtDeclarer)attr;
                    declareEvent(ty, declarer.eventName, declarer.argsTypes);
                }
            }, typeof(EvtDeclarer), loadAssemblies);

            ReflectionUtils.forStaticMethods((mi, de) => {
                    EvtListener attr = (EvtListener)mi.GetCustomAttribute(typeof(EvtListener));
                    Assembly assembly = mi.DeclaringType.Assembly;
                    string fullName = assembly.FullName + "::" + mi.DeclaringType.Name + "::" + mi.Name;
                    try {
                        registerListener(attr.type, attr.eventName, fullName, attr.priority, (MulticastDelegate)de);
                    }
                    catch (Exception e) {
                        Debug.LogError(
                            $"Catch error, check whether you have the valid method signature: void (any) for listener {fullName} \n {e}");
                    }
                }, typeof(EvtListener), BindingFlags.Public | BindingFlags.NonPublic, typeof(void),
                loadAssemblies);
        }
    }
}