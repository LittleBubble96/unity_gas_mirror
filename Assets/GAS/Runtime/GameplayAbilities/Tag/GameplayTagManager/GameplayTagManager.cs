using System.Collections.Generic;
using UnityEditor;

namespace VSEngine.GAS
{
    public partial class GameplayTagManager
    {
        private GameplayTagNode _rootNode;
        
        private Dictionary<GameplayTag,GameplayTagNode> _tagNodeMap = new Dictionary<GameplayTag, GameplayTagNode>();

        public void Init()
        {
            //加载所有的标签数据
            var asset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(GASSettingAsset.GAS_TAG_ASSET_PATH);
            _rootNode = new GameplayTagNode();
            if (asset != null)
            {
                foreach (var tag in asset.Tags)
                {
                    AddTag(tag);
                }
            }
        }

        public IEnumerator<GameplayTag> GetParentTags(GameplayTag tag)
        {
            GameplayTagNode currentNode = _tagNodeMap.ContainsKey(tag) ? _tagNodeMap[tag] : null;
            while (currentNode != null)
            {
                yield return currentNode.Tag;
                currentNode = currentNode.ParentNode;
            }
        }

        private void AddTag(GameplayTag tag)
        {
            var pathSegments = tag.TagName.Split('.');
            var currentNode = _rootNode;
            if (pathSegments.Length == 0)
            {
                return;
            }
            string currentPath = "";
            foreach (var segment in pathSegments)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}.{segment}";
                if (!currentNode.ChildNodes.TryGetValue(segment, out var childNode))
                {
                    childNode = new GameplayTagNode();
                    childNode.Tag = new GameplayTag
                    {
                        TagName = currentPath,
                    };
                    childNode.NodeName = segment;
                    childNode.ParentNode = currentNode;
                    currentNode.ChildNodes.Add(segment, childNode);
                    _tagNodeMap.Add(childNode.Tag, childNode);
                }
                currentNode = childNode;
            }
        }
        
    }
}