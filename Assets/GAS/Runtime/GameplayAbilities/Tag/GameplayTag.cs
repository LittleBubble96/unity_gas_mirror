using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VSEngine.GAS
{
    [Serializable]
    public struct GameplayTag
    {
        [HideInInspector]
        public string TagName;
        
        public bool Equals(GameplayTag other)
        {
            return TagName == other.TagName;
        }
        
        public bool HasAnyParentTag(GameplayTag other)
        {
            IEnumerator<GameplayTag> parentTags = GetParentTags();
            while (parentTags.MoveNext())
            {
                if (parentTags.Current.Equals(other))
                {
                    return true;
                }
            }
            return false;
        }
        
        public IEnumerator<GameplayTag> GetParentTags()
        {
            return AbilitySystemGlobals.Get().GetTagManager().GetParentTags(this);
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(TagName);
        }

        public override string ToString()
        {
            return TagName;
        }
        
        public GameplayTag(string tagName)
        {
            TagName = tagName;
        }
    }
    
    //CustomReadWriteFunctions
    public static class GameplayTagSerializer
    {
        public static void WriteGameplayTag(this Mirror.NetworkWriter writer, GameplayTag tag)
        {
            writer.WriteString(tag.TagName);
        }

        public static GameplayTag ReadGameplayTag(this Mirror.NetworkReader reader)
        {
            GameplayTag tag = new GameplayTag();
            tag.TagName = reader.ReadString();
            return tag;
        }
    }
}