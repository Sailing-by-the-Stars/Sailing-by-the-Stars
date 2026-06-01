using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SpriteSheetGen : EditorWindow
{
    private DefaultAsset sourceFolder;
    private int columns = 4;
    private string outputName = "SpriteSheet";

    [MenuItem("Tools/Sprite Sheet Generator")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSheetGen>("Sprite Sheet Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generate Sprite Sheet", EditorStyles.boldLabel);

        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "Frames Folder",
            sourceFolder,
            typeof(DefaultAsset),
            false);

        columns = EditorGUILayout.IntField("Columns", columns);
        outputName = EditorGUILayout.TextField("Output Name", outputName);

        if (GUILayout.Button("Generate"))
        {
            GenerateSpriteSheet();
        }
    }

    private void GenerateSpriteSheet()
    {
        if (sourceFolder == null)
        {
            Debug.LogError("Please select a source folder.");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(sourceFolder);

        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        var textures = textureGuids
            .Select(g => AssetDatabase.LoadAssetAtPath<Texture2D>(
                AssetDatabase.GUIDToAssetPath(g)))
            .OrderBy(t => ExtractNumber(t.name))
            .ToArray();

        if (textures.Length == 0)
        {
            Debug.LogError("No textures found.");
            return;
        }

        int frameWidth = textures[0].width;
        int frameHeight = textures[0].height;

        foreach (var tex in textures)
        {
            if (tex.width != frameWidth || tex.height != frameHeight)
            {
                Debug.LogError(
                    $"Texture {tex.name} has different dimensions.");
                return;
            }
        }

        int rows = Mathf.CeilToInt((float)textures.Length / columns);

        Texture2D sheet = new Texture2D(
            frameWidth * columns,
            frameHeight * rows,
            TextureFormat.RGBA32,
            false);

        Color[] clearPixels = Enumerable.Repeat(
            Color.clear,
            sheet.width * sheet.height).ToArray();

        sheet.SetPixels(clearPixels);

        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D tex = GetReadableTexture(textures[i]);

            int x = (i % columns) * frameWidth;

            // Fill top-to-bottom visually
            int y = (rows - 1 - (i / columns)) * frameHeight;

            sheet.SetPixels(
                x,
                y,
                frameWidth,
                frameHeight,
                tex.GetPixels());
        }

        sheet.Apply();

        string outputPath = Path.Combine(
            folderPath,
            outputName + ".png");

        File.WriteAllBytes(outputPath, sheet.EncodeToPNG());

        AssetDatabase.Refresh();

        Debug.Log($"Sprite sheet created: {outputPath}");
    }

    private Texture2D GetReadableTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(
            source.width,
            source.height,
            TextureFormat.RGBA32,
            false);

        readable.ReadPixels(
            new Rect(0, 0, rt.width, rt.height),
            0,
            0);

        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }


    private int ExtractNumber(string name)
    {
        string digits = new string(name.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out int result) ? result : 0;
    }
}