using System.Collections.Generic;
using UnityEditor;

namespace VSEngine.GAS
{
    public class GameplayCueManager
    {
        private Dictionary<uint,List<ICue>> _cueMap = new Dictionary<uint, List<ICue>>();
        
        private Dictionary<GameplayTag,ICue> _cueTagMap = new Dictionary<GameplayTag, ICue>();
        
        private Dictionary<GameplayTag,Queue<ICue>> _cueTagQueueMap = new Dictionary<GameplayTag, Queue<ICue>>();

        public void Init()
        {
            //加载所有的标签数据
            var asset = AssetDatabase.LoadAssetAtPath<CueGlobalAsset>(GASSettingAsset.GAS_Cue_ASSET_PATH);
            if (asset != null)
            {
                foreach (var mapping in asset.CueMappings)
                {
                    if (mapping.Cue != null)
                    {
                        _cueTagMap[mapping.Tag] = mapping.Cue;
                        _cueTagMap[mapping.Tag].CueTag = mapping.Tag;
                    }
                }
            }
        }

        /// <summary>
        /// 执行GameplayCue
        /// </summary>
        public void HandleGameplayCue(GameplayTag cueTag , ExecuteCueType executeCueType , GameplayCueContext context)
        {
            if (!_cueTagMap.TryGetValue(cueTag, out var cue))
            {
                return;
            }

            if (cue is GameplayCueNotifyStatic notifyStatic)
            {
                if (executeCueType == ExecuteCueType.OnExecute)
                {
                    notifyStatic.OnExecute(context);
                }
                else if (executeCueType == ExecuteCueType.Remove)
                {
                    notifyStatic.OnRemove();
                }
            }
            else if (cue is GameplayCueNotifyActor notifyActor)
            {
                AbilitySystemComponent asc = context.Instigator;
                if (!asc)
                {
                    return;
                }
                if (executeCueType == ExecuteCueType.OnExecute)
                {
                    if (_cueTagQueueMap.TryGetValue(cueTag, out var queue) && queue != null && queue.Count > 0)
                    {
                        cue = queue.Dequeue();
                    }
                    else
                    {
                        cue = cue.CopyCue();
                    }
                    cue.OnExecute(context);
                    if (!_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        _cueMap[asc.AscId] = new List<ICue>();
                    }
                    _cueMap[asc.AscId].Add(cue);
                }
                else if (executeCueType == ExecuteCueType.Remove)
                {
                    if (_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        foreach (var cueT in cueList)
                        {
                            if (cueT.CueTag.Equals(cueTag))
                            {
                                cueT.OnRemove();
                                if (!_cueTagQueueMap.TryGetValue(cueTag, out var queue) || queue == null)
                                {
                                    queue = new Queue<ICue>();
                                    _cueTagQueueMap[cueTag] = queue;
                                }
                                queue.Enqueue(cue);
                                cueList.Remove(cueT);
                            }
                        }
                        if (cueList.Count == 0)
                        {
                            _cueMap.Remove(asc.AscId);
                        }
                    }
                }
                else if (executeCueType == ExecuteCueType.Activate)
                {
                    if (_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        foreach (var cueT in cueList)
                        {
                            if (cueT.CueTag.Equals(cueTag))
                            {
                                cueT.OnGameplayEffectActivate();
                            }
                        }
                    }
                }
                else if (executeCueType == ExecuteCueType.Deactivate)
                {
                    if (_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        foreach (var cueT in cueList)
                        {
                            if (cueT.CueTag.Equals(cueTag))
                            {
                                cueT.OnGameplayEffectDeactivate();
                            }
                        }
                    }
                }
            }
        }
        
        
        /// <summary>
        /// 执行GameplayCue
        /// </summary>
        public void HandleGameplayCueInEditor(GameplayTag cueTag , ExecuteCueType executeCueType , GameplayCueContext context)
        {
            if (!_cueTagMap.TryGetValue(cueTag, out var cue))
            {
                return;
            }

            if (cue is GameplayCueNotifyStatic notifyStatic)
            {
                if (executeCueType == ExecuteCueType.OnExecute)
                {
                    notifyStatic.OnExecuteEditor(context);
                }
                else if (executeCueType == ExecuteCueType.Remove)
                {
                    notifyStatic.OnRemove();
                }
            }
            else if (cue is GameplayCueNotifyActor notifyActor)
            {
                AbilitySystemComponent asc = context.Instigator;
                if (!asc)
                {
                    return;
                }
                if (executeCueType == ExecuteCueType.OnExecute)
                {
                    if (_cueTagQueueMap.TryGetValue(cueTag, out var queue) && queue != null && queue.Count > 0)
                    {
                        cue = queue.Dequeue();
                    }
                    else
                    {
                        cue = cue.CopyCue();
                    }
                    cue.OnExecute(context);
                    if (!_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        _cueMap[asc.AscId] = new List<ICue>();
                    }
                    _cueMap[asc.AscId].Add(cue);
                }
                else if (executeCueType == ExecuteCueType.Remove)
                {
                    if (_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        foreach (var cueT in cueList)
                        {
                            if (cueT.CueTag.Equals(cueTag))
                            {
                                cueT.OnRemove();
                                if (!_cueTagQueueMap.TryGetValue(cueTag, out var queue) || queue == null)
                                {
                                    queue = new Queue<ICue>();
                                    _cueTagQueueMap[cueTag] = queue;
                                }
                                queue.Enqueue(cue);
                                cueList.Remove(cueT);
                            }
                        }
                        if (cueList.Count == 0)
                        {
                            _cueMap.Remove(asc.AscId);
                        }
                    }
                }
                else if (executeCueType == ExecuteCueType.Activate)
                {
                    if (_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        foreach (var cueT in cueList)
                        {
                            if (cueT.CueTag.Equals(cueTag))
                            {
                                cueT.OnGameplayEffectActivate();
                            }
                        }
                    }
                }
                else if (executeCueType == ExecuteCueType.Deactivate)
                {
                    if (_cueMap.TryGetValue(asc.AscId, out var cueList))
                    {
                        foreach (var cueT in cueList)
                        {
                            if (cueT.CueTag.Equals(cueTag))
                            {
                                cueT.OnGameplayEffectDeactivate();
                            }
                        }
                    }
                }
            }
        }
    }
}