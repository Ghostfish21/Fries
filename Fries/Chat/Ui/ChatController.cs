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
        # if UNITY_EDITOR
        private KeyCode exitKey = KeyCode.BackQuote;
        # else
        private KeyCode exitKey = KeyCode.Escape;
        # endif
        
# if TMPro
        private TMP_InputField inputField;
        private void Awake() {
            inputField ??= gameObject.GetComponentInChildren<TMP_InputField>(true);
            if (!inputField) throw new InvalidOperationException("Input field does not exist!");
        }

        private IEnumerator lockCursor() {
            yield return null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

                    StartCoroutine(lockCursor());
                    Evt.TriggerNonAlloc<OnChatboxClosed>();
                }
                else if (isChatboxOpen && Input.GetKeyDown(exitKey)) {
                    inputField.text = "";
                    isChatboxOpen = false;
                    inputField.DeactivateInputField();
                    EventSystem.current.SetSelectedGameObject(null);
                    
                    StartCoroutine(lockCursor());
                    Evt.TriggerNonAlloc<OnChatboxClosed>();
                }
            }

            else if (!isChatboxOpen) {
                if (Input.GetKeyDown(KeyCode.T)) {
                    isChatboxOpen = true;
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    
                    Evt.TriggerNonAlloc<OnChatboxOpened>();
                }
                else if (Input.GetKeyDown(KeyCode.Slash)) {
                    isChatboxOpen = true;
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    inputField.text = "/";
                    
                    StartCoroutine(moveCaretToEnd());
                    Evt.TriggerNonAlloc<OnChatboxOpened>();
                }
            }
        }

        private const float FastCaretRate = 4;
        private const float SlowCaretRate = 0.85f;

        private static readonly WaitForEndOfFrame wait = new();
        private IEnumerator moveCaretToEnd() {
            yield return wait;
            
            inputField.caretPosition = 1;
            inputField.selectionStringAnchorPosition = 1;
            inputField.selectionStringFocusPosition  = 1;
            inputField.ForceLabelUpdate();
        }
# endif

        private IEnumerator refocus(int index) {
            yield return null;
            inputField.caretPosition = index;
            inputField.selectionStringAnchorPosition = index;
            inputField.selectionStringFocusPosition  = index;
            inputField.ForceLabelUpdate();
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

            StartCoroutine(refocus(charIndex));
        }
        
        // 1. 按下 Esc 的时候，没有自动把鼠标吸回去     修复
            // 观察：Editor 中按下 Esc 时，鼠标 Cursor 的脱离层级更高，不由 Cursor 这一层抽象接管
            // 只有鼠标重新点击 Editor 后，Cursor 这一层抽象才会继续生效
        // 3. 重新 Focus 失败了                     DEBUG测试
            // 因为 Deselect 根本没有被触发，可能只是 光标不在文本区 中了
        // 4. 在外部点击时，设置 Cursor Index 失败了  DEBUG测试
        // 5. 打开聊天框后仍然会转动头视角
    }
}