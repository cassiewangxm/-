using System;
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
    private Vector3 curCameraPosition = Vector3.zero;
    private Quaternion curCameraRotation = Quaternion.Euler(Vector3.one);

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
            // bool screenSizeChange = curScreenWidth != Screen.width || curScreenHeight != Screen.height;
            // bool cameraPosChange = (curCameraPosition != curCamera.gameObject.transform.position) ||
            //                        (curCameraRotation != curCamera.gameObject.transform.rotation);
            //
            // if (screenSizeChange || cameraPosChange || asRaycastReadbackTexture == null)
            {
                Texture texture = ve.GetTexture("RaycastTexture");
                if (texture == null)
                    return;

                rtd.width = texture.width;
                rtd.height = texture.height;

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

                // curScreenWidth = texture.width;
                // curScreenHeight = texture.height;
                // curCameraPosition = curCamera.gameObject.transform.position;
                // curCameraRotation = curCamera.gameObject.transform.rotation;
                // Debug.Log("--->" + screenSizeChange + "," + cameraPosChange);
            }

            if (curInputX != -1 && curInputY != -1)
            {
                Color color = asRaycastReadbackTexture.GetPixel(curInputX, curInputY);
                if (color.b > 0)
                    return;

                int indexX = (int) Math.Round(color.r * 256);
                int indexY = (int) Math.Round(color.g * 256);
                ASFilter.FilterBySelectedAS(indexX, indexY);

                // Debug.Log("---->" + curInputX + "-" + curInputY + "Color --> " + color);
                // Debug.Log("------>" + indexX + "," + indexY);
            }

            // if (screenSizeChange || cameraPosChange)
            // {
            InitOrResizeASRaycastTexture();
            // }
        };
    }

    private void InitOrResizeASRaycastTexture()
    {
        if (asRaycastTexture == null)
            asRaycastTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false, true);
        else
            asRaycastTexture.Resize(Screen.width, Screen.height);
        int totalColor = Screen.width * Screen.height;
        Color[] colors = new Color[totalColor];
        for (int i = 0; i < totalColor; i++)
            colors[i] = new Color(0, 0, 1, 0);
        asRaycastTexture.SetPixels(colors);
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
        else
        {
            curInputX = -1;
            curInputY = -1;
        }
    }
}
