using UnityEngine;

namespace VSEngine.GAS
{
    [IgnoreReadTypeEditor]
    public class GameplayCueNotifyActor :  ICue
    {
        public virtual GameplayTag CueTag { get; set; }
        
        public void OnExecute(GameplayCueContext context)
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

        public Transform Parent { get; set; }

        public void ExecuteCue(GameplayCueContext context)
        {
            
        }
        
        public void RemoveCue()
        {
            
        }

        public virtual ICue CopyCue()
        {
            return new GameplayCueNotifyActor();
        }

#if UNITY_EDITOR
        public virtual void OnInitEditor()
        {
            
        }

        public virtual void OnExecuteEditor(GameplayCueContext context)
        {
            
        }
#endif
    }
}