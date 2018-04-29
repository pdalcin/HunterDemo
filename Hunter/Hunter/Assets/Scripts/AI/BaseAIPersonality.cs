using System;
using UnityEngine;


namespace Hunter.AI
{
    [Serializable]
    public class BaseAIPersonality
    {
        [Header("Hunger")]
        [Range(0,1)]
        public float HungryThreshold = 0.3f;
        public Vector2 HungryThresholdRange = new Vector2(0.3f, 0.45f);
        public float HungerDecayRatio = 0.01f;
        public Vector2 HungryDecayRange = new Vector2(0.1f, 0.25f);
        public float NeedDetection = 5f;
        public Vector2 NeedDetectionRange = new Vector2(6f, 8f);
        [Header("Health")]
        public bool DecayHealthWhenStarving = true;
        public float HealthDecayRatio = 0.1f;
        public Vector2 HealthDecayRange = new Vector2(6f, 8f);

        public void RandomizePersonality()
        {
            HungryThreshold = UnityEngine.Random.Range(HungryThresholdRange.x, HungryThresholdRange.y);
            HungerDecayRatio = UnityEngine.Random.Range(HungryDecayRange.x, HungryDecayRange.y);
            NeedDetection = UnityEngine.Random.Range(NeedDetectionRange.x, NeedDetectionRange.y);
            HealthDecayRatio = UnityEngine.Random.Range(HealthDecayRange.x, HealthDecayRange.y);
        }


        public static BaseAIPersonality Child(BaseAIPersonality parent1, BaseAIPersonality parent2)
        {
            BaseAIPersonality newPersonality = new BaseAIPersonality();

            newPersonality.HungryThreshold = UnityEngine.Random.Range(0f, 1f) > .5f ? parent1.HungryThreshold : parent2.HungryThreshold;
            newPersonality.HungerDecayRatio = UnityEngine.Random.Range(0f, 1f) > .5f ? parent1.HungerDecayRatio : parent2.HungerDecayRatio;
            newPersonality.NeedDetection = UnityEngine.Random.Range(0f, 1f) > .5f ? parent1.NeedDetection : parent2.NeedDetection;
            newPersonality.HealthDecayRatio = UnityEngine.Random.Range(0f, 1f) > .5f ? parent1.HealthDecayRatio : parent2.HealthDecayRatio;

            return newPersonality;
        }
        
    }
}