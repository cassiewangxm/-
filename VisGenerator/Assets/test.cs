using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    drawallcurve testdraw;
    Vector3[] testarray = { new Vector3(3, 0, 4), new Vector3(3, 0, 5) , new Vector3(3, 0, 6) };
    Vector3[] testarray2 = { new Vector3(0, 0, 4), new Vector3(0, 0, 5) };
    // Start is called before the first frame update
    void Start()
    {
        testdraw = GetComponent<drawallcurve>();
    }
    int aaa=0;
    // Update is called once per frame
    void Update()
    {
        /*
        if (aaa++ == 0)
        {
            //add lines
            testdraw.Addlines(testarray, testarray2);
            testdraw.Addlines(Vector3.zero, testarray);
            testdraw.Addlines(Vector3.zero, new Vector3(5, 0, 5));
            testdraw.Addlines(testarray2, new Vector3(6, 2, 6));
            //delete lines
            testdraw.Deletelines(Vector3.zero, testarray);
        }
        */
        
    }

    public void AddLine(Vector3 vectorA, Vector3 vectorB, string tag, float thickness)
    {
        testdraw.Addlines(vectorA, vectorB, tag, thickness);
    }
    public void DeleteLines()
    {
        testdraw.DeleteAllLines();
    }
}
