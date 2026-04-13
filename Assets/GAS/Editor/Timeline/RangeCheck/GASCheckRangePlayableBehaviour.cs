using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using VSEngine.GAS;

namespace GAS.Editor.RangeCheck
{
    public class GASCheckRangePlayableBehaviour : PlayableBehaviour
    {
        private GASCheckRangeClip _clip;
        private int _gizmosId;
        
        public void SetClip(GASCheckRangeClip clip)
        {
            _clip = clip;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (info.evaluationType != FrameData.EvaluationType.Playback)
            {
                return;
            }
            base.OnBehaviourPlay(playable, info);
            if (_gizmosId > 0)
            {
                DrawGizmosMonoIns.Instance.RemoveDrawGizmosData(_gizmosId);
                _gizmosId = 0;
            }

            DrawGizmosData data = new DrawGizmosData()
            {
                Range = _clip.RangeData,
                Color = Color.red,
            };
           DrawGizmosMonoIns.Instance.AddDrawGizmosData(ref data);
           _gizmosId = data.Id;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (info.evaluationType != FrameData.EvaluationType.Playback)
            {
                return;
            }
            base.OnBehaviourPause(playable, info);
            DrawGizmosMonoIns.Instance.RemoveDrawGizmosData(_gizmosId);
        }
    }
}