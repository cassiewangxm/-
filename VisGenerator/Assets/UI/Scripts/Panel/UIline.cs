using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIline : MonoBehaviour
{
    public float LineWidth = 6f;

    private RectTransform m_Rect;

    
    public void SetTarget(Vector3 target, Vector3 source)
    {
        Debug.Log("target:" + target);
        Debug.Log("source:" + source);
        transform.position = source;
        Debug.Log("source:" + transform.position);
        m_Rect.sizeDelta = new Vector2(LineWidth, Vector3.Distance(target, source));
        double angle = Math.Atan2(target.y - source.y, target.x - source.x) * 180 / Math.PI;
        transform.rotation = Quaternion.Euler(0, 0, (float)angle + 270);
    }

    public void SetTarget(Vector3 target)
    {
        SetTarget(target, transform.position);
    }
    // Start is called before the first frame update
    void Awake()
    {
        m_Rect = transform.GetComponent<RectTransform>();
    }
}
