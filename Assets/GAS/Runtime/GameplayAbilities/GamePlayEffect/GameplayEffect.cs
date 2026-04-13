using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    [System.Serializable]
    public class GameplayEffect
    {
        //缓存赋予的标签
        public GameplayTagContainer GrantedTags;
        //执行组件列表 
        //客户端暂时 不写入执行组件列表
        public List<ExecuteComponentFunc> ExecuteComponentFuncList = new List<ExecuteComponentFunc>();
        
        //属性修改
        public List<GameplayEffectModifier> Modifiers = new List<GameplayEffectModifier>();
        //周期
        public GameplayEffectDurationType DurationType;
        public float DurationMagnitude;
        public float Period;
        //Cues
        public GameplayTagContainer CueOnExecute;
        public GameplayTagContainer CueOnRemove;
        public GameplayTagContainer CueOnAdd;
        public GameplayTagContainer CueOnActivate;
        public GameplayTagContainer CueOnDeactivate;
        public GameplayTagContainer CueDurational;

        public GameplayEffect()
        {
            
        }

        public GameplayEffect(GamePlayEffectAsset asset)
        {
            if (asset != null)
            {
                ExecuteComponentFuncList.AddRange(asset.GeComponents);
                DurationType = asset.DurationType;
                DurationMagnitude = asset.Duration;
                Period = asset.Period;
                Modifiers.AddRange(asset.AttributeModifiers);
                CueOnExecute = new GameplayTagContainer(asset.CueOnExecute);
                CueOnRemove = new GameplayTagContainer(asset.CueOnRemove);
                CueOnAdd = new GameplayTagContainer(asset.CueOnAdd);
                CueOnActivate = new GameplayTagContainer(asset.CueOnActivate);
                CueOnDeactivate = new GameplayTagContainer(asset.CueOnDeactivate);
                CueDurational = new GameplayTagContainer(asset.CueDurational);
            }
            foreach (var execute in ExecuteComponentFuncList)
            {
                GrantedTags.AddTags(execute.GetCacheGrantedTags());
            }
        }
    }
    
    //CustomReadWriteFunctions
    public static class GameplayEffectCustomSerialization
    {
        public static void WriteGameplayEffect(this Mirror.NetworkWriter writer, GameplayEffect value)
        {
            if (value == null)
            {
                writer.WriteInt(0);
                return;
            }
            writer.WriteInt(1);
            writer.WriteGameplayTagContainer(value.GrantedTags);
            writer.WriteInt((int)value.DurationType);
            writer.WriteFloat(value.DurationMagnitude);
            writer.WriteFloat(value.Period);
            writer.WriteInt(value.Modifiers.Count);
            foreach (var modifier in value.Modifiers)
            {
                writer.WriteGameplayEffectModifier(modifier);
            }
            
        }

        public static GameplayEffect ReadGameplayEffect(this Mirror.NetworkReader reader)
        {
            if (reader.ReadInt() == 0)
            {
                return null;
            }
            var grantedTags = reader.ReadGameplayTagContainer();
            var durationType = (GameplayEffectDurationType)reader.ReadInt();
            var durationMagnitude = reader.ReadFloat();
            var period = reader.ReadFloat();
            var modifiers = new List<GameplayEffectModifier>();
            int modifierCount = reader.ReadInt();
            for (int i = 0; i < modifierCount; i++)
            {
                modifiers.Add(reader.ReadGameplayEffectModifier());
            }
            return new GameplayEffect
            {
                GrantedTags = grantedTags,
                DurationType = durationType,
                DurationMagnitude = durationMagnitude,
                Period = period,
                Modifiers = modifiers,
            };
        }
    }
}