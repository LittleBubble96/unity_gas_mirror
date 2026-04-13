using System;
using Mirror;

namespace VSEngine.GAS
{
    [System.Serializable]
    public class AttributeSet
    {
        public AttributeSetContainer Owner { get; private set; }
        
        public void SetOwner(AttributeSetContainer owner)
        {
            Owner = owner;
            //设置Owner后，属性集中的每个属性的Owner也要设置
            foreach (var attName in AttributeNames)
            {
                AttributeBase attribute = Get(attName);
                if (attribute != null)
                {
                    attribute.SetOwner(owner);
                }
            }
        }
        
        public AttributeBase Get(string attName)
        {
            AttributeBase attribute = GetInternal(attName);
            if (attribute != null)
            {
                return attribute;
            }
            return null;
        }

        protected virtual AttributeBase GetInternal(string attName)
        {
            return null;
        }

        public virtual string[] AttributeNames { get; } = null;

    }

    [System.Serializable]
    public class AttributeBase
    {
        public static float TOLERANCE = 0.0001f;
        public readonly string Name;
        public readonly string SetName;
        public readonly string ShortName;
        
        private event Action<AttributeBase, float, float> _onPostCurrentValueChange;
        private event Action<AttributeBase, float, float> _onPostBaseValueChange;
        private event Action<AttributeBase, float> _onPreCurrentValueChange;
        private event Func<AttributeBase, float, float> _onPreBaseValueChange;
        
        public AttributeSetContainer Owner { get; private set; }

        public AttributeBase(string attrSetName, string attrName)
        {
            SetName = attrSetName;
            Name = $"{attrSetName}.{attrName}";
            ShortName = attrName;
        }
        
        public void SetOwner(AttributeSetContainer owner)
        {
            Owner = owner;
        }
        
        public float BaseValue
        {
            get
            {
                if (!Owner)
                {
                    GasLogger.Error($"[GAS] GetBaseValue AttributeBase has no Owner");
                    return 0f;
                }
                return Owner.GetBaseValue_Internal(MakeKey());
            }
        }
        
        public float CurrentValue
        {
            get
            {
                if (!Owner)
                {
                    GasLogger.Error($"[GAS] GetCurrentValue AttributeBase has no Owner");
                    return 0f;
                }
                return Owner.GetCurrentValue_Internal(MakeKey());
            }
        }

        public void SetBaseValue(float value)
        {
            if (!Owner)
            {
                GasLogger.Error($"[GAS] SetBaseValue AttributeBase has no Owner");
                return;
            }
            BroadcastPreBaseValueChange(value);
            float oldValue = BaseValue;
            Owner.SetBaseValue_Internal(MakeKey(),value);
            Owner.OnAttributeChangedInServer?.Invoke(this);
            if (Math.Abs(oldValue - value) > TOLERANCE)
            {
                _onPostBaseValueChange?.Invoke(this, oldValue, value);
            }
        }

        public void SetCurrentValue(float value)
        {
            if (!Owner)
            {
                GasLogger.Error($"[GAS] SetBaseValue AttributeBase has no Owner");
                return;
            }
            BroadcastPreCurrentValueChange(value);
            float oldValue = CurrentValue;
            Owner.SetCurrentValue_Internal(MakeKey(),value);
            Owner.OnAttributeChangedInServer?.Invoke(this);
            float newValue = CurrentValue;
            if (Math.Abs(oldValue - newValue) > TOLERANCE)
            {
                BroadcastCurrentValueChange(oldValue, newValue);
            }
        }
        
        private AttributeKey MakeKey()
        {
            return AttributeKey.Make(SetName, ShortName);
        }
        
        //广播修改
        public void BroadcastBaseValueChange(float oldValue, float newValue)
        {
            _onPostBaseValueChange?.Invoke(this, oldValue, newValue);
        }
        
        public void BroadcastCurrentValueChange(float oldValue, float newValue)
        {
            _onPostCurrentValueChange?.Invoke(this, oldValue, newValue);
        }
        
        public void BroadcastPreBaseValueChange(float newValue)
        {
            _onPreBaseValueChange?.Invoke(this, newValue);
        }
        
        public void BroadcastPreCurrentValueChange(float newValue)
        {
            _onPreCurrentValueChange?.Invoke(this, newValue);
        }
        
        public void RegisterPreBaseValueChange(Func<AttributeBase, float,float> func)
        {
            _onPreBaseValueChange += func;
        }

        public void RegisterPostBaseValueChange(Action<AttributeBase, float, float> action)
        {
            _onPostBaseValueChange += action;
        }

        public void RegisterPreCurrentValueChange(Action<AttributeBase, float> action)
        {
            _onPreCurrentValueChange += action;
        }

        public void RegisterPostCurrentValueChange(Action<AttributeBase, float, float> action)
        {
            _onPostCurrentValueChange += action;
        }

        public void UnregisterPreBaseValueChange(Func<AttributeBase, float,float> func)
        {
            _onPreBaseValueChange -= func;
        }

        public void UnregisterPostBaseValueChange(Action<AttributeBase, float, float> action)
        {
            _onPostBaseValueChange -= action;
        }

        public void UnregisterPreCurrentValueChange(Action<AttributeBase, float> action)
        {
            _onPreCurrentValueChange -= action;
        }

        public void UnregisterPostCurrentValueChange(Action<AttributeBase, float, float> action)
        {
            _onPostCurrentValueChange -= action;
        }

        public virtual void Dispose()
        {
            _onPreBaseValueChange = null;
            _onPostBaseValueChange = null;
            _onPreCurrentValueChange = null;
            _onPostCurrentValueChange = null;
        }
    }
}