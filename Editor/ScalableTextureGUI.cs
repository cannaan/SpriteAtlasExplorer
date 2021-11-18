using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    private int m_controlID;
    private bool m_initialized = false;
    public void SetDirty()
    {
        m_initialized = false;
    }

    public void Reset(Rect rect)
    {
        m_offset = Vector2.zero;
        m_scale = GetMinimumScale(rect);
        m_isDragging = false;
        m_initialized = true;
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
        Rect normalized = new Rect(0, 0, 1, 1);
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
        Vector2 texSize = new Vector2(1.0f, 1.0f / aspect) * m_scale;
        Vector2 texStart = Vector2.one * 0.5f + m_offset - texSize * 0.5f;
        Rect uv = new Rect(texStart, texSize);
        Vector2 posInOne = (pos - one.min) / one.size;
        return (posInOne - uv.min) / uv.size;
    }

    public Vector2 NormalizedToPoint(Vector2 coord, Rect rect)
    {
        Rect one = new Rect(rect.xMin, rect.center.y - rect.width * 0.5f, rect.width, rect.width);
        Vector2 texSize = new Vector2(1.0f, 1.0f / aspect) * m_scale;
        Vector2 texStart = Vector2.one * 0.5f + m_offset - texSize * 0.5f;
        Rect uv = new Rect(texStart, texSize);
        Vector2 posInOne = uv.min + coord * uv.size;
        return one.min + posInOne * one.size;
    }

    public Vector2 DirectionToNormalized(Vector2 dir, Rect rect)
    {
        Vector2 one = Vector2.one * rect.width;
        Vector2 texSize = new Vector2(1.0f, 1.0f / aspect) * m_scale;
        Vector2 dirInOne = dir / one;
        return dirInOne / texSize;
    }
    public Vector2 NormalizedToDirection(Vector2 dir, Rect rect)
    {
        Vector2 one = Vector2.one * rect.width;
        Vector2 texSize = new Vector2(1.0f, 1.0f / aspect) * m_scale;
        Vector2 dirInOne = dir * texSize;
        return dirInOne * one;
    }

    public bool OnGUI(Rect rect)
    {
        if(!m_initialized)
        {
            Reset(rect);
        }
        Vector2 pos = Event.current.mousePosition;
        bool repaint = false;
        m_controlID = GUIUtility.GetControlID(FocusType.Passive);
        EventType eventType = Event.current.GetTypeForControl(m_controlID);
        if (Event.current.type == EventType.ScrollWheel)
        {
            if (rect.Contains(pos))
            {
                Vector2 oldCoord = PointToNormalized(pos, rect);
                m_scale *= 1.0f + Event.current.delta.y * scrollSpeed;
                Vector2 newCoord = PointToNormalized(pos, rect);
                Vector2 delta = NormalizedToDirection(newCoord - oldCoord, rect);
                m_offset += delta / rect.width;
                repaint = true;
                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            if (rect.Contains(pos))
            {
                if (Event.current.button == 1)
                {
                    m_isDragging = true;
                    GUIUtility.hotControl = m_controlID;
                    Event.current.Use();
                }
            }
        }
        if (eventType == EventType.MouseDrag)
        {
            if (m_isDragging)
            {
                Vector2 delta = Event.current.delta;
                delta.x /= rect.width;
                delta.y /= rect.width / aspect;
                m_offset += delta;
                repaint = true;
            }
        }
        else if (eventType == EventType.MouseUp)
        {
            if (m_isDragging)
            {
                m_isDragging = false;
                Event.current.Use();
                m_controlID = 0;
                GUIUtility.hotControl = 0;
            }
        }

        EnsureScaleAndOffset(rect);

        EditorGUI.DrawRect(rect, new Color32(75, 75, 75, 255));
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
                m_offset.y += (rect.yMin - activeRect.yMin) / rect.width * aspect;
            }
            else if(activeRect.yMax < rect.yMax)
            {
                m_offset.y += (rect.yMax - activeRect.yMax) / rect.width * aspect;
            }
        }
    }

    private float GetMinimumScale(Rect rect)
    {
        float rectAspect = rect.width / rect.height;
        if(rectAspect <= aspect)
        {
            return 1.0f;
        }
        else
        {
            return aspect / rectAspect;
        }
    }
}
