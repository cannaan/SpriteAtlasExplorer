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
    public float scrollSpeed { get; set; } = 0.01f;
    public Texture2D texture { get; set; }
    public Texture2D background { get; set; }
    public float aspect => texture == null ? 1 : texture.width / (float)texture.height;

    private bool m_isDragging = false;

    public void Reset()
    {
        m_offset = Vector2.zero;
        m_scale = 1.0f;
        m_isDragging = false;
    }

    public Rect GetDrawRect(Rect rect, out Rect uvRect)
    {
        Rect activeRect = GetActiveRect(rect);
        Rect ret = Rect.MinMaxRect(Mathf.Max(activeRect.xMin, rect.xMin), Mathf.Max(activeRect.yMin, rect.yMin), Mathf.Min(activeRect.xMax, rect.xMax), Mathf.Min(activeRect.yMax, rect.yMax));
        uvRect = Rect.MinMaxRect((ret.xMin - activeRect.xMin) / activeRect.width, 1 - (ret.yMax - activeRect.yMin) / activeRect.height, (ret.xMax - activeRect.xMin) / activeRect.width, 1 - (ret.yMin - activeRect.yMin) / activeRect.height);
        return ret;
    }

    private Rect GetActiveRect(Rect rect)
    {
        Vector2 size = new Vector2(rect.width * m_scale, rect.width * m_scale / aspect);
        Vector2 center = rect.center + m_offset * size;
        return new Rect(center - size * 0.5f, size);
    }

    public bool OnGUI(Rect rect)
    {
        Vector2 pos = Event.current.mousePosition;
        Rect activeRect = GetActiveRect(rect);
        bool repaint = false;
        if (activeRect.Contains(pos))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                Vector2 normalizedPos = (pos - activeRect.min) / activeRect.size;
                m_scale *= 1.0f + Event.current.delta.y * scrollSpeed;
                activeRect = GetActiveRect(rect);
                Vector2 newNormalizedPos = (pos - activeRect.min) / activeRect.size;
                m_offset += newNormalizedPos - normalizedPos;
                repaint = true;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.MouseDown)
            {
                if(Event.current.button == 1)
                {
                    m_isDragging = true;
                    Event.current.Use();
                }
            }
        }
        if (Event.current.type == EventType.MouseDrag)
        {
            if (m_isDragging)
            {
                Vector2 delta = Event.current.delta;
                m_offset += delta / activeRect.size;
                repaint = true;
            }
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            if (m_isDragging)
            {
                m_isDragging = false;
                Event.current.Use();
            }
        }

        EnsureScaleAndOffset(rect);

        Rect drawRect = GetDrawRect(rect, out var uvRect);
        if(background != null)
        {
            Rect bgRect = uvRect;
            Vector2 scale = new Vector2(texture.width / (float)background.width, texture.height / (float)background.height);
            bgRect.min *= scale;
            bgRect.max *= scale;
            GUI.DrawTextureWithTexCoords(drawRect, background, bgRect);
        }
        GUI.DrawTextureWithTexCoords(drawRect, texture, uvRect);
        return repaint;
    }

    private void EnsureScaleAndOffset(Rect rect)
    {
        float minScale = GetMinimumScale(rect);
        m_scale = Mathf.Max(m_scale, minScale);

        Rect activeRect = GetActiveRect(rect);
        if(activeRect.width <= rect.width)
        {
            m_offset.x = 0.0f;
        }
        else
        {
            if(activeRect.xMin > rect.xMin)
            {
                m_offset.x = (activeRect.width / rect.width - 1) * 0.5f / m_scale;
            }
            else if(activeRect.xMax < rect.xMax)
            {
                m_offset.x = (1 - activeRect.width / rect.width) * 0.5f / m_scale;
            }
        }
        if(activeRect.height <= rect.height)
        {
            m_offset.y = 0.0f;
        }
    }

    private float GetMinimumScale(Rect rect)
    {
        float rectAspect = rect.width / rect.height;
        if(rectAspect < aspect)
        {
            return 1.0f;
        }
        else
        {
            return aspect / rectAspect;
        }
    }
}
