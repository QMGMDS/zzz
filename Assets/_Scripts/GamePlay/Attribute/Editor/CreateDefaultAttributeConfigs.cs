using UnityEditor;
using UnityEngine;

namespace GamePlay.Attribute.Editor
{
    /// <summary>创建默认属性配置 SO 资产，菜单项 Tools/Create Default Attribute Configs</summary>
    public static class CreateDefaultAttributeConfigs
    {
        [MenuItem("Tools/Create Default Attribute Configs")]
        private static void Create()
        {
            string dataPath = "Assets/Data/Attribute";
            if (!AssetDatabase.IsValidFolder(dataPath))
            {
                string parent = "Assets/Data";
                if (!AssetDatabase.IsValidFolder(parent))
                    AssetDatabase.CreateFolder("Assets", "Data");
                AssetDatabase.CreateFolder(parent, "Attribute");
            }

            CreateConfig(dataPath, "Player_AttributeConfig", 100f, 10f, 5f, 5f);
            CreateConfig(dataPath, "Enemy_AttributeConfig", 100f, 8f, 3f, 3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("默认属性配置 SO 已创建到 " + dataPath);
        }

        private static void CreateConfig(string path, string name, float hp, float atk, float def, float spd)
        {
            string fullPath = $"{path}/{name}.asset";
            CharacterAttributeSO existing = AssetDatabase.LoadAssetAtPath<CharacterAttributeSO>(fullPath);
            if (existing != null)
            {
                Debug.Log($"{fullPath} 已存在，跳过");
                return;
            }

            CharacterAttributeSO config = ScriptableObject.CreateInstance<CharacterAttributeSO>();
            config.MaxHealth = hp;
            config.Attack = atk;
            config.Defense = def;
            config.MoveSpeed = spd;

            AssetDatabase.CreateAsset(config, fullPath);
            Debug.Log($"已创建 {fullPath}");
        }
    }
}
