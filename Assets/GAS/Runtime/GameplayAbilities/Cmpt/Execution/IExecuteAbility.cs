using System;

namespace VSEngine.GAS
{
    public interface IExecuteAbility
    {
        bool TryActivateAbility(GameplayAbilitySpec abilitySpec, GameplayEventData eventData = default);
    }
    
    public abstract class ExecuteAbilityBase : IExecuteAbility , IDisposable
    {
        private AbilitySystemComponent _owner;
        
        public AbilitySystemComponent Owner
        {
            get => _owner;
            set => _owner = value;
        }
        
        public virtual bool TryActivateAbility(GameplayAbilitySpec abilitySpec , GameplayEventData eventData = default)
        {
            return false;
        }
        
        public void Clear()
        {
            _owner = null;
        }

        public virtual void Dispose()
        {
            // MemoryPool.Release(this);
        }
    }
}