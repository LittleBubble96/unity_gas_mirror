using System;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS
{
    public enum CheckRangeType
    {
        [LabelText("球形")]
        Sphere,
        [LabelText("盒形")]
        Box,
    }
    
    [Serializable]
    public struct RangeStruct
    {
        [LabelText("检测范围类型")]
        public CheckRangeType RangeType;
        [LabelText("中心点")]
        public Vector3 Center;
        [LabelText("半径")] [ShowIf("RangeType", CheckRangeType.Sphere)]
        public float Radius;
        [LabelText("盒子大小")] [ShowIf("RangeType", CheckRangeType.Box)]
        public Vector3 BoxSize;

        public void DrawGizmos(Color color)
        {
            Gizmos.color = color;
            if (RangeType == CheckRangeType.Sphere)
            {
                Gizmos.DrawWireSphere(Center, Radius);
            }
            else if (RangeType == CheckRangeType.Box && BoxSize != Vector3.zero)
            {
                Gizmos.DrawWireCube(Center, BoxSize);
            }
        }

        
        public Collider[] CheckOverlap(Transform transform, LayerMask layerMask)
        {
            if (RangeType == CheckRangeType.Sphere)
            {
                return Physics.OverlapSphere(transform.TransformPoint(Center), Radius, layerMask);
            }
            else if (RangeType == CheckRangeType.Box)
            {
                Vector3 worldCenter = transform.TransformPoint(Center);
                Quaternion worldRotation = transform.rotation;
                return Physics.OverlapBox(worldCenter, BoxSize * 0.5f, worldRotation, layerMask);
            }
            return null;
        }
        
        
        public RaycastHit CheckSingleRayCast(Transform transform, Collider targetCollider, LayerMask layerMask)
        {
            Vector3 worldCenter = transform.TransformPoint(Center);
            Vector3 targetPos = targetCollider.transform.position;
            if (targetCollider is CapsuleCollider capsule)
            {
                targetPos = capsule.transform.TransformPoint(capsule.center);
            }
            else if (targetCollider is BoxCollider box)
            {
                targetPos = box.transform.TransformPoint(box.center);
            }
            else if (targetCollider is SphereCollider sphere)
            {
                targetPos = sphere.transform.TransformPoint(sphere.center);
            }
            Vector3 direction = (targetPos - worldCenter).normalized;
            float distance = Vector3.Distance(worldCenter, targetPos);
            if (Physics.Raycast(worldCenter, direction, out var hitInfo, distance, layerMask))
            {
                return hitInfo;
            }
            return default;
        }
        
        public void Read(NetworkReader reader)
        {
            RangeType = (CheckRangeType)reader.ReadInt();
            Center = reader.ReadVector3();
            if (RangeType == CheckRangeType.Sphere)
            {
                Radius = reader.ReadFloat();
            }
            else if (RangeType == CheckRangeType.Box)
            {
                BoxSize = reader.ReadVector3();
            }
        }
        
        public void Write(NetworkWriter writer)
        {
            writer.WriteInt((int)RangeType);
            writer.WriteVector3(Center);
            if (RangeType == CheckRangeType.Sphere)
            {
                writer.WriteFloat(Radius);
            }
            else if (RangeType == CheckRangeType.Box)
            {
                writer.WriteVector3(BoxSize);
            }
        }
    }
    /// <summary>
    /// 检测范围 任务数据
    /// </summary>
    [Serializable]
    public class CheckRangeTaskData : AbilityTaskData
    {
        public GameplayTag CueTag;
        
        public GameplayEffect effect;
        
        public bool isCheckSameTarget = true;
        
        public float sameTargetCheckInterval = 1f;
        
        public RangeStruct rangeStruct;
        
        public LayerMask targetLayerMask;

        public override void Read(NetworkReader reader)
        {
            base.Read(reader);
            CueTag = reader.ReadGameplayTag();
            effect = reader.ReadGameplayEffect();
            isCheckSameTarget = reader.ReadBool();
            sameTargetCheckInterval = reader.ReadFloat();
            rangeStruct.Read(reader);
            targetLayerMask = reader.ReadInt();
        }

        public override void Write(NetworkWriter writer)
        {
            base.Write(writer);
            writer.WriteGameplayTag(CueTag);
            writer.WriteGameplayEffect(effect);
            writer.WriteBool(isCheckSameTarget);
            writer.WriteFloat(sameTargetCheckInterval);
            rangeStruct.Write(writer);
            writer.WriteInt(targetLayerMask);
        }
    }

    /// <summary>
    /// Cue 任务 - 执行 GameplayCue 效果
    /// </summary>
    public class CheckRangeGameplayTask : AbilityGameplayTask<CheckRangeTaskData>
    {
        private Dictionary<uint, CheckAscInfo> _checkedTargets = new Dictionary<uint, CheckAscInfo>();
        private Queue<uint> _removeQueue = new Queue<uint>();
        
        private static Queue<CheckAscInfo> _checkAscInfoPool = new Queue<CheckAscInfo>();
        public override void OnStart()
        {
        }

        public override bool DoUpdate(float dt)
        {
            if (AbilitySystemComponent && !AbilitySystemComponent.isServer)
            {
                return false;
            }
            UpdateCheckedTargets(dt);
            CheckRayCast();
            return base.DoUpdate(dt);
        }

        public override void OnEnd()
        {
        }

        private void CheckRayCast()
        {
            if (!AbilitySystemComponent)
            {
                return;
            }
            Collider[] colliders = Data.rangeStruct.CheckOverlap(AbilitySystemComponent.transform, Data.targetLayerMask);
            if (colliders is { Length: > 0 })
            {
                foreach (var collider in colliders)
                {
                    if (!collider)
                    {
                        continue;
                    }
                    AbilitySystemComponent asc = collider.GetComponent<AbilitySystemComponent>();
                    if (!asc)
                    {
                        continue;
                    }
                    if (asc.AscId == AbilitySystemComponent.AscId)
                    {
                        continue;
                    }
                    RaycastHit hitInfo = Data.rangeStruct.CheckSingleRayCast(AbilitySystemComponent.transform, collider, Data.targetLayerMask);
                    GameplayEffectParam effectParam = new GameplayEffectParam()
                    {
                        HitInfo = hitInfo,
                    };
                    if (Data.isCheckSameTarget)
                    {
                        if (_checkedTargets.TryGetValue(asc.AscId, out var checkAsc))
                        {
                            if (checkAsc.LastCheckTime > Data.sameTargetCheckInterval)
                            {
                                _checkedTargets[asc.AscId].LastCheckTime = 0f;
                                AbilitySystemComponent.ApplyGameplayEffectToTarget(asc , Data.effect, effectParam);
                            }
                        }
                        else
                        {
                            _checkedTargets.Add(asc.AscId, GetCheckAscInfo());
                            _checkedTargets[asc.AscId].LastCheckTime = 0f;
                            AbilitySystemComponent.ApplyGameplayEffectToTarget(asc , Data.effect, effectParam);
                        }
                    }
                    else
                    {
                        AbilitySystemComponent.ApplyGameplayEffectToTarget(asc , Data.effect, effectParam);
                    }
                }
            }
        }
        
        private void UpdateCheckedTargets(float dt)
        {
            _removeQueue.Clear();
            foreach (var checkAsc in _checkedTargets)
            {
                if (checkAsc.Value.LastCheckTime > Data.sameTargetCheckInterval)
                {
                    _removeQueue.Enqueue(checkAsc.Key);
                }
                else
                {
                    _checkedTargets[checkAsc.Key].LastCheckTime = checkAsc.Value.LastCheckTime + dt;
                }
            }

            while (_removeQueue.Count > 0)
            {
                uint ascId = _removeQueue.Dequeue();
                CheckAscInfo checkAscInfo = _checkedTargets[ascId];
                _checkedTargets.Remove(ascId);
                ReleaseCheckAscInfo(checkAscInfo);
            }
        }
        
        private CheckAscInfo GetCheckAscInfo()
        {
            if (_checkAscInfoPool.Count > 0)
            {
                return _checkAscInfoPool.Dequeue();
            }
            return new CheckAscInfo();
        }
        
        private void ReleaseCheckAscInfo(CheckAscInfo checkAscInfo)
        {
            _checkAscInfoPool.Enqueue(checkAscInfo);
        }

        public override void OnClear()
        {
            base.OnClear();
            _checkedTargets.Clear();
            _removeQueue.Clear();
        }
    }

    public class CheckAscInfo
    {
        public float LastCheckTime;
    }
}
