using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    public class GASSetting
    {
        private static GASSettingAsset settingAsset;
        private static UnityEditor.Editor _editor;
        
        [SettingsProvider]
        private static SettingsProvider AttributeSetManagerSetting()
        {
            var provider = new SettingsProvider("Project/GAS Setting", SettingsScope.Project)
            {
                guiHandler = key => { SettingGUI(); },
                keywords = new[] { "GAS", "Setting" }
            };
            return provider;
        }
        
        private static void SettingGUI()
        {
            if (settingAsset == null) Load();

            EditorGUILayout.BeginVertical();
            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        private static void Load()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GASSettingAsset>(GasDefine.GAS_SYSTEM_ASSET_PATH);
            if (asset == null)
            {
                GasDefine.CheckGasAssetFolder();
                var a = ScriptableObject.CreateInstance<GASSettingAsset>();
                AssetDatabase.CreateAsset(a, GasDefine.GAS_SYSTEM_ASSET_PATH);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                asset = ScriptableObject.CreateInstance<GASSettingAsset>();
            }

            settingAsset = asset;
            _editor = UnityEditor.Editor.CreateEditor(asset);
        }
        
    }
}