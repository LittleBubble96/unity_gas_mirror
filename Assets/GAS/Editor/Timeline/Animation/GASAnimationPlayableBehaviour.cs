using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace GAS.Editor.Animation
{
    public class GASAnimationPlayableBehaviour : PlayableBehaviour
    {
        private GASAnimationClip _animationClip;
        
        public void SetAnimationClip(GASAnimationClip clip)
        {
            _animationClip = clip;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            Animator animator = playerData as Animator;
            if (_animationClip == null || animator == null || _animationClip.AnimationClip == null) return;
            float normalizedTime = (float)(playable.GetTime() / playable.GetDuration());
            AnimationMode.StopAnimationMode();
            AnimationMode.StartAnimationMode();
            AnimationMode.SampleAnimationClip(animator.gameObject, _animationClip.AnimationClip, normalizedTime * _animationClip.AnimationClip.length);
        }
    }
}