using System;
using System.Collections.Generic;
using UnityEngine;

namespace VSEngine.GAS
{
    //受不了了 那个弱智写的有问题啊 这个加入规则是。
    //1.如果这个效果 非立即触发 且不周期触发 则加入
    public class AttributeAggregator
    {
        private readonly AttributeBase _processedAttribute;
        
        //当聚合器被修改时，标记为脏，需要重新计算属性值
        public bool IsDirty { get; private set; }

        // 缓存属性修改器，uint为 效果id ，GameplayEffectModifier为修改器
        private List<Tuple<GameplayEffectSpecHandle,GameplayEffectModifier>> _modifierCache = new List<Tuple<GameplayEffectSpecHandle, GameplayEffectModifier>>();
        
        public AttributeAggregator()
        {
        }
        
        public AttributeAggregator(AttributeSetContainer owner, AttributeBase processedAttribute)
        {
            this._processedAttribute = processedAttribute;
            this._processedAttribute.RegisterPostBaseValueChange(UpdateCurrentValueWhenBaseValueIsDirty);
        }
        
        public void Dispose()
        {
            _processedAttribute.UnregisterPostBaseValueChange(UpdateCurrentValueWhenBaseValueIsDirty);
        }
        
        public void AddModifier(GameplayEffectSpecHandle handle, GameplayEffectModifier modifier)
        {
            _modifierCache.Add(new Tuple<GameplayEffectSpecHandle, GameplayEffectModifier>(handle, modifier));
            IsDirty = true;
        }
        
        public void RemoveModifier(GameplayEffectSpecHandle handle)
        {
            _modifierCache.RemoveAll(tuple => tuple.Item1.Equals(handle));
            IsDirty = true;
        }

        void SetClean()
        {
            IsDirty = false;
        }
        
        float CalculateNewValue()
        {
            float newValue = _processedAttribute.BaseValue;
            foreach (var tuple in _modifierCache)
            {
                var specHandle = tuple.Item1;
                GameplayEffectModifier modifier = tuple.Item2;
                var magnitude = modifier.CalculateMagnitude(specHandle);
                newValue = modifier.CalculateByOperation(newValue,magnitude);
            }
            return newValue;
        }

        //当GE 添加 移除后 需要重新刷新 属性得 CurrentValue
        public void UpdateCurrentValueWhenModifierIsDirty()
        {
            SetClean();
            float newValue = CalculateNewValue();
            _processedAttribute.SetCurrentValue(newValue);
        }

        //当BaseValue变化时 需要重新刷新 属性得 CurrentValue
        private void UpdateCurrentValueWhenBaseValueIsDirty(AttributeBase attribute, float oldBaseValue, float newBaseValue)
        {
            if (Math.Abs(oldBaseValue - newBaseValue) < AttributeBase.TOLERANCE) return;
            SetClean();
            float newValue = CalculateNewValue();
            _processedAttribute.SetCurrentValue(newValue);
        }
    }
}