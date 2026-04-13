using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EffectManager
{
    private static EffectManager _instance;
    public static EffectManager Instance
    {
        get { return _instance ??= new EffectManager(); }
        private set => _instance = value;
    }
    
    public Dictionary<string, EffectUpdateInfo> ActiveEffects { get; private set; } = new Dictionary<string, EffectUpdateInfo>();
    
    public Dictionary<string, Queue<EffectUpdateInfo>> EffectPools { get; private set; } = new Dictionary<string, Queue<EffectUpdateInfo>>();
    
    private List<string> _removeEffects = new List<string>();
    

    public void PlayEffect(string effectName , Vector3 pos , Quaternion rotation)
    {
        EffectUpdateInfo activeEffect;
        if (EffectPools.ContainsKey(effectName) && EffectPools[effectName].Count > 0)
        {
            activeEffect = EffectPools[effectName].Dequeue();
            activeEffect.EffectObject.SetActive(true);
            activeEffect.ElapsedTime = 0f;
        }
        else
        {
            var effectObjPrefab = Resources.Load<GameObject>(effectName);
            var effectObj = Object.Instantiate(effectObjPrefab);
            if (!effectObj)
            {
                return;
            }

            effectObj.AddComponent<EffectBase>();
            activeEffect = new EffectUpdateInfo
            {
                EffectName = effectName,
                EffectObject = effectObj,
                ElapsedTime = 0f
            };
        }
        activeEffect.EffectObject.transform.position = pos;
        activeEffect.EffectObject.transform.rotation = rotation;
        ActiveEffects[effectName] = activeEffect;
    }

    #region Editor

#if UNITY_EDITOR
    
    public void PlayEffect_InEditor(string effectName , Vector3 pos , Quaternion rotation)
    {
        EffectUpdateInfo activeEffect;
        if (EffectPools.ContainsKey(effectName) && EffectPools[effectName].Count > 0)
        {
            activeEffect = EffectPools[effectName].Dequeue();
            activeEffect.EffectObject.SetActive(true);
            activeEffect.ElapsedTime = 0f;
        }
        else
        {
            GameObject parent = GameObject.Find("TempContainer");
            var effectObjPrefab = Resources.Load<GameObject>(effectName);
            var effectObj = Object.Instantiate(effectObjPrefab,parent.transform);
            if (!effectObj)
            {
                return;
            }

            var effectBase = effectObj.AddComponent<EffectBase>();
            effectBase.OnInit();
            activeEffect = new EffectUpdateInfo
            {
                EffectName = effectName,
                EffectObject = effectObj,
                ElapsedTime = 0f
            };
        }
        activeEffect.EffectObject.transform.position = pos;
        activeEffect.EffectObject.transform.rotation = rotation;
        ParticleSystem particleSystems = activeEffect.EffectObject.GetComponentInChildren<ParticleSystem>();
        if (particleSystems)
        {
            particleSystems.Play();
        }
        ActiveEffects[effectName] = activeEffect;
    }
#endif

    #endregion

    public void Update(float dt)
    {
        _removeEffects.Clear();
        foreach (var kvp in ActiveEffects)
        {
            var effectInfo = kvp.Value;
            effectInfo.ElapsedTime += dt;
            // 这里假设特效持续时间为1秒，实际项目中可以根据需要调整
            if (effectInfo.ElapsedTime >= 2f)
            {
                effectInfo.ElapsedTime = 0f;
                _removeEffects.Add(kvp.Key);
                effectInfo.EffectObject.SetActive(false);
            }
        }
        foreach (var effectName in _removeEffects)
        {
            EffectUpdateInfo effectInfo = ActiveEffects[effectName];
            if (!EffectPools.ContainsKey(effectName))
            {
                EffectPools[effectName] = new Queue<EffectUpdateInfo>();
            }
            EffectPools[effectName].Enqueue(effectInfo);
            ActiveEffects.Remove(effectName);
        }
    
    }

    public class EffectUpdateInfo
    {
        public string EffectName;
        public GameObject EffectObject;
        public float ElapsedTime;
    }
}