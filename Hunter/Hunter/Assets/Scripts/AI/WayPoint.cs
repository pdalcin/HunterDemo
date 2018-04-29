using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.AI
{
    public class WayPoint : MonoBehaviour
    {
        private Transform m_Target;
        private float m_Distance;
        private BaseAI m_TargetAI;

        public void Set(Transform target, float Distance)
        {
            m_Target = target;
            m_Distance = Distance;
            m_TargetAI = target.GetComponent<BaseAI>();
            if (m_TargetAI)
                m_TargetAI.OnDeathEvent += OnTargetDeath;
        }

        public void OnTargetDeath()
        {
            Destroy(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
            var target = m_Target.position;
            target.y = transform.position.y;

            if (Vector3.Distance(target, transform.position) < m_Distance)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (m_TargetAI)
                m_TargetAI.OnDeathEvent -= OnTargetDeath;
        }
    }
}