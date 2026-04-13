using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    [System.Serializable]
    public struct GameplayTagContainer
    {
        public GameplayTagContainer(IEnumerable<GameplayTag> tags)
        {
            if (tags == null)
            {
                GameplayTags = null;
                return;
            }
            GameplayTags = new List<GameplayTag>(tags);
        }
        
        public GameplayTagContainer(params GameplayTag[] tags)
        {
            if (tags == null)
            {
                GameplayTags = null;
                return;
            }
            GameplayTags = new List<GameplayTag>(tags);
        }
        
        public List<GameplayTag> GameplayTags { get; private set; }

        public bool IsEmpty()
        {
            return GameplayTags == null || GameplayTags.Count == 0;
        }
        
        public void AddTags(IEnumerable<GameplayTag> tags)
        {
            if (tags == null)
            {
                return;
            }
            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }
        
        public void AddTag(GameplayTag tag)
        {
            GameplayTags ??= new List<GameplayTag>();
            if (!GameplayTags.Contains(tag))
            {
                GameplayTags.Add(tag);
            }
        }
        
        public void RemoveTag(GameplayTag tag)
        {
            GameplayTags?.Remove(tag);
        }

        /// <summary>
        /// 检查是否存在所有标签
        /// </summary>
        public bool HasAll(GameplayTagContainer checkContainer)
        {
            if (checkContainer.IsEmpty())
            {
                return true;
            }
            foreach (var targetTag in checkContainer.GameplayTags)
            {
                if (!GameplayTags.Contains(targetTag) && !HasTagWithParents(targetTag))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 是否存在任意标签
        /// </summary>
        public bool HasAny(GameplayTagContainer checkContainer)
        {
            if (checkContainer.IsEmpty())
            {
                return false;
            }
            foreach (var targetTag in checkContainer.GameplayTags)
            {
                if (GameplayTags.Contains(targetTag) || HasTagWithParents(targetTag))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private bool HasTagWithParents(GameplayTag checkTag)
        {
            foreach (var gameplayTag in GameplayTags)
            {
                if (gameplayTag.HasAnyParentTag(checkTag))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    //Custom Write and Read for Mirror
    public static class GameplayTagContainerSerializer
    {
        public static void WriteGameplayTagContainer(this NetworkWriter writer, GameplayTagContainer ability)
        {
            if (ability.GameplayTags == null)
            {
                writer.WriteInt(0);
                return;
            }
            writer.WriteInt(ability.GameplayTags.Count);
            foreach (var tag in ability.GameplayTags)
            {
                writer.WriteGameplayTag(tag);
            }
        }

        public static GameplayTagContainer ReadGameplayTagContainer(this NetworkReader reader)
        {
            int tagCount = reader.ReadInt();
            List<GameplayTag> tags = new List<GameplayTag>();
            for (int i = 0; i < tagCount; i++)
            {
                tags.Add(reader.ReadGameplayTag());
            }
            return new GameplayTagContainer(tags);
        }
    }
}