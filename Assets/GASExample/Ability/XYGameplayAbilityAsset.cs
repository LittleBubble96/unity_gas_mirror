using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS.Ability
{
    [CreateAssetMenu(fileName = "GameplayAbilityAsset", menuName = "XY/GameplayAbilityAsset")]
    public class XYGameplayAbilityAsset : GameplayAbilityAsset
    {
        [LabelText("是否从给予技能时立即激活")]
        public bool BActiveFromGiveAbility = true;
    }
}