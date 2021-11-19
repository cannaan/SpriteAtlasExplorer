using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpriteAtlasExplorer
{
    public static class MaskableRectGUI
    {
        public static void Draw(Rect rect, Color color, Rect mask)
        {
            rect.min = Vector2.Max(rect.min, mask.min);
            rect.max = Vector2.Min(rect.max, mask.max);
            EditorGUI.DrawRect(rect, color);
        }

        public static void Draw(Rect rect, Color color, Rect mask, float borderWidth, Color borderColor)
        {
            Draw(rect, color, mask);
            Rect top = new Rect(rect.xMin - borderWidth * 0.5f, rect.yMax - borderWidth * 0.5f, rect.width + borderWidth, borderWidth);
            Draw(top, borderColor, mask);
        }
    }
}
