using UnityEngine;

namespace VSEngine.GAS
{
    //技能动画播放器
    public abstract class AbilityAnimationPlayerBase
    {
        protected AbilitySystemComponent Owner;
        
        public void Init(AbilitySystemComponent owner)
        {
            Owner = owner;
            OnInit();
        }

        protected virtual void OnInit()
        {
            
        }
        
        public virtual void PlayAnimation(string animationStateName , int layer = 0 , float transitionDuration = 0.1f , float playSpeed = 1f , bool isLooping = false)
        {
            
        }
        
        public virtual void EndAnimation()
        {
            
        }
    }

    /// <summary>
    /// 默认动画播放器 - 直接使用Animator组件播放动画
    /// </summary>
    public class DefaultAbilityAnimationPlayer : AbilityAnimationPlayerBase
    {
        private Animator _animator;
        private bool _isPlaying = false;
        
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
        }

        public override void EndAnimation()
        {
            base.EndAnimation();
            if (_animator == null) return;
            _isPlaying = false;
            _animator.speed = 1f;
        }
    }
}