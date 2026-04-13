namespace VSEngine.GAS
{
    public interface ICue
    {
        GameplayTag CueTag { get; set; }
        
        void OnExecute(GameplayCueContext context);
        
        void OnRemove();
        
        void OnGameplayEffectActivate();
        
        void OnGameplayEffectDeactivate();
        
        void OnTick(float dt);
        
        ICue CopyCue();
        
#if UNITY_EDITOR
        void OnExecuteEditor(GameplayCueContext context);
#endif
    }
}