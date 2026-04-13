using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS.Ext
{
    [Serializable]
    [CueLabel("跟随Cue")]
    public class GameplayCueFollowActorEffect : GameplayCueNotifyActor
    {
        [LabelText("资源名")][ShowInInspector]
        public string effectResName;

        public override ICue CopyCue()
        {
            GameplayCueFollowActorEffect newCue = new GameplayCueFollowActorEffect
            {
                CueTag = CueTag,
                effectResName = effectResName
            };
            return newCue;
        }
    }
}