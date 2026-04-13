using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VSEngine.GAS
{
    public class GameplayEffectContainer : AbilityDependencyNetMono
    {
        public SyncDictionary<GameplayEffectSpecHandle,GameplayEffectSpec> GameplayEffectSpecs = new SyncDictionary<GameplayEffectSpecHandle, GameplayEffectSpec>();
        //获取持续时间的容器，方便客户端获取冷却时间
        public SyncDictionary<GameplayEffectSpecHandle,GameplayEffectUpdateInfo> GameplayEffectDurations = new SyncDictionary<GameplayEffectSpecHandle, GameplayEffectUpdateInfo>();

        public static uint GenerateHandle = 1;
        
        //服务器或者主机有权修改GE
        private List<GameplayEffectSpecHandle> _needRemoveHandles = new List<GameplayEffectSpecHandle>();
        // 记录修改前的GE Spec 用于比较是否需要触发激活或者失效
        private Dictionary<GameplayEffectSpecHandle,GameplayEffectSpec> _recordModifySpecs = new Dictionary<GameplayEffectSpecHandle, GameplayEffectSpec>();

        public override void OnStartServer()
        {
            base.OnStartServer();
            Owner.GetActivateTagCountContainer().RegisterTagChangedCallback(RefreshActivateEffect);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Owner.GetActivateTagCountContainer().UnregisterTagChangedCallback(RefreshActivateEffect);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            GameplayEffectSpecs.OnAdd += OnGameplayEffectSpecsAddInClient;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            GameplayEffectSpecs.OnAdd -= OnGameplayEffectSpecsAddInClient;
        }

        private void OnGameplayEffectSpecsAddInClient(GameplayEffectSpecHandle obj)
        {
            if (GameplayEffectSpecs.ContainsKey(obj))
            {
                Owner.NotifyClientAddedGameplayEffect(GameplayEffectSpecs[obj]);
            }
        }


        /// <summary>
        /// 应用GE
        /// </summary>
        public GameplayEffectSpecHandle ApplyGameplayEffectSpec(GameplayEffectSpec spec , GameplayEffectParam effectParam)
        {
            //只有服务器 或者 主机有权修改
            if (!authority)
            {
                return GameplayEffectSpecHandle.UnValidHandle;
            }
            // GameplayEffectSpecs.Add(spec);
            if (!spec.IsValid())
            {
                return GameplayEffectSpecHandle.UnValidHandle;
            }
            //检查组件是否可以通过
            if (!spec.CheckCanApply(Owner))
            {
                return GameplayEffectSpecHandle.UnValidHandle;
            }
            GameplayEffectDurationType durationType = spec.GetDurationType();
            bool isInstant = durationType == GameplayEffectDurationType.Instant || 
                             (durationType == GameplayEffectDurationType.Duration && spec.GetDuration() <= GameplayEffectDefine.NoDuration);
            if (isInstant)
            {
                //直接应用
                spec.TriggerInstantExecute(Owner);
                return GameplayEffectSpecHandle.UnValidHandle;
            }
            //只有 非瞬时的效果才会被添加到容器中
            spec.handle = new GameplayEffectSpecHandle(GenerateHandle++);
            spec.effectParam = effectParam;
            spec.TriggerAdded(Owner);
            spec.Apply(Owner);
            GameplayEffectSpecs.Add(spec.handle, spec);
            GameplayEffectDurations.Add(spec.handle, new GameplayEffectUpdateInfo(spec.GetDuration(),spec.GetPeriod(),NetworkTime.time));
            if (spec.GetPeriod() <= GameplayEffectDefine.NoPeriod)
            {
                //加入属性更新系统
                Owner.ApplyModAggregators(spec);
            }
            return spec.handle;
        }

        internal void RemoveGameplayEffectSpec(GameplayEffectSpecHandle handle)
        {
            if (!authority)
            {
                return;
            }
            if (!GameplayEffectSpecs.ContainsKey(handle))
            {
                return;
            }
            GameplayEffectSpec spec = GameplayEffectSpecs[handle];
            spec.TriggerRemoved(Owner);
            spec.DisApply(Owner);
            if (spec.GetPeriod() <= GameplayEffectDefine.NoPeriod)
            {
                //移除属性更新系统
                Owner.RemoveModAggregators(spec);
            }
            GameplayEffectSpecs.Remove(handle);
            GameplayEffectDurations.Remove(handle);
        }
        
        private void RefreshActivateEffect(GameplayTag targetTag)
        {
            _recordModifySpecs.Clear();
            foreach (var effectSpecKey in GameplayEffectSpecs.Keys)
            {
                GameplayEffectSpec effectSpec = GameplayEffectSpecs[effectSpecKey];
                bool needApply = effectSpec.CheckCanApply(Owner);
                if (!effectSpec.IsActive && needApply)
                {
                    effectSpec.Activate(Owner);
                }
                if (effectSpec.IsActive && !needApply)
                {
                    effectSpec.DeActivate(Owner);
                }
                _recordModifySpecs.Add(effectSpecKey,effectSpec);
            }
            //赋值
            foreach (var kvp in _recordModifySpecs)
            {
                GameplayEffectSpecs[kvp.Key] = kvp.Value;
            }
        }
        
        //获取剩余持续时间，只有服务器或者主机获取的时间是准确的，客户端可能不太准确
        public float GetRemainingDuration(GameplayEffectSpecHandle handle)
        {
            if (GameplayEffectSpecs.ContainsKey(handle) == false)
            {
                return 0;
            }

            if (GameplayEffectDurations.ContainsKey(handle) == false)
            {
                return 0;
            }
            if (isServer && GameplayEffectDurations.ContainsKey(handle))
            {
                return GameplayEffectDurations[handle].RemainingDuration;
            }
            //一般为 客户端，模拟剩余时间，可能不太准确
            double elapsed = NetworkTime.time - GameplayEffectDurations[handle].StartTime;
            float duration = GameplayEffectSpecs[handle].GetDuration();
            return Mathf.Max(0, duration - (float)elapsed);
        }

        public void DoUpdate(float dt)
        {
            if (!isServer)
            {
                return;
            }
            //更新时间 和周期执行
            foreach (var effectSpecKey in GameplayEffectSpecs.Keys)
            {
                GameplayEffectSpec effectSpec = GameplayEffectSpecs[effectSpecKey];
                if (!effectSpec.IsActive)
                {
                    continue;
                }
                //更新持续时间
                GameplayEffectUpdateInfo updateInfo = GameplayEffectDurations[effectSpecKey];
                if (effectSpec.GetDurationType() != GameplayEffectDurationType.Infinite)
                {
                    updateInfo.RemainingDuration -= dt;
                    if (updateInfo.RemainingDuration <= 0)
                    {
                        _needRemoveHandles.Add(effectSpecKey);
                        continue;
                    }
                }
                //周期执行
                if (effectSpec.GetPeriod() > GameplayEffectDefine.NoPeriod)
                {
                    updateInfo.PeriodTimeCount += dt;
                    if (updateInfo.PeriodTimeCount >= effectSpec.GetPeriod())
                    {
                        updateInfo.PeriodTimeCount = 0;
                        effectSpec.TriggerPeriodicExecute(Owner);
                    }
                }
                GameplayEffectDurations[effectSpecKey] = updateInfo;
            }
            HandleRemoveGameplayEffectSpec();
        }
        
        private void HandleRemoveGameplayEffectSpec()
        {
            foreach (var handle in _needRemoveHandles)
            {
                RemoveGameplayEffectSpec(handle);
            }
            _needRemoveHandles.Clear();
        }
        
    }
}