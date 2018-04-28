using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.AI
{
    public class RabbitAI : BaseAI
    {
        [SerializeField]
        private Animator m_Animator;

        protected override void Awake()
        {
            base.Awake();
            if (!m_Animator)
                m_Animator.GetComponentInChildren<Animator>();
        }

        protected override bool IsDoingSomething()
        {
            var state = m_Animator.GetCurrentAnimatorStateInfo(0);
            return !state.IsTag("Movement") && !state.IsTag("Idle");
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