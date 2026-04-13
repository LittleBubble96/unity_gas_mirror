using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    public class AbilityTimelineGlobalData
    {
        // 技能时间线工厂类，负责创建技能时间线实例
        public AbilityTimelineFactory Factory { get; private set; } = DefaultAbilityTimelineFactory.MakeInstance();
        // 注册技能解析数据 技能名称=》技能解析数据  技能名称最好不要重复
        private Dictionary<string,AbilityTimelineData> _abilityTimelineDataMap = new Dictionary<string, AbilityTimelineData>();
        
        public AbilityTimelineData GetAbilityTimelineData(string abilityName)
        {
            if (_abilityTimelineDataMap.TryGetValue(abilityName, out var data))
            {
                return data;
            }
            bool parseResult = TryParseAbilityTimelineData(abilityName);
            if (parseResult)
            {
                return _abilityTimelineDataMap[abilityName];
            }
            return null;
        }
        
        private bool TryParseAbilityTimelineData(string abilityName)
        {
            if (_abilityTimelineDataMap.ContainsKey(abilityName))
            {
                return false;
            }
            string readPath = $"{GASSettingAsset.GAS_Timeline_ASSET_PATH}{abilityName}_Timeline.bytes";
            if (!System.IO.File.Exists(readPath))
            {
                GasLogger.Error($"[GAS] AbilityTimelineGlobalData.TryParseAbilityTimelineData: Timeline data file not found for ability {abilityName} at path {readPath}");
                return false;
            }
            NetworkReader reader = new NetworkReader(System.IO.File.ReadAllBytes(readPath));
            AbilityTimelineData timelineData = new AbilityTimelineData();
            timelineData.Read(reader);
            _abilityTimelineDataMap.Add(abilityName, timelineData);
            return true;
        }
    }
}