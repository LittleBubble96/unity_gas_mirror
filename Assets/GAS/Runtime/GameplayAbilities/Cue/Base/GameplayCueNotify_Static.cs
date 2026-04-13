namespace VSEngine.GAS
{
    [IgnoreReadTypeEditor]
    public class GameplayCueNotifyStatic : ICue
    {
        public GameplayTag CueTag { get; set; }
        public virtual void OnExecute(GameplayCueContext context)
        {
        }

        public void OnRemove()
        {
        }

        public void OnGameplayEffectActivate()
        {
        }

        public void OnGameplayEffectDeactivate()
        {
        }

        public void OnTick(float dt)
        {
        }

        public ICue CopyCue()
        {
            return new GameplayCueNotifyStatic();
        }

#if UNITY_EDITOR
        public virtual void OnInitEditor()
        {
            
        }
        
        //编辑器执行Cue
        public virtual void OnExecuteEditor(GameplayCueContext context)
        {
        }
#endif
    }
}