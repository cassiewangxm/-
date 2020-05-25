using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Linq;
using UnityEngine.UI;
using System;

public class visualtransfer : MonoBehaviour
{
    void rot(int n,ref int x,ref int y,int rx,int ry)
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
    long xy2d(int n,int x,int y)
    {
        long d = 0;
        int s = n / 2;
        int rx = 0;
        int ry = 0;
        while (s > 0)
        {
            rx = (x & s) > 0 ? 1 : 0;
            ry = (y & s) > 0 ? 1 : 0;
            d += s * s * ((3*rx)^ry);
            rot(s,ref x,ref y, rx, ry);
            s /= 2;
        }
        return d;
    }
    void d2xy(int n,long d,out int x,out int y)
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
            rot(s,ref x,ref y,(int) rx,(int) ry);
            x += s * (int)rx;
            y += s * (int)ry;
            t /= 4;
            s *= 2;
        }
    }

    /// <summary>
    /// read file
    /// </summary>
    Texture2D tex;
    Texture2D texid;
    Texture2D texas;
    Texture2D asc;
    Texture2D asn;
    Texture2D asl;
    Texture2D asnl;
    public RawImage tee;
    public RawImage teeas;
    public RawImage ascolor;
    int[,,] data;
    int[,,] ascdata;
    Dictionary<uint, uint> asnum;

    void readTextFile(string file_path, bool getid = false)
    {
        StreamReader inp_stm = new StreamReader(file_path);
        char []se = {' '};
        int side_length = 1024;
        //int count = 0;
        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            string[] strlist = inp_ln.Split(se, System.StringSplitOptions.RemoveEmptyEntries);
            uint ips = strlist[0].Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);
            uint ipe = strlist[1].Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);

            uint asn = uint.Parse(strlist[2]);
            uint ipnum = (ipe - ips);
            int red = getid ? ((int)asn / 256) : (int.Parse(strlist[3]) + 100) % 256;
            int green = getid ? ((int)asn % 256) : (int.Parse(strlist[4]) + 100) % 256;
            int blue = getid ? 0 : (int.Parse(strlist[5]) + 100) % 256;
            if (asn < 65536)
            {
                if (asnum.ContainsKey(asn))
                {
                    asnum[asn] += ipnum;
                }
                else
                {
                    asnum.Add(asn, ipnum);
                }

                int ax, ay;
                d2xy(256, asn, out ax, out ay);


                ascdata[ax, ay, 0] = red;
                ascdata[ax, ay, 1] = green;
                ascdata[ax, ay, 2] = blue;
            }


            ips = ips >> 12;
            ipe = ipe >> 12;
            for (uint i = ips; i < ipe; i++)
            {
                int x;
                int y;
                d2xy(side_length, i, out x, out y);
                //tex.SetPixel(x, y, new Color(red, green, blue));
                data[x, y, 0] = red;
                data[x, y, 1] = green;
                data[x, y, 2] = blue;
            }
        }

        inp_stm.Close();
    }

    float Mapper(float v)
    {
        return Mathf.Sqrt(Mathf.Log(v) * Mathf.Sqrt(v));
    }

    [Serializable]
    public class Location
    {
        public string country;
        public string name;
        public string lat;
        public string lng;
    }

    [Serializable]
    public class InfosCollection
    {
        public Location[] locations;
    }

    // Start is called before the first frame update
    void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("tmplocation");
        InfosCollection infoCollection = JsonUtility.FromJson<InfosCollection>(textAsset.text);
        Debug.Log(infoCollection.locations[0].lng);

        asnum = new Dictionary<uint, uint>();
        tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        texid = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        texas = new Texture2D(256, 256, TextureFormat.RGB24, false);
        asc = new Texture2D(256, 256, TextureFormat.RGB24, false);
        asn = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        asl = new Texture2D(256, 256, TextureFormat.RGB24, false);
        asnl = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        tee.texture = tex;
        teeas.texture = texas;
        ascolor.texture = asc;
        data = new int[1024, 1024,3];
        ascdata = new int[256, 256, 3];
        readTextFile("/Users/x-dt/workspace/Qinghua/newtransfered.txt");
        Debug.Log("----------------------");
        for(int i = 0; i < 1024; i++)
        {
            for(int j = 0; j < 1024; j++)
            {
                tex.SetPixel(i, j, new Color((float)data[i, j, 0] / (float)255, (float)data[i, j, 1] / (float)255, (float)data[i, j, 2] / (float)255));
                if (i >= 256 || j >= 256) continue;
                asc.SetPixel(i, j, new Color((float)ascdata[i, j, 0] / (float)255, (float)ascdata[i, j, 1] / (float)255, (float)ascdata[i, j, 2] / (float)255));
                texas.SetPixel(i, j, new Color(0, 0, 0));
                asn.SetPixel(i, j, new Color(0, 0, 0, 0));
                asnl.SetPixel(i, j, new Color32(0, 0, 0, 1));
            }
        }
        readTextFile("/Users/x-dt/workspace/Qinghua/newtransfered.txt", true);
        Debug.Log("----------------------");
        for (int i = 0; i < 1024; i++)
        {
            for (int j = 0; j < 1024; j++)
            {
                texid.SetPixel(i, j, new Color((float)data[i, j, 0] / (float)255, (float)data[i, j, 1] / (float)255, (float)data[i, j, 2] / (float)255));
                
            }
        }
        tex.Apply();
        float maxnum = 0;
        foreach (var i in asnum)
        {
            uint num = i.Value;
            //Debug.Log(num);
            maxnum = Mathf.Max(num, maxnum);
        }
        Debug.Log("Max = " + maxnum);
        maxnum = Mapper(maxnum);
        
        foreach (var i in asnum)
        {
            uint dd = i.Key;
            //if (dd >= 512 * 512) continue;
            float num = Mapper(i.Value);
            int x, y;
            d2xy(256, dd, out x, out y);
            Color co = new Color(num / maxnum, num / maxnum, num / maxnum);
            texas.SetPixel(x, y, co);
            asn.SetPixel(x, y, new Color32((byte)(i.Value >> 24), (byte)(i.Value >> 16), (byte)(i.Value >> 8), (byte)(i.Value)));
            asl.SetPixel(x, y, new Color((float.Parse(infoCollection.locations[x * 256 + y].lat) + 90.0f) / 180.0f, (float.Parse(infoCollection.locations[x * 256 + y].lng) + 180.0f) / 360.0f, 0.0f));
            //Debug.Log(co);
        }
        for (int x = 0; x < 256; x ++)
        {
            for (int y = 0; y < 256; y ++)
            {
                asnl.SetPixel(x, y, new Color32((byte)x, (byte)y, 0, 1));
            }
        }
        //texas.SetPixel(1,1,Color.white);
        texas.Apply();
        asc.Apply();

        Texture2D as1024 = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        //int[,,] asdata1024=new int[1024,1024,3];
        for (int i=0;i<1024;i++)
            for(int j = 0; j < 1024; j++)
            {
                as1024.SetPixel(i, j, texas.GetPixel(i/4,j/4));
            }
        as1024.Apply();
        //save as png
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/ip.png", bytes);
        byte[] bytesid = texid.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/ipid.png", bytesid);
        byte[] bytesas = texas.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/as.png", bytesas);
        byte[] bytesasc = asc.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/asc.png", bytesasc);
        byte[] bytesasn = asn.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/asn.png", bytesasn);
        byte[] bytesasl = asl.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/asl.png", bytesasl);
        byte[] bytesasnl = asnl.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/asnl.png", bytesasnl);
        byte[] bytesas1024 = as1024.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/as1024.png", bytesas1024);


        //generate a Hilbert curve image
        //2^orderh is side length 
        Texture2D texh = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        int orderh = 8;
        int sidelen = (int)Mathf.Pow(2, orderh);
        int totalpx = sidelen * sidelen;
        for (int i = 0; i < 1024; i++)
        {
            for (int j = 0; j < 1024; j++)
            {
                texh.SetPixel(i, j, Color.black);
            }
        }
        for(int i = 0; i < totalpx-1; i++)
        {
            int hx, hy;
            d2xy(sidelen, i, out hx, out hy);
            int hxn, hyn;
            d2xy(sidelen, i+1, out hxn, out hyn);
            int offset =(int) (0.5f / sidelen * 1024);
            hx = hx * 1024 / sidelen + offset;
            hxn = hxn * 1024 / sidelen + offset;
            hy = hy * 1024 / sidelen + offset;
            hyn = hyn * 1024 / sidelen + offset;
            int tmpx, tmpy;
            if (hx > hxn)
            {
                tmpx = hx;
                hx = hxn;
                hxn = tmpx;
            }
            if (hy > hyn)
            {
                tmpy = hy;
                hy = hyn;
                hyn = tmpy;
            }

            for(int ii = hx; ii <= hxn; ii++)
            {
                for(int jj = hy; jj <= hyn; jj++)
                {
                    texh.SetPixel(ii, jj, Color.white);
                }
            }
        }

        texh.Apply();
        byte[] texhb = texh.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/hcurve.png", texhb);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
