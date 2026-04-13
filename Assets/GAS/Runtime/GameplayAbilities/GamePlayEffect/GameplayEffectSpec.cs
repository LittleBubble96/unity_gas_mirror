using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VSEngine.GAS
{
    [Serializable]
    public struct GameplayEffectSpec
    {
        //唯一id
        public GameplayEffectSpecHandle handle;
        //效果实例
        public GameplayEffect effect;
        //上下文信息
        public GameplayEffectParam effectParam;
        //是否激活
        public bool IsActive { get; private set; }
        //是否应用
        public bool IsApply { get; private set; }

        public List<ExecuteComponentFunc> GetExecuteComponents()
        {
            if (effect == null)
            {
                GasLogger.Error("[GAS] GameplayEffectSpec.GetExecuteComponents: effect is null");
                return new List<ExecuteComponentFunc>();
            }
            return effect.ExecuteComponentFuncList;
        }

        public List<GameplayEffectModifier> GetModifiers()
        {
            if (effect == null)
            {
                GasLogger.Error("[GAS] GameplayEffectSpec.GetModifiers: effect is null");
                return new List<GameplayEffectModifier>();
            }
            return effect.Modifiers;
        }

        public float GetDuration()
        {
            if (effect == null)
            {
                GasLogger.Error("[GAS] GameplayEffectSpec.GetDuration: effect is null");
                return 0f;
            }
            return effect.DurationMagnitude;
        }
        
        public GameplayEffectDurationType GetDurationType()
        {
            if (effect == null)
            {
                GasLogger.Error("[GAS] GameplayEffectSpec.GetDurationType: effect is null");
                return GameplayEffectDurationType.Instant;
            }
            return effect.DurationType;
        }
        
        public GameplayTagContainer GetGrantedTags()
        {
            if (effect == null)
            {
                GasLogger.Error("[GAS] GameplayEffectSpec.GetGrantedTags: effect is null");
                return new GameplayTagContainer();
            }
            return effect.GrantedTags;
        }
        
        public float GetPeriod()
        {
            if (effect == null)
            {
                GasLogger.Error("[GAS] GameplayEffectSpec.GetPeriod: effect is null");
                return GameplayEffectDefine.NoPeriod;
            }
            if (effect.DurationType == GameplayEffectDurationType.Instant)
            {
                return GAS.GameplayEffectDefine.NoPeriod;
            }
	
            return effect.Period;
        }

        //触发瞬时执行
        public void TriggerInstantExecute(AbilitySystemComponent owner)
        {
            owner.ApplyModFormInstant(this);
            TriggerCues(owner, effect.CueOnExecute);
            ApplyComponents(owner);
        }
        
        public void TriggerAdded(AbilitySystemComponent owner)
        {
            TriggerCues(owner, effect.CueOnAdd);
            GameplayCueContext context = GameplayCueContext.MakeContext(owner,effectParam.HitInfo,effectParam.TargetAscId);
            if (!effect.CueDurational.IsEmpty())
            {
                foreach (var cue in effect.CueDurational.GameplayTags)
                {
                    owner.AddGameplayCue(cue, true, context);
                }
            }
            ApplyComponents(owner);
        }
        
        public void TriggerRemoved(AbilitySystemComponent owner)
        {
            TriggerCues(owner, effect.CueOnRemove);
            if (!effect.CueDurational.IsEmpty())
            {
                foreach (var cue in effect.CueDurational.GameplayTags)
                {
                    owner.RemoveGameplayCue(cue);
                }
            }
            
            UnApplyComponents(owner);
        }
        
        private void TriggerActivate(AbilitySystemComponent owner)
        {
            TriggerCues(owner, effect.CueOnActivate);
            GameplayCueContext context = GameplayCueContext.MakeContext(owner,effectParam.HitInfo,effectParam.TargetAscId);
            if (!effect.CueDurational.IsEmpty())
            {
                foreach (var cue in effect.CueDurational.GameplayTags)
                {
                    owner.ExecuteGameplayCueByType(cue, ExecuteCueType.Activate, context);
                }
            }
        }
        
        private void TriggerDeactivate(AbilitySystemComponent owner)
        {
            TriggerCues(owner, effect.CueOnDeactivate);
            GameplayCueContext context = GameplayCueContext.MakeContext(owner,effectParam.HitInfo,effectParam.TargetAscId);
            if (!effect.CueDurational.IsEmpty())
            {
                foreach (var cue in effect.CueDurational.GameplayTags)
                {
                    owner.ExecuteGameplayCueByType(cue, ExecuteCueType.Deactivate, context);
                }
            }
        }
        
        public void TriggerPeriodicExecute(AbilitySystemComponent owner)
        {
            owner.ApplyModFormInstant(this);
            TriggerCues(owner, effect.CueOnExecute);
        }
        
        private void TriggerCues(AbilitySystemComponent owner, GameplayTagContainer cues)
        {
            if (cues.IsEmpty())
            {
                return;
            }
            GameplayCueContext context = GameplayCueContext.MakeContext(owner,effectParam.HitInfo,effectParam.TargetAscId);
            foreach (var cue in cues.GameplayTags)
            {
                owner.AddGameplayCue(cue, false, context);
            }
        }
        
        //应用组件信息
        private void ApplyComponents(AbilitySystemComponent owner)
        {
            foreach (var component in GetExecuteComponents())
            {
                component.OnApply(this, GameplayEffectContext.MakeOutgoingSpec(owner));
            }
        }
        
        //移除组件信息
        private void UnApplyComponents(AbilitySystemComponent owner)
        {
            foreach (var component in GetExecuteComponents())
            {
                component.OnUnapply(this, GameplayEffectContext.MakeOutgoingSpec(owner));
            }
        }

        public void Apply(AbilitySystemComponent owner)
        {
            if (IsApply)
            {
                return;
            }
            IsApply = true;
            Activate(owner);
        }
        
        public void Activate(AbilitySystemComponent owner)
        {
            if (IsActive)
            {
                return;
            }
            IsActive = true;
            TriggerActivate(owner);
        }

        public void DeActivate(AbilitySystemComponent owner)
        {
            if (!IsActive)
            {
                return;
            }
            IsActive = false;
            TriggerDeactivate(owner);
        }

        public void DisApply(AbilitySystemComponent owner)
        {
            if (!IsApply)
            {
                return;
            }
            IsActive = false;
            DeActivate(owner);
        }

        public bool CheckCanApply(AbilitySystemComponent owner)
        {
            foreach (var component in GetExecuteComponents())
            {
                if (!component.CanApply(this, GameplayEffectContext.MakeOutgoingSpec(owner)))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsValid()
        {
            return effect != null;
        }
        
        public static GameplayEffectSpec MakeSpec(GameplayEffect effect)
        {
            return new GameplayEffectSpec
            {
                effect = effect
            };
        }
        
        public static GameplayEffectSpec MakeSpec(GamePlayEffectAsset effectAsset)
        {
            return new GameplayEffectSpec
            {
                effect = new GameplayEffect(effectAsset)
            };
        }
    }
    
    //CustomReadWriteFunctions
    public static class GameplayEffectSpecCustomSerialization
    {
        public static void WriteGameplayEffectSpec(this NetworkWriter writer, GameplayEffectSpec value)
        {
            writer.WriteUInt(value.handle.Handle);
            writer.WriteGameplayEffect(value.effect);
        }

        public static GameplayEffectSpec ReadGameplayEffectSpec(this NetworkReader reader)
        {
            uint handle = reader.ReadUInt();
            GameplayEffect effect = reader.ReadGameplayEffect();
            GameplayEffectSpec spec = new GameplayEffectSpec
            {
                handle = new GameplayEffectSpecHandle(handle),
                effect = effect,
            };
            return spec;
        }
    }

    public struct GameplayEffectParam
    {
        public uint TargetAscId;
        public RaycastHit HitInfo;
    }

    public struct GameplayEffectSpecHandle
    {
        public uint Handle;
        
        public GameplayEffectSpecHandle(uint handle)
        {
            this.Handle = handle;
        }
        
        public bool IsValid()
        {
            return Handle != 0;
        }
        
        public static GameplayEffectSpecHandle UnValidHandle = new GameplayEffectSpecHandle(0);
    }

    public struct GameplayEffectUpdateInfo
    {
        public float RemainingDuration;
        public float PeriodTimeCount;
        public double StartTime;
        
        public GameplayEffectUpdateInfo(float remainingDuration, float periodTimeCount, double startTime)
        {
            RemainingDuration = remainingDuration;
            PeriodTimeCount = periodTimeCount;
            StartTime = startTime;
        }
    }
}