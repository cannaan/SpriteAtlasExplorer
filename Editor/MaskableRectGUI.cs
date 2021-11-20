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
            if(rect.xMin > mask.xMax || rect.xMax < mask.xMin || rect.yMin > mask.yMax || rect.yMax < mask.yMin)
            {
                return;
            }
            rect.min = Vector2.Max(rect.min, mask.min);
            rect.max = Vector2.Min(rect.max, mask.max);
            EditorGUI.DrawRect(rect, color);
        }

        public static void Draw(Rect rect, Color color, Rect mask, float borderWidth, Color borderColor)
        {
            Draw(rect, color, mask);
            Rect top = new Rect(rect.xMin - borderWidth * 0.5f, rect.yMax - borderWidth * 0.5f, rect.width + borderWidth, borderWidth);
            Draw(top, borderColor, mask);
            Rect bottom = new Rect(rect.xMin - borderWidth * 0.5f, rect.yMin - borderWidth * 0.5f, rect.width + borderWidth, borderWidth);
            Draw(bottom, borderColor, mask);
            Rect left = new Rect(rect.xMin - borderWidth * 0.5f, rect.yMin - borderWidth * 0.5f, borderWidth, rect.height + borderWidth);
            Draw(left, borderColor, mask);
            Rect right = new Rect(rect.xMax - borderWidth * 0.5f, rect.yMin - borderWidth * 0.5f, borderWidth, rect.height + borderWidth);
            Draw(right, borderColor, mask);
        }
    }
}
