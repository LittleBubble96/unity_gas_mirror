using Mirror;

namespace VSEngine.GAS
{
    public static class NetBehaviorExtension
    {
        public static NetMonoInfo GetNetMonoInfo(this NetworkBehaviour networkBehavior)
        {
            var info = new NetMonoInfo();
            if (networkBehavior.isClient)
            {
                info.IsClient = true;
            }
            if (networkBehavior.isServer)
            {
                info.IsServer = true;
            }
            return info;
        }
    }

    public struct NetMonoInfo
    {
        public bool IsClient;
        public bool IsServer;
    }
}