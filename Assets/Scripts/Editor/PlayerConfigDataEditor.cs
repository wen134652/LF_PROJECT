using UnityEngine;
using UnityEditor;

/// <summary>
/// PlayerConfigData 的编辑器工具，用于快速创建和配置玩家配置资产
/// </summary>
public class PlayerConfigDataEditor : EditorWindow
{
    private string assetName = "PlayerConfig";
    private string savePath = "Assets/Data/Player";

    [MenuItem("Tools/Player/Create Player Config Data")]
    public static void ShowWindow()
    {
        GetWindow<PlayerConfigDataEditor>("创建玩家配置数据");
    }

    private void OnGUI()
    {
        GUILayout.Label("创建玩家配置数据资产", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        assetName = EditorGUILayout.TextField("资产名称:", assetName);
        savePath = EditorGUILayout.TextField("保存路径:", savePath);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("点击下方按钮创建新的玩家配置数据资产。创建后可以在Inspector中编辑所有配置参数。", MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("创建玩家配置数据", GUILayout.Height(30)))
        {
            CreatePlayerConfigData();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("打开保存文件夹", GUILayout.Height(25)))
        {
            if (AssetDatabase.IsValidFolder(savePath))
            {
                EditorUtility.FocusProjectWindow();
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(savePath);
                Selection.activeObject = obj;
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "文件夹不存在，请先创建文件夹或使用有效路径。", "确定");
            }
        }
    }

    private void CreatePlayerConfigData()
    {
        // 确保保存路径存在
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            // 尝试创建文件夹
            string[] folders = savePath.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }

        // 创建ScriptableObject实例
        PlayerConfigData configData = ScriptableObject.CreateInstance<PlayerConfigData>();

        // 生成唯一文件名
        string fileName = assetName;
        string fullPath = savePath + "/" + fileName + ".asset";
        int counter = 1;
        while (AssetDatabase.LoadAssetAtPath<PlayerConfigData>(fullPath) != null)
        {
            fileName = assetName + "_" + counter;
            fullPath = savePath + "/" + fileName + ".asset";
            counter++;
        }

        // 保存资产
        AssetDatabase.CreateAsset(configData, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 选中新创建的资产
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = configData;

        EditorUtility.DisplayDialog("成功", $"玩家配置数据已创建：\n{fullPath}", "确定");
    }
}

