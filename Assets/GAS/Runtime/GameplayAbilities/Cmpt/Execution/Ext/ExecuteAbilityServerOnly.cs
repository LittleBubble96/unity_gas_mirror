namespace VSEngine.GAS
{
    public class ExecuteAbilityServerOnly : ExecuteAbilityBase
    {
        public override bool TryActivateAbility(GameplayAbilitySpec abilitySpec , GameplayEventData eventData = default)
        {
            GameplayAbilityNetExecutionPolicy netExecutionPolicy = abilitySpec.Ability.GetNetExecutionPolicy();
            //如果只是单纯服务器 如果是 本地预测 或者 只在本地执行的技能 则不激活技能
            if (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.LocalOnly || 
                                 netExecutionPolicy == GameplayAbilityNetExecutionPolicy.LocalPredicted)
            {
                //向客户端发送激活技能的请求
                Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 只能在客户端执行");
                return true;
            }
            //如果不是服务器 则不激活技能
            if (!Owner.isServer)
            {
                Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 只能由服务器激活");
                return false;   
            }

            GameplayAbility ability = abilitySpec.Ability;
            if (!ability.CanActivateAbility(Owner,eventData))
            {
                Owner.NotifyActivateAbilityFailed(abilitySpec.Handle, $"技能 {abilitySpec.Ability} 激活条件没有通过");
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

            if (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerInitiated)
            {
                if (eventData.IsValid())
                {
                    Owner.ClientActivateAbilitySuccessWithEventData(abilitySpec.Handle, eventData);
                }
                else
                {
                    Owner.ClientActivateAbilitySuccess(abilitySpec.Handle);
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