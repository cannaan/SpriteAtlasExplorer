using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using System.IO;

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
    private int m_atlasIndex;
    private List<Texture2D> m_previewTextures = new List<Texture2D>();
    private Dictionary<Sprite, Sprite> m_spriteMap = new Dictionary<Sprite, Sprite>();
    private Rect m_windowRect;
    private Texture m_transparentBackground;

    private void SetSpriteAtlas(SpriteAtlas spriteAtlas)
    {
        m_spriteAtlas = spriteAtlas;
    }

    private void InitSpriteAtlasInfo()
    {
        m_atlasIndex = 0;
        m_previewTextures.Clear();
        m_spriteMap = new Dictionary<Sprite, Sprite>();
        List<Sprite> m_packedSprites = new List<Sprite>();
        if (m_spriteAtlas != null)
        {
            foreach (Object package in m_spriteAtlas.GetPackables())
            {
                CollectSprites(package, m_packedSprites);
            }
        }
    }

    private void CollectSprites(Object obj, List<Sprite> result)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        string[] guids = AssetDatabase.FindAssets("t:sprite", new string[] { path });
        foreach(string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            result.Add(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath));
        }
    }

    private void OnGUI()
    {
        BeginGUI();
        Rect spriteAtlasRect = Newline();
        EditorGUI.BeginChangeCheck();
        m_spriteAtlas = EditorGUI.ObjectField(spriteAtlasRect, m_spriteAtlas, typeof(SpriteAtlas), false) as SpriteAtlas;
        if(EditorGUI.EndChangeCheck())
        {
            InitSpriteAtlasInfo();
        }
        if (m_spriteAtlas != null)
        {
            Rect page = Newline();
            DrawPageField(page);
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

    private void DrawPageField(Rect rect)
    {
        if (GUI.Button(rect, "Print"))
        {
            foreach (Object package in m_spriteAtlas.GetPackables())
            {
                Debug.Log(package, package);
            }
            Sprite[] sprites = new Sprite[m_spriteAtlas.spriteCount];
            m_spriteAtlas.GetSprites(sprites);
            foreach (Sprite s in sprites)
            {
                
                Debug.Log($"{s.name}:{AssetDatabase.GetAssetPath(s)}", s);
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
