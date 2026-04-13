using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS
{
    [CreateAssetMenu(fileName = "VSAbilitySystemComponentPreset", menuName = "XY/VSAbilitySystemComponentPreset")]
    public class VSAbilitySystemComponentPreset : AbilitySystemComponentPreset
    {
        [LabelText("初始效果（属性初始化）")] 
        public List<GamePlayEffectAsset> InitEffects = new List<GamePlayEffectAsset>();
    }
}