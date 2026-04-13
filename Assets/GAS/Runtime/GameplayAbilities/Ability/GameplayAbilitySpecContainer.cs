using System;
using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    public class GameplayAbilitySpecContainer : AbilityDependencyNetMono// SyncList<GameplayAbilitySpec>
    {
        //服务器专用 技能生成ID
        public static uint AbilityGenerateHandle = 1;

        //复制
        public SyncList<GameplayAbilitySpec> Items = new SyncList<GameplayAbilitySpec>();
        
        //服务器专用 待移除技能列表 服务器在移除技能时 先将技能标记为待移除 然后放入这个列表里 等待技能实例结束后再真正移除这个技能规格 这样可以避免技能被移除后 但是技能实例还在执行的情况
        private Queue<GameplayAbilitySpec> _pendingRemoveSpecs = new Queue<GameplayAbilitySpec>();

        public void DoUpdate(float dt)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].IsPendingRemove)
                {
                    _pendingRemoveSpecs.Enqueue(Items[i]);
                }
                else
                {
                    Items[i].DoUpdate(dt);
                }
            }
            RemovePendingSpecs();
        }

        private void RemovePendingSpecs()
        {
            while (_pendingRemoveSpecs.Count > 0)
            {
                GameplayAbilitySpec spec = _pendingRemoveSpecs.Dequeue();
                spec.Dispose();
                Items.Remove(spec);
            }
        }
    }
}