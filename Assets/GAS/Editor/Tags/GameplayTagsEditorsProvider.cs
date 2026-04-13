using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    public class GameplayTagsEditorsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateGameplayTagsSettingsProvider()
        {
            var provider = new SettingsProvider("Project/GAS/GameplayTags", SettingsScope.Project)
            {
                label = "Gameplay Tags",
                guiHandler = (searchContext) =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(GASSettingAsset.GAS_TAG_ASSET_PATH);
                    if (asset == null)
                    {
                        EditorGUILayout.HelpBox("No GameplayTagsAsset found. Please create one.", MessageType.Warning);
                        if (GUILayout.Button("Create GameplayTagsAsset"))
                        {
                            GasDefine.CheckGasAssetFolder();
                            var newAsset = ScriptableObject.CreateInstance<GameplayTagsAsset>();
                            AssetDatabase.CreateAsset(newAsset, GASSettingAsset.GAS_TAG_ASSET_PATH);
                            AssetDatabase.SaveAssets();
                            EditorUtility.FocusProjectWindow();
                            Selection.activeObject = newAsset;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Edit your Gameplay Tags in the GameplayTagsAsset.");
                        if (GUILayout.Button("Open GameplayTagsAsset"))
                        {
                            Selection.activeObject = asset;
                        }
                    }
                },
            };
            return provider;
        }
    }
}