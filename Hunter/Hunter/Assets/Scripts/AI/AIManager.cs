using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            StartCoroutine(SpawnTry(mother, father));
        }

        IEnumerator SpawnTry(RabbitAI mother, RabbitAI father)
        {
            LayerMask mask = LayerMask.GetMask(new string[] { "Herbivore", "Carnivore", "Food" });
            Vector3 randomPoint = mother.transform.position + Random.insideUnitSphere.normalized * Random.Range(1.3f, 2.5f);
            NavMeshHit hit;
            BaseAIPersonality fatherPersonality = father.Personality; // Storing father personality in case father dies;
            int tries = 0;
            while (tries < 500)
            {
                if (!mother)
                    yield break; // mother died before giving birth
                randomPoint = mother.transform.position + Random.insideUnitSphere.normalized * Random.Range(1.3f, 2.5f);
                if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
                {
                    if (Physics.OverlapSphereNonAlloc(hit.position, .3f, null, mask) <= 0)
                    {
                        GameObject child = Instantiate(m_RabbitChildPrefab, mother.transform.parent);
                        child.transform.position = hit.position;
                        child.GetComponent<RabbitAI>().SetParents(mother.Personality, fatherPersonality);
                        yield break;
                    }
                }
                tries++;
                yield return new WaitForSeconds(0.1f);
            }
        }
        #endregion
    }
}