using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    [UnityEditor.CustomEditor(typeof(AbilitySystemComponent))]
    public class AbilitySystemComponentEditor : UnityEditor.Editor
    {
        // 要添加到子物体上的脚本类型（可根据需要修改）
        private List<System.Type> dependencyScriptTypes = new List<Type>()
        {
            typeof(GameplayAbilitySpecContainer),
            typeof(ActivateTagCountContainer),
            typeof(BlockAbilityTagCountContainer),
            typeof(GameplayEffectContainer),
            typeof(AttributeSetContainer),
            typeof(GameplayCueContainer)
        }; // 替换为你的依赖脚本

        // 子物体的固定名称（方便识别）
        private const string ChildName = "_DependencyChild";

        public override void OnInspectorGUI()
        {
            // 绘制默认 Inspector
            DrawDefaultInspector();

            EditorGUILayout.Space();

            // 添加依赖按钮
            if (GUILayout.Button("添加依赖组件"))
            {
                AddDependency();
            }

            // 移除依赖按钮
            if (GUILayout.Button("移除依赖组件"))
            {
                RemoveDependency();
            }
        }

        private void AddDependency()
        {
            AbilitySystemComponent targetComp = (AbilitySystemComponent)target;
            Transform parent = targetComp.transform;

            // 检查是否已存在子物体
            Transform existingChild = parent.Find(ChildName);
            if (existingChild != null)
            {
                Debug.LogWarning($"子物体 {ChildName} 已存在，无需重复添加。");
                return;
            }

            // 创建空子物体
            GameObject child = new GameObject(ChildName);
            // 注册 Undo 以便撤销
            Undo.RegisterCreatedObjectUndo(child, "Add Dependency Child");
            child.transform.SetParent(parent, false); // 保持本地坐标归零

            foreach (var dependency in dependencyScriptTypes)
            {
                // 添加指定脚本
                if (dependency != null && dependency.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    Undo.AddComponent(child, dependency);
                }
                else
                {
                    Debug.LogError($"依赖脚本类型 {dependency?.Name} 无效，请确保它继承自 MonoBehaviour。");
                }
            }
           

            // 刷新 Inspector 显示
            EditorUtility.SetDirty(parent.gameObject);
            Selection.activeGameObject = child; // 可选：选中新创建的子物体
        }

        private void RemoveDependency()
        {
            AbilitySystemComponent targetComp = (AbilitySystemComponent)target;
            Transform parent = targetComp.transform;

            Transform existingChild = parent.Find(ChildName);
            if (existingChild == null)
            {
                Debug.LogWarning($"未找到子物体 {ChildName}，无法移除。");
                return;
            }

            // 记录销毁操作以便撤销
            Undo.DestroyObjectImmediate(existingChild.gameObject);
            EditorUtility.SetDirty(parent.gameObject);
        }
    }
}