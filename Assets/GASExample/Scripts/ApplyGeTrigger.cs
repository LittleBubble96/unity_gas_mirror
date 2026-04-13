using System;
using UnityEngine;
using VSEngine.GAS;

public class ApplyGeTrigger : MonoBehaviour
{
    [SerializeField] private GamePlayEffectAsset applyGeAsset;

    private GameplayEffectSpecHandle _ownSpecHandle;
    private void OnTriggerEnter(Collider other)
    {
        var abilitySystem = other.GetComponent<AbilitySystemComponent>();
        if (_ownSpecHandle.IsValid())
        {
            return;
        }
        if (abilitySystem != null && abilitySystem.isServer)
        {
            _ownSpecHandle = abilitySystem.ApplyGameplayEffectSpecToSelf(GameplayEffectSpec.MakeSpec(applyGeAsset),default);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var abilitySystem = other.GetComponent<AbilitySystemComponent>();
        if (abilitySystem != null && _ownSpecHandle.IsValid()  && abilitySystem.isServer)
        {
            abilitySystem.RemoveGameplayEffectSpec(_ownSpecHandle);
            _ownSpecHandle = GameplayEffectSpecHandle.UnValidHandle;
        }
    }
}