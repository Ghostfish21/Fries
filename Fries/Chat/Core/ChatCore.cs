using System;
using System.Collections.Generic;
using Fries.EvtSystem;
using UnityEngine;

namespace Fries.Chat {
    [DefaultExecutionOrder(-100)]
    public class ChatCore : MonoBehaviour {
        [EvtDeclarer] public struct BeforeMsgPrinted { ChatEventData data; }
        [EvtDeclarer] public struct PostMsgPrinted { Message data; }
        [EvtDeclarer] public struct OnInitiated { }
        [EvtDeclarer] public struct OnApplicationExit { }

        public interface Writer {
            void push(object obj);
            object pop();
            void write(string msg);
        }

        private sealed class _Writer : Writer {
            private ChatCore core;
            private int id;

            public _Writer(int id, ChatCore core) {
                this.id = id;
                this.core = core;
            }

            public void push(object obj) => core.push(obj);
            public object pop() => core.pop();
            public void write(string msg) => core.write(core.users[id], msg);
        }

        // Int 储存使用者ID， String 储存实际使用者程序名
        private int nextId = 0;
        private readonly HashSet<string> existingUsers = new();
        private readonly Dictionary<int, string> users = new();
        public CommandMap commands { get; private set; }
        
        private List<Message> messages = new();
        
        private void Awake() {
            DontDestroyOnLoad(gameObject);
            _inst = this;
            commands = new();
            Evt.TriggerNonAlloc<OnInitiated>();
        }

        private void OnApplicationQuit() {
            Evt.TriggerNonAlloc<OnApplicationExit>();
        }

        public static int baseIndex { get; private set; }
        public static int lineNumber(int localIndex) => baseIndex + localIndex;
        public bool getMessagesByBaseIndex(Action<Message, int, Break> runnable, int baseIndex, int loadSize = 30) {
            if (runnable is null) throw new ArgumentNullException(nameof(runnable));
            if (loadSize <= 0) throw new ArgumentException(nameof(loadSize));
            if (baseIndex < 0) throw new ArgumentOutOfRangeException(nameof(baseIndex));
            if (baseIndex >= messages.Count) return false;

            int end = Math.Min(messages.Count, baseIndex + loadSize);

            ChatCore.baseIndex = baseIndex;
            Break b = new Break();
            for (int i = baseIndex; i < end; i++) {
                runnable(messages[i], i, b);
                if (b.b) break;
            }

            return true;
        }

        public int msgCount => messages.Count;
        public bool getMessages(Action<Message, int, Break> runnable, int frameId, int loadSize = 30) {
            return getMessagesByBaseIndex(runnable, frameId * loadSize, loadSize);
        }
        public bool getMessages(Action<Message, int> runnable, int frameId, int loadSize = 30) {
            return getMessagesByBaseIndex((m, i, _) => runnable(m, i), frameId * loadSize, loadSize);
        }
        public bool getMessages(Action<Message> runnable, int frameId, int loadSize = 30) {
            return getMessagesByBaseIndex((m, _, _) => runnable(m), frameId * loadSize, loadSize);
        }

        private readonly Stack<object> parameters = new();
        private void push(object obj) => parameters.Push(obj);
        public object pop() {
            if (parameters.Count == 0)
                throw new InvalidOperationException("Parameter stack has no parameters when requesting one!");
            object obj = parameters.Pop();
            return obj;
        }
        private void write(string senderId, string msg) {
            if (!msg.StartsWith('/')) {
                Message message = new Message(senderId, msg);
                ChatEventData chatEventData = new ChatEventData { message = message };

                Evt.TriggerNonAlloc<BeforeMsgPrinted>(chatEventData);
                if (chatEventData.isCancelled) return;
                messages.Add(message);
                Evt.TriggerNonAlloc<PostMsgPrinted>(message);
            }
            else {
                string command = msg.Substring(1);
                Message message = new Message(senderId, command);
                commands.tryExecuteCommand(message);
                if (parameters.Count == 0) return;
                parameters.Clear();
                throw new InvalidOperationException(
                    "Parameter stack has unconsumed parameters after command is called!");
            }
        }

        private Writer createWriter(string moduleName) {
            if (existingUsers.Contains(moduleName)) 
                throw new InvalidOperationException($"Module with name {moduleName} already exists! The writer will not be created.");
            
            int id = nextId;
            nextId++;
            users[id] = moduleName;
            return new _Writer(id, this);
        }

        private static ChatCore _inst;
        public static ChatCore inst {
            get {
                if (_inst) return _inst;
                throw new InvalidOperationException("Chat Core is not initialized! Please call after it is initiated!");
            }
        }

        public static Writer create(string moduleName) {
            ChatCore core = inst;
            return core.createWriter(moduleName);
        }
    }
}