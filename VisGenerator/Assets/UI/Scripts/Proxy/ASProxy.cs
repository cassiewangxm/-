using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASDetail
{
    public uint Number; //AS序号
    public Vector2Int Location; //AS坐标
    public string OrgName; //AS名称
    public ASSegment[] Segments; //AS区段数组

    public void FakeInit(int x, int y, int segNum)
    {
        Number = (uint)Random.Range(1,99999);
        Location = new Vector2Int(x,y);
        OrgName = "name";
        
        segNum = segNum > 0 ? segNum : 1;

        if(segNum > 0)
        {
            Segments = new ASSegment[segNum];
            for(int i = 0; i < segNum; i++)
            {
                Segments[i] = new ASSegment();
            }
        }
    }
}

public class ASSegment
{
    public string BornDate; //分配时间
    public string HeadIP;   //IP段起始IP  ？？
    public string TailIP;   //IP段终止IP  ？？
    public uint IPCount;    //IP数量
    public string[] IPList; //IP数组

    public ASSegment()
    {
        FakeInit();
    }

    //制造假数据
    void FakeInit()
    {
        BornDate = Random.Range(1990,2019).ToString();
        IPCount = (uint)Random.Range(33,233);
        IPList = new string[IPCount];
        for(int i =0; i<IPList.Length; i++)
        {
            IPList[i] = Random.Range(16,255).ToString()+"."+Random.Range(0,155).ToString()+"."+Random.Range(0,133).ToString()+"."+Random.Range(0,255).ToString();;
        }
    }

    //根据ip在数组中的index获取ip值，生成color
    public Color GetIPColor(int index)
    {
        if(index < IPCount)
        {
            string[] strlist = IPList[index].Split('.');
            return new Color(int.Parse(strlist[0])/255.0f, int.Parse(strlist[1])/255.0f, int.Parse(strlist[2])/255.0f, int.Parse(strlist[3])/255.0f);
        }
        return Color.black;
    }
}

public class ASProxy : MonoBehaviour
{
    private static ASProxy s_instance;
    public static ASProxy instance
    {
        get {
            if(s_instance == null)
            {
                s_instance = Instantiate(new GameObject("IPProxy")).AddComponent<ASProxy>();

            }
            return s_instance;
        }
    }

    void Awake()
    {
        if(s_instance == null)
            s_instance = this;
    }

    public void GetASByPosition(int x, int y, float height, out ASDetail asDetail)
    {
        asDetail = new ASDetail();
        asDetail.FakeInit(x,y,(int)height);
    }
}
