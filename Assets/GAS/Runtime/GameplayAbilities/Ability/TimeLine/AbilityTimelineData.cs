using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    [System.Serializable]
    public class AbilityTimelineData
    {
        public List<AbilityTaskData> AbilityTasks = new List<AbilityTaskData>();

        public void Write(NetworkWriter writer)
        {
            writer.WriteInt(AbilityTasks.Count);
            foreach (var task in AbilityTasks)
            {
                task.Write(writer);
            }
        }

        public void Read(NetworkReader reader)
        {
            int count = reader.ReadInt();
            AbilityTasks = new List<AbilityTaskData>(count);
            for (int i = 0; i < count; i++)
            {
                string taskTypeName = reader.ReadString();
                var taskType = System.Type.GetType(taskTypeName);
                if (taskType == null)
                {
                    GasLogger.Error($"AbilityTimelineData.Read: Unknown task type {taskTypeName}");
                    continue;
                }
                var taskData = (AbilityTaskData)System.Activator.CreateInstance(taskType);
                taskData.Read(reader);
                AbilityTasks.Add(taskData);
            }
        }
    }
    
}