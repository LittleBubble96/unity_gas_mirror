
namespace VSEngine.GAS
{
    public static class ExecuteFactory
    {
        public static ExecuteAbilityBase CreateExecuteAbility(NetMonoInfo netMono , AbilitySystemComponent asc)
        {
            ExecuteAbilityBase executeAbility;
            if (netMono.IsClient && netMono.IsServer)
            {
                executeAbility = new ExecuteAbilityHost();
                // executeAbility = MemoryPool.Acquire<ExecuteAbilityHost>();
            }
            else if (netMono.IsClient)
            {
                executeAbility = new ExecuteAbilityClientOnly();
                // executeAbility = MemoryPool.Acquire<ExecuteAbilityClientOnly>();
            }
            else if (netMono.IsServer)
            {
                executeAbility = new ExecuteAbilityServerOnly();
                 //
                // executeAbility = MemoryPool.Acquire<ExecuteAbilityServerOnly>();
            }
            else
            {
                executeAbility = new ExecuteAbilityHost();
                // executeAbility = MemoryPool.Acquire<ExecuteAbilityError>();
            }
            executeAbility.Owner = asc;
            return executeAbility;
        }
    }
}