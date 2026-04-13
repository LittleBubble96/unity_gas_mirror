using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VSEngine.GAS;


[Serializable]
[CueLabel("技能1Cue")]
public class FireSkillCue : GameplayCueNotifyStatic
{
    [LabelText("资源名")][ShowInInspector]
    public string effectResName;

    [LabelText("偏移")][ShowInInspector]
    public Vector3 offset;

    public override void OnExecute(GameplayCueContext context)
    {
        base.OnExecute(context);
        if (context.Instigator)
        {
            Vector3 spawnLocation = context.Instigator.transform.position;
            Quaternion spawnRotation = context.Instigator.transform.rotation;
            Vector3 realOffset = Quaternion.Euler(0, spawnRotation.eulerAngles.y, 0) * offset;
            EffectManager.Instance.PlayEffect(effectResName , spawnLocation + realOffset, spawnRotation);
        }
    }

#if UNITY_EDITOR
    public override void OnExecuteEditor(GameplayCueContext context)
    {
        base.OnExecuteEditor(context);
        if (context.Instigator)
        {
            Vector3 spawnLocation = context.Instigator.transform.position;
            Quaternion spawnRotation = context.Instigator.transform.rotation;
            Vector3 realOffset = Quaternion.Euler(0, spawnRotation.eulerAngles.y, 0) * offset;
            EffectManager.Instance.PlayEffect_InEditor(effectResName , spawnLocation + realOffset, spawnRotation);
        }
    }
#endif

}
