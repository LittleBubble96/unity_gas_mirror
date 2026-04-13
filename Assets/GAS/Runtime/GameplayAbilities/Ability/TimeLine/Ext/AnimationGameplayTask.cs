using System;
using Mirror;
using UnityEngine;
using Sirenix.OdinInspector;

namespace VSEngine.GAS
{
    /// <summary>
    /// 动画任务数据
    /// </summary>
    [Serializable]
    public class AnimationTaskData : AbilityTaskData
    {
        [LabelText("动画状态名称")]
        public string AnimationStateName;

        [LabelText("动画层级")]
        [Range(0, 3)]
        public int AnimationLayer = 0;

        [LabelText("混合过渡时间")]
        [Min(0f)]
        public float TransitionDuration = 0.1f;

        [LabelText("播放速度")]
        [Min(0.1f)]
        public float PlaySpeed = 1f;

        [LabelText("是否循环")]
        public bool IsLooping = false;

        public override void Read(NetworkReader reader)
        {
            base.Read(reader);
            AnimationStateName = reader.ReadString();
            AnimationLayer = reader.ReadInt();
            TransitionDuration = reader.ReadFloat();
            PlaySpeed = reader.ReadFloat();
            IsLooping = reader.ReadBool();
        }

        public override void Write(NetworkWriter writer)
        {
            base.Write(writer);
            writer.WriteString(AnimationStateName);
            writer.WriteInt(AnimationLayer);
            writer.WriteFloat(TransitionDuration);
            writer.WriteFloat(PlaySpeed);
            writer.WriteBool(IsLooping);
        }
    }

    /// <summary>
    /// 动画任务 - 播放技能动画
    /// </summary>
    public class AnimationGameplayTask : AbilityGameplayTask<AnimationTaskData>
    {
        public override void OnStart()
        {
            //
            if (IsServer())
            {
                AbilitySystemComponent.PlayAnimationMulticast(Data.AnimationStateName, Data.AnimationLayer, Data.TransitionDuration, Data.PlaySpeed, Data.IsLooping);
            }

            if (IsClientOnly())
            {
                AbilitySystemComponent.PlayClientAnimation(Data.AnimationStateName, Data.AnimationLayer, Data.TransitionDuration, Data.PlaySpeed, Data.IsLooping);
            }

        }

        public override void OnEnd()
        {
            if (IsServer())
            {
                AbilitySystemComponent.EndPlayAnimationMulticast();
            }

            if (IsClientOnly())
            {
                AbilitySystemComponent.EndClientAnimation();
            }
        }
        
    }
}
