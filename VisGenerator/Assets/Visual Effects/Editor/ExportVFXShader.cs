using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class VFXShaderTool
{
    [MenuItem("VFXShaderTool/ExportShaderFromVFXAsset")]
    public static void ExportShader()
    {
        UnityEngine.Object selectedObj = Selection.objects[0];
        string assetPath = AssetDatabase.GetAssetPath(selectedObj);

        Assembly asm = typeof(UnityEditor.Tool).Assembly;
        Type verType = asm.GetType("UnityEditor.VFX.VisualEffectResource");
        MethodInfo getResourceAtPath = verType.GetMethod("GetResourceAtPath");
        object retObj = getResourceAtPath.Invoke(null, new object[] {assetPath});

        PropertyInfo pi = verType.GetProperty("shaderSources");
        object shaderSources = pi.GetValue(retObj);
        object shaderSource = (shaderSources as Array).GetValue(2);
        Type ssdType = shaderSource.GetType();
        FieldInfo fi = ssdType.GetField("source");
        string source = fi.GetValue(shaderSource) as string;

        if (source.IndexOf("ModifyVertexPosition") == -1)
        {
            string prefix = "\t\t\t";
            string insertCode = "\n" + prefix;
            insertCode += "void ModifyVertexPosition(float2 SegmentDetailInfo, float3 VertexPosition, out float3 OutputVertexPosition)\n";
            insertCode += prefix + "{\n";
            insertCode += prefix + "\tfloat3 vertexPos = VertexPosition;\n";
            insertCode += prefix + "\tfloat yScale = step(VertexPosition.y, -0.001f);\n";
            insertCode += prefix + "\tfloat xScale = step(VertexPosition.x, -0.001f);\n";
            insertCode += prefix + "\tfloat zScale = step(VertexPosition.z, -0.001f);\n";
            insertCode += prefix + "\tfloat offset = yScale * SegmentDetailInfo.x + (1 - yScale) * SegmentDetailInfo.y;\n";
            insertCode += prefix + "\tvertexPos.x = vertexPos.x + (1 - xScale) * offset - xScale * offset;\n";
            insertCode += prefix + "\tvertexPos.z = vertexPos.z + (1 - zScale) * offset - zScale * offset;\n";
            insertCode += prefix + "\tOutputVertexPosition = vertexPos;\n";
            insertCode += prefix + "}\n";
            insertCode += prefix + "\n";
            insertCode += prefix + "#pragma vertex vert\n";

            source = source.Replace("#pragma vertex vert", insertCode);

            prefix += "\t";
            insertCode = "\n" + prefix;
            insertCode += "float3 inputVertexPosition = i.pos;\n";
            insertCode += prefix + "ModifyVertexPosition(attributes.color.rg, i.pos, inputVertexPosition);\n";
            source = source.Replace("float3 inputVertexPosition = i.pos;", insertCode);
        }

        string shaderPath = assetPath.Replace("vfx", "shader");
        File.WriteAllText(shaderPath, source);
    }

    [MenuItem("VFXShaderTool/SetShaderToVFXAsset")]
    public static void SetShader()
    {
        UnityEngine.Object selectedObj = Selection.objects[0];
        string assetPath = AssetDatabase.GetAssetPath(selectedObj);

        Assembly asm = typeof(UnityEditor.Tool).Assembly;
        Type verType = asm.GetType("UnityEditor.VFX.VisualEffectResource");
        MethodInfo getResourceAtPath = verType.GetMethod("GetResourceAtPath");
        object retObj = getResourceAtPath.Invoke(null, new object[] {assetPath});

        PropertyInfo pi = verType.GetProperty("shaderSources");
        object shaderSources = pi.GetValue(retObj);
        object shaderSource = (shaderSources as Array).GetValue(2);
        Type ssdType = shaderSource.GetType();
        FieldInfo fi = ssdType.GetField("source");

        string shaderPath = assetPath.Replace("vfx", "shader");
        string source = File.ReadAllText(shaderPath);
        fi.SetValue(shaderSource, source);
        (shaderSources as Array).SetValue(shaderSource, 2);
        pi.SetValue(retObj, shaderSources);
        Debug.Log("Success");
    }

    [MenuItem("VFXShaderTool/CheckSaveSuccess")]
    public static void Check()
    {
        UnityEngine.Object selectedObj = Selection.objects[0];
        string assetPath = AssetDatabase.GetAssetPath(selectedObj);

        Assembly asm = typeof(UnityEditor.Tool).Assembly;
        Type verType = asm.GetType("UnityEditor.VFX.VisualEffectResource");
        MethodInfo getResourceAtPath = verType.GetMethod("GetResourceAtPath");
        object retObj = getResourceAtPath.Invoke(null, new object[] {assetPath});

        PropertyInfo pi = verType.GetProperty("shaderSources");
        object shaderSources = pi.GetValue(retObj);
        object shaderSource = (shaderSources as Array).GetValue(2);
        Type ssdType = shaderSource.GetType();
        FieldInfo fi = ssdType.GetField("source");
        string source = fi.GetValue(shaderSource) as string;
        Debug.Log(source.IndexOf("ModifyVertexPosition"));
    }

    [MenuItem("VFXShaderTool/GenerateSegmentDetailSizeTexture")]
    public static void GenerateSegmentDetailSizeTexture()
    {
        int width = 16;
        int height = 16;
        Color[] colors = new Color[width * height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Color curColor = Color.black;
                if (j % 2 != 0)
                    curColor.r = 0.5f;
                colors[j * 16 + i] = curColor;
            }
            
        }

        Texture2D tex = new Texture2D(16, 16, TextureFormat.RGB24, false);
        tex.SetPixels(colors);
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Visual Effects/SegemntSizeDetail.png", bytes);
    }
}