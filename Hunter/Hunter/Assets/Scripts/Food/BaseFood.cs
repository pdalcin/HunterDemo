using Hunter.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.Objects
{
    public class BaseFood : MonoBehaviour
    {
        [SerializeField]
        private float m_Value = 0.3f;

        [SerializeField]
        private LayerMask m_EatenBy;

        public void Awake()
        {
            Destroy(gameObject, 10f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(m_EatenBy == (m_EatenBy | (1 << collision.gameObject.layer)))
            {
                BaseAI ai = collision.rigidbody.GetComponent<BaseAI>();

                if(ai)
                {
                    ai.Feed(m_Value);
                    Destroy(gameObject);
                }
            }
        }


        public void SetHerbivore(float value)
        {
            m_EatenBy = LayerMask.GetMask(new string[] { "Herbivore" });
            m_Value = value;
        }

        public void SetCarnivore(float value)
        {
            m_EatenBy = LayerMask.GetMask(new string[] { "Carnivore" });
            m_Value = value;
        }
    }
}
