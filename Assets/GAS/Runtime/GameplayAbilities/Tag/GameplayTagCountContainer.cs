using System;
using System.Collections.Generic;
using Mirror;

namespace VSEngine.GAS
{
    public class GameplayTagCountContainer : AbilityDependencyNetMono
    {
        private SyncDictionary<GameplayTag, int> _gameplayTagCounts = new SyncDictionary<GameplayTag, int>();
        
        //当标签有新添加后回调
        private Action<GameplayTag> _onTagAdded;
        //当标签被移除后回调
        private Action<GameplayTag> _onTagRemoved;
        //当标签移除或者添加后回调
        private Action<GameplayTag> _onTagChanged;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!isServer)
            {
                //注册同步事件
                _gameplayTagCounts.OnChange += OnGameplayTagCountsChangedInClient;
            }
        }
        
        public override void OnStopClient()
        {
            base.OnStopClient();
            if (!isServer)
            {
                //注销同步事件
                _gameplayTagCounts.OnChange -= OnGameplayTagCountsChangedInClient;
            }
        }

        public void UpdateTagMap(GameplayTagContainer tagContainer , int countChange)
        {
            if (tagContainer.IsEmpty())
            {
                return;
            }
            foreach (var t in tagContainer.GameplayTags)
            {
                UpdateTagMap_Internal(t, countChange);
            }
        }
        
        public void UpdateTagMap(GameplayTag t , int countChange)
        {
            UpdateTagMap_Internal(t, countChange);
        }
        
        private void UpdateTagMap_Internal(GameplayTag t, int countChange)
        {
            int oldCount = _gameplayTagCounts.ContainsKey(t) ? _gameplayTagCounts[t] : 0;
            if (_gameplayTagCounts.ContainsKey(t))
            {
                _gameplayTagCounts[t] += countChange;
            }
            else
            {
                _gameplayTagCounts[t] = countChange;
            }
            //确保计数不为负数
            if (_gameplayTagCounts[t] < 0)
            {
                _gameplayTagCounts[t] = 0;
            }
            //根据计数变化触发事件
            int newCount = _gameplayTagCounts[t];
            if (newCount > 0 && oldCount <= 0)
            {
                _onTagAdded?.Invoke(t);
                _onTagChanged?.Invoke(t);
            }
            else if (newCount <= 0 && oldCount > 0)
            {
                _onTagRemoved?.Invoke(t);
                _onTagChanged?.Invoke(t);
            }
        }

        //是否存在某个标签
        public bool HasAnyMatchingGameplayTags(GameplayTagContainer tags)
        {
            if (tags.IsEmpty())
            {
                return false;
            }
            foreach (var targetTag in tags.GameplayTags)
            {
                if (_gameplayTagCounts.ContainsKey(targetTag) && _gameplayTagCounts[targetTag] > 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool HasAnyMatchingGameplayTag(GameplayTag t)
        {
            return _gameplayTagCounts.ContainsKey(t) && _gameplayTagCounts[t] > 0;
        }
        
        public bool HasAnyMatchingGameplayTagsWithParent(GameplayTagContainer tags)
        {
            if (tags.IsEmpty())
            {
                return false;
            }
            foreach (var targetTag in tags.GameplayTags)
            {
                if ((_gameplayTagCounts.ContainsKey(targetTag) && _gameplayTagCounts[targetTag] > 0) || HasTagWithParents(targetTag))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 检查是否存在所有标签
        /// </summary>
        /// <returns></returns>
        public bool HasAllMatchingGameplayTags(GameplayTagContainer tags)
        {
            if (tags.IsEmpty())
            {
                return true;
            }
            foreach (var targetTag in tags.GameplayTags)
            {
                if (!_gameplayTagCounts.ContainsKey(targetTag) || _gameplayTagCounts[targetTag] <= 0)
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 检查 是否存在所有标签或者其父标签
        /// </summary>
        /// <returns></returns>
        public bool HasAllMatchingGameplayTagsWithParent(GameplayTagContainer tags)
        {
            if (tags.IsEmpty())
            {
                return true;
            }
            foreach (var targetTag in tags.GameplayTags)
            {
                if ((!_gameplayTagCounts.ContainsKey(targetTag) || _gameplayTagCounts[targetTag] <= 0) && !HasTagWithParents(targetTag))
                {
                    return false;
                }
            }
            return true;
        }
        
        private bool HasTagWithParents(GameplayTag targetTag)
        {
            IEnumerator<GameplayTag> parentTags = targetTag.GetParentTags();
            while (parentTags.MoveNext())
            {
                GameplayTag parentTag = parentTags.Current;
                if (_gameplayTagCounts.ContainsKey(parentTag) && _gameplayTagCounts[parentTag] > 0)
                {
                    return true;
                }
            }
            return false;
        }

        #region 事件

        public void RegisterTagAddedCallback(Action<GameplayTag> callback)
        {
            _onTagAdded += callback;
        }
        
        public void UnregisterTagAddedCallback(Action<GameplayTag> callback)
        {
            _onTagAdded -= callback;
        }
        
        public void RegisterTagRemovedCallback(Action<GameplayTag> callback)
        {
            _onTagRemoved += callback;
        }
        
        public void UnregisterTagRemovedCallback(Action<GameplayTag> callback)
        {
            _onTagRemoved -= callback;
        }
        
        public void RegisterTagChangedCallback(Action<GameplayTag> callback)
        {
            _onTagChanged += callback;
        }
        
        public void UnregisterTagChangedCallback(Action<GameplayTag> callback)
        {
            _onTagChanged -= callback;
        }

        #endregion

        #region Client

        //客户端同步事件，当标签计数发生变化时触发
        private void OnGameplayTagCountsChangedInClient(SyncIDictionary<GameplayTag, int>.Operation op, GameplayTag key, int old)
        {
            int newCount = _gameplayTagCounts.ContainsKey(key) ? _gameplayTagCounts[key] : 0;
            if (newCount > 0 && old <= 0)
            {
                _onTagAdded?.Invoke(key);
                _onTagChanged?.Invoke(key);
            }
            else if (newCount <= 0 && old > 0)
            {
                _onTagRemoved?.Invoke(key);
                _onTagChanged?.Invoke(key);
            }
        }


        #endregion
    }
}