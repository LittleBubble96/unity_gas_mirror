using System;
using System.Collections.Generic;
using UnityEngine;
using VSEngine.GAS;

public static class AttributeGlobalLib
{
    public static Dictionary<string, Func<AttributeSet>> GlobalAttributeSets = new Dictionary<string, Func<AttributeSet>>()
    {
        { "AS_Fight", () => new AS_Fight() },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        
    }
}