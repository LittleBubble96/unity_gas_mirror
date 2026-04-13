using Mirror;
using UnityEngine;

namespace VSEngine.GAS
{
    [System.Serializable]
    public struct GameplayCueSpec
    {
        public GameplayTag GameplayCueTag;    
    }

    //CustomReadWriteFunctions
    public static class GameplayCueSpecSerializer
    {
        public static void WriteGameplayAbilitySpec(this NetworkWriter writer, GameplayCueSpec spec)
        {
            writer.WriteGameplayTag(spec.GameplayCueTag);
        }

        public static GameplayCueSpec ReadGameplayAbilitySpec(this NetworkReader reader)
        {
            GameplayCueSpec spec = new GameplayCueSpec();
            spec.GameplayCueTag = reader.ReadGameplayTag();
            return spec;
        }
    }
    
    public struct GameplayCueContext
    {
        public AbilitySystemComponent Instigator;
        public RaycastHit HitResult;
        public uint SourceAscId;
        
        public static GameplayCueContext MakeContext(AbilitySystemComponent instigator, RaycastHit hitResult, uint sourceAscId)
        {
            GameplayCueContext context = new GameplayCueContext();
            context.Instigator = instigator;
            context.HitResult = hitResult;
            context.SourceAscId = sourceAscId;
            return context;
        }
    }
    
    
}