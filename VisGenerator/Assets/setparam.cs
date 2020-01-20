using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setparam : MonoBehaviour
{
    public Material mat;
    public int order = 8;
    public float singletime = 0.1f;
    int sidelength
    {
        get
        {
            return (int)Mathf.Pow(2, order);
        }
    }
    // Start is called before the first frame update
    void rot(int n, ref int x, ref int y, int rx, int ry)
    {
        if (ry == 0)
        {
            if (rx == 1)
            {
                x = n - 1 - x;
                y = n - 1 - y;
            }
            int t = x;
            x = y;
            y = t;
        }
    }
    long xy2d(int n, int x, int y)
    {
        long d = 0;
        int s = n / 2;
        int rx = 0;
        int ry = 0;
        while (s > 0)
        {
            rx = (x & s) > 0 ? 1 : 0;
            ry = (y & s) > 0 ? 1 : 0;
            d += s * s * ((3 * rx) ^ ry);
            rot(s, ref x, ref y, rx, ry);
            s /= 2;
        }
        return d;
    }
    void d2xy(int n, long d, out int x, out int y)
    {
        long t = d;
        int s = 1;
        long rx, ry;
        rx = ry = x = y = 0;
        while (true)
        {
            if (s >= n) break;
            rx = 1 & (t / 2);
            ry = 1 & (t ^ rx);
            rot(s, ref x, ref y, (int)rx, (int)ry);
            x += s * (int)rx;
            y += s * (int)ry;
            t /= 4;
            s *= 2;
        }
    }
    int nums = 0;
    int ind = 0;
    int indn = 1;
    float ti = 0;
    void Start()
    {
        nums = sidelength * sidelength;
        ti = singletime;
        ind = 0;
        indn = 0;
    }
    float xf, yf;
    float xfn, yfn;
    // Update is called once per frame
    void Update()
    {
        ti += Time.deltaTime;

        if (ti >= singletime)
        {
            ti = 0;
            ind = indn;
            indn = (indn + 1) % nums;
            if (indn == 0)
            {
                ind = 0;
                indn = 1;
            }
            int x, y;

            d2xy(sidelength, ind, out x, out y);
            xf = (float)x / (float)sidelength;
            yf = (float)y / (float)sidelength;

            d2xy(sidelength, indn, out x, out y);
            xfn = (float)x / (float)sidelength;
            yfn = (float)y / (float)sidelength;

        }

        float rati = ti / singletime;
        float xx = xf * (1 - rati) + xfn * rati;
        float yy = yf * (1 - rati) + yfn * rati;



        mat.SetVector("pos", new Vector2(xx+0.5f/sidelength, yy+0.5f/sidelength));
    }
}
