using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    [CreateAssetMenu(fileName = "CueGlobalAsset", menuName = "GAS/Cue Global Asset")]
    public class CueGlobalAsset : ScriptableObject
    {
        [Serializable]
        public class CueMapping
        {
            [LabelText("GameplayTag 标签")]
            [ValueDropdown("TagChoices", HideChildProperties = true)]
            [HorizontalGroup("Cue")]
            public GameplayTag Tag;

            [LabelText("ICue 类型")] [HorizontalGroup("Cue")]
            [ValueDropdown("CueTypeChoices", HideChildProperties = true)]
            [OnValueChanged("OnCueTypeChanged")]
            public string CueType;

            [SerializeReference]
            [LabelText("Cue 实例")]
            [ShowIf("ShowCueInstance")]
            public ICue Cue;

            [LabelText("描述信息")]
            [TextArea]
            public string Description;

            private bool ShowCueInstance => !string.IsNullOrEmpty(CueType);

            private static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();
            private static IEnumerable CueTypeChoices = new ValueDropdownList<string>();

            private void OnCueTypeChanged()
            {
                if (!string.IsNullOrEmpty(CueType))
                {
                    var type = Type.GetType(CueType);
                    if (type != null && typeof(ICue).IsAssignableFrom(type))
                    {
                        Cue = (ICue)Activator.CreateInstance(type);
                    }
                }
            }

            public static void UpdateTagChoices(GameplayTagsAsset tagsAsset)
            {
                var choices = new ValueDropdownList<GameplayTag>();
                if (tagsAsset != null)
                {
                    foreach (var tag in tagsAsset.Tags)
                    {
                        choices.Add(tag.TagName, tag);
                    }
                }
                TagChoices = choices;
            }
            
            public static void UpdateCueTypeChoices()
            {
                var choices = new ValueDropdownList<string>();
                var allTypes = Assembly.GetAssembly(typeof(ICue)).GetTypes();
                var cueTypes = allTypes
                    .Where(t => !t.IsInterface && !t.IsAbstract && CanBeICue(t))
                    .OrderBy(t => t.FullName);

                foreach (var type in cueTypes)
                {
                    if (type.GetCustomAttribute<CueLabelAttribute>() == null)
                    {
                        continue;
                    }
                    var labelAttr = type.GetCustomAttribute<CueLabelAttribute>();
                    string displayName = labelAttr != null ? labelAttr.Text : type.Name;
                    choices.Add(displayName, type.AssemblyQualifiedName);
                }

                CueTypeChoices = choices;
            }

            private static bool CanBeICue(Type type)
            {
                // 递归检查类型及其所有父类是否实现了 ICue 接口
                var currentType = type;
                while (currentType != null)
                {
                    // 检查当前类型是否直接实现了 ICue
                    if (currentType.GetInterface("VSEngine.GAS.ICue") != null)
                    {
                        return true;
                    }
                    currentType = currentType.BaseType;
                }
                return false;
            }
        }

        [ListDrawerSettings(
            CustomAddFunction = "CustomAddCueMapping",
            CustomRemoveElementFunction = "CustomRemoveCueMapping",
            ShowItemCount = true,
            NumberOfItemsPerPage = 20
        )]
        [LabelText("GameplayTag 与 ICue 的映射关系")]
        public List<CueMapping> CueMappings = new List<CueMapping>();

        private static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();

        [Button(ButtonSizes.Medium)]
        [GUIColor(0.7f, 0.9f, 0.7f)]
        private void ClearAllMappings()
        {
            if (EditorUtility.DisplayDialog("确认清除", "确定要清空所有映射关系吗?", "确定", "取消"))
            {
                CueMappings.Clear();
                EditorUtility.SetDirty(this);
            }
        }

        [Button(ButtonSizes.Medium)]
        [GUIColor(0.9f, 0.9f, 0.7f)]
        private void RemoveInvalidMappings()
        {
            int removedCount = 0;
            for (int i = CueMappings.Count - 1; i >= 0; i--)
            {
                if (!CueMappings[i].Tag.IsValid() || CueMappings[i].Cue == null)
                {
                    CueMappings.RemoveAt(i);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                EditorUtility.SetDirty(this);
                EditorUtility.DisplayDialog("完成", $"已移除 {removedCount} 个无效映射", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "没有发现无效映射", "确定");
            }
        }

        private CueMapping CustomAddCueMapping()
        {
            var mapping = new CueMapping();
            CueMappings.Add(mapping);
            SetTagChoices();
            CueMapping.UpdateCueTypeChoices();
            return mapping;
        }

        private void CustomRemoveCueMapping(CueMapping mapping)
        {
            CueMappings.Remove(mapping);
        }

        private void OnElementEdit(CueMapping mapping)
        {
            CueMapping.UpdateTagChoices(GetTagsAsset());
        }

        private GameplayTagsAsset GetTagsAsset()
        {
            return AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(GASSettingAsset.GAS_TAG_ASSET_PATH);
        }

        private void SetTagChoices()
        {
            var asset = GetTagsAsset();
            CueMapping.UpdateTagChoices(asset);
        }

        private void OnEnable()
        {
            SetTagChoices();
            CueMapping.UpdateCueTypeChoices();
        }

        /// <summary>
        /// 根据 GameplayTag 获取对应的 ICue
        /// </summary>
        public ICue GetCueByTag(GameplayTag tag)
        {
            foreach (var mapping in CueMappings)
            {
                if (mapping.Tag.Equals(tag))
                {
                    return mapping.Cue;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查是否存在某个 Tag 的映射
        /// </summary>
        public bool HasMappingForTag(GameplayTag tag)
        {
            foreach (var mapping in CueMappings)
            {
                if (mapping.Tag.Equals(tag))
                {
                    return true;
                }
            }
            return false;
        }

        private void OnValidate()
        {
            // 确保 CueMappings 不为空
            if (CueMappings == null)
            {
                CueMappings = new List<CueMapping>();
            }
        }
    }
}