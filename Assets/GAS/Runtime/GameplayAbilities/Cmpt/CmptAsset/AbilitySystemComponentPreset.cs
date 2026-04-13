using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace VSEngine.GAS
{
    [CreateAssetMenu(fileName = "AbilitySystemComponentPreset", menuName = "XY/AbilitySystemComponentPreset")]
    public class AbilitySystemComponentPreset : ScriptableObject
    {
        [LabelText("角色定位名称")]
        public string Name;

        [LabelText("定位描述")]
        public string Description;
        
        [LabelText("属性集")] 
        [ValueDropdown("AttributeSetChoice")]
        public List<string> AttributeSets = new List<string>();

        [LabelText("初始技能")] 
        public List<GameplayAbilityAsset> GameplayAbilities = new List<GameplayAbilityAsset>();
        
        [Title("Tags",bold:true)]
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("描述性质的标签，用来描述角色初始标签，比如阵容等。")]
        public List<GameplayTag> AttachTags = new List<GameplayTag>();

        private static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();
        private static IEnumerable AttributeSetChoice = new ValueDropdownList<string>();

        [Button("刷新所有资源")]
        private void Refresh()
        {
            SetTagChoices();
            SetAttributeSetChoices();
        }

        private void OnEnable()
        {
            SetTagChoices();
            SetAttributeSetChoices();
        }
        
        static void SetAttributeSetChoices()
        {
            var asset = AssetDatabase.LoadAssetAtPath<AttributeGlobalAsset>(GASSettingAsset.GAS_Attribute_ASSET_PATH);
            if (asset != null)
            {
                var choices = new ValueDropdownList<string>();

                foreach (var attributeSet in asset.attributeSets)
                {
                    choices.Add(attributeSet.SetName, attributeSet.SetName);
                }
                AttributeSetChoice = choices;
            }
            else
            {
                AttributeSetChoice = new ValueDropdownList<string>();
            }
        }

        private void SetTagChoices()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(GASSettingAsset.GAS_TAG_ASSET_PATH);
            if (asset != null)
            {
                var choices = new ValueDropdownList<GameplayTag>();

                foreach (var tag in asset.Tags)
                {
                    choices.Add(tag.TagName, tag);
                }
                TagChoices = choices;
            }
            else
            {
                TagChoices = new ValueDropdownList<GameplayTag>();
            }
        }
    }
}