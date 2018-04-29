using Hunter.Objects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.AI
{
    public class RabbitAI : BaseAI
    {
        public enum RabbitActions
        {
            Mate = BaseAIOrder.BaseActions.Highest+1
        }

        public enum RabbitMatingState
        {
            MaleSatisfied = 0,
            MaleLooking,
            FemaleLooking,
            FemaleSatisfied
        }

        [SerializeField]
        private Animator m_Animator;


        private RabbitAI m_DesiredMate;
        private float m_TimeLastMating = 0f;
        private RabbitMatingState m_MatingState;

        public bool IsMale
        {
            get { return m_MatingState == RabbitMatingState.MaleLooking || m_MatingState == RabbitMatingState.MaleSatisfied; }
        }

        public RabbitMatingState MatingState
        {
            get { return m_MatingState; }
        }

        public float Apperance
        {
            get { return (m_Hunger + m_Health) / 2f; }
        }

        protected override void Awake()
        {
            base.Awake();
            if (!m_Animator)
                m_Animator.GetComponentInChildren<Animator>();
            m_MatingState = (RabbitMatingState) Random.Range(0, 5);
        }

        protected override bool IsDoingSomething()
        {
            var state = m_Animator.GetCurrentAnimatorStateInfo(0);
            return !state.IsTag("Movement") && !state.IsTag("Idle");
        }

        public bool Seduce(RabbitAI mate)
        {
            if ((!m_DesiredMate || m_DesiredMate.Apperance <= mate.Apperance)) {
                m_DesiredMate = mate;
                return true;
            }
            return false;
        }

        public void MakeBaby(RabbitAI with)
        {
            if (m_MatingState == RabbitMatingState.FemaleLooking)
                m_MatingState = RabbitMatingState.FemaleSatisfied;
            if (m_MatingState == RabbitMatingState.MaleLooking)
                m_MatingState = RabbitMatingState.MaleSatisfied;
            m_TimeLastMating = Time.time;
            if(m_MatingState == RabbitMatingState.FemaleSatisfied && with.IsMale)
            {
                AIManager.Instance.SpawnChild(this, with);
            }
        }

        protected override void Mate()
        {
            if (m_ActionsNeeds.Count > 0)
            {
                m_ActionsInterests.Add(m_ActionCurrent);
                m_DesiredMate = null;
                m_ActionCurrent = null;
                return;
            }
            if (MatingState == RabbitMatingState.FemaleSatisfied || MatingState == RabbitMatingState.MaleSatisfied)
            {
                m_DesiredMate = null;
                m_ActionCurrent = null;
                return;
            }
            if (m_DesiredMate)
            {
                if (Vector3.Distance(transform.position, m_DesiredMate.transform.position) < 0.2f)
                {
                    if (m_DesiredMate.Seduce(this)) {
                        m_DesiredMate.MakeBaby(this);
                        MakeBaby(m_DesiredMate);
                        m_DesiredMate = null;
                        m_ActionCurrent = null;
                        return;
                    }
                } else
                {
                    SetNavmeshTarget(m_DesiredMate.transform);
                }
            } else
            {
                int amount = Physics.OverlapSphereNonAlloc(transform.position, m_Personality.NeedDetection, m_NonAllocResults, gameObject.layer);
                var DesiredState = m_MatingState == RabbitMatingState.MaleLooking ? RabbitMatingState.FemaleLooking : RabbitMatingState.MaleLooking;

                int index = -1;
                float max = 0;
                RabbitAI mateCandidate = null;
                for (int i = 0; i < amount; i++)
                {
                    mateCandidate = m_NonAllocResults[i].GetComponent<RabbitAI>();
                    if (!mateCandidate || mateCandidate.MatingState != DesiredState) continue;
                    
                    if (mateCandidate.Apperance >= max)
                    {
                        max = mateCandidate.Apperance;
                        index = i;
                    }
                }
                if (index < 0)
                {
                    WanderRandomDirection(m_Personality.NeedDetection);
                    return;
                }
                m_DesiredMate = mateCandidate;
                SetNavmeshTarget(m_NonAllocResults[index].transform);

            }
            base.Mate();
        }

        public override bool EvaluateInterests()
        {
            if(m_TimeLastMating + 2f < Time.time)
            {
                if (m_ActionCurrent == null || (BaseAIOrder.BaseActions)m_ActionCurrent.Action != BaseAIOrder.BaseActions.Mate)
                {
                    var exists = m_ActionsInterests.FindIndex(x => (BaseAIOrder.BaseActions)x.Action == BaseAIOrder.BaseActions.Mate);
                    if (exists < 0)
                        m_ActionsNeeds.Add(new BaseAIOrder(BaseAIOrder.BaseActions.Mate, 1f));
                }
            }
            else
            {
                m_ActionsInterests.RemoveAll(x => (BaseAIOrder.BaseActions)x.Action == BaseAIOrder.BaseActions.Mate);
            }
            return base.EvaluateInterests();
        }

        protected override bool WanderRandomDirection(float range)
        {
            StartCoroutine(SearchAnimation());
            return base.WanderRandomDirection(range);
        }

        IEnumerator SearchAnimation()
        {
            m_Animator.SetBool("Search", true);
            yield return new WaitForSeconds(Random.Range(1f,2.5f));
            m_Animator.SetBool("Search", false);
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            AIManager.Instance.Remove(this);
            BaseFood food = gameObject.AddComponent<BaseFood>();
            food.SetCarnivore(.5f);
            m_Animator.SetBool("Walk", false);
            m_Animator.SetBool("Run", false);
            m_Animator.SetBool("Search", false);
            m_Animator.SetBool("Jump", false);
            m_Animator.SetBool("Death" + Random.Range(0, 2).ToString(), true);
            m_Agent.enabled = false;
            Destroy(m_Agent);
            Destroy(this);
        }

        protected override void Update()
        {
            base.Update();
            if (m_Agent.enabled)
            {
                var velocity = m_Agent.velocity.magnitude;
                if (velocity > 0f && velocity < 1.2f)
                {
                    m_Animator.SetBool("Walk", true);
                    m_Animator.SetBool("Run", false);
                }
                else if (velocity >= 1.2f)
                {
                    m_Animator.SetBool("Walk", false);
                    m_Animator.SetBool("Run", true);
                }
            } else
            {
                m_Animator.SetBool("Walk", false);
                m_Animator.SetBool("Run", false);
            }
        }
    }
}