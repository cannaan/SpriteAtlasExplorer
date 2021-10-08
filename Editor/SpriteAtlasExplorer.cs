using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using System.IO;


namespace SpriteAtlasExplorer
{
    public class SpriteAtlasExplorer : EditorWindow
    {
        [MenuItem("Window/UI/Sprite Atlas Explorer")]
        [MenuItem("CONTEXT/SpriteAtlas/Open Sprite Atlas Explorer")]
        public static void OpenSpriteAtlasExplorer(MenuCommand command)
        {
            SpriteAtlasExplorer wnd = EditorWindow.GetWindow<SpriteAtlasExplorer>();
            wnd.SetSpriteAtlas(command.context as SpriteAtlas);
            wnd.Show();
        }

        private static string s_transparentBGPath => "Default-Checker-Gray.png";

        private SpriteAtlas m_spriteAtlas;
        private SpriteAtlasMapData m_spriteAtlasData;
        private int m_atlasIndex;
        private Rect m_windowRect;
        private Texture m_transparentBackground;
        private string[] m_atlasPopupNames;

        private void SetSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            m_spriteAtlas = spriteAtlas;
            InitSpriteAtlasInfo();
        }

        private void InitSpriteAtlasInfo()
        {
            m_spriteAtlasData = null;
            m_atlasIndex = 0;
            if (m_spriteAtlas != null)
            {
                m_spriteAtlasData = SpriteAtlasMapData.Create(m_spriteAtlas);
            }
        }

        private void OnGUI()
        {
            BeginGUI();
            DrawSpriteAtlasField();
            if (m_spriteAtlas != null)
            {
                DrawPageField();
                DrawInfos();
                DrawTexturePreview();
                Rect newline = Newline();
                Rect box = NewRect(500);
                Texture2D grayChecker = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Checker-Gray.png");
                float scaleX = box.width / grayChecker.width;
                float scaleY = box.height / grayChecker.height;
                GUI.DrawTextureWithTexCoords(box, grayChecker, new Rect(0, 0, scaleX, scaleY));
            }
            EndGUI();
        }

        private Rect BeginGUI()
        {
            float margin = 10.0f;
            m_windowRect = position;
            m_windowRect.x = margin;
            m_windowRect.y = margin;
            m_windowRect.width -= margin * 2;
            m_windowRect.height -= margin * 2;
            return m_windowRect;
        }

        private void EndGUI()
        {
        }

        private void DrawSpriteAtlasField()
        {
            Rect rect = Newline();
            EditorGUI.BeginChangeCheck();
            SpriteAtlas spriteAtlas = EditorGUI.ObjectField(rect, m_spriteAtlas, typeof(SpriteAtlas), false) as SpriteAtlas;
            if (EditorGUI.EndChangeCheck())
            {
                SetSpriteAtlas(spriteAtlas);
            }
        }

        private void DrawPageField()
        {
            if(m_spriteAtlasData != null && m_spriteAtlasData.textureCount > 0)
            {
                Rect rect = Newline();
                int textureCount = m_spriteAtlasData.textureCount;
                if(m_atlasPopupNames == null || m_atlasPopupNames.Length != textureCount)
                {
                    m_atlasPopupNames = new string[textureCount];
                    for(int i = 0;i < textureCount;++i)
                    {
                        m_atlasPopupNames[i] = "# " + (i + 1);
                    }
                }
                m_atlasIndex = EditorGUI.Popup(rect, "Page:", m_atlasIndex, m_atlasPopupNames);
            }
        }

        private void DrawInfos()
        {
            bool drawRefreshButton = false;
            if(m_spriteAtlasData == null)
            {
                Rect rect = NewRect(EditorGUIUtility.singleLineHeight * 2);
                EditorGUI.HelpBox(rect, "sprite atlas is not read correctly, please refresh and try again", MessageType.Error);
                drawRefreshButton = true;
            }
            else
            {
                if(m_spriteAtlasData.error != SpriteAtlasMapData.SpriteAtlasError.None)
                {
                    Rect rect = NewRect(EditorGUIUtility.singleLineHeight * 2);
                    drawRefreshButton = true;
                    switch (m_spriteAtlasData.error)
                    {
                        case SpriteAtlasMapData.SpriteAtlasError.NoTextures:
                            EditorGUI.HelpBox(rect, "sprite atlas is not packed. click \"Pack Preview\" on its inspector", MessageType.Error);
                            break;
                        case SpriteAtlasMapData.SpriteAtlasError.NotPacked:
                            EditorGUI.HelpBox(rect, $"{m_spriteAtlasData.errorInfo}\r\nTry click \"Pack Preview\" or enter and quit play mode for once. Or set Sprite Packer Mode to Always Enabled in editor settings", MessageType.Error);
                            break;
                        case SpriteAtlasMapData.SpriteAtlasError.TextureNotFound:
                            EditorGUI.HelpBox(rect, $"{m_spriteAtlasData.errorInfo}\r\nTry click \"Pack Preview\" or enter and quit play mode for once. Or set Sprite Packer Mode to Always Enabled in editor settings", MessageType.Error);
                            break;
                        case SpriteAtlasMapData.SpriteAtlasError.UnknownException:
                            EditorGUI.HelpBox(rect, $"Caught unhandled error.\r\n{m_spriteAtlasData.errorInfo}", MessageType.Error);
                            break;
                    }
                }
            }
            if(drawRefreshButton)
            {
                Rect rect = Newline();
                if(GUI.Button(rect, "Refresh"))
                {
                    SetSpriteAtlas(m_spriteAtlas);
                }
            }
        }

        private void DrawTexturePreview()
        {

        }

        private Rect Newline()
        {
            return NewRect(EditorGUIUtility.singleLineHeight);
        }
        private Rect NewRect(float height)
        {
            Rect ret = m_windowRect;
            ret.height = height;
            m_windowRect.yMin = ret.yMax;
            return ret;
        }
    }
}
