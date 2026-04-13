using UnityEditor;
using UnityEngine;
using VSEngine.GAS;

namespace GAS.Editor
{
    public class GameplayAttEditorsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateGameplayTagsSettingsProvider()
        {
            var provider = new SettingsProvider("Project/GAS/AttributeGlobalAsset", SettingsScope.Project)
            {
                label = "Att Global",
                guiHandler = (searchContext) =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath<AttributeGlobalAsset>(GASSettingAsset.GAS_Attribute_ASSET_PATH);
                    if (asset == null)
                    {
                        EditorGUILayout.HelpBox("No AttributeGlobalAsset found. Please create one.", MessageType.Warning);
                        if (GUILayout.Button("Create AttributeGlobalAsset"))
                        {
                            GasDefine.CheckGasAssetFolder();
                            var newAsset = ScriptableObject.CreateInstance<AttributeGlobalAsset>();
                            AssetDatabase.CreateAsset(newAsset, GASSettingAsset.GAS_Attribute_ASSET_PATH);
                            AssetDatabase.SaveAssets();
                            EditorUtility.FocusProjectWindow();
                            Selection.activeObject = newAsset;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Edit your att in the AttributeGlobalAsset.");
                        if (GUILayout.Button("Open AttributeGlobalAsset"))
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