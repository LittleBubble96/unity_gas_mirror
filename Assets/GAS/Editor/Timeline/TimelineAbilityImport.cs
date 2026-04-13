using System;
using GAS.Editor.AbilityLogic;
using GAS.Editor.Animation;
using Mirror;
using UnityEngine.Timeline;
using VSEngine.GAS;

namespace GAS.Editor
{
    public class TimelineAbilityImport
    {
        public static string SaveAbilityTimelineAsset(TimelineAsset timelineAsset)
        {
            try
            {
                AbilityTimelineData timelineData = new AbilityTimelineData();
                foreach (var track in timelineAsset.GetOutputTracks())
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset is GasTimelineClip abilityClip)
                        {
                            float startTime = (float)clip.start;
                            float duration = (float)clip.duration;
                            var taskData = abilityClip.GetAbilityTaskData(startTime,duration);
                            timelineData.AbilityTasks.Add(taskData);
                        }
                    }
                }
                //获取标记
                foreach (var marker in timelineAsset.markerTrack.GetMarkers())
                {
                    if (marker is AbilityLogicMarker logicMarker)
                    {
                        float startTime = (float)logicMarker.time;
                        var taskData = logicMarker.GetAbilityTaskData(startTime,0);
                        timelineData.AbilityTasks.Add(taskData);
                    }
                }
                NetworkWriter writer = new NetworkWriter();
                timelineData.Write(writer);
                byte[] bytes = writer.ToArray();
                string writePath = GASSettingAsset.GAS_Timeline_ASSET_PATH + timelineAsset.name + ".bytes";
                System.IO.File.WriteAllBytes(writePath, bytes);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to save ability timeline asset: {e}");
                return $"Failed to save ability timeline asset: {e.Message}";
            }
            
            return "";
        }
    }
}