using System;
using System.Collections;
using GAS.Editor.AbilityCue;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using VSEngine.GAS;

namespace GAS.Editor.RangeCheck
{
    [Serializable]
    public class GASCheckRangeClip : GasTimelineClip
    {
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [LabelText("目标Tag")]
        public GameplayTag cueTag;
        
        [LabelText("附加得GE")]
        public GamePlayEffectAsset effectAsset;
        
        [LabelText("是否检测同一个对象" )]
        public bool isCheckSameTarget = true;
        
        [LabelText("同一对象检测间隔")]
        public float sameTargetCheckInterval = 1f;
        
        [Title("范围数据")]
        public RangeStruct RangeData = new RangeStruct();
        [Title("射线检测层级")]
        public LayerMask targetLayerMask;

        private static IEnumerable TagChoices = new ValueDropdownList<GameplayTag>();

        private void OnEnable()
        {
            SetTagChoices();
        }

        private void SetTagChoices()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameplayTagsAsset>(GASSettingAsset.GAS_TAG_ASSET_PATH);
            if (asset != null)
            {
                var choices = new ValueDropdownList<GameplayTag>();

                foreach (var tag in asset.Tags)
                {
                    choices.Add(tag.TagName, tag);
                }
                TagChoices = choices;
            }
            else
            {
                TagChoices = new ValueDropdownList<GameplayTag>();
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<GASCheckRangePlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SetClip(this);
            return playable;
        }

        public override AbilityTaskData GetAbilityTaskData(float startTime, float dur)
        {
            return new CheckRangeTaskData()
            {
                StartTime = startTime,
                Duration = dur,
                CueTag = cueTag,
                effect = effectAsset != null ? new GameplayEffect(effectAsset) : null,
                isCheckSameTarget = isCheckSameTarget,
                sameTargetCheckInterval = sameTargetCheckInterval,
                rangeStruct = RangeData,
                targetLayerMask = targetLayerMask
            };
        }
        
    }
}