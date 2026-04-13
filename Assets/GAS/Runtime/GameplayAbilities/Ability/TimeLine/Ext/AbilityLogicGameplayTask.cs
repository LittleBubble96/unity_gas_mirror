using System;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS
{
    
    [System.Flags]
    public enum ClipLogicType
    {
        None        = 0,
        [LabelText("提交技能")]
        CommitAbility = 1 << 0,
        [LabelText("结束技能")]
        EndAbility    = 1 << 1,
    }
    
    /// <summary>
    /// 动画任务数据
    /// </summary>
    [Serializable]
    public class AbilityLogicGameplayTaskData : AbilityTaskData
    {
        [LabelText("逻辑类型")]
        public ClipLogicType logicType;

        public override void Read(NetworkReader reader)
        {
            base.Read(reader);
            logicType = (ClipLogicType)reader.ReadInt();
        }

        public override void Write(NetworkWriter writer)
        {
            base.Write(writer);
            writer.WriteInt((int)logicType);
        }
    }
    
    public class AbilityLogicGameplayTask : AbilityGameplayTask<AbilityLogicGameplayTaskData>
    {
        public override void OnStart()
        {
            if (Data.logicType.HasFlag(ClipLogicType.CommitAbility))
            {
                Timeline.OwnerIns.CommitAbility();
            }
            if (Data.logicType.HasFlag(ClipLogicType.EndAbility))
            {
                Timeline.OwnerIns.EndAbility(true);
            }
        }

        public override void OnEnd()
        {
            
        }
    }
}