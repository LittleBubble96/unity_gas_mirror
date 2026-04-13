using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    public class GASSettingAsset : ScriptableObject
    {
        [LabelText("GAS原始配置路径")] 
        [LabelWidth(200)]
        [FolderPath]
        [OnValueChanged("Save")]
        public string GASAssetPath = "Assets/GAS";
        
        [LabelText("GAS 配置路径")] 
        [LabelWidth(200)]
        [FolderPath]
        [OnValueChanged("Save")]
        public string GASConfigAssetPath = "Assets/GAS_Config";
        
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        [LabelWidth(200)]
        [LabelText("Tags 资源配置")]
        public string Gas_Tag_Asset_Path => $"{Setting.GASConfigAssetPath}/GameplayTagsAsset.asset";
        
        public static string GAS_TAG_ASSET_PATH => Setting.Gas_Tag_Asset_Path;
        
        [LabelText("Attribute 模板路径")]
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        public string Gas_attribute_template_Path => $"{Setting.GASAssetPath}/Editor/Attribute/Template/AttributeSetTemplate.txt";
        public static string GAS_ATTRIBUTE_TEMPLATE_PATH => Setting.Gas_attribute_template_Path;
        
        [LabelText("Attribute 全局模板路径")]
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        public string Gas_attribute_global_template_Path => $"{Setting.GASAssetPath}/Editor/Attribute/Template/AttributeSetGlobalLibTemplate.txt";
        
        public static string GAS_ATTRIBUTE_GLOBAL_TEMPLATE_PATH => Setting.Gas_attribute_global_template_Path;

        
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        [LabelWidth(200)]
        [LabelText("Attribute 资源配置")]
        public string Gas_AttributeGolbal_Asset_Path => $"{Setting.GASConfigAssetPath}/GameplayAttributeAsset.asset";
        
        public static string GAS_Attribute_ASSET_PATH => Setting.Gas_AttributeGolbal_Asset_Path;
        
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        [LabelWidth(200)]
        [LabelText("Attribute Set导出路径")]
        public string Gas_AttributeSetScript_Path => $"{Setting.GASConfigAssetPath}/Attribute";
        
        public static string GAS_AttributeSet_SCRIPT_PATH => Setting.Gas_AttributeSetScript_Path;


        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        [LabelWidth(200)]
        [LabelText("Cue 资源配置")]
        public string Gas_CueGolbal_Asset_Path => $"{Setting.GASConfigAssetPath}/GameplayCueAsset.asset";
        
        public static string GAS_Cue_ASSET_PATH => Setting.Gas_CueGolbal_Asset_Path;
        
        [ShowInInspector]
        [DisplayAsString(TextAlignment.Left,true)]
        [LabelWidth(200)]
        [LabelText("技能Timeline 资源配置")]
        public string Gas_Timeline_Asset_Path => $"{Setting.GASConfigAssetPath}/Ability/Timeline/";
        
        public static string GAS_Timeline_ASSET_PATH => Setting.Gas_Timeline_Asset_Path;
        
        
        
        private static GASSettingAsset _setting;
        private static GASSettingAsset Setting
        {
            get
            {
                if (_setting == null) _setting = Load();

                return _setting;
            }
        }
        
        private static GASSettingAsset Load()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GASSettingAsset>(GasDefine.GAS_SYSTEM_ASSET_PATH);
            if (asset == null)
            {
                GasDefine.CheckGasAssetFolder();

                var a = CreateInstance<GASSettingAsset>();
                AssetDatabase.CreateAsset(a, GasDefine.GAS_SYSTEM_ASSET_PATH);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                asset = AssetDatabase.LoadAssetAtPath<GASSettingAsset>(GasDefine.GAS_SYSTEM_ASSET_PATH);
            }

            return asset;
        }
        
        void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}