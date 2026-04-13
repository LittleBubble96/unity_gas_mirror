using UnityEditor;
using UnityEngine;
using VSEngine.GAS;

namespace GAS.Editor
{
    public class GameplayCueEditorsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateGameplayCueSettingsProvider()
        {
            var provider = new SettingsProvider("Project/GAS/CueGlobalAsset", SettingsScope.Project)
            {
                label = "Cue Global",
                guiHandler = (searchContext) =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath<CueGlobalAsset>(GASSettingAsset.GAS_Cue_ASSET_PATH);
                    if (asset == null)
                    {
                        EditorGUILayout.HelpBox("No CueGlobalAsset found. Please create one.", MessageType.Warning);
                        if (GUILayout.Button("Create CueGlobalAsset"))
                        {
                            GasDefine.CheckGasAssetFolder();
                            var newAsset = ScriptableObject.CreateInstance<CueGlobalAsset>();
                            AssetDatabase.CreateAsset(newAsset, GASSettingAsset.GAS_Cue_ASSET_PATH);
                            AssetDatabase.SaveAssets();
                            EditorUtility.FocusProjectWindow();
                            Selection.activeObject = newAsset;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Edit your att in the CueGlobalAsset.");
                        if (GUILayout.Button("Open CueGlobalAsset"))
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