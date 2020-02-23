using System.Collections;
using System.Collections.Generic;
using UnityEngine;
struct kk
{
    public Vector3 a;
    public Vector3 b;
    public kk(Vector3 aa,Vector3 bb)
    {
        a = aa;
        b = bb;
    }
};
public class drawallcurve : MonoBehaviour
{
    
    // Start is called before the first frame update
    public GameObject prefab;
    public Material mat;
    public float lineinterval = 0.05f;
    public float widthoflines=0.2f;
    Dictionary<kk, GameObject> lines;
    void Start()
    {
        lines = new Dictionary<kk, GameObject>();
    }
    public void Addlines(Vector3 v1,Vector3 v2, string tag)
    {
        GameObject li= Instantiate(prefab, Vector3.zero, Quaternion.identity);
        li.tag = tag;
        
        lines.Add(new kk(v1, v2), li);

        drawcur curve = li.GetComponent<drawcur>();
        if (lineinterval > 0)
            curve.lineint = lineinterval;
        if (widthoflines > 0)
            curve.linewidth = widthoflines;
        if (mat != null)
        {
            curve.mat = mat;
        }
        curve.SetPoints(v1, v2);
    }
    public void Addlines(Vector3[] v11, Vector3 v2)
    {
        for(int i = 0; i < v11.Length; i++)
        {
            Vector3 v1 = v11[i];
            //Addlines(v1, v2);
        }
    }
    public void Addlines(Vector3 v1, Vector3[] v22)
    {
        for(int i = 0; i < v22.Length; i++)
        {
            Vector3 v2 = v22[i];
            //Addlines(v1, v2);
        }
    }
    public void Addlines(Vector3[] v11, Vector3[] v22)
    {
        for(int i = 0; i < v11.Length; i++)
        {
            Vector3 v1 = v11[i];
            Addlines(v1, v22);
        }
    }
    
    public void Deletelines(Vector3 v1,Vector3 v2)
    {
        var tmp = new kk( v1, v2 );
        if (lines.ContainsKey(tmp))
        {
            var tmp2 = lines[tmp];
            lines.Remove(tmp);
            Destroy(tmp2);
        }
    }
    public void Deletelines(Vector3[] v1, Vector3 v2)
    {
        foreach(var i in v1)
        {
            Deletelines(i, v2);
        }
    }
    public void Deletelines(Vector3 v1, Vector3[] v2)
    {
        foreach(var i in v2)
        {
            Deletelines(v1, i);
        }

    }
    public void Deletelines(Vector3[] v1, Vector3[] v2)
    {
        foreach(var i in v1)
        {
            Deletelines(i, v2);
        }
    }

    public void DeleteAllLines()
    {
        OnDisable();
    }
    
    private void OnDisable()
    {
        foreach(var i in lines.Keys)
        {
            var tmp = lines[i];
            Destroy(tmp);
        }
        lines.Clear();
    }
}
