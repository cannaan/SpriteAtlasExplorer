using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditorInternal;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using System;
using System.Reflection;
using System.Linq;

namespace SpriteAtlasExplorer
{
    public class SpriteAtlasMapData
    {
        internal struct SpriteRect
        {
            public Sprite sprite;
            public Rect rect;
        }
        internal class SpriteTexture
        {
            public Texture2D texture;
            public List<SpriteRect> sprites = new List<SpriteRect>();
        }

        private List<SpriteTexture> m_spriteTextures = new List<SpriteTexture>();

        public int textureCount => m_spriteTextures.Count;

        public static SpriteAtlasMapData Create(SpriteAtlas spriteAtlas)
        {
            Type spriteAtlasExtensionsType = typeof(SpriteAtlasExtensions);
            MethodInfo getPreviewTextureMethod = spriteAtlasExtensionsType.GetMethod("GetPreviewTextures", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Texture2D[] previewTextures = getPreviewTextureMethod.Invoke(null, new object[] { spriteAtlas }) as Texture2D[];
            if(previewTextures != null && previewTextures.Length > 0)
            { 
                SpriteAtlasMapData newSpriteAtlasMap = new SpriteAtlasMapData();
                foreach(Texture2D texture in previewTextures)
                {
                    SpriteTexture spriteTexture = new SpriteTexture();
                    spriteTexture.texture = texture;
                    newSpriteAtlasMap.m_spriteTextures.Add(spriteTexture);
                }
                MethodInfo getPackedSpritesMethod = spriteAtlasExtensionsType.GetMethod("GetPackedSprites", BindingFlags.Static | BindingFlags.NonPublic);
                Sprite[] sprites = getPackedSpritesMethod.Invoke(null, new object[] { spriteAtlas }) as Sprite[];
                foreach(Sprite s in sprites)
                {
                    Texture2D texture = SpriteUtility.GetSpriteTexture(s, true);
                    foreach(SpriteTexture st in newSpriteAtlasMap.m_spriteTextures)
                    {
                        if(st.texture == texture)
                        {
                            Rect rect = CalculateRect(s);
                            st.sprites.Add(new SpriteRect { sprite = s, rect = rect });
                            break;
                        }
                    }
                }
                return newSpriteAtlasMap;
            }
            return null;
        }

        private static Rect CalculateRect(Sprite sprite)
        {
            Vector2[] uvs = SpriteUtility.GetSpriteUVs(sprite, true);
            if(uvs.Length > 0)
            {
                float minX = uvs[0].x, minY = uvs[0].y, maxX = uvs[0].x, maxY = uvs[0].y;
                for(int i = 1;i < uvs.Length;++i)
                {
                    if(uvs[i].x < minX)
                    {
                        minX = uvs[i].x;
                    }
                    if(uvs[i].x > maxX)
                    {
                        maxX = uvs[i].x;
                    }
                    if(uvs[i].y < minY)
                    {
                        minY = uvs[i].y;
                    }
                    if(uvs[i].y > maxY)
                    {
                        maxY = uvs[i].y;
                    }
                }
                return Rect.MinMaxRect(minX, minY, maxX, maxY);
            }
            return Rect.MinMaxRect(0, 0, 0, 0);
        }
    }
}