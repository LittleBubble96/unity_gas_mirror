namespace VSEngine.GAS
{
    public struct GameplayEventData
    {
        public GameplayTagContainer SourceTags;

        public bool IsValid()
        {
            return !SourceTags.IsEmpty();
        }
    }
}