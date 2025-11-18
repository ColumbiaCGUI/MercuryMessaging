// BallFlash.cs
using UnityEngine;
using MercuryMessaging;

public class BallFlash : MonoBehaviour
{
    public Renderer m_renderer;
    
    private void Awake()
    {
        m_renderer = GetComponent<Renderer>();
        m_renderer.enabled = false;
    }

    public void SetColor(Color color)
    {
        m_renderer.material.color = color;
    }
    
    public void SetShow(bool show)
    {
        m_renderer.enabled = show;
    }
}