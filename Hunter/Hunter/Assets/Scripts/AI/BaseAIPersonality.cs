using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hunter.AI
{
    [Serializable]
    public class BaseAIPersonality
    {
        [Range(0,1)]
        public float HungryThreshold = 0.3f;
        public float HungerDecayRatio = 0.01f;
        public float NeedDetectionRange = 5f;

        
    }
}