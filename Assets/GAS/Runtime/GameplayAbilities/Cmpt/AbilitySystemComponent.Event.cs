using System;

namespace VSEngine.GAS
{
    //事件相关
    public partial class AbilitySystemComponent
    {
        //回调  技能执行位置，如果在服务器则服务器调用，如果在客户端则客户端调用
        private Action<uint,string> _onActivateAbilityFailed;
        private Action<uint> _onActivateAbilitySuccess;
        private Action<uint> _onGameplayAbilityCancelled;
        private Action<uint> _onGameplayAbilityEnded;
        private Action<uint> _onGameplayAbilityCommitted;

        //Client
        private Action<GameplayEffectSpec> _onClientAddedGameplayEffect;
        
        //广播技能激活失败的回调
        internal void NotifyActivateAbilityFailed(uint abilityHandle, string reason)
        {
            _onActivateAbilityFailed?.Invoke(abilityHandle, reason);
        }
        
        //注册技能激活失败的回调
        public void RegisterActivateAbilityFailedCallback(Action<uint,string> callback)
        {
            _onActivateAbilityFailed += callback;
        }
        
        //广播技能激活成功的回调
        internal void NotifyActivateAbilitySuccess(uint abilityHandle)
        {
            _onActivateAbilitySuccess?.Invoke(abilityHandle);
        }
        
        //注册技能激活成功的回调
        public void RegisterActivateAbilitySuccessCallback(Action<uint> callback)
        {
            _onActivateAbilitySuccess += callback;
        }
        
        //广播技能被打断的回调
        internal void NotifyGameplayAbilityCancelled(uint abilityHandle)
        {
            _onGameplayAbilityCancelled?.Invoke(abilityHandle);
        }
        
        //注册技能被打断的回调
        public void RegisterGameplayAbilityCancelledCallback(Action<uint> callback)
        {
            _onGameplayAbilityCancelled += callback;
        }
        
        //广播技能提交的回调
        internal void NotifyGameplayAbilityCommitted(uint abilityHandle)
        {
            _onGameplayAbilityCommitted?.Invoke(abilityHandle);
        }
        
        //注册技能提交的回调
        public void RegisterGameplayAbilityCommittedCallback(Action<uint> callback)
        {
            _onGameplayAbilityCommitted += callback;
        }
        
        //广播GE 添加的回调
        internal void NotifyClientAddedGameplayEffect(GameplayEffectSpec spec)
        {
            _onClientAddedGameplayEffect?.Invoke(spec);
        }
        
        //注册GE 添加的回调
        public void RegisterClientAddedGameplayEffectCallback(Action<GameplayEffectSpec> callback)
        {
            _onClientAddedGameplayEffect += callback;
        }
        
        //注册属性修改后回调
        public void RegisterAttributeChangedCallback_InServer(Action<AttributeBase> callback)
        {
            _attributeSetContainer.OnAttributeChangedInServer += callback;
        }
        
        public void UnregisterAttributeChangedCallback_InServer(Action<AttributeBase> callback)
        {
            _attributeSetContainer.OnAttributeChangedInServer -= callback;
        }

        #region Tag Event

        public void RegisterTagAddedCallback(Action<GameplayTag> callback)
        {
            _tagCountContainer.RegisterTagAddedCallback(callback);
        }
        
        public void UnregisterTagAddedCallback(Action<GameplayTag> callback)
        {
            _tagCountContainer.UnregisterTagAddedCallback(callback);
        }
        
        public void RegisterTagRemovedCallback(Action<GameplayTag> callback)
        {
            _tagCountContainer.RegisterTagRemovedCallback(callback);
        }
        
        public void UnregisterTagRemovedCallback(Action<GameplayTag> callback)
        {
            _tagCountContainer.UnregisterTagRemovedCallback(callback);
        }
        
        public void RegisterTagChangedCallback(Action<GameplayTag> callback)
        {
            _tagCountContainer.RegisterTagChangedCallback(callback);
        }
        
        public void UnregisterTagChangedCallback(Action<GameplayTag> callback)
        {
            _tagCountContainer.UnregisterTagChangedCallback(callback);
        }
        #endregion
    }
}