namespace VSEngine.GAS
{
    public abstract class AbilityTimelineFactory
    {
        /// <summary>
        /// 创建一个新的AbilityTimeline实例
        /// </summary>
        /// <returns></returns>
        public abstract AbilityTimeline CreateAbilityTimeline();
        
        /// <summary>
        /// 销毁一个AbilityTimeline实例
        /// </summary>
        /// <param name="timeline"></param>
        public abstract void DestroyAbilityTimeline(AbilityTimeline timeline);
        
        /// <summary>
        /// 创建一个新的AbilityGameplayTask实例
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns></returns>
        public abstract AbilityGameplayTaskBase CreateAbilityTask(AbilityTaskData taskData);
        
        /// <summary>
        /// 销毁一个AbilityGameplayTask实例
        /// </summary>
        /// <param name="task"></param>
        public abstract void DestroyAbilityTask(AbilityGameplayTaskBase task);
    }
    
    //默认的技能时间线工厂，直接创建和销毁AbilityTimeline实例，不进行任何对象池管理
    //如果业务需要对象池管理，可以继承AbilityTimelineFactory并重写CreateAbilityTimeline和DestroyAbilityTimeline方法来实现自定义的对象池逻辑
    public class DefaultAbilityTimelineFactory : AbilityTimelineFactory
    {
        public static DefaultAbilityTimelineFactory MakeInstance()
        {
            return new DefaultAbilityTimelineFactory();
        }


        public override AbilityTimeline CreateAbilityTimeline()
        {
            return new AbilityTimeline();
        }

        public override void DestroyAbilityTimeline(AbilityTimeline timeline)
        {
            timeline.ClearTasks();
        }
        
        public override AbilityGameplayTaskBase CreateAbilityTask(AbilityTaskData taskData)
        {
            AbilityGameplayTaskBase task = null;
            if (taskData is AnimationTaskData abilityTaskData)
            {
                task = new AnimationGameplayTask();
            }
            else if (taskData is CueTaskData cueTaskData)
            {
                task = new CueGameplayTask();
            }
            else if (taskData is AbilityLogicGameplayTaskData logicTaskData)
            {
                task = new AbilityLogicGameplayTask();
            }
            else if (taskData is CheckRangeTaskData checkRangeTaskData)
            {
                task = new CheckRangeGameplayTask();
            }
            return task;
        }
        
        public override void DestroyAbilityTask(AbilityGameplayTaskBase task)
        {
            
        }
    }
}