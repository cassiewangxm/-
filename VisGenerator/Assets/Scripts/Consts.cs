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

    // Start is called before the first frame update
    void Start()
    {
        ASGameObject.GetComponent<VisualEffect>().SetVector3("position", MapPos);
        ASGameObject.GetComponent<VisualEffect>().SetVector3("size", MapSize);
    }
}
