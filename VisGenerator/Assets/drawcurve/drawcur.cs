using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawcur : MonoBehaviour
{
    // Start is called before the first frame update
    LineRenderer lrender;
    //private bool pointschanged;
    public float linewidth = 0.2f;
    public float lineint = 0.05f;
    public Material mat;
    void Awake()
    {
        lrender = GetComponent<LineRenderer>();
        if(lrender==null)
            Debug.Log("start"); 
        //SetPoints(Vector3.zero, Vector3.one * 5);
    }
    Vector3[] calpoints(Vector3 v1, Vector3 v2,Vector3 vmid,int nums)
    {
        Vector3[] res = new Vector3[nums];
        for(int i = 0; i < nums; i++)
        {
            float t = (float)i / ((float)nums - 1.0f);
            float _tadd1 = 1 - t;
            res[i] = _tadd1 * _tadd1 * v1 + 2 * t * _tadd1 * vmid + t * t * v2;
        }
        return res;
    }
    public void SetPoints(Vector3 v1, Vector3 v2)
    {
        
        float dist = Vector3.Distance(v1, v2);
        Vector3 vmid = (v1 + v2) / 2f + new Vector3(0,  dist / 2,0);
        int ctrlnums = (int)(dist / lineint) + 1;

        if (lrender == null)
        {
            lrender = GetComponent<LineRenderer>();
            Debug.Log("LineRenderer is null");
        }
            
        lrender.positionCount = ctrlnums;
        Vector3[] points = calpoints(v1, v2, vmid, ctrlnums);
        lrender.SetPositions(points);
        lrender.endWidth = linewidth;
        lrender.startWidth = linewidth;
        lrender.material = mat;
    }
    
}
