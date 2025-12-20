# if InputSys

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fries.InputDispatch {
    public class InputLayer : MonoBehaviour {
        [SerializeField] private ConsumeType consumeType;
        [SerializeField] private int btnAmount;
        
        private readonly Dictionary<InputId, float> heldInputs = new();

        internal void reset() => heldInputs.Clear();
        
        private readonly Dictionary<InputKind, HashSet<int>> interestedLookupTable = new();
        private readonly Dictionary<InputKind, List<int>> interested = new();
        private int actualBtnAmount = 0;
        public void addInterestedInput(InputId inputId) {
            if (actualBtnAmount >= btnAmount) 
                throw new InvalidOperationException("InputLayer only supports up to " + btnAmount + " buttons.");
            
            interestedLookupTable.TryAdd(inputId.kind, new HashSet<int>());
            interested.TryAdd(inputId.kind, new List<int>());
            if (!interestedLookupTable[inputId.kind].Add(inputId.code)) return;
            interested[inputId.kind].Add(inputId.code); 
            actualBtnAmount++;
        }
        public void exchangeInterestedInput(InputId oldInput, InputId newInput) {
            if (!interestedLookupTable.TryGetValue(oldInput.kind, out HashSet<int> codes)) 
                throw new ArgumentException("The old input is not registered in this layer.");
            if (!codes.Remove(oldInput.code))
                throw new ArgumentException("The old input is not registered in this layer.");

            if (!interested.TryGetValue(oldInput.kind, out List<int> codes1))
                throw new Exception("Inconsistent state detected! Input Dispatcher maybe corrupted. This is a bug, the process won't continue");
            if (!codes1.Remove(oldInput.code))
                throw new Exception("Inconsistent state detected! Input Dispatcher maybe corrupted. This is a bug, the process won't continue");
            
            actualBtnAmount--;
            lastStatus.Remove(oldInput);
            heldUpDownStatus.Remove(oldInput);
            
            addInterestedInput(newInput);
        }

        private const byte NONE = 0;
        private const byte DOWN = 1;
        private const byte UP = 2;
        private const byte HELD = 3;
        private Dictionary<InputId, float> lastStatus = new();
        private Dictionary<InputId, byte> heldUpDownStatus = new();
        public void fetchUpdate(InputDispatcher dispatcher) {
            foreach (var kind in interested.Keys) {
                List<int> codes = interested[kind];
                dispatcher.requestStates(kind, codes, heldInputs);

                foreach (int code in codes) {
                    InputId inputId = new(kind, code);
                    bool prevValue = false;
                    if (lastStatus.TryGetValue(inputId, out var value)) prevValue = InputDispatcher.f2b(value);
                    bool curValue = InputDispatcher.f2b(heldInputs[inputId]);
                    
                    if (!prevValue && curValue) heldUpDownStatus[inputId] = DOWN;
                    else if (!prevValue && !curValue) heldUpDownStatus[inputId] = NONE;
                    else if (prevValue && !curValue) heldUpDownStatus[inputId] = UP;
                    else if (prevValue && curValue) heldUpDownStatus[inputId] = HELD;
                    
                    lastStatus[inputId] = heldInputs[inputId];
                }
            }
        }
        
        internal void consume(InputDispatcher dispatcher) {
            if (consumeType == ConsumeType.ConsumeOnly) {
                foreach (var heldInputsKey in heldInputs.Keys) 
                    dispatcher.consume(heldInputsKey);
            }
            else if (consumeType == ConsumeType.Complex) {
                resetConsumeState();
                shouldConsume(dispatcher);
                foreach (InputId shouldBeConsumedId in shouldBeConsumed)
                    dispatcher.consume(shouldBeConsumedId);
            }
            else if (consumeType == ConsumeType.BlockAll) dispatcher.blockAll();
            else if (consumeType == ConsumeType.Transparent) { }
        }
        
        private readonly HashSet<InputId> shouldBeConsumed = new();
        protected void setShouldBeConsumed(InputId inputId) => shouldBeConsumed.Add(inputId);

        protected virtual void resetConsumeState() => shouldBeConsumed.Clear();
        protected virtual void shouldConsume(InputDispatcher disp) { }

        public void disable() {
            enabled = false;
            gameObject.SetActive(false);
        }
        public void enable() {
            enabled = true;
            reset();
            gameObject.SetActive(true);
        }

        private void OnDisable() => gameObject.SetActive(false);
        
        public float getFloat(InputId inputId) {
            if (interestedLookupTable.TryGetValue(inputId.kind, out var codes) && codes.Contains(inputId.code)) {
                if (heldInputs.TryGetValue(inputId, out var value)) return value;
            }
            else addInterestedInput(inputId);
            return 0;
        }
        public bool isHeld(InputId inputId) => InputDispatcher.f2b(getFloat(inputId));
        public bool isUp(InputId inputId) {
            if (interestedLookupTable.TryGetValue(inputId.kind, out var codes) && codes.Contains(inputId.code))
                return heldUpDownStatus.TryGetValue(inputId, out var status) && status == UP;
            addInterestedInput(inputId);
            return false;
        }
        public bool isDown(InputId inputId) {
            if (interestedLookupTable.TryGetValue(inputId.kind, out var codes) && codes.Contains(inputId.code))
                return heldUpDownStatus.TryGetValue(inputId, out var status) && status == DOWN;
            addInterestedInput(inputId);
            return false;
        }
    }
}

# endif