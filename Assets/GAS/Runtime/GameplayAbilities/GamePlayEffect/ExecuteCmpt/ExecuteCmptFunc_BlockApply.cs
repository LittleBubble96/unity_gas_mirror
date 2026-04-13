using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace VSEngine.GAS
{
    public class ExecuteComponentFuncBlockApply : ExecuteComponentFunc
    {
        [ListDrawerSettings]
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [LabelText("标签")]
        public List<GameplayTag> tags = new List<GameplayTag>();
        
        public override bool CanApply(GameplayEffectSpec spec, GameplayEffectContext context)
        {
            AbilitySystemComponent abilitySystemComponent = context.Owner;
            if (!abilitySystemComponent)
            {
                return true;
            }
            //如果拥有标签则阻止应用GE
            foreach (var t in tags)
            {
                if (abilitySystemComponent.HasAnyMatchingGameplayTag(t))
                {
                    return false;
                }
            }
            return true;
        }
    }
}