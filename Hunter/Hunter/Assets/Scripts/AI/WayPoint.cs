using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.AI
{
    public class WayPoint : MonoBehaviour
    {
        private Transform m_Target;
        private float m_Distance;

        public void Set(Transform target, float Distance)
        {
            m_Target = target;
            m_Distance = Distance;
        }

        // Update is called once per frame
        void Update()
        {
            if (Vector3.Distance(m_Target.position, transform.position) < m_Distance)
            {
                Destroy(gameObject);
            }
        }
    }
}