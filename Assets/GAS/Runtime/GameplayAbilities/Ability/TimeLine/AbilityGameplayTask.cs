using Mirror;

namespace VSEngine.GAS
{
    public enum TimelineRunClipState
    {
        NotStarted,
        Running,
        Finished
    }
    public abstract class AbilityGameplayTaskBase
    {
        public AbilitySystemComponent AbilitySystemComponent { get; set; }
        public AbilityTimeline Timeline { get; set; }

        public TimelineRunClipState RunState { get; set; } = TimelineRunClipState.NotStarted;
        //开始时间
        public float StartTime { get; set; }

        //持续时间
        public float Duration { get; set; }

        public virtual void OnInit(AbilityTaskData data)
        {
            StartTime = data.StartTime;
            Duration = data.Duration;
        }

        public abstract void OnStart();

        public abstract void OnEnd();

        public bool CheckIsFinished(float currentTime)
        {
            return currentTime >= StartTime + Duration;
        }

        public bool CheckIsStarted(float currentTime)
        {
            return currentTime >= StartTime;
        }

        public virtual bool DoUpdate(float dt)
        {
            return true;
        }


        public virtual void OnClear()
        {
            RunState = TimelineRunClipState.NotStarted;
        }
    }

    public abstract class AbilityGameplayTask <T> : AbilityGameplayTaskBase where T : AbilityTaskData
    {
        public T Data { get; private set; }

        public override void OnInit(AbilityTaskData data)
        {
            base.OnInit(data);
            Data = data as T;
        }
        
        protected bool IsServer()
        {
            return AbilitySystemComponent != null && AbilitySystemComponent.isServer;
        }
        
        protected bool IsClientOnly()
        {
            return AbilitySystemComponent != null && !AbilitySystemComponent.isClientOnly;
        }
    }
    
    
    public class AbilityTaskData
    {
        public float StartTime { get; set; }
        public float Duration { get; set; }

        public virtual void Write(NetworkWriter writer)
        {
            writer.WriteString(GetType().AssemblyQualifiedName);
            writer.WriteFloat(StartTime);
            writer.WriteFloat(Duration);
        }

        public virtual void Read(NetworkReader reader)
        {
            StartTime = reader.ReadFloat();
            Duration = reader.ReadFloat();
        }
    }
}