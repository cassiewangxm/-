using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class lightdata
{
    private int nums;
    private int ind;
    private int indn;
    private float ti;
    private float xf, yf;
    private float xfn, yfn;

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

    public lightdata(int sidelength, float singletime, float x)
    {
        nums = sidelength * sidelength;
        ti = singletime;
        ind = (int)(x * nums);
        indn = (int)(x * nums);
    }

    public Vector2 update(float delta, int sidelength, float singletime)
    {
        ti += delta;

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

        return (new Vector2(xx + 0.5f / sidelength, yy + 0.5f / sidelength));
    }
}

public class movingpls : MonoBehaviour
{
    public Light[] lights;
    lightdata[] lightdatas;
    public int order;
    public float widthx;
    public float widthz;
    public float singletime;
    public float step;

    int sidelength
    {
        get
        {
            return (int)Mathf.Pow(2, order);
        }
    }

    void Start()
    {
        lightdatas = new lightdata[lights.Length];
        for (int i = 0; i < lights.Length; i ++)
        {
            lightdatas[i] = new lightdata(sidelength, singletime, i * 1.0f / lights.Length);
        }
    }

    void Update()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            Vector2 pos = lightdatas[i].update(Time.deltaTime, sidelength, singletime);
            lights[i].transform.position = new Vector3(pos.x * widthx, 1.0f, pos.y * widthz);
        }
    }
}
