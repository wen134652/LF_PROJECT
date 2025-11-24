using UnityEngine;
using UnityEditor;

/// <summary>
/// ReadOnly属性的绘制器，使字段在Inspector中显示为灰色且不可编辑
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 保存原始GUI状态
        bool wasEnabled = GUI.enabled;
        
        // 禁用GUI，使字段不可编辑
        GUI.enabled = false;
        
        // 绘制属性字段
        EditorGUI.PropertyField(position, property, label, true);
        
        // 恢复GUI状态
        GUI.enabled = wasEnabled;
    }
}

