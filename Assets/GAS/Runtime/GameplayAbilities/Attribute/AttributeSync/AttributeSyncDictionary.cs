using Mirror;

namespace VSEngine.GAS
{
    public class AttributeSyncDictionary : SyncDictionary<string,AttributeSet>
    {
        private AttributeSetContainer _owningContainer;

        public void SetOwner(AttributeSetContainer owningContainer)
        {
            _owningContainer = owningContainer;
        }

        protected override AttributeSet ReadValueItem(NetworkReader reader, string key)
        {
            AttributeSet attributeSet = base.ReadValueItem(reader, key);
            if (AttributeGlobalLib.GlobalAttributeSets.TryGetValue(key, out var createFunc))
            {
                attributeSet = createFunc();
                attributeSet.SetOwner(_owningContainer);
                return attributeSet;
            }
            GasLogger.Error($"[GAS] AttributeSyncDictionary无法识别的属性集类型: {key}");
            return attributeSet;
        }
    }
}