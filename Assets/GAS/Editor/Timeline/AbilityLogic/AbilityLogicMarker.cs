using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VSEngine.GAS;

namespace GAS.Editor.AbilityLogic
{
    public class AbilityLogicMarker : Marker, INotification
    {
        [SerializeField]
        [LabelText("执行逻辑")]
        private ClipLogicType logicType;
        
        public ClipLogicType LogicType => logicType;
        
        public PropertyName id => new PropertyName(GetType().Name);

        public AbilityTaskData GetAbilityTaskData(float startTime , float duration)
        {
            return new AbilityLogicGameplayTaskData()
            {
                StartTime = startTime,
                Duration = duration,
                logicType = logicType,
            };
        }
    }
}