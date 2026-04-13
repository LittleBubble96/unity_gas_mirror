using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using VSEngine.GAS.Ability;

namespace VSEngine.GAS
{
    public class XYPlayer : NetworkBehaviour
    {
        public VSAbilitySystemComponentPreset abilitySystemComponentPreset;
        private AbilitySystemComponent abilitySystemComponent;
        private ThirdPersonCharacterController characterController;
        private PlayerAction _input;//client
        
        //技能cdTag
        private GameplayTagContainer _abilityCdTag = new GameplayTagContainer(new GameplayTag("CoolDown.Ability.Fire"));

        public override void OnStartServer()
        {
            base.OnStartServer();
            //在服务器上进行技能添加
            abilitySystemComponent = GetComponent<AbilitySystemComponent>(); 
            GasLogger.Log($"[GAS] 服务器添加技能");
            //添加初始技能
            OnAddAbilities();
            //添加属性
            OnAddAttributes();
            SetLevel(1);
            //应用初始效果  也可以 直接 设置基础属性
            foreach (var initEffect in abilitySystemComponentPreset.InitEffects)
            {
                abilitySystemComponent.ApplyGameplayEffectSpecToSelf(GameplayEffectSpec.MakeSpec(initEffect),default);
            }
            RegisterAttrPostChange();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            abilitySystemComponent = GetComponent<AbilitySystemComponent>(); 
            characterController = GetComponent<ThirdPersonCharacterController>();
            //绑定技能输入
            _input = new PlayerAction();
            _input.Enable();
            foreach (var inputAction in _input)
            {
                inputAction.performed += OnInputActionPerformed;
            }

            if (isLocalPlayer)
            {
                GASExampleManager.Instance.SetLocalPlayer(this);
            }
            RegisterAttrClientAttChange();
            abilitySystemComponent.InjectAbilityAnimationPlayer(new XYAnimationPlayer());
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            UnregisterAttrPostChange();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            foreach (var inputAction in _input)
            {
                inputAction.performed -= OnInputActionPerformed;
            }
            UnregisterAttrClientAttChange();
        }

        private void OnAddAbilities()
        {
            foreach (var abilityAsset in abilitySystemComponentPreset.GameplayAbilities)
            {
                GameplayAbilitySpec spec = new GameplayAbilitySpec(abilityAsset,abilitySystemComponent);
                uint handle = abilitySystemComponent.GiveAbility(spec);
                //如果技能实例化策略是每个角色实例化一个技能对象 那么就创建一个技能对象
                if (spec.GetInstancingPolicy() == GameplayAbilityInstancingPolicy.InstancedPerActor)
                {
                    spec.CreateNewInstance();
                }
                if (abilityAsset is XYGameplayAbilityAsset { BActiveFromGiveAbility: true })
                {
                    abilitySystemComponent.TryActivateAbility(handle);
                }
            }
        }

        private void OnAddAttributes()
        {
            foreach (var attributeSet in abilitySystemComponentPreset.AttributeSets)
            {
                abilitySystemComponent.AddAttributeSet(AttributeGlobalLib.GlobalAttributeSets[attributeSet]());
            }
        }

        private void OnInputActionPerformed(InputAction.CallbackContext context)
        {
            abilitySystemComponent.InputActivateAbility(context.action.name);
        }
        
        public void SetLevel(int level)
        {
            abilitySystemComponent.AttrSet<AS_Fight>().Level.SetBaseValue(level);
        }
        
        public bool IsWalking => characterController.IsWalking;
        
        public bool IsGround => characterController.IsGrounded;
        
        public bool IsIdle => !characterController.IsWalking && characterController.IsGrounded;

        public int Level => (int)abilitySystemComponent.AttrSet<AS_Fight>().Level.CurrentValue;
        
        public float Hp => abilitySystemComponent.AttrSet<AS_Fight>().Hp.CurrentValue;

        public float Mp => abilitySystemComponent.AttrSet<AS_Fight>().Mp.CurrentValue;
        
        public float Atk => abilitySystemComponent.AttrSet<AS_Fight>().Atk.CurrentValue;
        
        public float HpMax => abilitySystemComponent.AttrSet<AS_Fight>().HpMax.CurrentValue;
        
        public float MpMax => abilitySystemComponent.AttrSet<AS_Fight>().MpMax.CurrentValue;
        
        public float Armor => abilitySystemComponent.AttrSet<AS_Fight>().Armor.CurrentValue;
        
        public float Mr => abilitySystemComponent.AttrSet<AS_Fight>().mr.CurrentValue;

        private void RegisterAttrPostChange()
        {
            abilitySystemComponent.RegisterAttributeChangedCallback_InServer(OnAttributeChanged);
        }
        
        private void UnregisterAttrPostChange()
        {
            abilitySystemComponent.UnregisterAttributeChangedCallback_InServer(OnAttributeChanged);
        }

        private void RegisterAttrClientAttChange()
        {
            abilitySystemComponent.AttrSet<AS_Fight>().Hp.RegisterPostCurrentValueChange(OnChangeHp);
            abilitySystemComponent.AttrSet<AS_Fight>().Mp.RegisterPostCurrentValueChange(OnChangeMp);
        }
        
        private void UnregisterAttrClientAttChange()
        {
            abilitySystemComponent.AttrSet<AS_Fight>().Hp.UnregisterPostCurrentValueChange(OnChangeHp);
            abilitySystemComponent.AttrSet<AS_Fight>().Mp.UnregisterPostCurrentValueChange(OnChangeMp);
        }

        private void OnChangeHp(AttributeBase hp, float old, float newValue)
        {
            GASExampleManager.Instance.UIMain.RefreshHp();
        }
        
        private void OnChangeMp(AttributeBase mp, float old, float newValue)
        {
            GASExampleManager.Instance.UIMain.RefreshMp();
        }

        private void OnAttributeChanged(AttributeBase attribute)
        {
            if (attribute == abilitySystemComponent.AttrSet<AS_Fight>().Hp)
            {
                if (attribute.BaseValue > HpMax)
                {
                    SetHp(HpMax);
                }
            }
            else if (attribute == abilitySystemComponent.AttrSet<AS_Fight>().Mp)
            {
                if (attribute.BaseValue > MpMax)
                {
                    SetMp(MpMax);
                }
            }
        }

        private void SetHp(float value)
        {
            abilitySystemComponent.AttrSet<AS_Fight>().Hp.SetBaseValue(value);
            abilitySystemComponent.AttrSet<AS_Fight>().Hp.SetCurrentValue(value);
        }

        private void SetMp(float value)
        {
            abilitySystemComponent.AttrSet<AS_Fight>().Mp.SetBaseValue(value);
            abilitySystemComponent.AttrSet<AS_Fight>().Mp.SetCurrentValue(value);
        }

        public float GetFireTotalCoolDown()
        {
            float coolDown = 0;
            abilitySystemComponent.ForeachAllAbilitySpec((a) =>
            {
                if (a.Ability.GetCoolDownTags().HasAll(_abilityCdTag))
                {
                    coolDown = Mathf.Max(coolDown, a.GetCoolDownTime());
                    return true;
                }
                return false;
            });
            return coolDown;
        }
        
        public float GetFireCoolDown()
        {
            return abilitySystemComponent.GetCooldownTimeRemainingByTags(_abilityCdTag);
        }
    }
}