# unity_gas_mirror

模仿 UE GAS 的 Unity 多人联机技能框架

CSDN 博客：https://blog.csdn.net/qq_39329287/article/details/160115016?spm=1001.2014.3001.5501

---

## 目录
- [介绍](sslocal://flow/file_open?url=%23%E4%BB%8B%E7%BB%8D&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [AbilitySystemGlobals - 全局ASC管理](sslocal://flow/file_open?url=%23abilitysystemglobals---%E5%85%A8%E5%B1%80asc%E7%AE%A1%E7%90%86&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [1. AbilitySystemComponent (ASC)](sslocal://flow/file_open?url=%231-abilitysystemcomponent-asc&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（1）重要接口](sslocal://flow/file_open?url=%231-%E9%87%8D%E8%A6%81%E6%8E%A5%E5%8F%A3&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.1）TryActivateAbility](sslocal://flow/file_open?url=%2311-tryactivateability&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.2）GiveAbility](sslocal://flow/file_open?url=%2312-giveability&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [1.1 GameplayAbility](sslocal://flow/file_open?url=%2311-gameplayability&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（1）技能资源参数解释](sslocal://flow/file_open?url=%231-%E6%8A%80%E8%83%BD%E8%B5%84%E6%BA%90%E5%8F%82%E6%95%B0%E8%A7%A3%E9%87%8A&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.1）技能执行策略](sslocal://flow/file_open?url=%2311-%E6%8A%80%E8%83%BD%E6%89%A7%E8%A1%8C%E7%AD%96%E7%95%A5&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.2）技能实例化策略](sslocal://flow/file_open?url=%2312-%E6%8A%80%E8%83%BD%E5%AE%9E%E4%BE%8B%E5%8C%96%E7%AD%96%E7%95%A5&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.3）输入事件](sslocal://flow/file_open?url=%2313-%E8%BE%93%E5%85%A5%E4%BA%8B%E4%BB%B6&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.4）技能相关 Tag](sslocal://flow/file_open?url=%2314-%E6%8A%80%E8%83%BD%E7%9B%B8%E5%85%B3-tag&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（1.5）GE 相关](sslocal://flow/file_open?url=%2315-ge-%E7%9B%B8%E5%85%B3&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（2）技能重要执行接口](sslocal://flow/file_open?url=%232-%E6%8A%80%E8%83%BD%E9%87%8D%E8%A6%81%E6%89%A7%E8%A1%8C%E6%8E%A5%E5%8F%A3&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（2.1）CanActivateAbility](sslocal://flow/file_open?url=%2321-canactivateability&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（2.2）CommitAbility](sslocal://flow/file_open?url=%2322-commitability&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（2.3）EndAbility](sslocal://flow/file_open?url=%2323-endability&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（3）Timeline 编技能效果](sslocal://flow/file_open?url=%233-timeline-%E7%BC%96%E6%8A%80%E8%83%BD%E6%95%88%E6%9E%9C&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（3.1）导出技能数据](sslocal://flow/file_open?url=%2331-%E5%AF%BC%E5%87%BA%E6%8A%80%E8%83%BD%E6%95%B0%E6%8D%AE&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
    - [（3.2）GameplayTask](sslocal://flow/file_open?url=%2332-gameplaytask&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
      - [（3.2.1）动画轨道](sslocal://flow/file_open?url=%23321-%E5%8A%A8%E7%94%BB%E8%BD%A8%E9%81%93&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
      - [（3.2.2）Cue 轨道](sslocal://flow/file_open?url=%23322-cue-%E8%BD%A8%E9%81%93&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
      - [（3.2.3）碰撞范围检测](sslocal://flow/file_open?url=%23323-%E7%A2%B0%E6%92%9E%E8%8C%83%E5%9B%B4%E6%A3%80%E6%B5%8B&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
      - [（3.2.4）技能逻辑执行](sslocal://flow/file_open?url=%23324-%E6%8A%80%E8%83%BD%E9%80%BB%E8%BE%91%E6%89%A7%E8%A1%8C&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [1.2 GameplayEffect (GE)](sslocal://flow/file_open?url=%2312-gameplayeffect-ge&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（1）GE 资源编辑](sslocal://flow/file_open?url=%231-ge-%E8%B5%84%E6%BA%90%E7%BC%96%E8%BE%91&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [1.3 GameplayCue (GC)](sslocal://flow/file_open?url=%2313-gameplaycue-gc&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [1.4 Attribute](sslocal://flow/file_open?url=%2314-attribute&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（1）AttributeSet](sslocal://flow/file_open?url=%231-attributeset&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（2）属性编辑器 & 属性集编辑器](sslocal://flow/file_open?url=%232-%E5%B1%9E%E6%80%A7%E7%BC%96%E8%BE%91%E5%99%A8--%E5%B1%9E%E6%80%A7%E9%9B%86%E7%BC%96%E8%BE%91%E5%99%A8&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
  - [（3）属性修改](sslocal://flow/file_open?url=%233-%E5%B1%9E%E6%80%A7%E4%BF%AE%E6%94%B9&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [1.5 GameplayTag](sslocal://flow/file_open?url=%2315-gameplaytag&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)
- [2 日志接口重写](sslocal://flow/file_open?url=%232-%E6%97%A5%E5%BF%97%E6%8E%A5%E5%8F%A3%E9%87%8D%E5%86%99&flow_extra=eyJsaW5rX3R5cGUiOiJjb2RlX2ludGVycHJldGVyIn0=)

---

## 介绍

项目地址：https://github.com/LittleBubble96/unity_gas_mirror.git

框架还有很多地方可以优化，后续会逐步完善，例如：
- 编辑器使用便利性
- Timeline 轨道扩展
- 等待技能确认逻辑（如选择地点进行抛射）
- 技能预测与回滚
- 通用抛射物逻辑（可按项目需求自定义）

---

## 文档更新日志
- 2026.4.13：内容还在逐步补充中，着急的同学可以直接看源码
- 2026.4.14：完善了 Timeline 中各轨道的说明与使用方式

---

## AbilitySystemGlobals - 全局ASC管理
用于全局管理所有 AbilitySystemComponent（ASC）实例。

---

## 1. AbilitySystemComponent (ASC)
对于玩家角色或 NPC，需要将 `AbilitySystemComponent` 挂载到 Prefab 上。

- `AscId`（同步）：Prefab 加载后自动生成并注册到 `AbilitySystemGlobals`，方便全局获取。
- `XyPlayer`：角色控制器，引用 ASC 的资源配置，用于配置和控制技能。
- **ASC 资源**：同一类角色共用的技能配置文件：
  - 名称、描述：备注用，不影响逻辑
  - 属性集：该角色拥有哪些属性
  - 初始技能：角色创建时直接赋予的技能（主动/被动）
  - 附加 Tags：角色创建时直接赋予的标签（例如阵营区分）
  - 初始效果：角色创建时直接赋予的 GameplayEffect
  - 初始化属性值：如初始 HP=100、MP=20
  - 简单被动效果：例如永久每秒回蓝（建议用技能实现）

> 初始化逻辑必须由服务器执行，再同步到各客户端。

---

### （1）重要接口
#### （1.1）TryActivateAbility
尝试激活技能：
```csharp
public bool TryActivateAbility(uint abilityHandle)
{
    GameplayAbilitySpec abilitySpec = FindAbilitySpecFromHandle(abilityHandle);
    if (!abilitySpec.IsValid())
    {
        GasLogger.Warning($"[GAS] [Client] 激活技能失败 无效的技能 Handle: {abilityHandle}");
        return false;
    }
    //如果本地模拟 则不激活技能
    if (isClient && !isLocalPlayer)
    {
        GasLogger.Log($"[GAS] [Client] 本地模拟 不激活技能 Handle: {abilityHandle}");
        return false;
    }
    GameplayAbilityNetExecutionPolicy netExecutionPolicy = abilitySpec.Ability.netExecutionPolicy;
    //如果只是单纯服务器 如果是 本地预测 或者 只在本地执行的技能 则不激活技能
    if (isServerOnly && (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.LocalOnly || 
                         netExecutionPolicy == GameplayAbilityNetExecutionPolicy.LocalPredicted))
    {
        ClientTryActivateAbility(abilityHandle);
        return true;
    }
    //如果只是单纯客户端 如果是 只在服务器执行的技能 则不激活技能
    if (isClientOnly && (netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerOnly ||
                        netExecutionPolicy == GameplayAbilityNetExecutionPolicy.ServerInitiated))
    {
        ServerTryActivateAbility(abilityHandle);
        return true;
    }

    return InternalTryActivateAbility(abilitySpec);
}

后续逻辑慢慢再补充
