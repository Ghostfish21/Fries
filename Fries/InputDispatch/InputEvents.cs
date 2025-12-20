using Fries.EvtSystem;

namespace Fries.InputDispatch {
    public class InputEvents {
        [EvtDeclarer] public struct BeforeInputDispatcherSetup { InputDispatcher dispatcher; }
        [EvtDeclarer] public struct BeforeKeyboardAxisSetup { KeyboardAxisInputModule module; }
        [EvtDeclarer] public struct BeforeMouseAxisSetup { MouseAxisInputModule module; }
    }
}