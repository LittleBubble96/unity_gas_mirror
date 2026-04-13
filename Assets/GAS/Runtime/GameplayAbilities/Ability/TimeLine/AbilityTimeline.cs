using System;
using System.Collections.Generic;

namespace VSEngine.GAS
{
    /// <summary>
    /// 能力时间线 - 管理能力执行过程中的任务调度
    /// 当前简单设计 只有任务集合 未来可以添加更多功能 比如轨道 行为等
    /// </summary>
    public class AbilityTimeline
    {
        private List<AbilityGameplayTaskBase> _tasks = new List<AbilityGameplayTaskBase>();
        private bool _isPlaying = false;
        private bool _isPendingEnd = false;
        private float _currentTime = 0f;
        private GameplayAbilityInstance _abilityInstance;
        private AbilitySystemComponent _owner;
        private AbilityTimelineData _abilityTimelineData;

        public GameplayAbilityInstance OwnerIns => _abilityInstance;
        public AbilitySystemComponent Owner => _owner;
        public AbilityTimelineGlobalData TimelineGlobalData => AbilitySystemGlobals.Get().GetTimelineGlobalData();

        public void Init(AbilitySystemComponent owner, GameplayAbilityInstance instance)
        {
            _owner = owner;
            _abilityInstance = instance;
            _abilityTimelineData = TimelineGlobalData.GetAbilityTimelineData(instance.SourceAbility.name);
            GenerateTasks();
        }

        /// <summary>
        /// 根据数据 生成任务
        /// </summary>
        private void GenerateTasks()
        {
            if (_abilityTimelineData == null)
            {
                return;
            }

            foreach (var taskData in _abilityTimelineData.AbilityTasks)
            {
                AbilityGameplayTaskBase taskBase = TimelineGlobalData.Factory.CreateAbilityTask(taskData);
                if (taskBase != null)
                {
                    taskBase.OnInit(taskData);
                    AddTask(taskBase);
                }
            }
        }

        public void Play()
        {
            if (_isPlaying) return;
            
            _isPlaying = true;
            _currentTime = 0f;
            // 初始化所有任务
            foreach (var task in _tasks)
            {
                task.AbilitySystemComponent = _owner;
                task.Timeline = this;
            }
        }
        

        public void End(bool bIsPendingEnd)
        {
            if (bIsPendingEnd)
            {
                PendingEnd();
            }
            else
            {
                EndTimeline();
            }
        }

        public void Update(float dt)
        {
            if (!_isPlaying) return;

            _currentTime += dt;

            // 检查并启动应该开始的任务
            for (int i = 0; i < _tasks.Count; i++)
            {
                AbilityGameplayTaskBase task = _tasks[i];
                if (task.CheckIsStarted(_currentTime))
                {
                    if (task.RunState == TimelineRunClipState.NotStarted)
                    {
                        task.OnStart();
                        task.RunState = TimelineRunClipState.Running;
                    }
                    task.DoUpdate(dt);
                }
                if (task.CheckIsFinished(_currentTime))
                {
                    if (task.RunState != TimelineRunClipState.Finished)
                    {
                        task.RunState = TimelineRunClipState.Finished;
                        task.OnEnd();
                    }
                }
            }

            // 检查所有任务是否完成
            CheckAllTasksCompleted();
            if (_isPendingEnd)
            {
                EndTimeline();
                _isPendingEnd = false;
            }
        }

        private void AddTask(AbilityGameplayTaskBase task)
        {
            _tasks.Add(task);
            if (_isPlaying)
            {
                task.AbilitySystemComponent = _owner;
                task.Timeline = this;
            }
        }

        public void ClearTasks()
        {
            foreach (var task in _tasks)
            {
                TimelineGlobalData.Factory.DestroyAbilityTask(task);
            }
            _tasks.Clear();
        }
        
        private void PendingEnd()
        {
            _isPendingEnd = true;
        }

        private void EndTimeline()
        {
            if (!_isPlaying) return;

            _isPlaying = false;

            // 结束所有任务
            foreach (var task in _tasks)
            {
                if (!task.CheckIsFinished(_currentTime))
                {
                    task.OnEnd();
                }
                task.OnClear();
            }
        }

        private void CheckAllTasksCompleted()
        {
            bool allCompleted = true;
            foreach (var task in _tasks)
            {
                if (!task.CheckIsFinished(_currentTime))
                {
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted && _tasks.Count > 0)
            {
                // 所有任务完成，可以通知 Ability
            }
        }
    }
}
