using System.Collections.Generic;

namespace VSEngine.GAS
{
    public class AbilitySystemGlobals
    {
        private static AbilitySystemGlobals _instance;
        
        public static AbilitySystemGlobals Get()
        {
            _instance ??= new AbilitySystemGlobals();
            _instance.Init();
            return _instance;
        }

        private bool _isInitialized = false;
        
        private GameplayTagManager _tagManager = new GameplayTagManager();
        
        public GameplayTagManager GetTagManager()
        {
            return _tagManager;
        }

        //是否忽略技能系统的冷却时间，通常用于测试或者调试模式，在这种模式下，技能的冷却时间将被忽略，玩家可以无限制地使用技能。
        private bool _bIgnoreAbilitySystemCoolDowns;
        
        public bool ShouldIgnoreCoolDowns()
        {
            return _bIgnoreAbilitySystemCoolDowns;
        }
        
        private bool _bIgnoreAbilitySystemCosts;
        
        public bool ShouldIgnoreCosts()
        {
            return _bIgnoreAbilitySystemCosts;
        }
        
        //注册修改器计算类
        private Dictionary<string,ModifierMagnitudeCalculation> _calculationClassMap = new Dictionary<string, ModifierMagnitudeCalculation>();
        
        public void RegisterModifierMagnitudeCalculation(string calculationClassName, ModifierMagnitudeCalculation calculation)
        {
            _calculationClassMap[calculationClassName] = calculation;
        }
        
        public ModifierMagnitudeCalculation GetModifierMagnitudeCalculation(string calculationClassName)
        {
            if (_calculationClassMap.TryGetValue(calculationClassName, out var calculation))
            {
                return calculation;
            }
            return null;
        }

        /*
         * 预加载防止 后续懒加载 卡顿
         */
        public void PreLoad()
        {
            Init();
        }

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            _tagManager.Init();
            _gameplayCueManager.Init();
        }

        #region ASC

        private static uint AscIsGenerated = 0;
        
        private Dictionary<uint,AbilitySystemComponent> _ascMap = new Dictionary<uint, AbilitySystemComponent>();

        internal void RegisterAscFromServer(AbilitySystemComponent asc)
        {
            if (asc == null)
            {
                return;
            }
            asc.AscId = ++AscIsGenerated;
            _ascMap[asc.AscId] = asc;
        }
        
        internal void RegisterAscFromClient(AbilitySystemComponent asc)
        {
            if (asc == null)
            {
                return;
            }
            _ascMap[asc.AscId] = asc;
        }
        
        internal void UnregisterAsc(AbilitySystemComponent asc)
        {
            if (asc == null)
            {
                return;
            }
            _ascMap.Remove(asc.AscId);
        }
        
        public AbilitySystemComponent GetAsc(uint ascId)
        {
            if (_ascMap.TryGetValue(ascId, out var asc))
            {
                return asc;
            }
            return null;
        }
        #endregion

        #region GameplayCueManager  

        private GameplayCueManager _gameplayCueManager = new GameplayCueManager();
        
        public GameplayCueManager GetGameplayCueManager()
        {
            return _gameplayCueManager;
        }

        #endregion

        #region TimelineGlobalData

        private AbilityTimelineGlobalData _timelineGlobalData = new AbilityTimelineGlobalData();
        
        public AbilityTimelineGlobalData GetTimelineGlobalData()
        {
            return _timelineGlobalData;
        }

        #endregion
    }
}