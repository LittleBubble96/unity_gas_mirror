using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    [System.Serializable]
    public class GameplayAbility
    {
        //技能名称
        public string name;
        //输入动作名称 用于绑定输入
        public string inputActionName;
        //技能执行策略 决定技能在服务器还是客户端执行
        public GameplayAbilityNetExecutionPolicy netExecutionPolicy;
        //技能实例化策略 决定技能对象的生命周期
        public GameplayAbilityInstancingPolicy instancingPolicy;
        
        //技能标签 用于技能的分类和查询
        public GameplayTagContainer AssetTags = new GameplayTagContainer();
        //技能取消标签 用于配置GA之间的打断关系
        public GameplayTagContainer CancelAbilitiesWithTag = new GameplayTagContainer();
        //技能互斥标签 用于配置GA之间的互斥关系
        public GameplayTagContainer BlockAbilitiesWithTag = new GameplayTagContainer();
        //技能激活拥有标签 该GA激活时会给GA的拥有者附加的Tag
        public GameplayTagContainer ActivationOwnedTags = new GameplayTagContainer();
        //技能激活要求标签 想要该GA能被激活 GA的拥有者必须要有的Tag集合
        public GameplayTagContainer ActivationRequiredTags = new GameplayTagContainer();
        //激活阻止标签 如果拥有任何一个标签，就不能激活
        public GameplayTagContainer ActivationBlockedTags = new GameplayTagContainer();
        //源所需标签 想要该GA能被激活 GA的来源必须要有的Tag集合
        public GameplayTagContainer SourceRequiredTags = new GameplayTagContainer();
        //源阻止标签 如果来源拥有任何一个标签，就不能激活
        public GameplayTagContainer SourceBlockedTags = new GameplayTagContainer();
        
        
        //技能冷却效果 用于管理技能的冷却时间和状态
        public GameplayEffect CooldownGameplayEffect;
        //技能消耗效果 用于管理技能的资源消耗
        public GameplayEffect CostGameplayEffect;
        
        public GameplayAbility()
        {
            
        }

        public GameplayAbility(GameplayAbilityAsset abilityAsset)
        {
            name = abilityAsset.name;
            inputActionName = abilityAsset.InputActionName;
            netExecutionPolicy = abilityAsset.NetExecutionPolicy;
            CooldownGameplayEffect = new GameplayEffect(abilityAsset.CooldownGameplayEffect);
            CostGameplayEffect = new GameplayEffect(abilityAsset.CostGameplayEffect);
            AssetTags = new GameplayTagContainer(abilityAsset.AssetTags);
            CancelAbilitiesWithTag = new GameplayTagContainer(abilityAsset.CancelAbilitiesWithTag);
            BlockAbilitiesWithTag = new GameplayTagContainer(abilityAsset.BlockAbilitiesWithTag);
            ActivationOwnedTags = new GameplayTagContainer(abilityAsset.ActivationOwnedTags);
            ActivationRequiredTags = new GameplayTagContainer(abilityAsset.ActivationRequiredTags);
            ActivationBlockedTags = new GameplayTagContainer(abilityAsset.ActivationBlockedTags);
            SourceRequiredTags = new GameplayTagContainer(abilityAsset.SourceRequiredTags);
            SourceBlockedTags = new GameplayTagContainer(abilityAsset.SourceBlockedTags);
        }
        
        public GameplayAbilityNetExecutionPolicy GetNetExecutionPolicy()
        {
            return netExecutionPolicy;
        }
        
        protected virtual bool OnCanActivateAbility(AbilitySystemComponent owner , GameplayEventData eventData)
        {
            return true;
        }
        
        internal virtual void OnActivateAbility(AbilitySystemComponent owner , GameplayEventData eventData)
        {
            
        }
        
        internal virtual void OnEndAbility(AbilitySystemComponent owner)
        {
            
        }

        /// <summary>
        /// 是否可以激活
        /// </summary>
        public bool CanActivateAbility(AbilitySystemComponent owner , GameplayEventData eventData)
        {
            if (owner == null)
            {
                return false;
            }
            //检查冷却
            if (!AbilitySystemGlobals.Get().ShouldIgnoreCoolDowns() && !CheckCooldown(owner))
            {
                return false;
            }
            //检查消耗
            if (!AbilitySystemGlobals.Get().ShouldIgnoreCosts() && !CheckCost(owner))
            {
                return false;
            }
            //检查标签
            if (!DoesAbilitySatisfyTagRequirements(owner,eventData.SourceTags))
            {
                return false;
            }
            //自定义
            if (OnCanActivateAbility(owner,eventData) == false)
            {
                return false;
            }
            return true;
        }

        public bool CommitCheck(AbilitySystemComponent owner)
        {
            if (owner == null)
            {
                return false;
            }
            //检查冷却
            if (!AbilitySystemGlobals.Get().ShouldIgnoreCoolDowns() && !CheckCooldown(owner))
            {
                return false;
            }
            //检查消耗
            if (!AbilitySystemGlobals.Get().ShouldIgnoreCosts() && !CheckCost(owner))
            {
                return false;
            }
            return true;
        }
        
        public void CommitExecute(AbilitySystemComponent owner)
        {
            if (owner == null)
            {
                return;
            }
            //应用冷却
            ApplyCooldown(owner);
            //应用消耗
            ApplyCost(owner);
        }
        
        public virtual void OnCommitAbility(AbilitySystemComponent owner)
        {
            
        }

        //检查标签是否满足激活条件
        private bool DoesAbilitySatisfyTagRequirements(AbilitySystemComponent owner , GameplayTagContainer sourceTags)
        {
            //1.如果该技能标签 与拥有者的任何一个阻止标签匹配 则不能激活
            //2.如果该技能的激活阻止标签， 在拥有者的标签中存在任何一个 则不能激活
            //3.如果源上有存在 任何一个源阻止标签 则不能激活
            bool bBlocked = owner.HasAnyMatchingBlockAbilityTagsWithParent(AssetTags) ||
                            owner.HasAnyMatchingGameplayTagsWithParent(ActivationBlockedTags) ||
                            sourceTags.HasAny( SourceBlockedTags);
            //1.想要激活该技能，必须拥有者的标签中存在所有的激活要求标签
            //2.想要激活该技能，源上必须存在所有的源要求标签
            bool bMissed = !owner.HasAllMatchingGameplayTagsWithParent(ActivationRequiredTags) ||
                           !sourceTags.HasAll(SourceRequiredTags);
            return !bBlocked && !bMissed;
        }
        
        private bool CheckCost(AbilitySystemComponent owner)
        {
            if (CostGameplayEffect != null && 
                !owner.CanApplyAttributeModifiers(CostGameplayEffect))
            {
                return false;
            }
            return true;
        }

        private bool CheckCooldown(AbilitySystemComponent owner)
        {
            GameplayTagContainer tags = GetCoolDownTags();
            if (!tags.IsEmpty() && owner.HasAnyMatchingGameplayTags(tags))
            {
                return false;
            }
            return true;
        }
        
        public void ApplyCooldown(AbilitySystemComponent owner)
        {
            if (CooldownGameplayEffect != null)
            {
                owner.ApplyGameplayEffectSpecToSelf(GameplayEffectSpec.MakeSpec(CooldownGameplayEffect),default);
            }
        }
        
        public void ApplyCost(AbilitySystemComponent owner)
        {
            if (CostGameplayEffect != null)
            {
                owner.ApplyGameplayEffectSpecToSelf(GameplayEffectSpec.MakeSpec(CostGameplayEffect),default);
            }
        }

        //获取冷却Tag
        public GameplayTagContainer GetCoolDownTags()
        {
            if (CooldownGameplayEffect == null)
            {
                return new GameplayTagContainer();
            }
            return CooldownGameplayEffect.GrantedTags;
        }
        
        //获取冷却时间
        public float GetCoolDownTime()
        {
            if (CooldownGameplayEffect == null)
            {
                return 0;
            }
            return CooldownGameplayEffect.DurationMagnitude;
        }

        public override string ToString()
        {
            return name;
        }
    }
    
    //CustomReadWriteFunctions
    public static class GameplayAbilitySerializer
    {
        public static void WriteGameplayAbility(this NetworkWriter writer, GameplayAbility ability)
        {
            writer.WriteString(ability.name);
            writer.WriteString(ability.inputActionName);
            writer.WriteInt((int)ability.netExecutionPolicy);
            writer.WriteInt((int)ability.instancingPolicy);
            writer.WriteGameplayEffect(ability.CooldownGameplayEffect);
            writer.WriteGameplayEffect(ability.CostGameplayEffect);
            writer.WriteGameplayTagContainer(ability.AssetTags);
            writer.WriteGameplayTagContainer(ability.CancelAbilitiesWithTag);
            writer.WriteGameplayTagContainer(ability.BlockAbilitiesWithTag);
            writer.WriteGameplayTagContainer(ability.ActivationOwnedTags);
            writer.WriteGameplayTagContainer(ability.ActivationRequiredTags);
            writer.WriteGameplayTagContainer(ability.ActivationBlockedTags);
            writer.WriteGameplayTagContainer(ability.SourceRequiredTags);
            writer.WriteGameplayTagContainer(ability.SourceBlockedTags);
        }

        public static GameplayAbility ReadGameplayAbility(this NetworkReader reader)
        {
            GameplayAbility ability = new GameplayAbility();
            ability.name = reader.ReadString();
            ability.inputActionName = reader.ReadString();
            ability.netExecutionPolicy = (GameplayAbilityNetExecutionPolicy)reader.ReadInt();
            ability.instancingPolicy = (GameplayAbilityInstancingPolicy)reader.ReadInt();
            ability.CooldownGameplayEffect = reader.ReadGameplayEffect();
            ability.CostGameplayEffect = reader.ReadGameplayEffect();
            ability.AssetTags = reader.ReadGameplayTagContainer();
            ability.CancelAbilitiesWithTag = reader.ReadGameplayTagContainer();
            ability.BlockAbilitiesWithTag = reader.ReadGameplayTagContainer();
            ability.ActivationOwnedTags = reader.ReadGameplayTagContainer();
            ability.ActivationRequiredTags = reader.ReadGameplayTagContainer();
            ability.ActivationBlockedTags = reader.ReadGameplayTagContainer();
            ability.SourceRequiredTags = reader.ReadGameplayTagContainer();
            ability.SourceBlockedTags = reader.ReadGameplayTagContainer();
            return ability;
        }
    }
}