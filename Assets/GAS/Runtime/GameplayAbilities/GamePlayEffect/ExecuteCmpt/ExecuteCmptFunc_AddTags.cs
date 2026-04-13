using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;

namespace VSEngine.GAS
{
    [System.Serializable]
    [LabelText("附加标签")]
    public class ExecuteComponentFuncAddTags : ExecuteComponentFuncTags
    {
        [ListDrawerSettings]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [LabelText("标签")]
        public List<GameplayTag> tags = new List<GameplayTag>();

        public override List<GameplayTag> GetCacheGrantedTags()
        {
            return tags;
        }
        
        public override void OnApply(GameplayEffectSpec spec, GameplayEffectContext context)
        {
            AbilitySystemComponent abilitySystemComponent = context.Owner;
            if (!abilitySystemComponent)
            {
                return;
            }
            foreach (var t in tags)
            {
                abilitySystemComponent.AddGameplayTagToCountMap(t);
            }
        }

        public override void OnUnapply(GameplayEffectSpec spec, GameplayEffectContext context)
        {
            AbilitySystemComponent abilitySystemComponent = context.Owner;
            if (!abilitySystemComponent)
            {
                return;
            }
            //移除标签
            foreach (var t in tags)
            {
                abilitySystemComponent.RemoveGameplayTagFromCountMap(t);
            }
        }
    }
    
    //CustomReadWriteFunctions
    public static class ExecuteComponentFuncAddTagsSerializer
    {
        public static void WriteExecuteComponentFuncAddTags(this NetworkWriter writer, ExecuteComponentFuncAddTags value)
        {
            writer.WriteInt(value.tags.Count);
            foreach (var tag in value.tags)
            {
                writer.Write(tag);
            }
        }

        public static ExecuteComponentFuncAddTags ReadExecuteComponentFuncAddTags(this NetworkReader reader)
        {
            int count = reader.ReadInt();
            var tags = new List<GameplayTag>();
            for (int i = 0; i < count; i++)
            {
                tags.Add(reader.Read<GameplayTag>());
            }
            return new ExecuteComponentFuncAddTags { tags = tags };
        }
    }
}