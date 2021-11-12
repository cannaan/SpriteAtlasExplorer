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
    public float scrollSpeed { get; set; } = -0.01f;
    public Texture2D texture { get; set; }
    public Texture2D background { get; set; }
    public float aspect => texture == null ? 1 : texture.width / (float)texture.height;
    private bool m_isDragging = false;

    public void Reset()
    {
        m_offset = new Vector2(0.5f, 0);
        m_scale = 2.0f;
        m_isDragging = false;
    }

    public Rect GetDrawRect(Rect rect, out Rect uvRect)
    {
        Rect activeRect = GetActiveRect(rect);
        activeRect.min = Vector2.Max(activeRect.min, rect.min);
        activeRect.max = Vector2.Min(activeRect.max, rect.max);
        uvRect = RectToNormalized(activeRect, rect);
        float tmp = uvRect.yMax;
        uvRect.yMax = 1.0f - uvRect.yMin;
        uvRect.yMin = 1.0f - tmp;
        return activeRect;
    }
    private Rect GetActiveRect(Rect rect)
    {
        Rect normalized = new Rect(0, 0.5f - 0.5f / aspect, 1, 1.0f / aspect);
        return NormalizedToRect(normalized, rect);
    }

    public Rect RectToNormalized(Rect rect, Rect refRect)
    {
        Vector2 min = PointToNormalized(rect.min, refRect);
        Vector2 max = PointToNormalized(rect.max, refRect);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
    public Rect NormalizedToRect(Rect coord, Rect refRect)
    {
        Vector2 min = NormalizedToPoint(coord.min, refRect);
        Vector2 max = NormalizedToPoint(coord.max, refRect);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public Vector2 PointToNormalized(Vector2 pos, Rect rect)
    {
        Rect one = new Rect(rect.xMin, rect.center.y - rect.width * 0.5f, rect.width, rect.width);
        Vector2 coord = (pos - one.min) / one.size;
        Vector2 texPos = Vector2.one * (0.5f - m_scale * 0.5f) + m_offset;
        Vector2 texSize = Vector2.one * m_scale;
        return (coord - texPos) / texSize;
    }

    public Vector2 NormalizedToPoint(Vector2 coord, Rect rect)
    {
        Rect one = new Rect(rect.xMin, rect.center.y - rect.width * 0.5f, rect.width, rect.width);
        Vector2 texPos = Vector2.one * (0.5f - m_scale * 0.5f) + m_offset;
        Vector2 texSize = Vector2.one * m_scale;
        Vector2 normalized = texPos + coord * texSize;
        return one.min + normalized * one.size;
    }

    public bool OnGUI(Rect rect)
    {
        Vector2 pos = Event.current.mousePosition;
        Rect activeRect = GetActiveRect(rect);
        bool repaint = false;
        if (rect.Contains(pos))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                Vector2 oldCoord = PointToNormalized(pos, rect);
                m_scale *= 1.0f + Event.current.delta.y * scrollSpeed;
                Vector2 newPos = NormalizedToPoint(oldCoord, rect);
                m_offset += pos / rect.width - newPos / rect.width;
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
                delta.x /= rect.width;
                delta.y /= rect.width;
                m_offset += delta;
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
                m_offset.x += (rect.xMin - activeRect.xMin) / rect.width;
            }
            else if(activeRect.xMax < rect.xMax)
            {
                m_offset.x += (rect.xMax - activeRect.xMax) / rect.width;
            }
        }
        if(activeRect.height <= rect.height)
        {
            m_offset.y = 0.0f;
        }
        else
        {
            if(activeRect.yMin > rect.yMin)
            {
                m_offset.y += (rect.yMin - activeRect.yMin) / rect.width;
            }
            else if(activeRect.yMax < rect.yMax)
            {
                m_offset.y += (rect.yMax - activeRect.yMax) / rect.width;
            }
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
