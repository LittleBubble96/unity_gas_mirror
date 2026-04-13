using System.Collections;
using Mirror;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace VSEngine.GAS
{
    [System.Serializable]
    public struct GameplayEffectModifier
    {
        [HorizontalGroup("attGroup",0.6f)]
        [LabelText("属性名")]
        [ValueDropdown("AttributeChoices")]
        [OnValueChanged( "OnChangeAttribute")]
        public string attributeName;
        
        [Sirenix.OdinInspector.ReadOnly][HorizontalGroup("attGroup")]
        [LabelText("Set名称")][LabelWidth(50)]
        public string attributeSetName;
        
        [Sirenix.OdinInspector.ReadOnly][HorizontalGroup("attGroup")]
        [LabelText("短名称")][LabelWidth(40)]
        public string attributeShortName;
        
        [LabelText("运算方式")]
        public GEOption modifierOp;
        
        [LabelText("数值")]
        public float magnitude;
        
        [LabelText("MMC函数")]
        public string MMCFunc;
        
        private void OnChangeAttribute()
        {
            string[] parts = attributeName.Split('.');
            if (parts.Length == 2)
            {
                attributeSetName = parts[0];
                attributeShortName = parts[1];
            }
            else
            {
                attributeSetName = "";
                attributeShortName = "";
            }
        }
        public float CalculateMagnitude(GameplayEffectSpecHandle specHandle)
        {
            if (string.IsNullOrEmpty(MMCFunc))
            {
                return magnitude;
            }
            var calculation = AbilitySystemGlobals.Get().GetModifierMagnitudeCalculation(MMCFunc);
            if (calculation != null)
            {
                return calculation.CalculateMagnitude(specHandle, magnitude);
            }
            return magnitude;
        }
        
        public float CalculateByOperation(float currentValue , float newMagnitude)
        {
            switch (modifierOp)
            {
                case GEOption.Add:
                    currentValue += newMagnitude;
                    break;
                case GEOption.Multiply:
                    currentValue *= newMagnitude;
                    break;
                case GEOption.Overwrite:
                    currentValue = newMagnitude;
                    break;
                case GEOption.Divide:
                    currentValue /= newMagnitude;
                    break;
                case GEOption.Max:
                    currentValue = Mathf.Max(currentValue, newMagnitude);
                    break;
                case GEOption.Min:
                    currentValue = Mathf.Min(currentValue, newMagnitude);
                    break;
            }
            return currentValue;
        }

#if UNITY_EDITOR
        
        public static IEnumerable AttributeChoices = new ValueDropdownList<string>();
        
#endif
    }
    
    //CustomReadWriteFunctions
    public static class GameplayEffectModifierCustomSerialization
    {
        public static void WriteGameplayEffectModifier(this Mirror.NetworkWriter writer, GameplayEffectModifier value)
        {
            writer.Write(value.attributeName);
            writer.Write(value.attributeSetName);
            writer.Write(value.attributeShortName);
            writer.WriteInt((int)value.modifierOp);
            writer.WriteFloat(value.magnitude);
            writer.Write(value.MMCFunc);
        }

        public static GameplayEffectModifier ReadGameplayEffectModifier(this Mirror.NetworkReader reader)
        {
            var modifier = new GameplayEffectModifier();
            modifier.attributeName = reader.ReadString();
            modifier.attributeSetName = reader.ReadString();
            modifier.attributeShortName = reader.ReadString();
            modifier.modifierOp = (GEOption)reader.ReadInt();
            modifier.magnitude = reader.ReadFloat();
            modifier.MMCFunc = reader.ReadString();
            return modifier;
        }
    }
}