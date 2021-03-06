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
        public enum SpriteAtlasError
        {
            None,
            AtlasNotGenerated,
            SpriteNotPacked,
            UnknownException
        }
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
        public SpriteAtlasError error { get; private set; }
        public string errorInfo { get; private set; }
        public Texture2D GetTextureAt(int index)
        {
            if(m_spriteTextures != null && m_spriteTextures.Count > index && index >= 0)
            {
                return m_spriteTextures[index].texture;
            }
            return null;
        }
        public int GetSpriteCount(int textureIndex)
        {
            if(textureIndex >= 0 && textureIndex < textureCount)
            {
                return m_spriteTextures[textureIndex].sprites.Count;
            }
            return 0;
        }
        public bool GetSpriteAt(int textureIndex, int index, out Rect rect, out Sprite sprite)
        {
            if(textureIndex >= 0 && textureIndex < textureCount)
            {
                if(index >= 0 && index < m_spriteTextures[textureIndex].sprites.Count)
                {
                    rect = m_spriteTextures[textureIndex].sprites[index].rect;
                    sprite = m_spriteTextures[textureIndex].sprites[index].sprite;
                    return true;
                }
            }
            rect = default;
            sprite = null;
            return false;
        }

        private void Update(SpriteAtlas spriteAtlas)
        {
            error = SpriteAtlasError.None;
            UpdateTextures(spriteAtlas);
            if (error == SpriteAtlasError.None)
            {
                UpdateSprites(spriteAtlas);
            }
        }

        private void UpdateTextures(SpriteAtlas spriteAtlas)
        {
            try
            {
                m_spriteTextures.Clear();
                Type spriteAtlasExtensionsType = typeof(SpriteAtlasExtensions);
                MethodInfo getPreviewTextureMethod = spriteAtlasExtensionsType.GetMethod("GetPreviewTextures", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                Texture2D[] previewTextures = getPreviewTextureMethod.Invoke(null, new object[] { spriteAtlas }) as Texture2D[];
                if (previewTextures == null || previewTextures.Length == 0)
                {
                    error = SpriteAtlasError.AtlasNotGenerated;
                }
                else
                { 
                    foreach(Texture2D t in previewTextures)
                    {
                        m_spriteTextures.Add(new SpriteTexture { texture = t });
                    }
                }
            }
            catch(Exception e)
            {
                error = SpriteAtlasError.UnknownException;
                Debug.LogError($"Caught exception {e.Message}\r\n{e.StackTrace}");
                m_spriteTextures.Clear();
            }
        }

        private void UpdateSprites(SpriteAtlas spriteAtlas)
        {
            try
            {
                int spriteCount = spriteAtlas.spriteCount;
                if(spriteCount == 0)
                {
                    error = SpriteAtlasError.AtlasNotGenerated;
                    Debug.LogError($"SpriteAtlas {spriteAtlas.name} sprite count is 0.");
                    return;
                }
                Type spriteAtlasExtensionsType = typeof(SpriteAtlasExtensions);
                MethodInfo getPackedSpritesMethod = spriteAtlasExtensionsType.GetMethod("GetPackedSprites", BindingFlags.Static | BindingFlags.NonPublic);
                Sprite[] sprites = getPackedSpritesMethod.Invoke(null, new object[] { spriteAtlas }) as Sprite[];
                foreach(Sprite s in sprites)
                {
                    bool match = false;
                    Texture2D texture = null;
                    try
                    {
                        texture = SpriteUtility.GetSpriteTexture(s, true);
                    }

                    catch(Exception exp)
                    {
                        error = SpriteAtlasError.SpriteNotPacked;
                        Debug.LogError($"{s.name} is not packed into atlas.\r\n{exp.Message}\r\n{exp.StackTrace}", s);
                        return;
                    }
                    if (texture != null)
                    {
                        Rect rect = CalculateRect(s);
                        foreach (SpriteTexture st in m_spriteTextures)
                        {
                            if (st.texture == texture)
                            {
                                st.sprites.Add(new SpriteRect { sprite = s, rect = rect });
                                match = true;
                                break;
                            }
                        }
                        if (!match)
                        {
                            Debug.LogWarning($"can not find sprite {s.name} in sprite atlas", s);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error = SpriteAtlasError.UnknownException;
                Debug.LogError($"Caught exception {e.Message}\r\n{e.StackTrace}");
            }
        }

        public static SpriteAtlasMapData Create(SpriteAtlas spriteAtlas)
        {
            SpriteAtlasMapData ret = new SpriteAtlasMapData();
            ret.Update(spriteAtlas);
            return ret;
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