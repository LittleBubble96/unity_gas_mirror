using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VSEngine.GAS;

namespace GAS.Editor.Animation
{
    [Serializable]
    public class GASAnimationClip : GasTimelineClip
    {
        [OnValueChanged(nameof(OnChangeAnimationClip))]
        public AnimationClip AnimationClip;

        [LabelText("动画层级")]
        [Range(0, 3)]
        public int AnimationLayer = 0;

        [LabelText("混合过渡时间")]
        [Min(0f)]
        public float TransitionDuration = 0.1f;

        [LabelText("播放速度")]
        [Min(0.1f)]
        public float PlaySpeed = 1f;

        public ClipCaps clipCaps => ClipCaps.None;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<GASAnimationPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SetAnimationClip(this);
            return playable;
        }

        public override AbilityTaskData GetAbilityTaskData(float startTime, float dur)
        {
            AnimationTaskData taskData = new AnimationTaskData
            {
                StartTime = startTime,
                Duration = dur,
                AnimationStateName = AnimationClip != null ? AnimationClip.name : string.Empty,
                AnimationLayer = AnimationLayer,
                TransitionDuration = TransitionDuration,
                PlaySpeed = PlaySpeed,
                IsLooping = AnimationClip != null && AnimationClip.isLooping
            };
            return taskData;
        }

        public override double duration
        {
            get
            {
                if (AnimationClip != null)
                {
                    return AnimationClip.length / PlaySpeed;
                }
                return base.duration;
            }
        }

        private void OnChangeAnimationClip()
        {
          
        }
    }
}