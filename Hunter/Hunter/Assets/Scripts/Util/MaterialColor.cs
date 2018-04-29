using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hunter.Util
{
    [ExecuteInEditMode]
    public class MaterialColor : MonoBehaviour
    {

        //public Gradient m_BaseColor;
        public Gradient m_Color;
        public Vector2 m_Smoothness;
        public Vector2 m_Metallic;
        public bool m_RandomizeColor = false;
        public Material m_DefaultMaterial;

        private float m_RandomColor;
        private float m_RandomSmoothness;
        private float m_RandomMetallic;

        private Material m_cachedSharedMaterial;

        // Use this for initialization
        void Start()
        {
            MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();

            m_cachedSharedMaterial = renderers[0].sharedMaterial;
            foreach (MeshRenderer r in renderers)
            {
                if (!r.sharedMaterial && Application.isPlaying)
                {
                    if (m_DefaultMaterial)
                        r.sharedMaterial = m_DefaultMaterial;
                    else
                        Debug.LogError("No default material!");
                }
                if (!r.sharedMaterial && !Application.isPlaying && m_DefaultMaterial)
                {
                    r.sharedMaterial = m_DefaultMaterial;
                }
                if (r.sharedMaterial)
                    r.sharedMaterial = new Material(r.sharedMaterial);
            }
            m_RandomColor = Random.Range(0f, 1f);
            m_RandomSmoothness = Random.Range(m_Smoothness.x, m_Smoothness.y);
            m_RandomMetallic = Random.Range(m_Metallic.x, m_Metallic.y);
            SetBaseHeight();
        }

        private void SetBaseHeight()
        {
            MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer r in renderers)
            {
                if (!r.sharedMaterial && Application.isPlaying)
                {
                    if (m_DefaultMaterial)
                        r.sharedMaterial = m_DefaultMaterial;
                    else
                        Debug.LogError("No default material!");
                }
                if (!r.sharedMaterial && !Application.isPlaying && m_DefaultMaterial)
                {
                    r.sharedMaterial = m_DefaultMaterial;
                }
                if (!r.sharedMaterial) continue;
                r.sharedMaterial.SetColor("_Color", m_Color.Evaluate(m_RandomColor));
                r.sharedMaterial.SetFloat("_Metallic", m_RandomMetallic);
                r.sharedMaterial.SetFloat("_Glossiness", m_RandomSmoothness);
            }
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
                SetBaseHeight();
            if (m_RandomizeColor)
            {
                m_RandomSmoothness = Random.Range(m_Smoothness.x, m_Smoothness.y);
                m_RandomMetallic = Random.Range(m_Metallic.x, m_Metallic.y);
                m_RandomColor = Random.Range(0f, 1f);
                m_RandomizeColor = false;
                SetBaseHeight();

            }
        }
#endif
    }
}
