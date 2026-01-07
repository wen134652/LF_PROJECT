using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System;

/// <summary>
/// MyPlayerInput脚本的自定义Inspector编辑器
/// 提供可视化的按键绑定功能
/// </summary>
[CustomEditor(typeof(MyPlayerInput))]
[CanEditMultipleObjects]
public class MyPlayerInputEditor : Editor
{
    private MyPlayerInput targetScript;
    
    // 折叠面板状态（静态以保持一致性）
    private static bool showKeyBindingPanel = false;
    
    // 按键监听状态
    private string listeningForField = "";
    private bool isListening = false;

    private void OnEnable()
    {
        targetScript = (MyPlayerInput)target;
        
        // 订阅编辑器更新事件，确保输入能够被检测
        EditorApplication.update += OnEditorUpdate;
        
        // 订阅InputSystem事件（用于手柄输入）
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        StopListening();
        
        EditorApplication.update -= OnEditorUpdate;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnEditorUpdate()
    {
        // 如果正在监听输入，定期刷新Inspector并检查输入
        if (isListening)
        {
            // 检查手柄输入（使用InputSystem）
            CheckForGamepadInput();
            
            // 也检查键盘输入（使用InputSystem作为备用方案）
            // 注意：这主要用于确保能够捕获输入，即使Event系统没有触发
            if (deviceType == InputDeviceType.Keyboard || deviceType == InputDeviceType.Any)
            {
                CheckForKeyboardInput();
            }
            
            // 定期刷新Inspector以更新UI
            Repaint();
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // 设备连接/断开时刷新
        if (isListening)
        {
            Repaint();
        }
    }

    /// <summary>
    /// 检查键盘输入（在EditorUpdate中调用，作为备用方案）
    /// </summary>
    private void CheckForKeyboardInput()
    {
        if (!isListening) return;
        if (deviceType != InputDeviceType.Keyboard && deviceType != InputDeviceType.Any) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 检查ESC键
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            StopListening();
            return;
        }

        // 使用Keyboard的所有按键控件来检查，而不是遍历所有Key枚举值
        // 这样可以避免访问无效的按键导致异常
        try
        {
            // 检查所有键盘上的按键控件
            foreach (var control in keyboard.allKeys)
            {
                if (control == null) continue;
                
                // 获取按键的Key值（KeyControl.keyCode返回Key?，我们需要检查是否为null）
                Key? keyValue = control.keyCode;
                if (!keyValue.HasValue) continue;
                
                Key key = keyValue.Value;
                
                // 跳过ESC键（已单独处理）
                if (key == Key.Escape) continue;
                
                // 检查按键是否被按下
                if (control.wasPressedThisFrame)
                {
                    // 构建路径
                    string keyName = key.ToString().ToLower();
                    string path = $"<Keyboard>/{keyName}";
                    CompleteBinding(path);
                    return;
                }
            }
        }
        catch (System.Exception ex)
        {
            // 如果出现任何异常，记录但不影响功能
            // 主要输入检测在HandleKeyboardInput()中进行
            UnityEngine.Debug.LogWarning($"检查键盘输入时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 检查手柄输入（在EditorUpdate中调用）
    /// </summary>
    private void CheckForGamepadInput()
    {
        if (!isListening) return;

        // 检查手柄输入（使用InputSystem）
        if (deviceType == InputDeviceType.Gamepad || deviceType == InputDeviceType.Any)
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                // 检查所有按钮
                if (gamepad.buttonSouth.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/buttonSouth");
                    return;
                }
                if (gamepad.buttonNorth.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/buttonNorth");
                    return;
                }
                if (gamepad.buttonEast.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/buttonEast");
                    return;
                }
                if (gamepad.buttonWest.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/buttonWest");
                    return;
                }
                if (gamepad.leftShoulder.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/leftShoulder");
                    return;
                }
                if (gamepad.rightShoulder.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/rightShoulder");
                    return;
                }
                if (gamepad.leftTrigger.wasPressedThisFrame || gamepad.leftTrigger.ReadValue() > 0.5f)
                {
                    CompleteBinding("<Gamepad>/leftTrigger");
                    return;
                }
                if (gamepad.rightTrigger.wasPressedThisFrame || gamepad.rightTrigger.ReadValue() > 0.5f)
                {
                    CompleteBinding("<Gamepad>/rightTrigger");
                    return;
                }
                if (gamepad.leftStickButton.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/leftStickButton");
                    return;
                }
                if (gamepad.rightStickButton.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/rightStickButton");
                    return;
                }
                if (gamepad.startButton.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/startButton");
                    return;
                }
                if (gamepad.selectButton.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/selectButton");
                    return;
                }

                // 检查方向键
                if (gamepad.dpad.up.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/dpad/up");
                    return;
                }
                if (gamepad.dpad.down.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/dpad/down");
                    return;
                }
                if (gamepad.dpad.left.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/dpad/left");
                    return;
                }
                if (gamepad.dpad.right.wasPressedThisFrame)
                {
                    CompleteBinding("<Gamepad>/dpad/right");
                    return;
                }

                // 检查摇杆（需要特殊处理，因为摇杆是Vector2）
                Vector2 leftStick = gamepad.leftStick.ReadValue();
                if (leftStick.magnitude > 0.7f)
                {
                    CompleteBinding("<Gamepad>/leftStick");
                    return;
                }
                Vector2 rightStick = gamepad.rightStick.ReadValue();
                if (rightStick.magnitude > 0.7f)
                {
                    CompleteBinding("<Gamepad>/rightStick");
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 将KeyCode转换为InputSystem的Key名称
    /// </summary>
    private string ConvertKeyCodeToInputSystemKey(KeyCode keyCode)
    {
        // KeyCode到InputSystem Key名称的映射
        // 大部分KeyCode名称与InputSystem的Key名称相同（小写）
        string keyName = keyCode.ToString().ToLower();
        
        // 处理一些特殊情况
        switch (keyCode)
        {
            case KeyCode.LeftControl:
                return "leftCtrl";
            case KeyCode.RightControl:
                return "rightCtrl";
            case KeyCode.LeftShift:
                return "leftShift";
            case KeyCode.RightShift:
                return "rightShift";
            case KeyCode.LeftAlt:
                return "leftAlt";
            case KeyCode.RightAlt:
                return "rightAlt";
            // LeftCommand 和 LeftApple 在Unity中是同一个值，只使用一个
            case KeyCode.LeftCommand:
                return "leftCommand";
            // RightCommand 和 RightApple 在Unity中是同一个值，只使用一个
            case KeyCode.RightCommand:
                return "rightCommand";
            case KeyCode.Mouse0:
                return "leftButton";
            case KeyCode.Mouse1:
                return "rightButton";
            case KeyCode.Mouse2:
                return "middleButton";
            default:
                return keyName;
        }
    }

    private string GetControlPath(InputControl control, InputDevice device)
    {
        // 直接使用InputSystem提供的路径，这是最可靠的方法
        try
        {
            string path = control.path;
            
            // InputSystem的路径格式通常是完整的，如 "/Keyboard/escape" 或 "/Gamepad/buttonSouth"
            // 我们需要将其转换为 "<Keyboard>/escape" 或 "<Gamepad>/buttonSouth" 格式
            
            if (path.StartsWith("/Keyboard/"))
            {
                // 移除开头的 "/Keyboard/" 并添加 "<Keyboard>/"
                string keyName = path.Substring("/Keyboard/".Length);
                return $"<Keyboard>/{keyName}";
            }
            else if (path.StartsWith("/Gamepad/"))
            {
                // 移除开头的 "/Gamepad/" 并添加 "<Gamepad>/"
                string buttonName = path.Substring("/Gamepad/".Length);
                return $"<Gamepad>/{buttonName}";
            }
            else if (path.Contains("<") && path.Contains(">"))
            {
                // 如果路径已经包含设备标签，直接返回
                return path;
            }
            else
            {
                // 尝试从设备路径推断
                if (device is Keyboard)
                {
                    // 移除设备路径前缀
                    string devicePath = device.path;
                    if (path.StartsWith(devicePath))
                    {
                        string controlPath = path.Substring(devicePath.Length);
                        if (controlPath.StartsWith("/"))
                            controlPath = controlPath.Substring(1);
                        return $"<Keyboard>/{controlPath}";
                    }
                }
                else if (device is Gamepad)
                {
                    // 移除设备路径前缀
                    string devicePath = device.path;
                    if (path.StartsWith(devicePath))
                    {
                        string controlPath = path.Substring(devicePath.Length);
                        if (controlPath.StartsWith("/"))
                            controlPath = controlPath.Substring(1);
                        return $"<Gamepad>/{controlPath}";
                    }
                }
            }
            
            // 如果以上都失败，返回原始路径
            return path;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"获取控制路径时出错: {ex.Message}");
            return "";
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制默认属性
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        showKeyBindingPanel = EditorGUILayout.Foldout(showKeyBindingPanel, "按键映射配置", true, EditorStyles.foldoutHeader);

        if (showKeyBindingPanel)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("点击'绑定'按钮后，按下您想要绑定的按键。支持键盘和手柄输入。", MessageType.Info);

            EditorGUILayout.Space(5);

            // 绘制移动绑定
            DrawMoveBinding();

            EditorGUILayout.Space(5);

            // 绘制其他按键绑定
            DrawKeyBinding("跳跃", serializedObject.FindProperty("jumpBinding"));
            DrawKeyBinding("左手使用", serializedObject.FindProperty("leftHandBinding"));
            DrawKeyBinding("右手使用", serializedObject.FindProperty("rightHandBinding"));
            DrawKeyBinding("左手投掷", serializedObject.FindProperty("throwLeftBinding"));
            DrawKeyBinding("右手投掷", serializedObject.FindProperty("throwRightBinding"));
            DrawKeyBinding("打开背包", serializedObject.FindProperty("openInventoryBinding"));
            DrawKeyBinding("交互", serializedObject.FindProperty("interactBinding"));
            DrawKeyBinding("记事短按", serializedObject.FindProperty("quickPickTapBinding"));
            DrawKeyBinding("记事长按", serializedObject.FindProperty("pickHoldBinding"));

            // 绘制瞄准摇杆绑定
            DrawStringBinding("瞄准摇杆", serializedObject.FindProperty("aimStickBinding"));
            EditorGUI.indentLevel--;
        }

        // 如果正在监听输入，显示提示并处理输入（即使面板收起也要处理）
        if (isListening)
        {
            EditorGUILayout.Space(5);
            
            string deviceTypeText = deviceType == InputDeviceType.Keyboard ? "键盘" : 
                                   deviceType == InputDeviceType.Gamepad ? "手柄" : "键盘或手柄";
            EditorGUILayout.HelpBox($"正在监听: {listeningForField}\n设备类型: {deviceTypeText}\n\n⚠️ 重要提示：\n1. 请先点击此Inspector窗口确保它有焦点\n2. 然后按下您想要绑定的按键\n3. 按ESC键可以取消绑定", MessageType.Warning);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("取消绑定", GUILayout.Height(30)))
            {
                StopListening();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            
            // 处理键盘输入（必须在所有GUI绘制之后调用）
            HandleKeyboardInput();
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// 绘制移动绑定（特殊处理，包含4个方向键）
    /// </summary>
    private void DrawMoveBinding()
    {
        EditorGUILayout.LabelField("移动绑定", EditorStyles.boldLabel);
        
        SerializedProperty moveBinding = serializedObject.FindProperty("moveBinding");
        
        EditorGUI.indentLevel++;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("上", GUILayout.Width(50));
        EditorGUILayout.PropertyField(moveBinding.FindPropertyRelative("keyboardUp"), GUIContent.none);
        if (GUILayout.Button("绑定键盘", GUILayout.Width(80)))
        {
            StartListening("移动-上方向键(键盘)", (path) => {
                moveBinding.FindPropertyRelative("keyboardUp").stringValue = path;
            }, InputDeviceType.Keyboard);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("下", GUILayout.Width(50));
        EditorGUILayout.PropertyField(moveBinding.FindPropertyRelative("keyboardDown"), GUIContent.none);
        if (GUILayout.Button("绑定键盘", GUILayout.Width(80)))
        {
            StartListening("移动-下方向键(键盘)", (path) => {
                moveBinding.FindPropertyRelative("keyboardDown").stringValue = path;
            }, InputDeviceType.Keyboard);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("左", GUILayout.Width(50));
        EditorGUILayout.PropertyField(moveBinding.FindPropertyRelative("keyboardLeft"), GUIContent.none);
        if (GUILayout.Button("绑定键盘", GUILayout.Width(80)))
        {
            StartListening("移动-左方向键(键盘)", (path) => {
                moveBinding.FindPropertyRelative("keyboardLeft").stringValue = path;
            }, InputDeviceType.Keyboard);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("右", GUILayout.Width(50));
        EditorGUILayout.PropertyField(moveBinding.FindPropertyRelative("keyboardRight"), GUIContent.none);
        if (GUILayout.Button("绑定键盘", GUILayout.Width(80)))
        {
            StartListening("移动-右方向键(键盘)", (path) => {
                moveBinding.FindPropertyRelative("keyboardRight").stringValue = path;
            }, InputDeviceType.Keyboard);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("手柄摇杆", GUILayout.Width(80));
        EditorGUILayout.PropertyField(moveBinding.FindPropertyRelative("gamepadStick"), GUIContent.none);
        if (GUILayout.Button("绑定手柄", GUILayout.Width(80)))
        {
            StartListening("移动-手柄摇杆", (path) => {
                moveBinding.FindPropertyRelative("gamepadStick").stringValue = path;
            }, InputDeviceType.Gamepad);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 绘制按键绑定（KeyBinding类型）
    /// </summary>
    private void DrawKeyBinding(string label, SerializedProperty property)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("键盘", GUILayout.Width(50));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("keyboardBinding"), GUIContent.none);
        if (GUILayout.Button("绑定", GUILayout.Width(60)))
        {
            StartListening($"{label}-键盘", (path) => {
                property.FindPropertyRelative("keyboardBinding").stringValue = path;
            }, InputDeviceType.Keyboard);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("手柄", GUILayout.Width(50));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("gamepadBinding"), GUIContent.none);
        if (GUILayout.Button("绑定", GUILayout.Width(60)))
        {
            StartListening($"{label}-手柄", (path) => {
                property.FindPropertyRelative("gamepadBinding").stringValue = path;
            }, InputDeviceType.Gamepad);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(3);
    }

    /// <summary>
    /// 绘制字符串绑定（如瞄准摇杆）
    /// </summary>
    private void DrawStringBinding(string label, SerializedProperty property)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(property, GUIContent.none);
        if (GUILayout.Button("绑定手柄", GUILayout.Width(80)))
        {
            StartListening($"{label}", (path) => {
                property.stringValue = path;
            }, InputDeviceType.Gamepad);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (!isListening) return;
        if (Event.current == null) return;

        Event evt = Event.current;
        
        // 只处理KeyDown事件，避免重复处理
        // 注意：在Inspector窗口中，按键事件只有在窗口有焦点时才会被触发
        if (evt.type == EventType.KeyDown)
        {
            KeyCode keyCode = evt.keyCode;
            
            // 跳过None键
            if (keyCode == KeyCode.None)
            {
                return;
            }
            
            // 检查ESC键
            if (keyCode == KeyCode.Escape)
            {
                StopListening();
                evt.Use();
                GUIUtility.ExitGUI();
                return;
            }
            
            // 检查其他键盘按键（仅在监听键盘输入时）
            if (deviceType == InputDeviceType.Keyboard || deviceType == InputDeviceType.Any)
            {
                // 跳过鼠标按键（Mouse0-Mouse6）
                if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
                {
                    return;
                }
                
                // 将KeyCode转换为InputSystem的Key名称
                string keyName = ConvertKeyCodeToInputSystemKey(keyCode);
                if (!string.IsNullOrEmpty(keyName) && keyName != "none")
                {
                    string path = $"<Keyboard>/{keyName}";
                    CompleteBinding(path);
                    // 标记事件已使用，防止被其他控件处理
                    evt.Use();
                    GUIUtility.ExitGUI();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 开始监听输入
    /// </summary>
    private void StartListening(string fieldName, Action<string> onComplete, InputDeviceType deviceType)
    {
        listeningForField = fieldName;
        isListening = true;
        this.deviceType = deviceType;
        this.onCompleteCallback = onComplete;
        showKeyBindingPanel = true;
        
        // 刷新Inspector
        Repaint();
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    private void StopListening()
    {
        isListening = false;
        listeningForField = "";
        deviceType = InputDeviceType.Any;
        onCompleteCallback = null;
    }

    private InputDeviceType deviceType = InputDeviceType.Any;
    private Action<string> onCompleteCallback;

    private enum InputDeviceType
    {
        Any,
        Keyboard,
        Gamepad
    }


    /// <summary>
    /// 完成绑定
    /// </summary>
    private void CompleteBinding(string path)
    {
        if (onCompleteCallback != null && !string.IsNullOrEmpty(path))
        {
            onCompleteCallback(path);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(targetScript);
            
            // 标记场景为已修改
            if (!EditorApplication.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
        StopListening();
        Repaint();
    }
}

