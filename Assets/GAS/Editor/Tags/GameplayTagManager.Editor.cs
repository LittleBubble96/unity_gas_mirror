namespace VSEngine.GAS
{
    public class GameplayTagManagerEditor
    {
        private GameplayTagNode _rootNode;
        
        public GameplayTagNode RootNode => _rootNode;
        
        public void Init()
        {
            _rootNode = new GameplayTagNode();
        }

        public void AddTag(GameplayTag tag)
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
                    childNode.Tag = new GameplayTag { TagName = currentPath };
                    childNode.ParentNode = currentNode;
                    childNode.NodeName = segment;
                    currentNode.ChildNodes.Add(segment, childNode);
                }

                currentNode = childNode;
            }
        }
    }
}