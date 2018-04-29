using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter.AI
{
    public class AIManager : MonoBehaviour
    {
        private static AIManager m_Instance;

        public static AIManager Instance
        {
            get
            {
                if (!m_Instance)
                    m_Instance = FindObjectOfType<AIManager>();
                return m_Instance;
            }
        }
        [SerializeField]
        private GameObject m_RabbitChildPrefab;

        private int m_ThoughtsPerUpdate = 10;
        private int m_CurrentThought = -1;

        private List<BaseAI> m_SceneAIs = new List<BaseAI>();


        private void Update()
        {
            int amount = Mathf.Min(m_ThoughtsPerUpdate, m_SceneAIs.Count);
            for(int i = 0; i < amount; i++)
            {
                m_CurrentThought++;
                m_CurrentThought %= m_SceneAIs.Count;
                if (m_SceneAIs[m_CurrentThought])
                {
                    if(m_SceneAIs[m_CurrentThought].EvaluateDanger())
                    {
                        m_SceneAIs[m_CurrentThought].ActOnDanger();
                        continue;
                    }
                    if (m_SceneAIs[m_CurrentThought].ExecuteCurrentAction())
                        continue;
                    if (m_SceneAIs[m_CurrentThought].EvaluateNeeds())
                    {
                        if (m_SceneAIs[m_CurrentThought].ExecuteNextNeed())
                            continue;
                    }
                    if (m_SceneAIs[m_CurrentThought].EvaluateInterests())
                    {
                        if (m_SceneAIs[m_CurrentThought].ExecuteNextInterest())
                            continue;
                    }
                }
            }
        }

        #region Util
        public void Add(BaseAI AI)
        {
            if(!m_SceneAIs.Contains(AI))
                m_SceneAIs.Add(AI);
        }

        public void Remove(BaseAI AI)
        {
            if (m_SceneAIs.Contains(AI))
                m_SceneAIs.Remove(AI);
        }
        #endregion

        #region AISpawn
        public void SpawnChild(RabbitAI mother, RabbitAI father)
        {
            Debug.Log("Spawn child!");
        }
        #endregion
    }
}