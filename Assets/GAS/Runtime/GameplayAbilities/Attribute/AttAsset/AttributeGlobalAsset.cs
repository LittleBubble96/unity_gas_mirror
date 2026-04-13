using UnityEngine;
using Debug = UnityEngine.Debug;

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace VSEngine.GAS
{

    [System.Serializable]
    public class AttributeSetData
    {
        [BoxGroup("$SetName"), LabelText("属性集名称")]
        public string SetName = "AS_NewSet";

        [BoxGroup("$SetName"), LabelText("属性列表",SdfIconType.SuitSpade)]
        [ListDrawerSettings(ShowIndexLabels = false, DraggableItems = true, OnTitleBarGUI = "DrawAddButton",HideAddButton = true)]
        public List<string> AttributeNames = new List<string>();

        private void DrawAddButton()
        {
            Color storedColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("添加属性", GUILayout.Height(25)))
            {
                AttributeNames.Add("NewAttribute");
            }
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("导出属性集", GUILayout.Height(25)))
            {
                ExportSingleSet(this);
                AssetDatabase.Refresh();
            }
            GUI.backgroundColor = storedColor;
        }

        public void ExportSingleSet(AttributeSetData data)
        {
            if (data == null) return;
            string className = ValidateClassName(data.SetName);
            string templateContent = LoadTemplate();
            string finalCode = ReplaceTemplate(templateContent, className, data.AttributeNames);
            string path = $"{GASSettingAsset.GAS_AttributeSet_SCRIPT_PATH}/{className}.cs";
            File.WriteAllText(path, finalCode);
            Debug.Log($"已生成：{path}");
        }

        private string LoadTemplate()
        {
            // 如果指定了模板文件且存在，则读取
            if (!string.IsNullOrEmpty(GASSettingAsset.GAS_ATTRIBUTE_TEMPLATE_PATH) && File.Exists(GASSettingAsset.GAS_ATTRIBUTE_TEMPLATE_PATH))
            {
                return File.ReadAllText(GASSettingAsset.GAS_ATTRIBUTE_TEMPLATE_PATH);
            }

            // 否则使用硬编码默认模板（防止缺失）
            Debug.LogWarning("未找到模板文件，使用内置默认模板。");
            return GetDefaultTemplate();
        }

        private string GetDefaultTemplate()
        {
            return @"[System.Serializable]
public class [ClassName] : AttributeSet
{
    [AttributeFields]

    protected override AttributeBase GetInternal(string attName)
    {
        [GetInternalBody]
        return null;
    }

    public override string[] AttributeNames => new string[]
    {
        [AttributeNamesArray]
    };
}";
        }

        private string ReplaceTemplate(string template, string className, List<string> attributes)
        {
            // 去重
            attributes = new List<string>(new HashSet<string>(attributes));
            // 生成字段声明
            StringBuilder fieldsBuilder = new StringBuilder();
            foreach (var attr in attributes)
            {
                string fieldName = attr.Trim();
                if (string.IsNullOrEmpty(fieldName)) continue;
                fieldsBuilder.AppendLine(
                    $"    public AttributeBase {fieldName} = new AttributeBase(\"{className}\", \"{fieldName}\");");
            }

            // 生成 GetInternal 方法体
            StringBuilder getInternalBuilder = new StringBuilder();
            foreach (var attr in attributes)
            {
                string fieldName = attr.Trim();
                if (string.IsNullOrEmpty(fieldName)) continue;
                getInternalBuilder.AppendLine($"        if (attName.Equals(\"{fieldName}\"))");
                getInternalBuilder.AppendLine($"        {{");
                getInternalBuilder.AppendLine($"            return {fieldName};");
                getInternalBuilder.AppendLine($"        }}");
            }

            // 生成属性名数组
            StringBuilder arrayBuilder = new StringBuilder();
            for (int i = 0; i < attributes.Count; i++)
            {
                string fieldName = attributes[i].Trim();
                if (string.IsNullOrEmpty(fieldName)) continue;
                arrayBuilder.Append($"        \"{fieldName}\"");
                if (i < attributes.Count - 1) arrayBuilder.AppendLine(",");
                else arrayBuilder.AppendLine();
            }

            // 替换占位符
            string result = template
                .Replace("[ClassName]", className)
                .Replace("[AttributeFields]", fieldsBuilder.ToString().TrimEnd())
                .Replace("[GetInternalBody]", getInternalBuilder.ToString().TrimEnd())
                .Replace("[AttributeNamesArray]", arrayBuilder.ToString().TrimEnd());

            return result;
        }

        public string ValidateClassName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName))
                rawName = "AS_Unknown";
            string valid = System.Text.RegularExpressions.Regex.Replace(rawName, @"[^a-zA-Z0-9_]", "");
            if (string.IsNullOrEmpty(valid))
                valid = "AS_Default";
            if (!char.IsLetter(valid[0]))
                valid = "A" + valid;
            return valid;
        }
        
        /// <summary>
        /// 生成单个属性集在全局字典里的条目
        /// </summary>
        public string GetGlobalDictionaryItem(string className)
        {
            return $"        {{ \"{className}\", new {className}() }},";
        }
    }
    
    public class AttributeGlobalAsset : ScriptableObject
    {
        
        [LabelText("所有属性集")]
        [ListDrawerSettings(DraggableItems = true, ShowIndexLabels = false, OnTitleBarGUI = "DrawGlobalAddButton" , HideAddButton = true)]
        public List<AttributeSetData> attributeSets = new List<AttributeSetData>();

        private void DrawGlobalAddButton()
        {
            if (GUILayout.Button("添加属性集", GUILayout.Height(30)))
            {
                attributeSets.Add(new AttributeSetData());
            }
        }
        
        [Button("导出全局属性集lib",ButtonSizes.Large), GUIColor(0.2f, 0.8f, 0.4f), BoxGroup("批量操作")]
        private void ExportGlobalAttributeLib()
        {
            // 1. 加载模板
            string template;
            if (!string.IsNullOrEmpty(GASSettingAsset.GAS_ATTRIBUTE_GLOBAL_TEMPLATE_PATH) && 
                File.Exists(GASSettingAsset.GAS_ATTRIBUTE_GLOBAL_TEMPLATE_PATH))
            {
                template = File.ReadAllText(GASSettingAsset.GAS_ATTRIBUTE_GLOBAL_TEMPLATE_PATH);
            }
            else
            {
                Debug.LogWarning("未找到全局字典模板，使用内置模板");
                template = @"﻿using System.Collections.Generic;
using VSEngine.GAS;
public static class AttributeGlobalLib
{
    public static Dictionary<string, AttributeSet> GlobalAttributeSets = new Dictionary<string, AttributeSet>()
    {
[GlobalDictionaryItems]
    };
}";
            }

            // 2. 生成所有字典条目
            StringBuilder sb = new StringBuilder();
            foreach (var set in attributeSets)
            {
                string className = set.ValidateClassName(set.SetName);
                sb.AppendLine(set.GetGlobalDictionaryItem(className));
            }

            // 3. 替换并写入
            string finalCode = template.Replace("[GlobalDictionaryItems]", sb.ToString().TrimEnd());
            string path = $"{GASSettingAsset.GAS_AttributeSet_SCRIPT_PATH}/AttributeGlobalLib.cs";
            File.WriteAllText(path, finalCode);

            AssetDatabase.Refresh();
            Debug.Log($"全局字典已生成：{path}");
        }

        [Button("导出所有属性集",ButtonSizes.Large), GUIColor(0.3f, 0.8f, 1f), BoxGroup("批量操作")]
        private void ExportAllAttributeSets()
        {
            foreach (var data in attributeSets)
            {
                data.ExportSingleSet(data);
            }
            // 自动生成全局字典
            ExportGlobalAttributeLib();
            AssetDatabase.Refresh();
            Debug.Log("所有属性集已导出");
        }

        [Button("清空所有属性",ButtonSizes.Large), GUIColor(0.8f, 0.6f, 0.2f), BoxGroup("批量操作")]
        private void ClearAll()
        {
            if (EditorUtility.DisplayDialog("清空所有", "确定要删除所有属性集配置吗？此操作不可撤销。", "确定", "取消"))
            {
                attributeSets.Clear();
                EditorUtility.SetDirty(this);
            }
        }
        
    }
}