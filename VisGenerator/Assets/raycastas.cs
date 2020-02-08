using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controller;
using UnityEngine.EventSystems;

public class raycastas : MonoBehaviour
{

    public Vector3 Position;
    public Vector2 Size;

    public Vector3 IPPosition;
    public Vector2 IPSize;

    public Texture2D Heights;
    public Texture2D Ids;
    public Texture2D Nums;
    public float HScale;
    public GameObject Cube;
    public Text Text;

    private Plane[] planes;

    private float[] pairs;

    public Camera Camera;
    private CameraController CameraController;

    private bool m_HasHit;

    // Start is called before the first frame update
    void Start()
    {
        planes = new Plane[256];
        for (int i = 0; i < planes.Length; i ++)
        {
            planes[i] = new Plane(Vector3.up, new Vector3(0.0f, i * HScale / 256.00f, 0.0f));
        }
        CameraController = Camera.GetComponent<CameraController>();
    }

    int Select(float v, float textureSize)
    {
        if (v < 0.0f)
        {
            return 0;
        }
        if (v > textureSize)
        {
            return (int)textureSize;
        }
        return (int)v;
    }

    Vector2 WorldtoUVHit(Vector3 point, float textureSize, Vector3 position, Vector2 size)
    {
        Vector3 newPoint = point;
        newPoint.x = (newPoint.x - position.x) / size.x * textureSize + 0.5f;
        newPoint.z = (newPoint.z - position.z) / size.y * textureSize + 0.5f;
        return new Vector2(Select(newPoint.x, textureSize), Select(newPoint.z, textureSize));
    }

    bool IsInside(Vector3 point, Vector3 position, Vector2 size)
    {
        if (((point.x - position.x) > -0.5f) && ((point.z - position.z) > -0.5f))
        {
            if (((point.x - position.x) < size.x + 0.5f) && ((point.z - position.z) < size.y + 0.5f))
            {
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraController.IsAS)
        {
            //Detect when there is a mouse click
            if (Input.GetMouseButton(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                m_HasHit = false;

                for (int i = planes.Length - 1; i >= 0; i--)
                {
                    float enter = 0.0f;
                    if (planes[i].Raycast(ray, out enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        if (IsInside(hitPoint, Position, Size))
                        {
                            Vector2 hitPointT = WorldtoUVHit(hitPoint, 256.0f, Position, Size);
                            float height = Heights.GetPixel((int)(hitPointT.x), (int)(hitPointT.y)).r * 256.00f;
                            if (hitPoint.y <= height * HScale / 256.00f + 0.005f)
                            {
                                hitPoint.x = (int)(hitPointT.x) * 1.00f / 256 * Size.x;
                                hitPoint.z = (int)(hitPointT.y) * 1.00f / 256 * Size.y;
                                hitPoint.y = height * HScale / 256.00f;
                                //Cube.transform.position = hitPoint;
                                Color32 color = Nums.GetPixel((int)(hitPointT.x), (int)(hitPointT.y));
                                uint num = (uint)(color.r) * (uint)(1 << 24) + (uint)(color.g) * (uint)(1 << 16) + (uint)(color.b) * (1 << 8) + (uint)(color.a);
                                //Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y) + " = " + height + ", " + num);
                                Debug.Log(Input.mousePosition);
                                //Text.transform.position = Input.mousePosition;
                                //Text.text = "IP number: " + num.ToString();
                                Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                                UIEventDispatcher.OpenIpMenuPanel(num.ToString(), screenPos);
                                //Debug.Log((int)(hitPointT.x) + ", " + (int)(hitPointT.y));
                                m_HasHit = true;
                                break;
                            }
                        }
                    }
                }

                if(!m_HasHit)
                {
                    UIEventDispatcher.HideIpMenuPanel();
                }
            }
            if (false)
            {
                for (int i = 0; i < pairs.Length / 4; i++)
                {
                    Vector2 hitPointT1 = new Vector2(pairs[i * 2], pairs[i * 2 + 1]);
                    float height1 = Heights.GetPixel((int)(hitPointT1.x), (int)(hitPointT1.y)).r * 256.00f;
                    Vector3 hitPoint1 = new Vector3();
                    hitPoint1.x = (int)(hitPointT1.x) * 1.00f / 256 * Size.x;
                    hitPoint1.z = (int)(hitPointT1.y) * 1.00f / 256 * Size.y;
                    hitPoint1.y = height1 * HScale / 256.00f;

                    Vector2 hitPointT2 = new Vector2(pairs[i * 2 + 2], pairs[i * 2 + 3]);
                    float height2 = Heights.GetPixel((int)(hitPointT2.x), (int)(hitPointT2.y)).r * 256.00f;
                    Vector3 hitPoint2 = new Vector3();
                    hitPoint2.x = (int)(hitPointT2.x) * 1.00f / 256 * Size.x;
                    hitPoint2.z = (int)(hitPointT2.y) * 1.00f / 256 * Size.y;
                    hitPoint2.y = height2 * HScale / 256.00f;
                }
            }
        }
        else
        {
            //Detect when there is a mouse click
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                float enter = 0.0f;
                if (planes[0].Raycast(ray, out enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    if (IsInside(hitPoint, IPPosition, IPSize))
                    {
                        Vector2 hitPointT = WorldtoUVHit(hitPoint, 1024.0f, IPPosition, IPSize);
                        Color idColor = Ids.GetPixel((int)(hitPointT.x), (int)(hitPointT.y));
                        float id = idColor.r * 256.00f + idColor.g;
                        Text.transform.position = Input.mousePosition;
                        Text.text = "AS number: " + ((int)id).ToString();
                        Debug.Log((int)(hitPointT.x) + ", " +(int)(hitPointT.y));
                    }
                }
            }
        }
    }
}
