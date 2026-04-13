using System;
using System.Collections;
using GAS.Editor.Animation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using VSEngine.GAS;

namespace GAS.Editor.AbilityCue
{
    [Serializable]
    public class GASCueClip : GasTimelineClip
    {
        [ValueDropdown("TagChoices",HideChildProperties = true)]
        [LabelText("Cue标签")]
        public GameplayTag cueTag;
        
        [LabelText("结束时移除Cue")]
        public bool isEndRemove = true;

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
            var playable = ScriptPlayable<GASCuePlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SetCueClip(this);
            return playable;
        }

        public override AbilityTaskData GetAbilityTaskData(float startTime, float dur)
        {
            return new CueTaskData
            {
                StartTime = startTime,
                Duration = dur,
                CueTag = cueTag,
                RemoveOnAbilityEnd = isEndRemove
            };
        }
    }
}