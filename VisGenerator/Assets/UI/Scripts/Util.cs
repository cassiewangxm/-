using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Util
{
    public static readonly int IP_STRIDE = 1024;
    public static readonly int AS_STRIDE = 256;

    public static void rot(int n, ref int x, ref int y, int rx, int ry)
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

    public static long xy2d(int n, int x, int y)
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
    public static void d2xy(int n, long d, out int x, out int y)
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

    public static uint IP2Uint(string ip)
    {
        uint ipUint = ip.Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);
        return ipUint;
    }

    public static void IP2WorldPos(string ip, out Vector2 position)
    {
        IpDetail ipDetail = IPProxy.instance.GetIpDetail(ip);
        position = -1*Vector2.one;
        if (ipDetail == null)
        {
            Debug.LogErrorFormat("could not find ip {0}", ip);
            return;
        }

        position = ipDetail.MapCoordinate;
    }

    public static void IP2ASPos(string ip, out Vector2 position)
    {
        IpDetail ipDetail = IPProxy.instance.GetIpDetail(ip);
        position = -1 * Vector2.one;
        if (ipDetail == null)
        {
            Debug.LogErrorFormat("could not find ip {0}", ip);
            return;
        }

        int asNum = ipDetail.ASNum;
        int x, y;
        d2xy(AS_STRIDE, asNum, out x, out y);
        position.x = x;
        position.y = y;
    }

    public static void IP2IPPos(string ip, out Vector2 position)
    {
        position = -1 * Vector2.one;

        uint ipUint = IP2Uint(ip);
        int x, y;
        d2xy(IP_STRIDE, ipUint, out x, out y);
        position.x = x;
        position.y = y;
    }
}
