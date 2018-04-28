using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.AI
{

    public class BaseAIOrder
    {

        public enum BaseActions
        {
            GetFood = 0,
            Sleep,
            Mate,
            Wander,
            Highest
        }

        public System.Enum Action =  BaseActions.GetFood;
        public float Priority = 0f;

        public BaseAIOrder(System.Enum action, float priority)
        {
            Action = action;
            Priority = priority;
        }
    }
}