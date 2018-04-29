using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Hunter.AI
{
    public class BaseAI : MonoBehaviour {

        [SerializeField]
        protected BaseAIPersonality m_Personality;

        /// <summary>
        /// Actions
        /// </summary>
        protected List<BaseAIOrder> m_ActionsInterests = new List<BaseAIOrder>();
        protected List<BaseAIOrder> m_ActionsNeeds = new List<BaseAIOrder>();
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
        [SerializeField]
        protected float m_Hunger = 1f; // 1 = not hungry , 0 = starving
        [SerializeField]
        protected float m_Health = 1f; // 1 = full health, 0 = dead

        public delegate void OnDeathDelegate();
        public event OnDeathDelegate OnDeathEvent;
        public delegate void OnInitializeDelegate();
        public event OnInitializeDelegate OnInitializeEvent;

        /// <summary>
        /// Agent
        /// </summary>
        protected NavMeshAgent m_Agent;

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
            if (!m_Agent)
                m_Agent = GetComponent<NavMeshAgent>();
            m_Personality.RandomizePersonality();
            AIManager.Instance.Add(this);
        }

        protected virtual void Start()
        {
            StartCoroutine(DelayedInitialization());
        }

        protected virtual IEnumerator DelayedInitialization()
        {
            yield return null;
            if (OnInitializeEvent != null)
            {
                OnInitializeEvent();
            }
        }

        #region Actions
        public virtual bool ExecuteCurrentAction()
        {
            if (m_ActionCurrent == null)
                return false;
            m_ActionDelegates[m_ActionCurrent.Action]();
            return true;
        }

        public virtual bool ExecuteNextNeed()
        {
            if (m_ActionsNeeds.Count > 0)
            {
                m_ActionCurrent = m_ActionsNeeds[0];
                m_ActionDelegates[m_ActionCurrent.Action]();
                return true;
            }
            return false;
        }

        public virtual bool ExecuteNextInterest()
        {
            if (m_ActionsInterests.Count > 0)
            {
                m_ActionCurrent = m_ActionsInterests[0];
                m_ActionDelegates[m_ActionCurrent.Action]();
                return true;
            }
            return false;
        }

        protected Collider[] m_NonAllocResults = new Collider[10];

        // Exectute GetFood Action
        protected LayerMask m_FoodLayer;

        protected virtual void GetFood()
        {
            if (1.1f - m_Hunger < m_ActionCurrent.Priority)
            {
                m_ActionCurrent = null;
                return;
            }
            if (m_CurrentTarget == null)
            {
                int amount = Physics.OverlapSphereNonAlloc(transform.position, m_Personality.NeedDetection, m_NonAllocResults, m_FoodLayer);
                if (amount <= 0)
                {
                    WanderRandomDirection(m_Personality.NeedDetection);
                    return;
                }
                int index = 0;
                float min = Mathf.Infinity;
                for (int i = 0; i < amount; i++)
                {
                    float mag = (m_NonAllocResults[i].transform.position - transform.position).sqrMagnitude;
                    if (mag < min)
                    {
                        min = mag;
                        index = i;
                    }
                }
                SetNavmeshTarget(m_NonAllocResults[index].transform);
            }
            // ends current action
        }

        // Exectute Sleep Action
        protected virtual void Sleep()
        {
            // ends current action
            m_ActionCurrent = null;
        }

        // Exectute Mate Action
        protected virtual void Mate()
        {
            // ends current action
            m_ActionCurrent = null;
        }

        // Exectute Wander Action
        protected virtual void Wander()
        {
            // ends current action
            m_ActionCurrent = null;
        }

        #endregion


        public virtual void RecieveOrder(BaseAIOrder order, bool Interrupt)
        {
            if (Interrupt && m_ActionCurrent != null)
            {
                m_ActionsNeeds.Add(m_ActionCurrent);
                return;
            }
            if (m_ActionCurrent == null)
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
            return m_IsInDanger;
        }

        public virtual void ActOnDanger()
        {

        }

        public virtual bool EvaluateNeeds()
        {
            // Evaluate the need to search for food
            if (m_Hunger <= m_Personality.HungryThreshold)
            {
                if (m_ActionCurrent == null || (BaseAIOrder.BaseActions)m_ActionCurrent.Action != BaseAIOrder.BaseActions.GetFood)
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

        protected virtual void Update()
        {
            HungerDecay();
            if (m_Hunger <= 0f)
                HealthDecay();
            CheckHealth();
            if(!m_CurrentTarget || IsDoingSomething())
            {
                m_Agent.enabled = false;
            }
            else if (m_CurrentTarget)
            {
                m_Agent.enabled = true;
                SetNavmeshTarget(m_CurrentTarget);
            }
        }

        #region Status
        protected virtual bool IsDoingSomething()
        {
            return false;
        }

        protected virtual void HungerDecay()
        {
            var amount = Mathf.Min(m_Personality.HungerDecayRatio * Time.deltaTime, m_Hunger);
            m_Hunger -= amount;
        }

        public virtual void Feed(float value)
        {
            m_Hunger += value;
        }

        protected virtual void HealthDecay()
        {
            var amount = Mathf.Min(m_Personality.HealthDecayRatio * Time.deltaTime, m_Health);
            m_Health -= amount;
        }

        protected virtual void CheckHealth()
        {
            if(m_Health <= 0)
            {
                OnDeath();
            }
        }

        protected virtual void OnDeath()
        {
            if(OnDeathEvent != null)
                OnDeathEvent();
        }

        #endregion

        #region Util
        protected virtual bool WanderRandomDirection(float range)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    GameObject newTarget = new GameObject();
                    newTarget.name = "WayPoint";
                    newTarget.transform.position = hit.position;
                    WayPoint point = newTarget.AddComponent<WayPoint>();
                    point.Set(transform, 0.1f);
                    SetAgentTarget(newTarget.transform, newTarget.transform.position);
                    return true;
                }
            }
            return false;
            
        }

        protected virtual bool SetNavmeshTarget(Transform target)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target.position, out hit, .5f, NavMesh.AllAreas))
            {
                SetAgentTarget(target, hit.position);
                return true;
            }
            return false;
        }

        protected virtual void SetAgentTarget(Transform target, Vector3 position)
        {
            m_CurrentTarget = target;
            if (m_CurrentTarget)
            {
                m_Agent.enabled = true;
                m_Agent.SetDestination(position);
            }
        }
        #endregion

        #region Editor

        void OnDrawGizmosSelected()
        {
            // Display the explosion radius when selected
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, m_Personality.NeedDetection);
        }
        #endregion

    }
}