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
            NoTextures,
            NotPacked,
            TextureNotFound,
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

        private void Update(SpriteAtlas spriteAtlas)
        {
            error = SpriteAtlasError.None;
            UpdateTextures(spriteAtlas);
            if (error != SpriteAtlasError.None)
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
                    error = SpriteAtlasError.NoTextures;
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
                errorInfo = e.Message + "\r\n" + e.StackTrace;
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
                    error = SpriteAtlasError.NotPacked;
                    errorInfo = "no sprite contains in sprite atlas";
                    return;
                }
                Sprite[] tmpSprites = new Sprite[spriteCount];
                Type spriteAtlasExtensionsType = typeof(SpriteAtlasExtensions);
                MethodInfo getPackedSpritesMethod = spriteAtlasExtensionsType.GetMethod("GetPackedSprites", BindingFlags.Static | BindingFlags.NonPublic);
                Sprite[] sprites = getPackedSpritesMethod.Invoke(null, new object[] { spriteAtlas }) as Sprite[];
                foreach(Sprite s in sprites)
                {
                    Texture2D texture = s.texture;
                    Rect rect = Rect.MinMaxRect(0, 0, 0, 0);
                    if(!s.packed)
                    {
                        int cnt = spriteAtlas.GetSprites(tmpSprites, s.name);
                        if(cnt == 0)
                        {
                            error = SpriteAtlasError.NotPacked;
                            errorInfo = $"can not find {s.name} in sprite atlas";
                            return;
                        }
                        int sameNameCnt = 0;
                        // get index of packedSprites with same name
                        for(int i = 0;i < sprites.Length;++i)
                        {
                            if(sprites[i] == s)
                            {
                                break;
                            }
                            if(sprites[i].name == s.name)
                            {
                                ++sameNameCnt;
                            }
                        }
                        texture = tmpSprites[sameNameCnt].texture;
                        rect = CalculateRect(tmpSprites[sameNameCnt]);
                    }
                    else
                    {
                        rect = CalculateRect(s);
                    }
                    bool match = false;
                    foreach(SpriteTexture st in m_spriteTextures)
                    {
                        if(st.texture == texture)
                        {
                            st.sprites.Add(new SpriteRect { sprite = s, rect = rect });
                            match = true;
                            break;
                        }
                    }
                    if(!match)
                    {
                        error = SpriteAtlasError.TextureNotFound;
                        errorInfo = $"can not find texture {texture.name} for {s.name}";
                    }
                }
            }
            catch (Exception e)
            {
                error = SpriteAtlasError.UnknownException;
                errorInfo = e.Message + "\r\n" + e.StackTrace;
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