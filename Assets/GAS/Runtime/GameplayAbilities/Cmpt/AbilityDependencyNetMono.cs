using Mirror;

namespace VSEngine.GAS
{
    //基础依赖于 ASC的组件 用于数据同步
    public class AbilityDependencyNetMono : NetworkBehaviour
    {
        protected AbilitySystemComponent Owner;

        public virtual void Init(AbilitySystemComponent owner)
        {
            Owner = owner;
        }

        public virtual void Dispose()
        {
            
        }
    }
}