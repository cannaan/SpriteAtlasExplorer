using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using System.IO;
using System;
using UnityEditorInternal;

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
        private List<Sprite> m_selectedSprites = new List<Sprite>();
        private Rect m_windowRect;
        private Texture2D m_backgroundTexture;
        private string[] m_atlasPopupNames;
        private ScalableTextureGUI m_previewGUI = new ScalableTextureGUI();

        private Color m_rectCol;

        private void OnEnable()
        {
            LoadRectColor();
        }

        private void SetSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            m_spriteAtlas = spriteAtlas;
            InitSpriteAtlasInfo();
            m_selectedSprites.Clear();
        }

        private void InitSpriteAtlasInfo()
        {
            m_spriteAtlasData = null;
            m_atlasIndex = 0;
            m_previewGUI = new ScalableTextureGUI();
            if (m_spriteAtlas != null)
            {
                m_spriteAtlasData = SpriteAtlasMapData.Create(m_spriteAtlas);
                m_previewGUI.texture = m_spriteAtlasData.GetTextureAt(m_atlasIndex);
                if(m_backgroundTexture == null)
                {
                    m_backgroundTexture = AssetDatabase.GetBuiltinExtraResource<Texture2D>(s_transparentBGPath);
                }
                m_previewGUI.background = m_backgroundTexture;
            }
        }

        private void OnGUI()
        {
            BeginGUI();
            DrawSpriteAtlasField();
            DrawRefreshButton();
            if (m_spriteAtlas != null)
            {
                DrawPageField();
                DrawInfos();
                Rect colRect = Newline();
                colRect.width = 200;
                EditorGUI.BeginChangeCheck();
                Color oldTextColor = GUI.contentColor;
                GUI.contentColor = new Color(m_rectCol.r, m_rectCol.g, m_rectCol.b);
                m_rectCol = EditorGUI.ColorField(colRect, "Rect Color", m_rectCol);
                GUI.contentColor = oldTextColor;
                if(EditorGUI.EndChangeCheck())
                {
                    SaveRectColor();
                }
                Newline();
                DrawTexturePreview();
                /*Rect newline = Newline();
                Rect box = NewRect(500);
                Texture2D grayChecker = AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Checker-Gray.png");
                float scaleX = box.width / grayChecker.width;
                float scaleY = box.height / grayChecker.height;
                GUI.DrawTextureWithTexCoords(box, grayChecker, new Rect(0, 0, scaleX, scaleY));*/
            }
            EndGUI();
        }

        private void ProcessSelectionEvent(Rect rect)
        {
            Vector2 pos = Event.current.mousePosition;
            if(Event.current.type == EventType.MouseUp)
            {
                if(rect.Contains(pos))
                {
                    int cnt = m_spriteAtlasData.GetSpriteCount(m_atlasIndex);
                    for (int i = 0; i < cnt; ++i)
                    {
                        if (m_spriteAtlasData.GetSpriteAt(m_atlasIndex, i, out Rect spriteRect, out _))
                        {
                            float tmp = spriteRect.yMin;
                            spriteRect.yMin = 1.0f - spriteRect.yMax;
                            spriteRect.yMax = 1.0f - tmp;
                            spriteRect = m_previewGUI.NormalizedToRect(spriteRect, rect);

                        }
                    }
                }
            }
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

        private void DrawRefreshButton()
        {
            Rect rect = Newline();
            if (GUI.Button(rect, "Refresh"))
            {
                SetSpriteAtlas(m_spriteAtlas);
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
                EditorGUI.BeginChangeCheck();
                m_atlasIndex = EditorGUI.Popup(rect, "Page:", m_atlasIndex, m_atlasPopupNames);
                if(EditorGUI.EndChangeCheck())
                {
                    m_previewGUI.texture = m_spriteAtlasData.GetTextureAt(m_atlasIndex);
                    m_previewGUI.SetDirty();
                    m_selectedSprites.Clear();
                }
            }
        }

        private void DrawInfos()
        {
            if(m_spriteAtlasData == null)
            {
                Rect rect = NewRect(EditorGUIUtility.singleLineHeight * 3);
                EditorGUI.HelpBox(rect, "sprite atlas is not read correctly, please refresh and try again", MessageType.Error);
            }
            else
            {
                if(m_spriteAtlasData.error != SpriteAtlasMapData.SpriteAtlasError.None)
                {
                    Rect rect = NewRect(EditorGUIUtility.singleLineHeight * 3);
                    switch (m_spriteAtlasData.error)
                    {
                        case SpriteAtlasMapData.SpriteAtlasError.AtlasNotGenerated:
                            EditorGUI.HelpBox(rect, "Sprite Atlas has no texture generated or has no sprites included.", MessageType.Error);
                            rect = Newline();
                            if(GUI.Button(new Rect(rect.center.x - 100, rect.yMin, 100, rect.height), "Fix me"))
                            {
                                PackSpriteAtlas(m_spriteAtlas);
                                Repaint();
                            }
                            break;
                        case SpriteAtlasMapData.SpriteAtlasError.SpriteNotPacked:
                            EditorGUI.HelpBox(rect, $"sprite(s) not connected to sprite atlas.\r\nTry change packable objects and change back to fix it.", MessageType.Error);
                            rect = Newline();
                            if (GUI.Button(new Rect(rect.center.x - 100, rect.yMin, 100, rect.height), "Fix me"))
                            {
                                EditorApplication.EnterPlaymode();
                                EditorUtility.DisplayDialog("Sprite Atlas Explorer", "Enter play mode for one time to let sprite atlas explorer read sprite data correctly.\r\nYou can exit play mode after closing this dialog.", "OK");
                                Repaint();
                            }
                            break;
                        case SpriteAtlasMapData.SpriteAtlasError.UnknownException:
                            EditorGUI.HelpBox(rect, $"Caught unhandled error.\r\n{m_spriteAtlasData.errorInfo}", MessageType.Error);
                            break;
                    }
                }
            }
        }

        private void PackSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            if(EditorSettings.spritePackerMode != SpritePackerMode.AlwaysOnAtlas)
            {
                EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            }
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget, true);
        }
        private void RepackSpriteAtlas(SpriteAtlas spriteAtlas)
        {
            UnityEngine.Object[] packables = spriteAtlas.GetPackables();
            SerializedObject serializedObject = new SerializedObject(spriteAtlas);
            SerializedProperty packableProperty = serializedObject.FindProperty("m_EditorData.packables");
            for(int i = packables.Length - 1;i >= 0;--i)
            {
                packableProperty.DeleteArrayElementAtIndex(i);
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.Refresh();
            PackSpriteAtlas(spriteAtlas);
            for(int i = 0;i < packables.Length;++i)
            {
                packableProperty.InsertArrayElementAtIndex(i);
                SerializedProperty element = packableProperty.GetArrayElementAtIndex(i);
                element.objectReferenceValue = packables[i];
            }
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.Refresh();
            PackSpriteAtlas(spriteAtlas);
        }

        private void DrawTexturePreview()
        {
            if(m_spriteAtlasData != null)
            {
                if(m_spriteAtlasData.textureCount == 0)
                {
                    Rect helpRect = NewRect(EditorGUIUtility.singleLineHeight * 2);
                    EditorGUI.HelpBox(helpRect, "No Textures generated.", MessageType.Warning);
                    return;
                }
                Rect previewRect = NewRectToBottom();
                bool repaint = m_previewGUI.OnGUI(previewRect);
                ProcessSelectionEvent(previewRect);
                DrawSpriteRects(previewRect);
                if(repaint)
                {
                    Repaint();
                }
            }
        }

        private void DrawSpriteRects(Rect rect)
        {
            int cnt = m_spriteAtlasData.GetSpriteCount(m_atlasIndex);
            for(int i = 0;i < cnt;++i)
            {
                if(m_spriteAtlasData.GetSpriteAt(m_atlasIndex, i, out Rect spriteRect, out _))
                {
                    Rect guiRect = m_previewGUI.NormalizedToRect(spriteRect, rect);
                    MaskableRectGUI.Draw(guiRect, m_rectCol, rect, 2.0f, new Color(m_rectCol.r, m_rectCol.g, m_rectCol.b));
                }
            }
        }

        private void SaveRectColor()
        {
            EditorPrefs.SetFloat("SpriteAtlasExplorer.RectColor.r", m_rectCol.r);
            EditorPrefs.SetFloat("SpriteAtlasExplorer.RectColor.g", m_rectCol.g);
            EditorPrefs.SetFloat("SpriteAtlasExplorer.RectColor.b", m_rectCol.b);
            EditorPrefs.SetFloat("SpriteAtlasExplorer.RectColor.a", m_rectCol.a);
        }
        private void LoadRectColor()
        {
            float r = EditorPrefs.GetFloat("SpriteAtlasExplorer.RectColor.r", 0);
            float g = EditorPrefs.GetFloat("SpriteAtlasExplorer.RectColor.g", 136.0f / 255);
            float b = EditorPrefs.GetFloat("SpriteAtlasExplorer.RectColor.b", 188.0f / 255);
            float a = EditorPrefs.GetFloat("SpriteAtlasExplorer.RectColor.a", 60.0f / 255);
            m_rectCol = new Color(r, g, b, a);
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
        private Rect NewRectToBottom()
        {
            return NewRect(m_windowRect.height);
        }
    }
}
