using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VSEngine.GAS
{
    public class GameplayTagsAsset : ScriptableObject
    {
        [LabelText("标签列表")] 
        [ListDrawerSettings( ShowIndexLabels = true)]
        public GameplayTag[] Tags;
    }
}