using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    [CreateAssetMenu(fileName = "GameplayAbilityAsset", menuName = "XY/GameplayAbilityAsset")]
    public class GameplayAbilityAsset : ScriptableObject
    {
        [LabelText("技能名称")]
        public string Name;

        [LabelText("技能描述")] public string Desc;

        [Title("技能策略",bold:true)]
        [Tooltip("技能的执行策略，决定了技能在网络环境下的执行方式。")]
        [LabelText("技能执行策略")]
        public GameplayAbilityNetExecutionPolicy NetExecutionPolicy;
        
        [Tooltip("技能的实例化策略，决定了技能实例的生命周期和共享方式。")]
        [LabelText("技能实例化策略")]
        public GameplayAbilityInstancingPolicy InstancingPolicy;
        
        [Title("Input",bold:true)]
        [Tooltip("技能输入的名称，和InputAction中的名称对应，用于AbilitySystemComponent中绑定输入事件。")]
        [LabelText("输入事件")]
        public string InputActionName;
        
        // Tags
        [Title("Tags",bold:true)]
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("技能标签，技能可以拥有多个标签，用于分类和查询技能。")]
        [LabelText("技能标签")]
        public List<GameplayTag> AssetTags = new List<GameplayTag>();
        
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("用于配置GA之间的打断关系")]
        [LabelText("取消带标签得能力")]
        public List<GameplayTag> CancelAbilitiesWithTag = new List<GameplayTag>();
        
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("用于配置GA之间的互斥关系。")]
        [LabelText("用标签组织能力")]
        public List<GameplayTag> BlockAbilitiesWithTag = new List<GameplayTag>();

        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("该GA激活时会给GA的拥有者附加的Tag。")]
        [LabelText("激活已拥有标签")]
        public List<GameplayTag> ActivationOwnedTags = new List<GameplayTag>();
        
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("想要该GA能被激活 GA的拥有者必须要有的Tag集合。")]
        [LabelText("激活所需标签")]
        public List<GameplayTag> ActivationRequiredTags = new List<GameplayTag>();
        
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("想要该GA能被激活 GA的拥有者必须不能有的Tag集合")]
        [LabelText("激活阻止标签")]
        public List<GameplayTag> ActivationBlockedTags = new List<GameplayTag>();
        
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("Source上必须拥有这些GameplayTag时才会被激活。")]
        [LabelText("源所需标签")]
        public List<GameplayTag> SourceRequiredTags = new List<GameplayTag>();
        
        [ListDrawerSettings()]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [Tooltip("Source上如果拥有这些标签则不会被激活。")]
        [LabelText("源被阻止标签")]
        public List<GameplayTag> SourceBlockedTags = new List<GameplayTag>();
        
        //GE 相关
        [Title("GE",bold:true)]
        [Tooltip("技能的冷却GameplayEffect，技能执行后会应用这个GameplayEffect来实现冷却效果。")]
        [LabelText("冷却GE")]
        public GamePlayEffectAsset CooldownGameplayEffect;
        
        [Tooltip("技能的消耗GameplayEffect，技能执行时会应用这个GameplayEffect来实现资源消耗效果。")]
        [LabelText("消耗GE")]
        public GamePlayEffectAsset CostGameplayEffect;
        
        
        private static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();

        private void OnEnable()
        {
            SetTagChoices();
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