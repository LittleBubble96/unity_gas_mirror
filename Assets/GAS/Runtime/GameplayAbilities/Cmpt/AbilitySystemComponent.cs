using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VSEngine.GAS
{
    public partial class AbilitySystemComponent : NetworkBehaviour
    {
        [SyncVar]
        public uint AscId = 0;
        //技能容器
        private GameplayAbilitySpecContainer _abilitySpecContainer;
        //标签容器
        private ActivateTagCountContainer _tagCountContainer;
        //打断技能标签容器
        private BlockAbilityTagCountContainer _blockAbilityTagCountContainer;
        //效果容器
        private GameplayEffectContainer _effectContainer;
        //属性容器
        private AttributeSetContainer _attributeSetContainer;
        //Cue容器
        private GameplayCueContainer _gameplayCueContainer;
        
        //动画播放器 
        private AbilityAnimationPlayerBase _abilityAnimationPlayer = new DefaultAbilityAnimationPlayer();
        
        private void Awake()
        {
            InitComponents();
        }
        
        //反注册Asc
        private void OnDestroy()
        {
            Dispose();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            AbilitySystemGlobals.Get().RegisterAscFromServer(this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!isServer)
            {
                AbilitySystemGlobals.Get().RegisterAscFromClient(this);
            }
            _abilityAnimationPlayer.Init(this);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            AbilitySystemGlobals.Get().UnregisterAsc(this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            if (!isServer)
            {
                AbilitySystemGlobals.Get().UnregisterAsc(this);
            }
        }

        private void InitComponents()
        {
            _abilitySpecContainer = GetComponentInChildren<GameplayAbilitySpecContainer>();
            _tagCountContainer = GetComponentInChildren<ActivateTagCountContainer>();
            _blockAbilityTagCountContainer = GetComponentInChildren<BlockAbilityTagCountContainer>();
            _effectContainer = GetComponentInChildren<GameplayEffectContainer>();
            _attributeSetContainer = GetComponentInChildren<AttributeSetContainer>();
            _gameplayCueContainer = GetComponentInChildren<GameplayCueContainer>();
            _abilitySpecContainer.Init(this);
            _tagCountContainer.Init(this);
            _blockAbilityTagCountContainer.Init(this);
            _effectContainer.Init(this);
            _attributeSetContainer.Init(this);
            _gameplayCueContainer.Init(this);
        }

        private void Update()
        {
            if (_abilitySpecContainer)
            {
                _abilitySpecContainer.DoUpdate(Time.deltaTime);
            }
            if (_effectContainer)
            {
                _effectContainer.DoUpdate(Time.deltaTime);
            }
        }

        private void Dispose()
        {
            _abilitySpecContainer.Dispose();
            _tagCountContainer.Dispose();
            _blockAbilityTagCountContainer.Dispose();
            _effectContainer.Dispose();
            _attributeSetContainer.Dispose();
            _gameplayCueContainer.Dispose();
            _abilitySpecContainer = null;
            _tagCountContainer = null;
            _blockAbilityTagCountContainer = null;
            _effectContainer = null;
            _attributeSetContainer = null;
            _gameplayCueContainer = null;
        }

        #region Ability

        /// <summary>
        /// 赋予技能
        /// </summary>
        /// <param name="spec"></param>
        public uint GiveAbility(GameplayAbilitySpec spec)
        {
            if (!spec.IsValid())
            {
                return 0;
            }

            //如果是服务器才可以添加技能
            if (!isServer)
            {
                return 0;
            }

            spec.SetHandle(GameplayAbilitySpecContainer.AbilityGenerateHandle++);
            _abilitySpecContainer.Items.Add(spec);
            GasLogger.Log($"[GAS] [Server] 服务器添加技能 Handle: {spec.Handle} Level: {spec.Level}");
            return spec.Handle;
        }
        
        public void RemoveAbility(uint abilityHandle)
        {
            if (!isServer)
            {
                return;
            }
            GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
            if (!abilitySpec.IsValid())
            {
                GasLogger.Warning($"[GAS] [Server] 移除技能失败 没有找到该技能 Handle: {abilityHandle}");
                return;
            }
            abilitySpec.IsPendingRemove = true;
        }
        
        public void InputActivateAbility(string inputID)
        {
            foreach (var abilitySpec in _abilitySpecContainer.Items)
            {
                if (!abilitySpec.IsValid())
                {
                    continue;
                }
                if (abilitySpec.Ability.inputActionName == inputID)
                {
                    GasLogger.Log($"[GAS] [Client] 激活技能 InputID: {inputID} Handle: {abilitySpec.Handle}");
                    TryActivateAbility(abilitySpec.Handle);
                }
            }
        }

        public bool TryActivateAbility(uint abilityHandle)
        {
            GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
            if (!abilitySpec.IsValid())
            {
                GasLogger.Warning($"[GAS] [Client] 激活技能失败 无效的技能 Handle: {abilityHandle}");
                return false;
            }
            //如果本地模拟 则不激活技能
            if (isClient && !isLocalPlayer)
            {
                GasLogger.Log($"[GAS] [Client] 本地模拟 不激活技能 Handle: {abilityHandle}");
                return false;
            }
            GameplayAbilityNetExecutionPolicy netExecutionPolicy = abilitySpec.Ability.netExecutionPolicy;
            //如果只是单纯服务器 如果是 本地预测 或者 只在本地执行的技能 则不激活技能
            if (isServerOnly && (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.LocalOnly || 
                                 netExecutionPolicy == GameplayAbilityNetExecutionPolicy.LocalPredicted))
            {
                ClientTryActivateAbility(abilityHandle);
                return true;
            }
            //如果只是单纯客户端 如果是 只在服务器执行的技能 则不激活技能
            if (isClientOnly && (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerOnly ||
                                netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerInitiated))
            {
                ServerTryActivateAbility(abilityHandle);
                return true;
            }

            return InternalTryActivateAbility(abilitySpec);
        }

        private bool InternalTryActivateAbility(GameplayAbilitySpec abilitySpec , GameplayEventData eventData = default)
        {
            if (!abilitySpec.IsValid())
            {
                return false;
            }
            using ExecuteAbilityBase executeAbility = ExecuteFactory.CreateExecuteAbility(this.GetNetMonoInfo(),this);
            return executeAbility.TryActivateAbility(abilitySpec,eventData);
        }
        
        
        //广播技能结束的回调
        internal void NotifyGameplayAbilityEnded(uint abilityHandle , GameplayAbilityInstance instance)
        {
            GameplayAbilitySpec spec = FindAbilitySpecFromHandle(abilityHandle);
            if (spec == null || !spec.IsValid())
            {
                GasLogger.Warning($"[GAS] [NotifyGameplayAbilityEnded] 没有找到该技能 Handle: {abilityHandle}");
                return;
            }
            spec.ActiveCount--;
            _onGameplayAbilityEnded?.Invoke(abilityHandle);
            if (spec.GetInstancingPolicy() == GameplayAbilityInstancingPolicy.InstancedPerExecution)
            {
                spec.NonReplicatedInstances.Remove(instance);
            }
        }
        
        //注册技能结束的回调
        public void RegisterGameplayAbilityEndedCallback(Action<uint> callback)
        {
            _onGameplayAbilityEnded += callback;
        }

        //向客户端 发送激活技能的请求
        [TargetRpc]
        public void ClientTryActivateAbility(uint abilityHandle)
        {
            GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
            if (!abilitySpec.IsValid())
            {
                GasLogger.Warning($"[GAS] [ClientTryActivateAbility] 没有找到该技能 Handle: {abilityHandle}");
                //TODO 这里可能会 客户端还没有初始化技能完毕 ，后续如果有问题 可以在这里加入paddingActivity或者重试机制
                return;
            }
            InternalTryActivateAbility(abilitySpec);
        }
        
        //告诉客户端 服务器技能激活成功
        [TargetRpc]
        public void ClientActivateAbilitySuccess(uint abilityHandle)
        {
            ClientActivateAbilitySuccessWithEventData_Implement(abilityHandle);
        }
        
        //告诉客户端 服务器技能激活成功
        [TargetRpc]
        public void ClientActivateAbilitySuccessWithEventData(uint abilityHandle, GameplayEventData eventData)
        {
            ClientActivateAbilitySuccessWithEventData_Implement(abilityHandle,eventData);
        }
        
        private void ClientActivateAbilitySuccessWithEventData_Implement(uint abilityHandle, GameplayEventData eventData = default)
        {
            GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
            if (!abilitySpec.IsValid())
            {
                GasLogger.Warning($"[GAS] [ClientActivateAbilitySuccess] 没有找到该技能 Handle: {abilityHandle}");
                return;
            }
            if (abilitySpec.GetInstancingPolicy() == GameplayAbilityInstancingPolicy.InstancedPerExecution)
            {
                GameplayAbilityInstance instancedAbility = abilitySpec.CreateNewInstance();
                instancedAbility.CallActivateAbility(eventData);
            }
            else
            {
                GameplayAbilityInstance instancedAbility = abilitySpec.GetPrimaryInstance();
                instancedAbility.CallActivateAbility(eventData);
            }
        }
        
        //向服务器发送激活技能的请求
        [Command]
        public void ServerTryActivateAbility(uint abilityHandle)
        {
            GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
            if (!abilitySpec.IsValid())
            {
                GasLogger.Warning($"[GAS] [ServerTryActivateAbility] 没有找到该技能 Handle: {abilityHandle}");
                return;
            }
            InternalTryActivateAbility(abilitySpec);
        }

        public GameplayAbilitySpec FindAbilitySpecFromHandle(uint abilityHandle)
        {
            foreach (var abilitySpec in _abilitySpecContainer.Items)
            {
                if (abilitySpec.Handle == abilityHandle)
                {
                    return abilitySpec;
                }
            }
            return null;
        }
        
        //遍历所有技能实例
        public void ForeachAllAbilitySpec(Func<GameplayAbilitySpec,bool> action)
        {
            foreach (var abilitySpec in _abilitySpecContainer.Items)
            {
                if (!abilitySpec.IsValid())
                {
                    continue;
                }

                if (action.Invoke(abilitySpec))
                {
                    break;
                }
            }
        }
        
        
        /// <summary>
        /// 应用技能的 打断
        /// </summary>
        internal void ApplyAbilityBlockTags(GameplayTagContainer blockTags , bool bEnableBlockTags)
        {
            if (!blockTags.IsEmpty())
            {
                return;
            }
            _blockAbilityTagCountContainer.UpdateTagMap(blockTags,bEnableBlockTags ? 1 : -1);
        }

        internal void ApplyCancelAbility(GameplayAbility ability , GameplayTagContainer cancelTags)
        {
            if (cancelTags.IsEmpty())
            {
                return;
            }
            foreach (var item in _abilitySpecContainer.Items)
            {
                if (!item.IsValid() || !item.IsActive() || ability == item.Ability)
                {
                    continue;
                }
                GameplayTagContainer activeTags = item.Ability.AssetTags;
                if (activeTags.HasAny(cancelTags))
                {
                    List<GameplayAbilityInstance> instancedAbility = item.GetAllInstances();
                    foreach (var abilityInstance in instancedAbility)
                    {
                        abilityInstance.CancelAbility();
                    }
                }
            }
        }
        
        public float GetCooldownTimeRemainingByTags(GameplayTagContainer cooldownTags)
        {
            float remind = 0;
            if (!cooldownTags.IsEmpty() && HasAllMatchingGameplayTags(cooldownTags))
            {
                foreach (var effectSpec in _effectContainer.GameplayEffectSpecs)
                {
                    GameplayTagContainer grantedTags = effectSpec.Value.GetGrantedTags();
                    if (!grantedTags.IsEmpty() && grantedTags.HasAll(cooldownTags))
                    {
                        float effRemind = _effectContainer.GetRemainingDuration(effectSpec.Value.handle);
                        if (remind < effRemind)
                        {
                            remind = effRemind;
                        }
                    }
                }
            }
            return remind;
        }
        
        /// <summary>
        /// 获取冷却剩余时间
        /// </summary>
        public float GetCooldownTimeRemaining(uint abilityHandle)
        {
            GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
            float remind = 0;
            if (abilitySpec != null && abilitySpec.IsValid())
            {
                GameplayTagContainer tagContainer = abilitySpec.Ability.GetCoolDownTags();
                if (!tagContainer.IsEmpty() && HasAllMatchingGameplayTags(tagContainer))
                {
                    foreach (var effectSpec in _effectContainer.GameplayEffectSpecs)
                    {
                        GameplayTagContainer grantedTags = effectSpec.Value.GetGrantedTags();
                        if (!grantedTags.IsEmpty() && grantedTags.HasAll(tagContainer))
                        {
                            float effRemind = _effectContainer.GetRemainingDuration(effectSpec.Value.handle);
                            if (remind < effRemind)
                            {
                                remind = effRemind;
                            }
                        }
                    }
                }
            }
            return remind;
        }

        #endregion

        #region GamePlayEffect

        /// <summary>
        /// 对目标应用效果
        /// </summary>
        public GameplayEffectSpecHandle ApplyGameplayEffectToTarget(AbilitySystemComponent target, GameplayEffect effect , GameplayEffectParam effectParam)
        {
            if (!target)
            {
                return default;
            }

            if (effect == null)
            {
                return default;
            }

            if (!authority)
            {
                return default;
            }
            effectParam.TargetAscId = target.AscId;
            return target.ApplyGameplayEffectSpecToSelf(GameplayEffectSpec.MakeSpec(effect),effectParam);
        }

        //对自己应用效果
        public GameplayEffectSpecHandle ApplyGameplayEffectSpecToSelf(GameplayEffectSpec effectSpec , GameplayEffectParam effectParam)
        {
            return _effectContainer.ApplyGameplayEffectSpec(effectSpec,effectParam);
        }
        
        public float GetGameplayEffectRemainingDuration(GameplayEffectSpecHandle handle)
        {
            return _effectContainer.GetRemainingDuration(handle);
        }
        
        /// <summary>
        /// 移除ge
        /// </summary>
        public void RemoveGameplayEffectSpec(GameplayEffectSpecHandle handle)
        {
            _effectContainer.RemoveGameplayEffectSpec(handle);
        }

        #endregion
        
        #region Tags
        
        internal ActivateTagCountContainer GetActivateTagCountContainer()
        {
            return _tagCountContainer;
        }

        //检查是否存在任意标签
        public bool HasAnyMatchingGameplayTag(GameplayTag t)
        {
            return _tagCountContainer.HasAnyMatchingGameplayTag(t);
        }
        
        //检查是否存在任意标签
        public bool HasAnyMatchingGameplayTags(GameplayTagContainer tags)
        {
            return _tagCountContainer.HasAnyMatchingGameplayTags(tags);
        }
        
        public bool HasAllMatchingGameplayTags(GameplayTagContainer tags)
        {
            return _tagCountContainer.HasAllMatchingGameplayTags(tags);
        }
        
        public bool HasAnyMatchingGameplayTagsWithParent(GameplayTagContainer tags)
        {
            return _tagCountContainer.HasAnyMatchingGameplayTagsWithParent(tags);
        }
        
        public bool HasAllMatchingGameplayTagsWithParent(GameplayTagContainer tags)
        {
            return _tagCountContainer.HasAllMatchingGameplayTagsWithParent(tags);
        }
        
        /// <summary>
        /// Tag 中是否存在任何一个标签的 BlockAbilityTags 匹配
        /// </summary>
        internal bool HasAnyMatchingBlockAbilityTagsWithParent(GameplayTagContainer tags)
        {
            return _blockAbilityTagCountContainer.HasAnyMatchingGameplayTagsWithParent(tags);
        }
        
        internal void AddGameplayTagToCountMap(GameplayTag t , int count = 1)
        {
            _tagCountContainer.UpdateTagMap(t,count);
        }
        
        internal void RemoveGameplayTagFromCountMap(GameplayTag t , int count = 1)
        {
            _tagCountContainer.UpdateTagMap(t,-count);
        }
        
        internal void AddGameplayTagsToCountMap(GameplayTagContainer tags , int count = 1)
        {
            _tagCountContainer.UpdateTagMap(tags,count);
        }
        
        internal void RemoveGameplayTagsFromCountMap(GameplayTagContainer tags , int count = 1)
        {
            _tagCountContainer.UpdateTagMap(tags,-count);
        }

        #endregion

        #region Att and Modify
        
        //添加属性集
        public void AddAttributeSet(AttributeSet attributeSet)
        {
            _attributeSetContainer.AddAttributeSet(attributeSet);
        }

        //获取属性集
        public T AttrSet<T>() where T : AttributeSet
        {
            return _attributeSetContainer.TryGetAttributeSet<T>();
        }
        
        //把效果修改器添加到属性上
        internal void ApplyModAggregators(GameplayEffectSpec effectSpec)
        {
            _attributeSetContainer.ApplyModAggregators(effectSpec);
        }
        
        internal void RemoveModAggregators(GameplayEffectSpec effectSpec)
        {
            _attributeSetContainer.RemoveModAggregators(effectSpec);
        }

        internal void ApplyModFormInstant(GameplayEffectSpec effectSpec)
        {
            _attributeSetContainer.ApplyModFromInstantGameplayEffect(effectSpec);
        }

        internal bool CanApplyAttributeModifiers(GameplayEffect effect)
        {
            return _attributeSetContainer.CanApplyAttributeModifiers(effect);
        }

        #endregion

        #region Cue

        internal void AddGameplayCue(GameplayTag t , bool isAttach , GameplayCueContext context)
        {
            if (isAttach)
            {
                _gameplayCueContainer.AddCue(t);
            }
            //广播
            context.Instigator = this;
            _gameplayCueContainer.InvokeCue_OnAuthority(t, ExecuteCueType.OnExecute, context);
        }
        
        internal void RemoveGameplayCue(GameplayTag t)
        {
            _gameplayCueContainer.RemoveCue(t);
            _gameplayCueContainer.InvokeCue_OnAuthority(t, ExecuteCueType.Remove, default);
        }
        
        internal void ExecuteGameplayCueByType(GameplayTag t , ExecuteCueType executeCueType , GameplayCueContext context)
        {
            _gameplayCueContainer.InvokeCue_OnAuthority(t, executeCueType, context);
        }
        
        public void InvokeCue_OnAuthority(GameplayTag t , ExecuteCueType executeCueType , GameplayCueContext context)
        {
            _gameplayCueContainer.InvokeCue_OnAuthority(t, executeCueType, context);
        }

        #endregion

        #region 动画

        //注入动画播放器
        public void InjectAbilityAnimationPlayer(AbilityAnimationPlayerBase animationPlayer)
        {
            _abilityAnimationPlayer = animationPlayer;
            _abilityAnimationPlayer.Init(this);
        }
        
        [ClientRpc]
        public void PlayAnimationMulticast(string animationStateName , int layer , float transitionDuration  , float playSpeed  , bool isLooping)
        {
            _abilityAnimationPlayer.PlayAnimation(animationStateName,layer,transitionDuration,playSpeed,isLooping);
        }

        [ClientRpc]
        public void EndPlayAnimationMulticast()
        {
            _abilityAnimationPlayer.EndAnimation();
        }
        
        [Client]
        public void PlayClientAnimation(string animationStateName , int layer = 0 , float transitionDuration = 0.1f  , float playSpeed = 1f  , bool isLooping = false)
        {
            _abilityAnimationPlayer.PlayAnimation(animationStateName,layer,transitionDuration,playSpeed,isLooping);
        }
        
        [Client]
        public void EndClientAnimation()
        {
            _abilityAnimationPlayer.EndAnimation();
        }
        #endregion
    }
}

