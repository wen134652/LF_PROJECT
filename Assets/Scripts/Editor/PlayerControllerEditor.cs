using UnityEngine;
using UnityEditor;

/// <summary>
/// PlayerController 的自定义 Inspector 编辑器
/// 添加按钮用于重新应用配置数据
/// </summary>
[CustomEditor(typeof(PlayerController))]
[CanEditMultipleObjects]
public class PlayerControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认 Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // 获取目标对象
        PlayerController playerController = (PlayerController)target;

        // 绘制分隔线
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // 配置数据应用区域
        EditorGUILayout.LabelField("配置数据管理", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("点击下方按钮将配置数据资产中的数值应用到所有相关组件（MovementController、GroundChecker 等）", MessageType.Info);

        EditorGUILayout.Space(5);

        // 应用配置数据按钮
        GUI.enabled = playerController.configData != null;
        if (GUILayout.Button("应用配置数据到所有组件", GUILayout.Height(30)))
        {
            // 应用配置数据
            playerController.ApplyConfigData();

            // 标记场景为已修改（如果在编辑器中）
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(playerController);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }
        GUI.enabled = true;

        // 如果未分配配置数据，显示警告
        if (playerController.configData == null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("未分配 PlayerConfigData 资产，请先分配配置数据资产。", MessageType.Warning);
        }
    }
}

