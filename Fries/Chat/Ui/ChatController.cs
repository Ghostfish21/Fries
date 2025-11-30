using System;
using System.Collections;
using Fries.EvtSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fries.Chat.Ui {
    public class ChatController : MonoBehaviour, IDeselectHandler {
        private static ChatCore.Writer writer;
        [EvtListener(typeof(ChatCore.OnInitiated))]
        private static void onChatInitiated() {
            writer = ChatCore.create("Player");
        }
        [EvtListener(typeof(ChatCore.PostMsgPrinted))]
        public static void onMsgPrinted(Message message) {
            Debug.Log(message.content);
        }
        
        [EvtDeclarer] public struct OnChatboxOpened {}
        [EvtDeclarer] public struct OnChatboxClosed {}
        
        private bool isChatboxOpen = false;
        
# if TMPro
        private TMP_InputField inputField;
        private void Awake() {
            inputField ??= gameObject.GetComponentInChildren<TMP_InputField>(true);
            if (!inputField) throw new InvalidOperationException("Input field does not exist!");
        }
        private void Update() {
            if (isChatboxOpen) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Input.GetMouseButtonDown(0)) 
                    moveCaretToMovePosX();
                
                if (Input.GetKeyDown(KeyCode.Return)) {
                    if (writer == null) 
                        throw new InvalidOperationException(
                            "Writer does not exist! Maybe you forgot to instantiate Chat Core prefab.");
                    
                    writer.write(inputField.text);
                    inputField.text = "";
                    isChatboxOpen = false;
                    
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Evt.TriggerNonAlloc<OnChatboxClosed>();
                }
                else if (isChatboxOpen && Input.GetKeyDown(KeyCode.Escape)) {
                    inputField.text = "";
                    isChatboxOpen = false;
                    inputField.DeactivateInputField();
                    EventSystem.current.SetSelectedGameObject(null);
                    
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Evt.TriggerNonAlloc<OnChatboxClosed>();
                }
            }

            else if (!isChatboxOpen) {
                if (Input.GetKeyDown(KeyCode.T)) {
                    isChatboxOpen = true;
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Evt.TriggerNonAlloc<OnChatboxOpened>();
                }
                else if (Input.GetKeyDown(KeyCode.Slash)) {
                    isChatboxOpen = true;
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    inputField.text = "/";
                    
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    StartCoroutine(MoveCaretToEnd());
                    Evt.TriggerNonAlloc<OnChatboxOpened>();
                }
            }
        }

        private const float FastCaretRate = 4;
        private const float SlowCaretRate = 0.85f;

        private static readonly WaitForEndOfFrame wait = new();
        private IEnumerator MoveCaretToEnd() {
            yield return wait;
            inputField.caretBlinkRate = FastCaretRate;
            
            inputField.caretPosition = 1;
            inputField.selectionStringAnchorPosition = 1;
            inputField.selectionStringFocusPosition  = 1;

            yield return wait;
            inputField.caretBlinkRate = SlowCaretRate;
        }
# endif
        
        public void OnDeselect(BaseEventData eventData) {
            if (!isChatboxOpen) return;
            StartCoroutine(refocus());
        }

        private IEnumerator refocus() {
            yield return wait;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void moveCaretToMovePosX() {
            Vector3 mousePos = Input.mousePosition;
            TMP_Text text = inputField.textComponent;
            int charIndex = TMP_TextUtilities.GetCursorIndexFromPosition(text, mousePos, null);

            if (charIndex == -1) {
                Vector3 textPos = text.transform.position;
                if (mousePos.x > Camera.main.WorldToScreenPoint(textPos).x) 
                    charIndex = inputField.text.Length;
                else charIndex = 0;
            }
            
            inputField.caretPosition = charIndex;
            inputField.ForceLabelUpdate();
        }
    }
}