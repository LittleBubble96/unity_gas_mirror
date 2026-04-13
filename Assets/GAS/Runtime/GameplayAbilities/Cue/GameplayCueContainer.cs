using Mirror;

namespace VSEngine.GAS
{
    /// <summary>
    /// Cue容器 处理 cue相关逻辑
    /// </summary>
    public class GameplayCueContainer : AbilityDependencyNetMono
    {
        public SyncList<GameplayCueSpec> GameplayCues = new SyncList<GameplayCueSpec>();

        private GameplayCueManager CueManager => AbilitySystemGlobals.Get().GetGameplayCueManager();

        public void AddCue(GameplayTag cueTag)
        {
            GameplayCueSpec spec = new GameplayCueSpec();
            spec.GameplayCueTag = cueTag;
            GameplayCues.Add(spec);
            Owner.AddGameplayTagToCountMap(cueTag);
        }

        public void RemoveCue(GameplayTag cueTag)
        {
            for (int i = GameplayCues.Count - 1; i >= 0; i--)
            {
                if (GameplayCues[i].GameplayCueTag.Equals(cueTag))
                {
                    GameplayCues.RemoveAt(i);
                    Owner.RemoveGameplayTagFromCountMap(cueTag);
                }
            }
        }

        public void InvokeCue_OnAuthority(GameplayTag cueTag , ExecuteCueType executeCueType , GameplayCueContext context)
        {
            //只有服务器权威才能调用Cue
            if (!isServer)
            {
                return;
            }
            MulticastInvokeCue(cueTag,executeCueType,context);
        }
        
        [ClientRpc]
        private void MulticastInvokeCue(GameplayTag cueTag , ExecuteCueType executeCueType, GameplayCueContext context)
        {
            context.Instigator = Owner;
            CueManager.HandleGameplayCue(cueTag, executeCueType , context);
        }
    }
}