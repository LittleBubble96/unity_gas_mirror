using System;

namespace VSEngine.GAS
{
    public abstract class ModifierMagnitudeCalculation
    {
        public abstract float CalculateMagnitude(GameplayEffectSpecHandle spec, float modifierMagnitude);
    }
    
    
}