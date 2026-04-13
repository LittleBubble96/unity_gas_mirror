using System;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS
{
    /// <summary>
    /// Cue 任务数据
    /// </summary>
    [Serializable]
    public class CueTaskData : AbilityTaskData
    {
        [LabelText("GameplayTag")]
        public GameplayTag CueTag;
        
        [LabelText("是否在技能结束时移除")]
        public bool RemoveOnAbilityEnd = true;

        public override void Read(NetworkReader reader)
        {
            base.Read(reader);
            CueTag = reader.ReadGameplayTag();
            RemoveOnAbilityEnd = reader.ReadBool();
        }

        public override void Write(NetworkWriter writer)
        {
            base.Write(writer);
            writer.WriteGameplayTag(CueTag);
            writer.WriteBool(RemoveOnAbilityEnd);
        }
    }

    /// <summary>
    /// Cue 任务 - 执行 GameplayCue 效果
    /// </summary>
    public class CueGameplayTask : AbilityGameplayTask<CueTaskData>
    {
        private bool _hasExecuted = false;

        public override void OnStart()
        {
            ExecuteCue();
        }

        public override void OnEnd()
        {
            if (Data.RemoveOnAbilityEnd)
            {
                RemoveCue();
            }
        }

        public override bool DoUpdate(float dt)
        {
            // Cue 任务通常是瞬时的，不需要持续更新
            return _hasExecuted;
        }

        public override void OnInit(AbilityTaskData data)
        {
            base.OnInit(data);
            _hasExecuted = false;
        }

        /// <summary>
        /// 执行 Cue 操作
        /// </summary>
        private void ExecuteCue()
        {
            if (AbilitySystemComponent == null) return;
            if (!AbilitySystemComponent.isServer)
            {
                return;
            }

            var context = new GameplayCueContext
            {
                Instigator = AbilitySystemComponent
            };
            AbilitySystemComponent.InvokeCue_OnAuthority (Data.CueTag, ExecuteCueType.OnExecute, context);
        }
        
        private void RemoveCue()
        {
            if (AbilitySystemComponent == null) return;
            if (!AbilitySystemComponent.isServer)
            {
                return;
            }
            var context = new GameplayCueContext
            {
                Instigator = AbilitySystemComponent
            };
            AbilitySystemComponent.InvokeCue_OnAuthority (Data.CueTag, ExecuteCueType.Remove, context);
        }
    }
}
