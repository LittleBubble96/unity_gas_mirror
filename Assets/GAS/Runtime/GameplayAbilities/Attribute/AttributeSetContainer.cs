using System;
using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    public class AttributeSetContainer : AbilityDependencyNetMono
    {
        //需要同步，缓存得到属性集的类型和实例，方便后续获取属性集实例
        private AttributeSyncDictionary _attributeSets = new AttributeSyncDictionary();
        //需要同步，属性集中的每个属性的当前值，key是属性句柄，value是属性值
        private SyncDictionary<AttributeKey, float> _currentAttributeValues = new SyncDictionary<AttributeKey, float>();
        //需要同步，属性集中的每个属性的基础值，key是属性句柄，value是属性值
        private SyncDictionary<AttributeKey, float> _baseAttributeValues = new SyncDictionary<AttributeKey, float>();
        //不需要同步，属性聚合器，key是属性句柄，value是属性聚合器实例 【只有服务器存在】
        private Dictionary<string,AttributeAggregator> _attributeAggregatorsInServer = new Dictionary<string, AttributeAggregator>();
        

        //当基础和当前属性修改后会触发 [服务器调用]
        public Action<AttributeBase> OnAttributeChangedInServer { get; set; }

        private void Awake()
        {
            _attributeSets.SetOwner(this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _currentAttributeValues.OnSet += OnCurrentAttributeValueChangedInClient;
            _baseAttributeValues.OnSet += OnBaseAttributeValueChangedInClient;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _currentAttributeValues.OnSet -= OnCurrentAttributeValueChangedInClient;
            _baseAttributeValues.OnSet -= OnBaseAttributeValueChangedInClient;
        }
        
        public void AddAttributeSet(AttributeSet attributeSet)
        {
            attributeSet.SetOwner(this);
            string setName = attributeSet.GetType().ToString();
            _attributeSets.Add(setName,attributeSet);
            for (int i = 0; i < attributeSet.AttributeNames.Length; i++)
            {
                string shortName = attributeSet.AttributeNames[i];
                string fullName = $"{setName}.{attributeSet.AttributeNames[i]}";
                AttributeBase attribute = attributeSet.Get(shortName);
                _currentAttributeValues[AttributeKey.Make(setName,shortName)] = 0f;
                _baseAttributeValues[AttributeKey.Make(setName,shortName)] = 0f;
                _attributeAggregatorsInServer[fullName] = new AttributeAggregator(this, attribute);
            }
        }

        public T TryGetAttributeSet<T>() where T : AttributeSet
        {
            if (_attributeSets.TryGetValue(typeof(T).ToString(), out var attributeSet))
            {
                return (T)attributeSet;
            }
            return null;
        }
        
        internal float GetCurrentValue_Internal(AttributeKey key)
        {
            if (_currentAttributeValues.TryGetValue(key, out var value))
            {
                return value;
            }
            return 0f;
        }
        
        internal float GetBaseValue_Internal(AttributeKey key)
        {
            if (_baseAttributeValues.TryGetValue(key, out var value))
            {
                return value;
            }
            return 0f;
        }
        
        //禁止外部调用
        internal void SetCurrentValue_Internal(AttributeKey key , float value)
        {
            _currentAttributeValues[key] = value;
        }
        
        internal void SetBaseValue_Internal(AttributeKey key, float value)
        {
            _baseAttributeValues[key] = value;
        }

        //应用属性修改器，添加属性修改器到聚合器中
        public void ApplyModAggregators(GameplayEffectSpec effectSpec)
        {
            for (int i = 0; i < effectSpec.GetModifiers().Count; i++)
            {
                GameplayEffectModifier modifier = effectSpec.GetModifiers()[i];
                if (_attributeAggregatorsInServer.TryGetValue(modifier.attributeName, out var aggregator))
                {
                    aggregator.AddModifier(effectSpec.handle,modifier);
                }
            }
            UpdateCurrentValueWhenModifierIsDirty();
        }

        //移除属性修改器，从聚合器中移除属性修改器
        public void RemoveModAggregators(GameplayEffectSpec effectSpec)
        {
            for (int i = 0; i < effectSpec.GetModifiers().Count; i++)
            {
                GameplayEffectModifier modifier = effectSpec.GetModifiers()[i];
                if (_attributeAggregatorsInServer.TryGetValue(modifier.attributeName, out var aggregator))
                {
                    aggregator.RemoveModifier(effectSpec.handle);
                }
            }
            UpdateCurrentValueWhenModifierIsDirty();
        }

        //更新当前值
        private void UpdateCurrentValueWhenModifierIsDirty()
        {
            foreach (var kvp in _attributeAggregatorsInServer)
            {
                AttributeAggregator aggregator = kvp.Value;
                if (!aggregator.IsDirty)
                {
                    continue;
                }
                aggregator.UpdateCurrentValueWhenModifierIsDirty();
            }
        }
        
        internal void ApplyModFromInstantGameplayEffect(GameplayEffectSpec spec)
        {
            for (int i = 0; i < spec.GetModifiers().Count; i++)
            {
                GameplayEffectModifier modifier = spec.GetModifiers()[i];
                if (!_attributeSets.TryGetValue(modifier.attributeSetName, out var attributeSet))
                {
                    continue;
                }
                AttributeBase attribute = attributeSet.Get(modifier.attributeShortName);
                if (attribute == null)
                {
                    continue;
                }
                var magnitude = modifier.CalculateMagnitude(spec.handle);
                float newValue = attribute.BaseValue;
                newValue = modifier.CalculateByOperation(newValue,magnitude);
                attribute.SetBaseValue(newValue);
            }
        }

        //是否可以 足够消耗
        public bool CanApplyAttributeModifiers(GameplayEffect effect)
        {
            for (int i = 0; i < effect.Modifiers.Count; i++)
            {
                GameplayEffectModifier modifier = effect.Modifiers[i];
                if (!_attributeSets.TryGetValue(modifier.attributeSetName, out var attributeSet))
                {
                    continue;
                }
                AttributeBase attribute = attributeSet.Get(modifier.attributeShortName);
                if (attribute == null)
                {
                    continue;
                }
                var magnitude = modifier.CalculateMagnitude(default);
                float newValue = attribute.BaseValue;
                newValue = modifier.CalculateByOperation(newValue,magnitude);
                if (newValue <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDestroy()
        {
            foreach (var kvp in _attributeAggregatorsInServer)
            {
                kvp.Value.Dispose();
            }
        }

        #region 通知客户端
        
        
        private void OnCurrentAttributeValueChangedInClient(AttributeKey key, float old)
        {
            if (_attributeSets.TryGetValue(key.SetName, out var attributeSet))
            {
                AttributeBase attribute = attributeSet.Get(key.ShortName);
                attribute?.BroadcastCurrentValueChange(old, _currentAttributeValues[key]);
            }
        }

        private void OnBaseAttributeValueChangedInClient(AttributeKey key, float old)
        {
            if (_attributeSets.TryGetValue(key.SetName, out var attributeSet))
            {
                AttributeBase attribute = attributeSet.Get(key.ShortName);
                attribute?.BroadcastBaseValueChange(old, _baseAttributeValues[key]);
            }
        }
        #endregion
        
    }
    
    public struct AttributeKey
    {
        public string SetName;
        public string ShortName;

        public AttributeKey(string setName, string shortName)
        {
            SetName = setName;
            ShortName = shortName;
        }
        
        public static AttributeKey Make(string setName, string shortName)
        {
            return new AttributeKey(setName, shortName);
        }
    }
}