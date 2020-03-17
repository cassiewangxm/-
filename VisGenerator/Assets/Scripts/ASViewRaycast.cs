using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using UnityEngine.Windows;

public class ASViewRaycast : MonoBehaviour
{
    public Filters ASFilter = null;

    private Texture2D asRaycastTexture = null;
    private Texture2D asRaycastReadbackTexture = null;
    private int curInputX = -1;
    private int curInputY = -1;
    private int curScreenWidth = 0;
    private int curScreenHeight = 0;
    private Color[] clearColors = null;
    public bool usingAsync = false;

    private void Awake()
    {
        InitOrResizeASRaycastTexture();

        GameObject cameraObj = GameObject.Find("CameraParent-AS");
        Camera curCamera = cameraObj.GetComponentInChildren<Camera>();

        VisualEffect ve = this.GetComponent<VisualEffect>();

        RenderTextureDescriptor rtd = new RenderTextureDescriptor();
        rtd.sRGB = false;
        rtd.volumeDepth = 1;
        rtd.msaaSamples = 1;
        rtd.dimension = TextureDimension.Tex2D;
        rtd.autoGenerateMips = false;
        rtd.colorFormat = RenderTextureFormat.ARGB32;
        rtd.graphicsFormat = GraphicsFormat.B8G8R8A8_UNorm;

        RenderPipelineManager.beginFrameRendering += (context, camera) =>
        {
            Texture texture = ve.GetTexture("RaycastTexture");
            if (texture == null)
                return;

            AsyncGPUReadback.Request(texture, 0, null);
            if (curInputX != -1 && curInputY != -1)
            {
                rtd.width = texture.width;
                rtd.height = texture.height;

                if (usingAsync)
                    AsyncGPUReadback.Request(texture, 0, AsyncGPUReadbackCallback);
                else
                    SyncGPUReadback(rtd, texture);
            }

            InitOrResizeASRaycastTexture();
        };
    }

    private void SyncGPUReadback(RenderTextureDescriptor rtd, Texture texture)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(rtd);
        Graphics.Blit(texture, renderTexture);

        if (asRaycastReadbackTexture == null)
            asRaycastReadbackTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false, true);
        else if (asRaycastReadbackTexture.width != texture.width || asRaycastReadbackTexture.height != texture.height)
            asRaycastReadbackTexture.Resize(texture.width, texture.height);
        RenderTexture.active = renderTexture;
        asRaycastReadbackTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        asRaycastReadbackTexture.Apply();
        RenderTexture.ReleaseTemporary(renderTexture);

        Color color = asRaycastReadbackTexture.GetPixel(curInputX, curInputY);
        if (color.b > 0)
        {
            curInputX = -1;
            curInputY = -1;
            return;
        }

        int indexX = (int) Math.Round(color.r * 256);
        int indexY = (int) Math.Round(color.g * 256);
        ASFilter.FilterBySelectedAS(indexX, indexY);

        Debug.Log("Sync---->" + curInputX + "-" + curInputY + "Color --> " + color);
        Debug.Log("Sync------>" + indexX + "," + indexY);

        curInputX = -1;
        curInputY = -1;
    }

    private void AsyncGPUReadbackCallback(AsyncGPUReadbackRequest request)
    {
        if (!request.done)
            return;
        if (curInputX == -1 || curInputY == -1)
            return;
        NativeArray<Color32> colors = request.GetData<Color32>();
        Color32 color = colors[curInputY * request.width + curInputY];
        Debug.Log("AsyncGPUReadbackCallback" + color);
        if (color.b > 0)
        {
            curInputX = -1;
            curInputY = -1;
            return;
        }

        int indexX = color.r;
        int indexY = color.g;
        ASFilter.FilterBySelectedAS(indexX, indexY);

        Debug.Log("Async---->" + curInputX + "-" + curInputY + "Color --> " + color);
        Debug.Log("Async------>" + indexX + "," + indexY);

        curInputX = -1;
        curInputY = -1;
    }

    private void InitOrResizeASRaycastTexture()
    {
        bool needClearColor = false;
        if (asRaycastTexture == null)
        {
            asRaycastTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false, true);
            needClearColor = true;
        }
        else if (asRaycastTexture.width != Screen.width || asRaycastTexture.height != Screen.height)
        {
            asRaycastTexture.Resize(Screen.width, Screen.height);
            needClearColor = true;
        }

        if (needClearColor)
        {
            int totalColor = Screen.width * Screen.height;
            clearColors = new Color[totalColor];
            for (int i = 0; i < totalColor; i++)
                clearColors[i] = new Color(0, 0, 1, 0);
        }

        asRaycastTexture.SetPixels(clearColors);
        asRaycastTexture.Apply();
        VisualEffect ve = this.GetComponent<VisualEffect>();
        ve.SetTexture("RaycastTexture", asRaycastTexture);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            curInputX = (int) Input.mousePosition.x;
            curInputY = (int) Input.mousePosition.y;
        }
    }

    private void OnDestroy()
    {
        VisualEffect ve = this.GetComponent<VisualEffect>();
        ve.SetTexture("RaycastTexture", null);
    }
}