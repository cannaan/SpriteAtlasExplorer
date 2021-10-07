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
                Rect page = Newline();
                DrawPageField();
            }
            Rect newline = Newline();
            Rect box = NewRect(500);
            Texture2D grayChecker = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Checker-Gray.png");
            float scaleX = box.width / grayChecker.width;
            float scaleY = box.height / grayChecker.height;
            GUI.DrawTextureWithTexCoords(box, grayChecker, new Rect(0, 0, scaleX, scaleY));
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
            if(m_spriteAtlasData != null)
            {
                Rect rect = Newline();
                int textureCount = m_spriteAtlasData.textureCount;
                if(textureCount > 0)
                {
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
                else
                {
                    EditorGUI.HelpBox(rect, "sprite atlas is not packed, click \"Pack Preview\" on inspector", MessageType.Warning);
                }
            }
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
