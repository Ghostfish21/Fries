using System;
using Fries.InsertionEventSys;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fries.Chat.Ui {
    public class ChatController : MonoBehaviour {
        private static ChatCore.Writer writer;
        [EvtListener(typeof(ChatCore), "OnInitiated")]
        private static void onChatInitiated() {
            writer = ChatCore.create("Player");
        }
        [EvtListener(typeof(ChatCore), "PostMsgPrinted")]
        private static void onMsgPrinted(Message message) {
            Debug.Log(message.content);
        }
        
        private bool isChatboxOpen = false;
        
# if TMPro
        private TMP_InputField inputField;
        private void Awake() {
            inputField ??= gameObject.GetComponentInChildren<TMP_InputField>(true);
            if (!inputField) throw new InvalidOperationException("Input field does not exist!");
        }
        private void Update() {
            if (isChatboxOpen) {
                if (Input.GetKeyDown(KeyCode.Return)) {
                    if (writer == null) 
                        throw new InvalidOperationException(
                            "Writer does not exist! Maybe you forgot to instantiate Chat Core prefab.");
                    
                    writer.write(inputField.text);
                    inputField.text = "";
                    isChatboxOpen = false;
                }
                else if (isChatboxOpen && Input.GetKeyDown(KeyCode.Escape)) {
                    inputField.text = "";
                    isChatboxOpen = false;
                    inputField.DeactivateInputField();
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            else if (!isChatboxOpen) {
                if (Input.GetKeyDown(KeyCode.T)) {
                    isChatboxOpen = true;
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                }
                else if (Input.GetKeyDown(KeyCode.Slash)) {
                    isChatboxOpen = true;
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    inputField.text = "/";
                    inputField.caretPosition = 1;
                    inputField.selectionStringAnchorPosition = 1;
                    inputField.selectionStringFocusPosition  = 1;
                }
            }
        }
# endif
    }
}