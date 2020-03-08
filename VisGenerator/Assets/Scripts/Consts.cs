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
    public Vector2 MapSize;
    public Vector2 MapPos;

    public GameObject ASGameObject;

    // Start is called before the first frame update
    void Start()
    {
        ASGameObject.GetComponent<VisualEffect>().SetVector3("position", new Vector3(ASPos.x, 0.0f, ASPos.y));
        ASGameObject.GetComponent<VisualEffect>().SetVector3("size", new Vector3(ASSize.x, 0.0f, ASSize.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
