# if InputSys
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Fries.InputDispatch {
    public interface InputModule {
        Type deviceType { get; }
        // 如果试图获取时，该设备还未准备好或不存在，此方法应返回 null
        InputControl[] controlsToListenTo { get; }
        InputKind kind { get; }
        
        // 这个方法应该无论设备是否存在都能够成功执行。不过这个方法外有异常处理，
        // 如果它一定要依赖设备存在后 再执行应该也不会报错
        void setup();
        // 这个方法以及本类的剩余方法应该无论设备是否存在都能够成功执行
        void reset();
        void catchInput(InputControl control, InputEventPtr eventPtr);
        void onDeviceChange(InputDevice device, InputDeviceChange change);

        void beginUpdate(ulong tickVersion);
        void consume(int code);
        void requestStates(List<int> codes, Dictionary<InputId, float> heldInputs);
    }
}
# endif