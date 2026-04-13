using UnityEngine.Timeline;

namespace GAS.Editor.Animation
{
    [UnityEngine.Timeline.TrackColor(0.8f, 0.2f, 0.2f)]
    [UnityEngine.Timeline.TrackClipType(typeof(GASAnimationClip))]
    [UnityEngine.Timeline.TrackBindingType(typeof(UnityEngine.Animator))]
    public class GASAnimationTrack : UnityEngine.Timeline.TrackAsset
    {
       
    }
}