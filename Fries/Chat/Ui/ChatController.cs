using System;
using System.Collections;
using Fries.EvtSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
# if InputSys
using Fries.InputDispatch;
using UnityEngine.InputSystem;
# endif

namespace Fries.Chat.Ui {
    public class ChatController : MonoBehaviour {
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

        [SerializeField] private bool cursorLocked = false;
        
        # if InputSys
        [SerializeField] private bool useInputDispatcher;
        [SerializeField] private string entranceInputLayerName;
        [SerializeField] private string blockAllLayerName;
        # endif
        
        private bool isChatboxOpen = false;
        # if InputSys
        private static InputId ret = Key.Enter;
        private static InputId t = Key.T;
        private static InputId slash = Key.Slash;
        private InputLayer entranceLayer;
        private InputLayer blockAllLayer;
        # endif
        
        # if UNITY_EDITOR
        private const KeyCode exitKey = KeyCode.BackQuote;
        # else
        private const KeyCode exitKey = KeyCode.Escape;
        # endif
        
# if TMPro
        private TMP_InputField inputField;
        private Image image;
        private void Awake() {
            inputField ??= gameObject.GetComponentInChildren<TMP_InputField>(true);
            if (!inputField) throw new InvalidOperationException("Input field does not exist!");
            float transparency = PlayerPrefs.GetFloat("ChatTransparency", 1);
            image = inputField.transform.parent.gameObject.getComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, transparency);
            
            image.enabled = false;
            inputField.gameObject.SetActive(false);
        }

        private void Start() {
            new TransparencyPropComm(image);
            # if InputSys
            if (!useInputDispatcher) return;
            if (string.IsNullOrEmpty(entranceInputLayerName))
                throw new ArgumentException("Input Layer Name cannot be null or empty!");
            entranceLayer = InputLayer.get(entranceInputLayerName);
            blockAllLayer = InputLayer.get(blockAllLayerName);
            # endif
        }
        
        private IEnumerator lockCursor() {
            if (!cursorLocked) yield break;
            
            yield return null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update() {
            if (isChatboxOpen) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                bool isLMBDown() {
                    # if InputSys
                    if (useInputDispatcher) {
                        if (blockAllLayer.isConsumingAllInputs()) return Input.GetMouseButtonDown(0);
                        return false;
                    }
                    # endif
                    return Input.GetMouseButtonDown(0);
                }
                if (isLMBDown()) moveCaretToMovePosX();

                bool isReturnKeyDown() {
                    # if InputSys
                    if (useInputDispatcher) {
                        if (blockAllLayer.isConsumingAllInputs()) return Input.GetKeyDown(KeyCode.Return);
                        return false;
                    }
                    # endif
                    return Input.GetKeyDown(KeyCode.Return);
                }
                bool isEscapeKeyDown() {
                    # if InputSys
                    if (useInputDispatcher) {
                        if (blockAllLayer.isConsumingAllInputs()) return Input.GetKeyDown(exitKey);
                        return false;
                    }
                    # endif
                    return Input.GetKeyDown(exitKey);
                }
                
                if (isReturnKeyDown()) {
                    if (writer == null) 
                        throw new InvalidOperationException(
                            "Writer does not exist! Maybe you forgot to instantiate Chat Core prefab.");
                    
                    writer.write(inputField.text);
                    inputField.text = "";
                    isChatboxOpen = false;
                    inputField.DeactivateInputField();
                    EventSystem.current.SetSelectedGameObject(null);

                    image.enabled = false;
                    inputField.gameObject.SetActive(false);
                    StartCoroutine(lockCursor());
                    Evt.TriggerNonAlloc<OnChatboxClosed>();
# if InputSys
                    if (useInputDispatcher) blockAllLayer.disable();
# endif
                }
                else if (isChatboxOpen && isEscapeKeyDown()) {
                    inputField.text = "";
                    isChatboxOpen = false;
                    inputField.DeactivateInputField();
                    EventSystem.current.SetSelectedGameObject(null);
                    
                    image.enabled = false;
                    inputField.gameObject.SetActive(false);
                    StartCoroutine(lockCursor());
                    Evt.TriggerNonAlloc<OnChatboxClosed>();
# if InputSys
                    if (useInputDispatcher) blockAllLayer.disable();
# endif
                }
            }

            else if (!isChatboxOpen) {
                bool isTKeyDown() {
                    # if InputSys
                    if (useInputDispatcher) return entranceLayer.isDown(t);
                    # endif
                    return Input.GetKeyDown(KeyCode.T);
                }
                bool isSlashKeyDown() {
                    # if InputSys
                    if (useInputDispatcher) return entranceLayer.isDown(slash);
                    # endif
                    return Input.GetKeyDown(KeyCode.Slash);
                }
                
                if (isTKeyDown()) {
                    isChatboxOpen = true;
                    image.enabled = true;
                    inputField.gameObject.SetActive(true);
                    
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    
                    Evt.TriggerNonAlloc<OnChatboxOpened>();
# if InputSys
                    if (useInputDispatcher) blockAllLayer.enable();
# endif
                }
                else if (isSlashKeyDown()) {
                    isChatboxOpen = true;
                    image.enabled = true;
                    inputField.gameObject.SetActive(true);
                    
                    EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
                    inputField.ActivateInputField();
                    inputField.text = "/";

                    StartCoroutine(moveCaretToEnd());
                    Evt.TriggerNonAlloc<OnChatboxOpened>();
# if InputSys
                    if (useInputDispatcher) blockAllLayer.enable();
# endif
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

            inputField.ActivateInputField();
            StartCoroutine(refocus(charIndex));
        }
# endif
    }
}