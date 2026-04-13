using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VSEngine.GAS;

namespace GAS.Editor
{
    public abstract class GasTimelineClip: PlayableAsset , ITimelineClipAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return default;
        }

        public ClipCaps clipCaps { get; }
        
        public abstract AbilityTaskData GetAbilityTaskData(float startTime , float duration);
    }
}