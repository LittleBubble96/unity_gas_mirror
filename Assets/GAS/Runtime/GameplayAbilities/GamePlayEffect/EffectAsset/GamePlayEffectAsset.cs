using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    [CreateAssetMenu(fileName = "GamePlayEffectAsset", menuName = "XY/GamePlayEffectAsset")]
    public class GamePlayEffectAsset : ScriptableObject
    {
        [Title("组件")]
        [SerializeReference]  // 关键！让序列化支持多态
        [ListDrawerSettings(DraggableItems = true, ShowIndexLabels = true, OnTitleBarGUI = "DrawAddButton", HideAddButton = true)]
        [LabelText("效果组件")]
        public List<ExecuteComponentFunc> GeComponents = new List<ExecuteComponentFunc>();
        
        [Title("修改属性")]
        [LabelText("属性修改")]
        public List<GameplayEffectModifier> AttributeModifiers = new List<GameplayEffectModifier>();
        
        [Title("触发周期")]
        [LabelText("触发类型")]
        public GameplayEffectDurationType DurationType;
        
        [LabelText("持续时间")]
        [ShowIf(nameof(DurationType), GameplayEffectDurationType.Duration)]
        public float Duration; // 持续时间，单位秒（如果 DurationType 是 Duration）
        
        [LabelText("周期时间")]
        [HideIf(nameof(DurationType), GameplayEffectDurationType.Instant)]
        public float Period; // 周期时间，单位秒（如果 DurationType 是 Duration 或 Infinite）
        
        //Cues
        [Title("Cues")]
        [LabelText("执行时Cue(瞬时和周期性触发调用)")]
        [ValueDropdown(nameof(TagChoices))]
        public GameplayTag[] CueOnExecute;
        [LabelText("移除时Cue(GE被移除后调用)")]
        [ValueDropdown(nameof(TagChoices))]
        public GameplayTag[] CueOnRemove;
        [LabelText("添加时Cue(GE被添加后调用)")]
        [ValueDropdown(nameof(TagChoices))]
        public GameplayTag[] CueOnAdd;
        [LabelText("激活时Cue")]
        [ValueDropdown(nameof(TagChoices))]
        public GameplayTag[] CueOnActivate;
        [LabelText("失效时Cue")]
        [ValueDropdown(nameof(TagChoices))]
        public GameplayTag[] CueOnDeactivate;
        [LabelText("持续时Cue")]
        [ValueDropdown(nameof(TagChoices))]
        public GameplayTag[] CueDurational;
#if UNITY_EDITOR

        private void OnEnable()
        {
            SetTagChoices();
            SetAttributeChoices();
            // 初始化编辑器数据
            foreach (var component in GeComponents)
            {
                component.InitEditor();
            }
            GameplayEffectModifier.AttributeChoices = AttributeChoices;
        }

        // 下拉选项列表
        private static List<ValueDropdownItem<Type>> typeOptions;

        // 获取所有派生类（静态初始化，避免多次反射）
        private static List<ValueDropdownItem<Type>> GetTypeOptions()
        {
            if (typeOptions != null) return typeOptions;

            typeOptions = new List<ValueDropdownItem<Type>>();
            // 获取当前程序集中所有继承自 MyBaseClass 的非抽象类
            var baseType = typeof(ExecuteComponentFunc);
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in type.GetTypes())
                {
                    if (baseType.IsAssignableFrom(t) && !t.IsAbstract && t.GetCustomAttributes(typeof(LabelTextAttribute), false).Length > 0)
                    {
                        typeOptions.Add(new ValueDropdownItem<Type>(GetDisplayName(t), t));
                    }
                }
            }
            return typeOptions;
        }

        // 获取中文描述（优先使用 LabelText，否则用类名）
        private static string GetDisplayName(Type type)
        {
            if (type.GetCustomAttributes(typeof(LabelTextAttribute), false) is LabelTextAttribute[] label && label.Length > 0)
                return label[0].Text;
            return type.Name;
        }

        // 自定义添加按钮
        private void DrawAddButton()
        {
            // 使用 Odin 的 ValueDropdown 绘制一个弹出按钮
            if (GUILayout.Button("添加组件"))
            {
                var menu = new GenericMenu();
                foreach (var option in GetTypeOptions())
                {
                    menu.AddItem(new GUIContent(option.Text), false, () =>
                    {
                        // 点击后创建实例并添加到列表
                        if (Activator.CreateInstance(option.Value) is ExecuteComponentFunc instance)
                        {
                            instance.InitEditor();
                            GeComponents.Add(instance);
                        }
                    });
                }

                menu.ShowAsContext();
            }
        }
        
        
        private static IEnumerable AttributeChoices = new ValueDropdownList<string>();

        public static void SetAttributeChoices()
        {
            var asset = AssetDatabase.LoadAssetAtPath<AttributeGlobalAsset>(GASSettingAsset.GAS_Attribute_ASSET_PATH);
            if (asset != null)
            {
                var choices = new ValueDropdownList<string>();

                foreach (var attributeSet in asset.attributeSets)
                {
                    foreach (var attributeName in attributeSet.AttributeNames)
                    {
                        string fullAttributeName = $"{attributeSet.SetName}.{attributeName}";
                        choices.Add(fullAttributeName, fullAttributeName);
                    }
                }
                AttributeChoices = choices;
            }
            else
            {
                AttributeChoices = new ValueDropdownList<string>();
            }
        }
        
        protected static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();

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
        
#endif
    }
}