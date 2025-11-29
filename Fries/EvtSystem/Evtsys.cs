using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fries.Inspector;
using UnityEngine;

// TODO 用 Editor 脚本提前扫描含有目标 Attr 的类并生成文件，消除启动反射
// TODO 在打包时通过预编译指令，将不必要的检查全部剔除

namespace Fries.EvtSystem {
    [Serializable]
    public class EvtInfo {
        public Type insertedClass;
        public string eventName;
        public Type[] argsTypes;
        public List<EvtListenerInfo> listeners;

        public ReadonlyEvtInfo toReadonly() {
            return new ReadonlyEvtInfo(insertedClass, eventName, argsTypes, listeners);
        }
    }

    public readonly struct ReadonlyEvtInfo {
        public readonly Type insertedClass;
        public readonly string eventName;
        public readonly IReadOnlyList<Type> argsTypes;
        public readonly IReadOnlyList<EvtListenerInfo> listeners;
        
        public ReadonlyEvtInfo(Type insertedClass, string eventName, Type[] argsTypes, List<EvtListenerInfo> listeners) {
            this.insertedClass = insertedClass;
            this.eventName = eventName;
            this.argsTypes = argsTypes;
            this.listeners = listeners;
        }
    }

    public static class EvtExt {
        public static void triggerListener(this object obj, string eventName, params object[] objects) {
            Evtsys.inst.triggerListener(obj.GetType(), eventName, objects);
        }

        public static void triggerListener<T>(this object obj, string eventName, T arg0) {
            object[] buffer = Evtsys.inst.rentBuffer(1);
            buffer[0] = arg0;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 1);
        }

        public static void triggerListener<T, T1>(this object obj, string eventName, T arg0, T1 arg1) {
            object[] buffer = Evtsys.inst.rentBuffer(2);
            buffer[0] = arg0;
            buffer[1] = arg1;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 2);
        }
        
        public static void triggerListener<T, T1, T2>(this object obj, string eventName, T arg0, T1 arg1, T2 arg2) {
            object[] buffer = Evtsys.inst.rentBuffer(3);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 3);
        }
        
        public static void triggerListener<T, T1, T2, T3>(this object obj, string eventName, T arg0, T1 arg1, T2 arg2, T3 arg3) {
            object[] buffer = Evtsys.inst.rentBuffer(4);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 4);
        }
        
        public static void triggerListener<T, T1, T2, T3, T4>(this object obj, string eventName, T arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            object[] buffer = Evtsys.inst.rentBuffer(5);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 5);
        }

        public static void triggerListener<T, T1, T2, T3, T4, T5>(this object obj, string eventName, T arg0, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            object[] buffer = Evtsys.inst.rentBuffer(6);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 6);
        }

        public static void triggerListener<T, T1, T2, T3, T4, T5, T6>(this object obj, string eventName, T arg0, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            
            object[] buffer = Evtsys.inst.rentBuffer(7);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 7);
        }

        public static void triggerListener<T, T1, T2, T3, T4, T5, T6, T7>(this object obj, string eventName, T arg0, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            
            object[] buffer = Evtsys.inst.rentBuffer(8);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 8);
        }
        
        public static void triggerListener<T, T1, T2, T3, T4, T5, T6, T7, T8>(this object obj, string eventName, T arg0, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            
            object[] buffer = Evtsys.inst.rentBuffer(9);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 9);
        }
        
        public static void triggerListener<T, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this object obj, string eventName, T arg0, T1 arg1,
            T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            
            object[] buffer = Evtsys.inst.rentBuffer(10);
            buffer[0] = arg0;
            buffer[1] = arg1;
            buffer[2] = arg2;
            buffer[3] = arg3;
            buffer[4] = arg4;
            buffer[5] = arg5;
            buffer[6] = arg6;
            buffer[7] = arg7;
            buffer[8] = arg8;
            buffer[9] = arg9;
            Evtsys.inst.triggerListenerNonAlloc(obj.GetType(), eventName, buffer, 10);
        }
        
    }
    
    [DefaultExecutionOrder(-10000)]
    public class Evtsys : MonoBehaviour {
        private static Evtsys ies;
        public static Evtsys inst => ies;
        
        // 事件参数类型列表 - 事件声明类 -> 事件名 -> 参数类型列表
        private Dictionary<Type, Dictionary<string, Type[]>> eventParamList = new();
        // 存储事件信息 - 事件完整路径名 -> 事件信息
        private Dictionary<EventKey, EvtInfo> eventInfoDict = new();
        // 所有事件的列表
        public List<EvtInfo> events = new();

        private void declareEvent(Type type, string eventName, Type[] parameters = null) {
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
            eventInfoDict[new EventKey(type, eventName)] = insertionEventInfo;
            
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

        private void registerListener(Type type, string eventName, string listenerName, float priority, MulticastDelegate listener, bool canBeExternallyCancelled, Func<string, bool> isFriendlyAssembly) {
            if (!listener.Method.IsStatic) {
                Debug.LogError($"Event listener {listenerName} must be a static method!");
                return;
            }
            
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
                    $"Event {eventName} of type {type} has {eventParamList[type][eventName].Length} parameters, but the listener has {types.Length} parameters!");
                return;
            }
            
            // 检查该事件监听器的参数是否和注册中留下的记录一致。如果不一致的话爆出错误
            for (int i = 0; i < types.Length; i++) {
                if (eventParamList[type][eventName][i] == null) {
                    Debug.LogError($"Event {eventName} of type {type} has Null (Type) parameter {i}");
                    return;
                }
                if (types[i] == null) {
                    Debug.LogError($"Argument of Event {eventName} of type {type} has Null parameter {i}");
                    return;
                }

                if (eventParamList[type][eventName][i] == null || types[i].IsAssignableFrom(eventParamList[type][eventName][i])) continue;
                Debug.LogError(
                    $"Event {eventName} of type {type} has parameter {i} of type {eventParamList[type][eventName][i]}, but the listener has parameter {i} of type {types[i]}!");
                return;
            }

            // 创建对应缓存目录
            if (!typeEvent.ContainsKey(type))
                typeEvent[type] = new();
            if (!typeEvent[type].ContainsKey(eventName))
                typeEvent[type][eventName] = new(new ListenerComparer());
            
            // 检查是否该监听器已经被注册。这需要相同的类有相同名字和相同参数的监听器被注册两次。
            // 如果发生这样的情况则抛出异常。通常这是由于用户错误的调用内部方法导致的
            var listenerInfo = new EvtListenerInfo(type, listenerName, priority, canBeExternallyCancelled, isFriendlyAssembly);
            if (typeEvent[type][eventName].ContainsKey(listenerInfo))
                throw new ArgumentException(
                    $"Event {eventName} of type {type} already has a listener called {listenerName} with priority {priority}!");
            
            // 记录该监听器
            typeEvent[type][eventName][listenerInfo] = listener;
            
            eventInfoDict[new EventKey(type, eventName)].listeners.Add(listenerInfo);
        }

        # region EventKey 实现
        private readonly struct EventKey : IEquatable<EventKey> {
            private readonly Type type;
            private readonly string name;
            public EventKey(Type type, string name) {
                this.type = type;
                this.name = name;
            }
            public bool Equals(EventKey other) => type == other.type && name == other.name;
            public override bool Equals(object obj) => obj is EventKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(type, name);
        }
        # endregion

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
        private readonly Dictionary<EventKey, Dictionary<EvtListenerInfo, Action<object[]>>> runnableCache = new();
        private Action<object[]> getRunnable(Type type, string eventName, EvtListenerInfo info, MulticastDelegate listener) {
            if (!listener.Method.IsStatic) throw new ArgumentException("Only static methods are supported!");
            
            var key = new EventKey(type, eventName);
            Type[] types = eventParamList[type][eventName];
            
            runnableCache.TryAdd(key, new Dictionary<EvtListenerInfo, Action<object[]>>());
            if (runnableCache[key].TryGetValue(info, out var runnable)) return runnable;
            
            Action<object[]> runnable1 = createInvoker(listener, types);
            runnableCache[key][info] = runnable1;
            return runnable1;
        }

        private static Stack<EvtHandle> evtHandles = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void reset() => evtHandles = new();
        
        public static EvtHandle evtHandle {
            get {
                if (evtHandles.TryPeek(out var evtHandle1)) return evtHandle1;
                return null;
            }
        }

        private readonly Stack<object[]> bufferPool = new();
        public object[] rentBuffer(int paramCount) {
            if (bufferPool.Count <= 0) return new object[10];
            
            var arr = bufferPool.Pop();
            if (arr.Length >= paramCount) return arr;
            
            bufferPool.Push(arr);
            int size = arr.Length;
            while (size < paramCount) size *= 2;
            return new object[size];
        }

        public void returnBuffer(object[] buffer, bool clear = true, int paramCount = -1) {
            if (clear) {
                int length = buffer.Length;
                if (paramCount > 0) length = Math.Min(paramCount, buffer.Length);
                Array.Clear(buffer, 0, length);
            }
            bufferPool.Push(buffer);
        }
        
        public void triggerListenerNonAlloc(Type type, string eventName, object[] buffer, int argCount) {
            try { internalTrigger(type, eventName, buffer, argCount); }
            finally { returnBuffer(buffer, true, argCount); }
        }

        public void triggerListener(Type type, string eventName, params object[] objects) {
            internalTrigger(type, eventName, objects, objects.Length);
        }
        
        private Dictionary<EventKey, Stack<EvtHandle>> evtHandleCache = new();

        private EvtHandle getEvtHandle(Type type, string eventName) {
            EventKey key = new(type, eventName);
            evtHandleCache.TryAdd(key, new Stack<EvtHandle>());
            
            Stack<EvtHandle> evtHandles1 = evtHandleCache[key];
            if (evtHandles1.Count > 0) {
                EvtHandle evtHandle1 = evtHandles1.Pop();
                evtHandle1.reset();
                return evtHandle1;
            }
            
            return new EvtHandle(eventInfoDict[new EventKey(type, eventName)].toReadonly());
        }

        private void retEvtHandle(Type type, string eventName, EvtHandle evtHandle) {
            EventKey key = new(type, eventName);
            evtHandleCache.TryAdd(key, new Stack<EvtHandle>());
            evtHandleCache[key].Push(evtHandle);
        }
        
        private void internalTrigger(Type type, string eventName, object[] parameters, int length) {
            if (!eventParamList.ContainsKey(type)) {
                Debug.LogError($"Given Type {type} has no available events!");
                return;
            }

            if (!eventParamList[type].ContainsKey(eventName)) {
                Debug.LogError($"Given Type {type} has no event called {eventName}!");
                return;
            }

            Type[] expectedParams = eventParamList[type][eventName];
            if (length != expectedParams.Length) {
                Debug.LogError(
                    $"Event {eventName} of type {type} expects {expectedParams.Length} parameters, " +
                    $"but received {length}!"
                );
                return;
            }

            for (int i = 0; i < length; i++) {
                var arg = parameters[i];
                var expected = expectedParams[i];
                if (arg != null && !expected.IsInstanceOfType(arg)) {
                    Debug.LogError(
                        $"Event {eventName} parameter {i} expects {expected}, " +
                        $"but got {arg.GetType()}!"
                    );
                    return;
                }
            }

            EvtHandle evtHandle = getEvtHandle(type, eventName);
            evtHandles.Push(evtHandle);

            try {
                if (!typeEvent.ContainsKey(type))
                    typeEvent[type] = new();
                if (!typeEvent[type].ContainsKey(eventName))
                    typeEvent[type][eventName] = new();
                foreach (var listenerKvp in typeEvent[type][eventName]) {
                    try {
                        evtHandle.nextListener = listenerKvp.Key;
                        if (!evtHandle.shouldProcess()) continue;
                        Action<object[]> runnable = getRunnable(type, eventName, listenerKvp.Key, listenerKvp.Value);
                        runnable.Invoke(parameters);
                    }
                    catch (Exception e) {
                        Debug.LogError($"Catch error in one of the Event Listener: {e}");
                    }
                }
            }
            finally {
                EvtHandle eh = evtHandles.Pop();
                retEvtHandle(type, eventName, eh);
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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            
            ReflectionUtils.forType(ty => {
                FieldInfo[] fields = ty.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                Type[] fieldTypes = fields.Select(f => f.FieldType).ToArray();
                declareEvent(ty.DeclaringType ?? typeof(GlobalEvt), ty.Name, fieldTypes);
            }, typeof(EvtDeclarer), loadAssemblies);
            
            ReflectionUtils.forStaticMethods((mi, de) => {
                    EvtListener attr = (EvtListener)mi.GetCustomAttribute(typeof(EvtListener));
                    if (mi.DeclaringType == null) {
                        Debug.LogError($"Method {mi.Name} is not declared in a class!");
                        return;
                    }
                    Assembly assembly = mi.DeclaringType.Assembly;
                    string fullName = assembly.FullName + "::" + mi.DeclaringType.Name + "::" + mi.Name;
                    try {
                        registerListener(attr.type.DeclaringType ?? typeof(GlobalEvt), attr.type.Name, fullName, attr.priority, (MulticastDelegate)de,
                            attr.canBeExternallyCancelled, attr.isFriendlyAssembly);
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