using System.Collections.Generic;
using UnityEditor;

namespace VSEngine.GAS
{
    public class GameplayAbilityInstance
    {
        private GameplayAbility _sourceAbility;
        
        public GameplayAbility SourceAbility => _sourceAbility;
        
        public AbilitySystemComponent Owner { get; private set; }

        public bool BIsActive { get; private set; }
        
        public uint SpecHandle { get; set; }
        
        public AbilityTimeline Timeline { get; private set; }
        
        public List<GameplayTag> TrackedGameplayCues { get; private set; } = new List<GameplayTag>();


        public GameplayAbilityInstance(GameplayAbility sourceAbility , AbilitySystemComponent owner , uint specHandle)
        {
            _sourceAbility = sourceAbility;
            Owner = owner;
            SpecHandle = specHandle;
            Timeline = AbilitySystemGlobals.Get().GetTimelineGlobalData().Factory.CreateAbilityTimeline();
            Timeline.Init(owner, this);
        }

        //激活技能
        public void CallActivateAbility(GameplayEventData eventData)
        {
            PreActivate();
            ActivateAbility(eventData);
        }

        //技能预激活
        private void PreActivate()
        {
            BIsActive = true;
            //添加技能标签
            Owner.AddGameplayTagsToCountMap(_sourceAbility.ActivationOwnedTags);
            //通知技能激活成功
            Owner.NotifyActivateAbilitySuccess(SpecHandle);
            //应用技能的标签阻挡和取消效果
            Owner.ApplyAbilityBlockTags(_sourceAbility.BlockAbilitiesWithTag , true);
            Owner.ApplyCancelAbility(_sourceAbility,_sourceAbility.CancelAbilitiesWithTag);
            
            //激活次数++
            GameplayAbilitySpec spec = Owner.FindAbilitySpecFromHandle(SpecHandle);
            if (spec != null)
            {
                spec.ActiveCount++;
            }
        }

        //激活技能
        private void ActivateAbility(GameplayEventData eventData)
        {
            if (_sourceAbility != null)
            {
                _sourceAbility.OnActivateAbility(Owner,eventData);
            }
            //TODO 此处可以添加技能确认阶段
            Timeline.Play();
        }
        
        public void Tick(float dt)
        {
            if (BIsActive && Timeline != null)
            {
                Timeline.Update(dt);
            }
        }

        //提交技能
        public bool CommitAbility()
        {
            if (!_sourceAbility.CommitCheck(Owner))
            {
                return false;
            }
            _sourceAbility.CommitExecute(Owner);
            _sourceAbility.OnCommitAbility(Owner);
            Owner.NotifyGameplayAbilityCommitted(SpecHandle);
            return true;
        }

        public void EndAbility(bool bIsPendingEnd)
        {
            //结束技能
            EndAbility_Internal(bIsPendingEnd);
        }

        private void EndAbility_Internal(bool bIsPendingEnd)
        {
            _sourceAbility.OnEndAbility(Owner);
            GasLogger.Log($"[GAS] EndAbility: {_sourceAbility.name} SpecHandle: {SpecHandle} BIsActive: {BIsActive}");
            if (!BIsActive)
            {
                //防止多次结束技能
                return;
            }
            BIsActive = false;
            Timeline.End(bIsPendingEnd);
            Owner.RemoveGameplayTagsFromCountMap(_sourceAbility.AssetTags);
            Owner.ApplyAbilityBlockTags(_sourceAbility.BlockAbilitiesWithTag,false);
            Owner.NotifyGameplayAbilityEnded(SpecHandle,this);
            //技能结束后 清除技能添加的GameplayCue
            foreach (var cueTag in TrackedGameplayCues)
            {
                Owner.RemoveGameplayCue(cueTag);
            }
            TrackedGameplayCues.Clear();
        }

        public void CancelAbility()
        {
            //取消技能
            Owner.NotifyGameplayAbilityCancelled(SpecHandle);
            EndAbility_Internal(false);
        }
        
        //添加GameplayCue到拥有者
        public void AddGameplayCueToOwner(GameplayTag cueTag , bool bRemoveOnEnd , GameplayCueContext context = default)
        {
            Owner.AddGameplayCue(cueTag,bRemoveOnEnd,context);
            if (bRemoveOnEnd)
            {
                TrackedGameplayCues.Add(cueTag);
            }
        }

        public void Dispose()
        {
            AbilitySystemGlobals.Get().GetTimelineGlobalData().Factory.DestroyAbilityTimeline(Timeline);
        }
    }
}