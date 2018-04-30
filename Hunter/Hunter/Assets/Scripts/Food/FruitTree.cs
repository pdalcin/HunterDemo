using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hunter.Objects
{
    public class FruitTree : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_FruitPrefab;
        [SerializeField]
        private Vector2 m_FruitSpawnDelay;

        private List<Transform> m_FruitSpawnPoints = new List<Transform>();

        private float m_LastSpawn = 0f;
        private float m_NextDelay = 0f;

        private Collider[] m_TreeColliders;

        private void Awake()
        {
            m_FruitSpawnPoints.AddRange( GetComponentsInChildren<Transform>().Where(go => go.gameObject != this.gameObject) );
            m_NextDelay = Random.Range(m_FruitSpawnDelay.x, m_FruitSpawnDelay.y);
            m_TreeColliders = GetComponentsInChildren<Collider>();
        }

        private void Update()
        {
            if(m_LastSpawn + m_NextDelay < Time.time)
            {
                m_NextDelay = Random.Range(m_FruitSpawnDelay.x, m_FruitSpawnDelay.y);
                m_LastSpawn = Time.time;
                StartCoroutine(SpawnFruit());
            }
        }

        private IEnumerator SpawnFruit()
        {
            Transform SpawnPoint = GetRandomFreeSpawnPoint();
            if (!SpawnPoint) yield break;

            GameObject newFruit = Instantiate(m_FruitPrefab);
            newFruit.transform.position = SpawnPoint.position;
            newFruit.transform.rotation = SpawnPoint.rotation;
            newFruit.transform.parent = SpawnPoint;

            Rigidbody rb = newFruit.GetComponentInChildren<Rigidbody>();

            if (!rb) yield break;

            rb.isKinematic = true;

            yield return new WaitForSeconds(Random.Range(0.4f, 1.2f));
            foreach(Collider c in rb.GetComponentsInChildren<Collider>())
            {
                foreach(Collider k in m_TreeColliders)
                {
                    Physics.IgnoreCollision(c, k, true);
                }
            }

            Vector3 reference = transform.position;
            reference.y = newFruit.transform.position.y;
            rb.isKinematic = false;
            rb.AddForce((newFruit.transform.position - reference)*2f + Vector3.up * 3f);

            yield return new WaitForSeconds(.1f);

            foreach (Collider c in rb.GetComponentsInChildren<Collider>())
            {
                foreach (Collider k in m_TreeColliders)
                {
                    Physics.IgnoreCollision(c, k, false);
                }
            }
        }

        private Transform GetRandomFreeSpawnPoint()
        {
            if (m_FruitSpawnPoints.Count == 0) return null;

            Transform freePoint = null;

            int index = Random.Range(0, m_FruitSpawnPoints.Count);

            for(int i = 0; i < m_FruitSpawnPoints.Count; i++)
            {
                if(Physics.OverlapSphereNonAlloc(m_FruitSpawnPoints[index].position, .3f, null, 1 << m_FruitPrefab.layer) <= 0)
                {
                    return m_FruitSpawnPoints[index];
                }
                index++;
                index %= m_FruitSpawnPoints.Count;
            }

            return freePoint;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            foreach(Transform t in GetComponentsInChildren<Transform>().Where(go => go.gameObject != this.gameObject))
            {
                Gizmos.color = new Color(0,.3f,1f,.6f);
                Gizmos.DrawSphere(t.position, .3f);
            }
        }
#endif
    }
}