using System.Collections.Generic;

namespace VSEngine.GAS
{
    public class GameplayTagNode
    {
        public GameplayTag Tag;

        public string NodeName;
        
        public Dictionary<string,GameplayTagNode> ChildNodes = new Dictionary<string,GameplayTagNode>();
        
        public GameplayTagNode ParentNode;
    }
}