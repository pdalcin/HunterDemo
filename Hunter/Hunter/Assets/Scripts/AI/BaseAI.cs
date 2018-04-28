using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hunter.AI
{
    public class BaseAI : MonoBehaviour {

        [SerializeField]
        protected BaseAIPersonality m_Personality;

        /// <summary>
        /// Actions
        /// </summary>
        protected List<BaseAIOrder> m_ActionsInterests;
        protected List<BaseAIOrder> m_ActionsNeeds;
        protected BaseAIOrder m_ActionCurrent;

        protected Transform m_CurrentTarget;

        protected delegate void ActionDelegate();
        protected Dictionary<System.Enum, ActionDelegate> m_ActionDelegates = new Dictionary<System.Enum, ActionDelegate>();

        /// <summary>
        /// Danger check
        /// </summary>
        protected bool m_IsInDanger = false;
        protected Transform m_CurrentDanger;


        /// <summary>
        /// AI Status
        /// </summary>
        protected float m_Hunger = 1f; // 1 = not hungry , 0 = starving
        protected float m_Health = 1f; // 1 = full health, 0 = dead

        public bool IsInDanger {
            get { return m_IsInDanger; }
        }
        
        protected virtual void Awake()
        {
            m_ActionDelegates.Add(BaseAIOrder.BaseActions.GetFood, GetFood);
            m_ActionDelegates.Add(BaseAIOrder.BaseActions.Sleep, Sleep);
            m_ActionDelegates.Add(BaseAIOrder.BaseActions.Mate, Mate);
            m_ActionDelegates.Add(BaseAIOrder.BaseActions.Wander, Wander);
            m_FoodLayer = LayerMask.GetMask(new string[] { "Food" });
        }

        #region Actions
        protected Collider[] m_NonAllocResults = new Collider[10];

        // Exectute GetFood Action
        protected LayerMask m_FoodLayer;

        public virtual void GetFood()
        {
            if (1 - m_Hunger <= m_ActionCurrent.Priority)
            {
                m_ActionCurrent = null;
                return;
            }
            if (m_CurrentTarget == null)
            {
                int amount = Physics.OverlapSphereNonAlloc(transform.position, m_Personality.NeedDetectionRange, m_NonAllocResults, m_FoodLayer);
                if(amount <= 0)
                {
                    return;
                }
                int index = 0;
                float min = Mathf.Infinity;
                for(int i = 0; i < amount; i++)
                {
                    float mag = (m_NonAllocResults[i].transform.position - transform.position).sqrMagnitude;
                    if (mag < min)
                    {
                        min = mag;
                        index = i;
                    }
                }
                m_CurrentTarget = m_NonAllocResults[index].transform;
            }
            // ends current action
        }

        // Exectute Sleep Action
        public virtual void Sleep()
        {
            // ends current action
            m_ActionCurrent = null;
        }

        // Exectute Mate Action
        public virtual void Mate()
        {
            // ends current action
            m_ActionCurrent = null;
        }

        // Exectute Wander Action
        public virtual void Wander()
        {
            // ends current action
            m_ActionCurrent = null;
        }

        #endregion


        public virtual void RecieveOrder(BaseAIOrder order, bool Interrupt)
        {
            if(Interrupt && m_ActionCurrent != null)
            {
                m_ActionsNeeds.Add(m_ActionCurrent);
                return;
            }
            if(m_ActionCurrent == null)
            {
                m_ActionCurrent = order;
                return;
            }
            m_ActionsNeeds.Add(order);
            return;
        }

        // Check surroundings  for dangers
        public virtual bool EvaluateDanger()
        {
            m_IsInDanger = false;
            return false;
        }

        public virtual void ActOnDanger()
        {

        }

        public virtual bool EvaluateNeeds()
        {   
            // Evaluate the need to search for food
            if(m_Hunger <= m_Personality.HungryThreshold)
            {
                if ((BaseAIOrder.BaseActions)m_ActionCurrent.Action != BaseAIOrder.BaseActions.GetFood)
                {
                    var exists = m_ActionsNeeds.FindIndex(x => (BaseAIOrder.BaseActions)x.Action == BaseAIOrder.BaseActions.GetFood);
                    if (exists < 0)
                        m_ActionsNeeds.Add(new BaseAIOrder(BaseAIOrder.BaseActions.GetFood, 1f - m_Hunger));
                }
            }
            else
            {
                m_ActionsNeeds.RemoveAll(x => (BaseAIOrder.BaseActions)x.Action == BaseAIOrder.BaseActions.GetFood);
            }
            
            // Check it there is any need at all
            if (m_ActionsNeeds.Count == 0)
                return false;

            // Order needs by priority
            m_ActionsNeeds = m_ActionsNeeds.OrderBy(x => x.Priority).ToList();
            
            return true;
        }

        public virtual bool EvaluateInterests()
        {
            if (m_ActionsInterests.Count == 0)
                return false;

            m_ActionsInterests = m_ActionsInterests.OrderBy(x => x.Priority).ToList();

            return true;
        }


    }
}