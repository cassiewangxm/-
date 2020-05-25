using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Consts : MonoBehaviour
{
    public Vector2 ASSize;
    public Vector2 ASPos;
    public Vector2 IPSize;
    public Vector2 IPPos;
    public Vector3 MapSize;
    public Vector3 MapPos;

    public GameObject ASGameObject;
    public GameObject IPGameObject;

    // Start is called before the first frame update
    void Start()
    {
        ASGameObject.GetComponent<VisualEffect>().SetVector3("position", MapPos);
        ASGameObject.GetComponent<VisualEffect>().SetVector3("size", MapSize);
        IPGameObject.transform.position = new Vector3(IPPos.x, 0.0f, IPPos.y);
        IPGameObject.transform.localScale = new Vector3(IPSize.x, 1.0f, IPSize.y);
    }
}
