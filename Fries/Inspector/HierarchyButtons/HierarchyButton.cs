using System;
using System.Reflection;

namespace Fries.Inspector.HierarchyButtons {
    using UnityEngine;
    using UnityEngine.Events;
    # if UNITY_EDITOR
    using UnityEditor;
    # endif

    /// <summary>
    /// HierarchyButton组件，允许引用脚本文件并选择其中的静态方法，或使用UnityEvent，在Hierarchy中点击时执行
    /// </summary>
    public class HierarchyButton : MonoBehaviour {
        private void Awake() {
            Destroy(this);
        }

# if UNITY_EDITOR

        /// <summary>
        /// 按钮响应模式
        /// </summary>
        public enum ResponseMode {
            StaticMethod, // 使用静态方法
            InstanceMethod // 使用UnityEvent
        }
        

        [SerializeField] private ResponseMode responseMode = ResponseMode.StaticMethod;

        [SerializeField] private MonoScript _targetScript;
        [SerializeField] private string _selectedMethodName;
        [SerializeField] private MonoBehaviour _targetMonoBehaviour;
        [SerializeField] private string _selectedMethodName4Mono;
        

        /// <summary>
        /// 获取按钮响应模式
        /// </summary>
        public ResponseMode mode => responseMode;

        /// <summary>
        /// 获取目标脚本
        /// </summary>
        public MonoScript targetScript => _targetScript;

        /// <summary>
        /// 获取选择的方法名称
        /// </summary>
        public string selectedMethodName => _selectedMethodName;

        /// <summary>
        /// 触发按钮点击事件
        /// </summary>
        public void triggerButtonClick(Event evt) {
            if (_targetMonoBehaviour == null) return;
            if (_selectedMethodName4Mono == null) return;
            if (responseMode == ResponseMode.InstanceMethod) {
                MethodInfo mi = _targetMonoBehaviour.GetType().GetMethod(_selectedMethodName4Mono);
                mi?.Invoke(_targetMonoBehaviour, new object[] {evt});
            }
        }
        #endif
    }

}