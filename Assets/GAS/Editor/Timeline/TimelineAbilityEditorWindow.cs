using VSEngine.GAS;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace GAS.Editor
{

    [CustomEditor(typeof(GameplayAbilityAsset))]
    public class TimelineAbilityEditorWindow : OdinEditor
    {
        private GameplayAbilityAsset _asset => target as GameplayAbilityAsset;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("编辑Timeline", GUILayout.Height(30), GUILayout.Width(300))) EditAbilityTimeline();
            EditorGUILayout.EndVertical();
        }

        private void EditAbilityTimeline()
        {
            AbilityTimelineEditorWindow.ShowWindow(_asset);
        }
    }
}
#endif