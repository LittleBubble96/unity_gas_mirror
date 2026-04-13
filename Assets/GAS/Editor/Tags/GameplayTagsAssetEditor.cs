﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VSEngine.GAS
{
    [CustomEditor(typeof(GameplayTagsAsset))]
    public class GameplayTagsAssetEditor : Editor
    {
        private GameplayTagsAsset _tagsAsset;
        private GameplayTagManagerEditor _tagManager;

        private readonly Dictionary<string, bool> _nodeFoldoutStates = new Dictionary<string, bool>();
        private string _newTagInput = ""; // 顶部新增标签输入框

        private void OnEnable()
        {
            _tagManager = new GameplayTagManagerEditor();
            _tagsAsset = (GameplayTagsAsset)target;
            RefreshTagManager();
        }

        /// <summary>
        /// 刷新树结构（每次操作后重新解析）
        /// </summary>
        private void RefreshTagManager()
        {
            _tagManager.Init();
            if (_tagsAsset.Tags != null)
            {
                foreach (var tag in _tagsAsset.Tags)
                {
                    if (tag.IsValid())
                        _tagManager.AddTag(tag);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            // ======================
            // 顶部：统一输入框添加标签
            // ======================
            GUILayout.BeginHorizontal();
            _newTagInput = EditorGUILayout.TextField("新增标签（例：A.B.C）", _newTagInput);
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                AddNewTagByInput();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.LabelField("=== 标签树状结构 ===", EditorStyles.boldLabel);
            DrawTagTree(_tagManager.RootNode, 0);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_tagsAsset);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// 绘制树节点
        /// 规则：非叶子节点 = 折叠 + 添加 + 删除
        ///      叶子节点 = 只显示名字，无任何按钮
        /// </summary>
        private void DrawTagTree(GameplayTagNode node, int indent)
        {
            if (node == null) return;
            bool isRoot = string.IsNullOrEmpty(node.NodeName);
            bool isLeaf = node.ChildNodes?.Count == 0; // 叶子节点（最后一层）

            // 绘制非根节点
            if (!isRoot)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(indent * 16);

                // ======================
                // 非叶子节点：显示折叠
                // ======================
                if (!isLeaf)
                {
                    string key = node.Tag.TagName;
                    if (!_nodeFoldoutStates.ContainsKey(key))
                        _nodeFoldoutStates[key] = true;

                    _nodeFoldoutStates[key] = EditorGUILayout.Foldout(_nodeFoldoutStates[key], node.NodeName, true);
                }
                // ======================
                // 叶子节点：只显示名字
                // ======================
                else
                {
                    EditorGUILayout.LabelField(node.NodeName, GUILayout.Width(180));
                }

                // ======================
                // 非叶子节点：显示按钮
                // 叶子节点：不显示任何按钮
                // ======================
                if (!isLeaf)
                {
                    // // 添加子节点
                    // if (GUILayout.Button("添加子", GUILayout.Width(50)))
                    // {
                    //     AddChildTag(node.Tag.TagName);
                    // }

                    // 删除当前节点
                    // if (GUILayout.Button("删除", GUILayout.Width(50)))
                    // {
                    //     DeleteTag(node.Tag.TagName);
                    // }
                }

                GUILayout.EndHorizontal();
            }

            // 判断是否展开子节点
            bool drawChildren = true;
            if (!isRoot && !isLeaf)
            {
                drawChildren = _nodeFoldoutStates.TryGetValue(node.Tag.TagName, out bool state) && state;
            }

            // 递归绘制子节点
            if (drawChildren && node.ChildNodes != null)
            {
                foreach (var child in node.ChildNodes.Values)
                {
                    DrawTagTree(child, indent + 1);
                }
            }
        }

        /// <summary>
        /// 顶部输入框添加标签
        /// </summary>
        private void AddNewTagByInput()
        {
            if (string.IsNullOrWhiteSpace(_newTagInput))
            {
                EditorUtility.DisplayDialog("提示", "标签不能为空", "确定");
                return;
            }

            var tags = _tagsAsset.Tags?.ToList() ?? new List<GameplayTag>();
            if (tags.Any(t => t.TagName == _newTagInput))
            {
                EditorUtility.DisplayDialog("提示", "标签已存在", "确定");
                return;
            }

            tags.Add(new GameplayTag { TagName = _newTagInput });
            _tagsAsset.Tags = tags.ToArray();
            _newTagInput = ""; // 清空输入框
            RefreshTagManager();
        }

        /// <summary>
        /// 给节点添加子标签（节点按钮用）
        /// </summary>
        private void AddChildTag(string parentPath)
        {
            string childName = $"New{DateTime.Now:MMssff}";
            string fullTag = $"{parentPath}.{childName}";

            var tags = _tagsAsset.Tags?.ToList() ?? new List<GameplayTag>();
            if (!tags.Any(t => t.TagName == fullTag))
            {
                tags.Add(new GameplayTag { TagName = fullTag });
                _tagsAsset.Tags = tags.ToArray();
                RefreshTagManager();
            }
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        private void DeleteTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) return;

            var tags = _tagsAsset.Tags?.ToList() ?? new List<GameplayTag>();
            var removeTag = tags.FirstOrDefault(t => t.TagName == tagName);
            tags.Remove(removeTag);
            _tagsAsset.Tags = tags.ToArray();
            RefreshTagManager();
        }
    }
}