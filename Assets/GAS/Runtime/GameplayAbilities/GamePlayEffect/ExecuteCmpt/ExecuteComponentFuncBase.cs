using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEditor;

namespace VSEngine.GAS
{
    [Serializable]
    [IgnoreReadTypeEditor]
    public class ExecuteComponentFunc
    {
        //是否可以执行GE
        public virtual bool CanApply(GameplayEffectSpec spec, GameplayEffectContext context)
        {
            return true;
        }
        
        public virtual void OnApply(GameplayEffectSpec spec, GameplayEffectContext context)
        {
            
        }
        
        public virtual void OnUnapply(GameplayEffectSpec spec, GameplayEffectContext context)
        {
            
        }

        //获取缓存赋予标签
        public virtual List<GameplayTag> GetCacheGrantedTags()
        {
            return null;
        }
        
#if UNITY_EDITOR
        
        public virtual void InitEditor()
        {
        }
#endif
    }
    
    [Serializable]
    [IgnoreReadTypeEditor]
    public class ExecuteComponentFuncTags : ExecuteComponentFunc
    {
#if UNITY_EDITOR
        
        public override void InitEditor()
        {
            SetTagChoices();
        }
        
        protected static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();

        protected void SetTagChoices()
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
    
    
    public struct GameplayEffectContext
    {
        public AbilitySystemComponent Owner;
        
        public static GameplayEffectContext MakeOutgoingSpec(AbilitySystemComponent target)
        {
            return new GameplayEffectContext
            {
                Owner = target
            };
        }
    }
}