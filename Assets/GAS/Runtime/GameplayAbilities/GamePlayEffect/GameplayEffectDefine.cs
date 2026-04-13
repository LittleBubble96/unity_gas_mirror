using Sirenix.OdinInspector;

namespace VSEngine.GAS
{
    public enum GameplayEffectDurationType
    {
        [LabelText("瞬时")] Instant,
        [LabelText("持续")] Duration,
        [LabelText("无限")] Infinite
    }
    
    public static class GameplayEffectDefine
    {
        public const float NoPeriod = 0;
        public const float NoDuration = 0;
    }
}