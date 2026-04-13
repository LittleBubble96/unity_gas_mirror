using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    [System.Serializable]
    public class GameplayAbilitySpec
    {
        public int Level;
        
        public uint Handle;
        
        public GameplayAbility Ability;
        
        public AbilitySystemComponent Owner;
        
        //不复制的技能 服务器 和 本地实例化的技能对象 存在这个列表里 服务器和本地都可以访问 但是不会复制到客户端
        public List<GameplayAbilityInstance> NonReplicatedInstances = new List<GameplayAbilityInstance>();
        
        //激活次数 用于追踪技能的激活状态 例如是否正在激活或者已经激活过一次
        public int ActiveCount { get; set; }
        
        //是否正在等待移除 例如技能被移除后 可能需要等待当前的技能实例结束后才能完全移除这个技能规格
        public bool IsPendingRemove { get; set; }

        public GameplayAbilitySpec() { }
        
        public GameplayAbilitySpec(GameplayAbilityAsset abilityAsset ,AbilitySystemComponent owner , int level = 1)
        {
            Ability = new GameplayAbility(abilityAsset);
            Level = level;
            Handle = 0;
            IsPendingRemove = false;
            NonReplicatedInstances = new List<GameplayAbilityInstance>();
            ActiveCount = 0;
            Owner = owner;
        }
        
        public GameplayAbilityNetExecutionPolicy GetNetExecutionPolicy()
        {
            if (Ability == null)
            {
                GasLogger.Error($"[GAS] GameplayAbilitySpec GetNetExecutionPolicy Warning: Ability is null Handle: {Handle}");
                return GameplayAbilityNetExecutionPolicy.LocalOnly;
            }
            return Ability.GetNetExecutionPolicy();
        }
        
        public GameplayAbilityInstancingPolicy GetInstancingPolicy()
        {
            if (Ability == null)
            {
                GasLogger.Error($"[GAS] GameplayAbilitySpec GetInstancingPolicy Warning: Ability is null Handle: {Handle}");
                return GameplayAbilityInstancingPolicy.InstancedPerActor;
            }
            return Ability.instancingPolicy;
        }
        
        //是否处于激活状态
        public bool IsActive()
        {
            return IsValid() &&  ActiveCount > 0;
        }
        
        public bool IsValid()
        {
            return Ability != null;
        }
        
        public void SetHandle(uint handle)
        {
            Handle = handle;
        }
        
        public GameplayAbilityInstance CreateNewInstance()
        {
            if (Ability == null)
            {
                GasLogger.Error($"[GAS] GameplayAbilitySpec CreateNewInstance Warning: Ability is null Handle: {Handle}");
                return null;
            }
            GameplayAbilityInstance newInstance = new GameplayAbilityInstance(Ability,Owner,Handle);
            NonReplicatedInstances.Add(newInstance);
            return newInstance;
        }
        
        public GameplayAbilityInstance GetPrimaryInstance()
        {
            if (NonReplicatedInstances.Count > 0)
            {
                return NonReplicatedInstances[0];
            }
            return null;
        }
        
        public List<GameplayAbilityInstance> GetAllInstances()
        {
            return NonReplicatedInstances;
        }
        
        public float GetCoolDownTime()
        {
            if (Ability == null)
            {
                GasLogger.Error($"[GAS] GameplayAbilitySpec GetCooldownRemaining Warning: Ability is null Handle: {Handle}");
                return 0f;
            }

            return Ability.GetCoolDownTime();
        }

        public void DoUpdate(float dt)
        {
            for (int i = NonReplicatedInstances.Count - 1; i >= 0; i--)
            {
                NonReplicatedInstances[i].Tick(dt);
            }
        }
        
        public void Dispose()
        {
            foreach (var instance in NonReplicatedInstances)
            {
                instance.Dispose();
            }
            NonReplicatedInstances.Clear();
        }
    }
    
    //CustomReadWriteFunctions
    public static class GameplayAbilitySpecSerializer
    {
        public static void WriteGameplayAbilitySpec(this NetworkWriter writer, GameplayAbilitySpec spec)
        {
            writer.WriteUInt(spec.Owner.AscId);
            writer.WriteInt(spec.Level);
            writer.WriteUInt(spec.Handle);
            writer.WriteGameplayAbility(spec.Ability);
        }

        public static GameplayAbilitySpec ReadGameplayAbilitySpec(this NetworkReader reader)
        {
            GameplayAbilitySpec spec = new GameplayAbilitySpec();
            uint ascId = reader.ReadUInt();
            spec.Owner = AbilitySystemGlobals.Get().GetAsc(ascId);
            spec.Level = reader.ReadInt();
            spec.Handle = reader.ReadUInt();
            spec.Ability = reader.ReadGameplayAbility();
            //防止复制过来后 没有执行实例
            if (spec.GetInstancingPolicy() == GameplayAbilityInstancingPolicy.InstancedPerActor && spec.GetPrimaryInstance() == null)
            {
                spec.CreateNewInstance();
            }
            return spec;
        }
    }
}