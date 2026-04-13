using UnityEngine;

namespace VSEngine.GAS
{
    public class XYAnimationPlayer : AbilityAnimationPlayerBase
    {
        private Animator _animator;
        private bool _isPlaying = false;
        private static readonly int InAbility = Animator.StringToHash("InAbility");

        /// <summary>
        /// 检查动画是否在播放中
        /// </summary>
        public bool IsAnimationPlaying()
        {
            if (_animator == null) return false;
            return _isPlaying;
        }
        
        protected override void OnInit()
        {
            base.OnInit();
            _animator = Owner.GetComponentInChildren<Animator>();
            if (_animator == null)
            {
                Debug.LogWarning($"DefaultAbilityAnimationPlayer: Animator not found on {Owner.name}");
            }
        }

        public override void PlayAnimation(string animationStateName , int layer = 0 , float transitionDuration = 0.1f , float playSpeed = 1f , bool isLooping = false)
        {
            if (_animator == null) return;
            if (layer == 2 && GASExampleManager.Instance.LocalPlayer.IsIdle)
            {
                layer = 3;//使用全身动画层
            }
            int stateHash = Animator.StringToHash(animationStateName);
            if (isLooping)
            {
                _animator.CrossFade(stateHash, transitionDuration, layer, 0f);
            }
            else
            {
                _animator.Play(stateHash, layer, 0f);
            }
            _isPlaying = true;
            _animator.speed = playSpeed;
            _animator.SetBool(InAbility,_isPlaying);
        }

        public override void EndAnimation()
        {
            base.EndAnimation();
            if (_animator == null) return;
            _isPlaying = false;
            _animator.speed = 1f;
            _animator.SetBool(InAbility,_isPlaying);
        }
    }
}