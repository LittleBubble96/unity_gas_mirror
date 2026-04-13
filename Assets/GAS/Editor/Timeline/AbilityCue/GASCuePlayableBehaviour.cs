using GAS.Editor.Animation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using VSEngine.GAS;

namespace GAS.Editor.AbilityCue
{
    public class GASCuePlayableBehaviour : PlayableBehaviour
    {
        private GASCueClip _cueClip;
        
        public void SetCueClip(GASCueClip clip)
        {
            _cueClip = clip;
        }
        
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            if (info.evaluationType != FrameData.EvaluationType.Playback)
            {
                return;
            }
            AbilitySystemComponent abilitySystemComponent = GameObject.FindObjectOfType<AbilitySystemComponent>();
            
            AbilityTimelineEditorWindow.CueManager.HandleGameplayCueInEditor(_cueClip.cueTag,ExecuteCueType.OnExecute,new GameplayCueContext()
            {
                Instigator = abilitySystemComponent
            });
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
        }
    }
}