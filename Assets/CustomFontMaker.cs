using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class CustomFontMaker
{
    /// <summary>
    /// 直接生成CustomFont，如果需要换行，请修改Custom Font属性Line Spacing，数值为图片的最大高度
    /// </summary>
    [MenuItem("Assets/Make Custom Font")]
    static void MakeCustomFont()
    {
        Object obj = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(obj);
        DirectoryInfo dir = null;
        if(Directory.Exists(path))
        {
            dir = new DirectoryInfo(path);
        }
        else
        {
            FileInfo file = new FileInfo(path);
            dir = file.Directory;
        }
        string fntPath = string.Empty;
        string tgaPath = string.Empty;
        foreach(FileInfo fileInfo in dir.GetFiles())
        {
            if (fileInfo.FullName.EndsWith("meta"))
                continue;
            if(fileInfo.FullName.EndsWith(".fnt"))
            {
                fntPath = AllPath2AssetPath(fileInfo.FullName);
            }
            if(fileInfo.FullName.EndsWith(".tga"))
            {
                tgaPath = AllPath2AssetPath(fileInfo.FullName);
            }
        }
        string matPath = fntPath.Replace(".fnt", "_Mat.mat");
        string customFontPath = fntPath.Replace(".fnt", "_CustomFont.fontsettings");

        Material mat = new Material(Shader.Find("GUI/Text Shader"));
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(tgaPath) as Texture2D;
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.Refresh();

        Font font = new Font();
        font.material = AssetDatabase.LoadAssetAtPath<Material>(matPath) as Material;;
        FillCustomFontInfo(fntPath, font);
        AssetDatabase.CreateAsset(font, customFontPath);
        AssetDatabase.Refresh();
        Debug.LogError("字体：" + customFontPath.ToString() + "生成完成");
    }

    private static void FillCustomFontInfo(string fntPath, Font font)
    {
        TextAsset xmlText = AssetDatabase.LoadAssetAtPath<TextAsset>(fntPath) as TextAsset;
        XmlDocument _doc = new XmlDocument();
        byte[] _array = Encoding.ASCII.GetBytes(xmlText.text);
        MemoryStream _stream = new MemoryStream(_array);
        _doc.Load(_stream);

        XmlNode _font = _doc.SelectSingleNode("font");
        XmlElement _common = (XmlElement)_font.SelectSingleNode("common");

        float _scaleW = float.Parse(_common.GetAttribute("scaleW"));
        float _scaleH = float.Parse(_common.GetAttribute("scaleH"));

        XmlNode _chars = _font.SelectSingleNode("chars");
        XmlNodeList _charsList = _chars.ChildNodes;

        CharacterInfo[] _infos = new CharacterInfo[_charsList.Count];
        for (int i = 0; i < _charsList.Count; i++)
        {
            XmlElement _element = (XmlElement)_charsList[i];
            CharacterInfo _characterInfo = new CharacterInfo();
            _characterInfo.index = int.Parse(_element.GetAttribute("id"));
            float _x = float.Parse(_element.GetAttribute("x"));
            float _y = float.Parse(_element.GetAttribute("y"));
            int _width = int.Parse(_element.GetAttribute("width"));
            int _height = int.Parse(_element.GetAttribute("height"));
            int _xadvance = int.Parse(_element.GetAttribute("xadvance"));
            Rect uvrecr = new Rect(_x / _scaleW, 1 - (_y + _height) / _scaleH, _width / _scaleW, _height / _scaleH);
            _characterInfo.uvBottomLeft = new Vector2(uvrecr.xMin, uvrecr.yMin);
            _characterInfo.uvTopRight = new Vector2(uvrecr.xMax, uvrecr.yMax);
            _characterInfo.uvBottomRight = new Vector2(uvrecr.xMax, uvrecr.yMin);
            _characterInfo.uvTopLeft = new Vector2(uvrecr.xMin, uvrecr.yMax);
            Rect vertrect = new Rect(0, -_height, _width, _height); ;
            _characterInfo.minX = (int)vertrect.xMin;
            _characterInfo.maxX = (int)vertrect.xMax;
            _characterInfo.minY = (int)vertrect.yMin;
            _characterInfo.maxY = -(int)vertrect.yMax;
            _characterInfo.advance = _xadvance;
            _infos[i] = _characterInfo;
        }
        font.characterInfo = _infos;
        
    }

    private static string AllPath2AssetPath(string allPath)
    {
        return allPath.Substring(allPath.IndexOf("Assets"));
    }
}
