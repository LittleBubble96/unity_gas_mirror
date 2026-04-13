namespace VSEngine.GAS
{
    /// <summary>
    /// 仅在客户端执行的Ability，通常用于一些只需要在客户端执行的逻辑，比如播放特效、声音等。
    /// </summary>
    public class ExecuteAbilityClientOnly : ExecuteAbilityBase
    {
        public override bool TryActivateAbility(GameplayAbilitySpec abilitySpec , GameplayEventData eventData = default)
        {
            GameplayAbilityNetExecutionPolicy netExecutionPolicy = abilitySpec.Ability.GetNetExecutionPolicy();
            //如果只是单纯客户端 如果是 只在服务器执行的技能 则不激活技能
            if (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerOnly ||
                                 netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerInitiated)
            {
                Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 只能在服务器执行");
                return false;
            }
            bool isLocalPlayer = Owner.isLocalPlayer;
            if (!isLocalPlayer)
            {
                Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 只能由本地玩家激活");
                return false;
            }
            GameplayAbility ability = abilitySpec.Ability;
            //TODO LocalPredicted 先和 LocalOnly保持一致 后续可以根据需要添加一些预测逻辑
            if (!ability.CanActivateAbility(Owner,eventData))
            {
                Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 激活条件没有通关");
                return false;
            }
            //检查 如果只能存在一个实例 则结束之前的技能实例
            GameplayAbilityInstance instancedAbility = abilitySpec.GetPrimaryInstance();
            if (abilitySpec.GetInstancingPolicy() == GameplayAbilityInstancingPolicy.InstancedPerActor)
            {
                if (instancedAbility == null)
                {
                    GasLogger.Warning($"[GAS] [ExecuteAbilityClientOnly] 没有找到技能实例 Handle: {abilitySpec.Handle}");
                    Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 没有找到实例");
                    return false;
                }
                if (abilitySpec.IsActive())
                {
                    //结束技能
                    instancedAbility.EndAbility(false);
                }
            }
            //可以激活
            if (abilitySpec.GetInstancingPolicy() == GameplayAbilityInstancingPolicy.InstancedPerExecution)
            {
                instancedAbility = abilitySpec.CreateNewInstance();
                instancedAbility.CallActivateAbility(eventData);
            }
            else
            {
                instancedAbility.CallActivateAbility(eventData);
            }
            return true;
        }
    }
}