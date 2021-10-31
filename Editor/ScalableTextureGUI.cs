using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Draw Texture with Landscape mode
// in rect x,y,w,h, render the texture with is aspect.
// when m_scale equals 1, texture's width stretch to the rect w. when m_scale equals 2, texcoord u [0.25,0.75] is displayed in rect.
public class ScalableTextureGUI
{
    private Vector2 m_offset = Vector2.zero;
    private float m_scale = 1.0f;
    public float maxScaling = 100.0f;
    public float scrollSpeed { get; set; } = 1.0f;
    public Texture2D texture { get; set; }
    public float aspect => texture == null ? 1 : texture.width / (float)texture.height;

    public void Reset()
    {
        m_offset = Vector2.zero;
        m_scale = 1.0f;
    }

    private Rect GetActiveRect(Rect rect)
    {
        Vector2 size = new Vector2(rect.width * m_scale, rect.width * m_scale / aspect);
        Vector2 center = rect.center + m_offset * size;
        return new Rect(center - size * 0.5f, size);
    }

    public void OnGUI(Rect rect)
    {
        if(Event.current.type == EventType.ScrollWheel)
        {
            Vector2 pos = Event.current.mousePosition;
            Rect activeRect = GetActiveRect(rect);
            if (activeRect.Contains(pos))
            {
                Vector2 normalizedPos = (pos - activeRect.min) / activeRect.size;
                m_scale += Event.current.delta.x * scrollSpeed;
                activeRect = GetActiveRect(rect);
                Vector2 newNormalizedPos = (pos - activeRect.min) / activeRect.size;
                m_offset += newNormalizedPos - normalizedPos;
            }
        }
    }
}
