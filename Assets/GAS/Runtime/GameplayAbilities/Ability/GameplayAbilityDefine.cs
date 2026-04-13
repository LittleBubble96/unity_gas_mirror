using Sirenix.OdinInspector;

namespace VSEngine.GAS
{
    public static class GameplayAbilityDefine
    {
         
    }

    //技能执行策略
    public enum GameplayAbilityNetExecutionPolicy
    {
        [LabelText("本地预测")]
        LocalPredicted, //本地预测
        [LabelText("仅本地执行")]
        LocalOnly, //本地执行
        [LabelText("服务器授权")]
        ServerInitiated, //服务器授权
        [LabelText("仅在服务器执行")] 
        ServerOnly, //服务器执行
    }

    //技能实例化策略
    public enum GameplayAbilityInstancingPolicy
    {
        [LabelText("Actor共享Ability实例")]
        InstancedPerActor, //每个Actor实例化一个Ability实例
        [LabelText("每次执行实例化一个Ability实例")]
        InstancedPerExecution, //每次执行实例化一个Ability实例
    }
}