using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

[Serializable]
public class RegionDetail
{
    public string country;
}

[Serializable]
public class RegionData
{
    public RegionDetail[] Regions;
}

class IPPair
{
    public string IPA;
    public string IPB;
    public IPPair(string a, string b)
    {
        IPA = a;
        IPB = b;
    }
}

class CurveLine
{
    public Vector3 PosA;
    public Vector3 PosB;
    public Vector3 GetPosT(float k)
    {
        return new Vector3((PosA.x * (1.0f - k) + PosB.x * k), 100.0f * (0.25f - (k - 0.5f) * (k - 0.5f)), (PosA.z * (1.0f - k) + PosB.z * k));
    }
    public CurveLine(Vector3 A, Vector3 B)
    {
        PosA = A;
        PosB = B;
    }
    public GameObject ParticlePathGO;
    public GameObject EffectA;
    public GameObject EffectB;
}

public class Filters : MonoBehaviour
{
    public Consts Consts;

    //public GameObject FilterRegion;
    //public GameObject FilterAS;
    //public GameObject FilterType;
    //public GameObject IPProxy;
    //private Dropdown dropdownRegion;
    //private Dropdown dropdownAS;
    //private Dropdown dropdownType;

    public GameObject GeoPointPrefab;
    public GameObject PosMarkerPrefab;
    private float ASWidth;
    private float ASHeight;
    private float MapWidth;
    private float MapHeight;
    private float IPWidth;
    private float IPHeight;

    public bool isRegionFilterOn = false;
    public bool isASFilterOn = false;
    public bool isTypeFilterOn = false;

    private bool isShowPosOn = false;

    public bool[] asFilterFlag = new bool[65536];
    private IPPair[] ipPairs = new IPPair[1];

    public Camera MapCamera;
    public Camera IPCamera;
    public Camera ASCamera;

    List<AttackInfo> AttackInfos;

    private List<CurveLine> ASCurveLines = new List<CurveLine>();
    private List<CurveLine> ASNavCurveLines = new List<CurveLine>();
    private List<CurveLine> MapCurveLines = new List<CurveLine>();
    private List<CurveLine> IPCurveLines = new List<CurveLine>();
    public GameObject CurveLinePrefab;
    public GameObject CurveLineParent;
    public GameObject AttackEffectPrefab;

    public InputField SearchBox;

    public GameObject ASGameObject;
    public ViewIP IPGameObject;

    RegionData ReadJsonFile(string filePath)
    {
        string path = Path.Combine(Application.dataPath, filePath);
        StreamReader sr = new StreamReader(path);
        string data = sr.ReadToEnd();

        RegionData regionData = JsonUtility.FromJson<RegionData>(data);

        return regionData;
    }

    // Start is called before the first frame update
    void Start()
    {
        ASWidth = Consts.ASSize.x;
        ASHeight = Consts.ASSize.y;
        MapWidth = Consts.MapSize.x;
        MapHeight = Consts.MapSize.y;
        IPWidth = IPGameObject.GetIPFullSize().x;
        IPHeight = IPGameObject.GetIPFullSize().y;

        //dropdownRegion = FilterRegion.GetComponent<Dropdown>();
        RegionData regionData = ReadJsonFile("UI/Config/RegionConfig.json");
        for (int i = 0; i < regionData.Regions.Length; i ++)
        {
            //dropdownRegion.options.Add(new Dropdown.OptionData(regionData.Regions[i].country));
        }

        //dropdownAS = FilterAS.GetComponent<Dropdown>();
        for (int i = 0; i < 65536; i++)
        {
            //dropdownAS.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        //dropdownType = FilterType.GetComponent<Dropdown>();
        for (int i = 0; i < 3; i++)
        {
            //dropdownType.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        // Fake ddos
        ipPairs[0] = new IPPair("89.151.0.0", "89.151.176.13");

        ModifyASHighlight(false);
        
        //搜索框加监听事件
        SearchBox.onEndEdit.AddListener(OnSearchBoxSubmit);

        IPProxy.instance.RegistAttackDataCallback((UnityEngine.Events.UnityAction) EnableAttacks);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAttacks();
    }

    // 发送搜索消息
    void OnSearchBoxSubmit(string key)
    {
        if(!string.IsNullOrEmpty(SearchBox.text))
        {
            IPProxy.instance.GetIpInfoByFilter(SearchBox.text);
        }
    }

    public void ShowPos()
    {
        isShowPosOn = !isShowPosOn;
        // Clear geopoints
        Destroy(GameObject.FindWithTag("PosPoint"));
        if (isShowPosOn)
        {
            string IP = "89.151.176.13";
            Dictionary<string, IpDetail> dictionary = IPProxy.instance.GetDictionary();
            string region;
            string asNumber;
            string type;
            foreach (var item in dictionary)
            {
                if (item.Value.IP == IP)
                {
                    region = item.Value.country;
                    asNumber = item.Value.ASNum.ToString();
                    type = "0";
                    // Show in AS View

                    // Show in Map View
                    float lat = item.Value.lat;
                    float lng = item.Value.lng;
                    GameObject newPosPoint = Instantiate(PosMarkerPrefab, new Vector3(lng / 180.0f * MapWidth, 0, lat / 90.0f * MapHeight), Quaternion.identity);
                    //newPosPoint.transform.parent = PosPointCollector.transform;
                    // Show in IP View

                }
            }
        }
    }

    void ModifyASHighlight(bool isHighlight)
    {
        
        Texture2D texas = new Texture2D(256, 256, TextureFormat.RGB24, false);

        for (int i = 0; i < 256; i ++)
        {
            for (int j = 0; j < 256; j ++)
            {
                if (!isHighlight)
                {
                    texas.SetPixel(i, j, new Color(0.0f, 1.0f, 0.0f));
                }
                else
                {
                    texas.SetPixel(i, j, asFilterFlag[i * 256 + j] ? new Color(1.0f, 0.0f, 0.0f) : new Color(0.0f, 1.0f, 0.0f));
                }
            }
        }
        texas.Apply();
        byte[] bytesas = texas.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/ashl.png", bytesas);
        ASGameObject.GetComponent<VisualEffect>().SetTexture("ashl", texas);
    }

    void MultipleFilters(bool isSelectedAS = false, int x = 0, int y = 0)
    {
        Dictionary<string, IpDetail> dictionary = IPProxy.instance.GetDictionary();
        //string region = dropdownRegion.options[dropdownRegion.value].text;
        //string asNumber = dropdownAS.options[dropdownAS.value].text;
        //string type = dropdownType.options[dropdownType.value].text;

        // Clear flags
        for (int i = 0; i < asFilterFlag.Length; i++)
        {
            asFilterFlag[i] = false;
        }
        ModifyASHighlight(false);

        // Clear geopoints
        Destroy(GameObject.FindWithTag("GeoPoint"));

        // Clear geo points

        bool isHighlight = false;
        foreach (var item in dictionary)
        {
            /*
            if ((isRegionFilterOn && (item.Value.country == region)) || !isRegionFilterOn)
            {
                if ((isASFilterOn && (item.Value.ASNum.ToString() == asNumber)) || !isASFilterOn)
                {
                    if ((isTypeFilterOn && (item.Value.country == type)) || !isTypeFilterOn)
                    {
                        // Highlight an as
                        if (isASFilterOn)
                        {
                            asFilterFlag[item.Value.ASNum] = true;
                            isHighlight = true;
                        }
                        // Highlight an IP


                        // Highlight an geo
                        if (isRegionFilterOn)
                        {
                            float lat = item.Value.lat;
                            float lng = item.Value.lng;
                            GameObject newGeoPoint = Instantiate(GeoPointPrefab, new Vector3(lng / 180.0f * MapWidth, 0, lat / 90.0f * MapHeight), Quaternion.identity);
                            //newGeoPoint.transform.parent = GeoPointCollector.transform;
                        }
                    }
                }
            }
            */
        }
        if (isSelectedAS)
        {
            asFilterFlag[x * 256 + y] = true;
            isHighlight = true;
        }
        ModifyASHighlight(isHighlight);
    }

    public void FilterBySearch()
    {
        isASFilterOn = true;
        MultipleFilters();
    }

    public void FilterBySelectedAS(int x, int y)
    {
        isASFilterOn = true;
        MultipleFilters(true, x, y);
    }

    public void FilterByRegion()
    {
        /*
        string region = dropdownRegion.options[dropdownRegion.value].text;
        Debug.Log("Filter by Region: " + region);
        if (dropdownRegion.value != 0)
        {
            isRegionFilterOn = true;
        }
        else
        {
            isRegionFilterOn = false;
        }
        MultipleFilters();
        */
    }

    public void FilterByAS()
    {
        /*
        string asNumber = dropdownAS.options[dropdownAS.value].text;
        if (dropdownAS.value != 0)
        {
            
            isASFilterOn = true;
        }
        else
        {
            isASFilterOn = false;
        }
        MultipleFilters();
        */
    }

    public void FilterByType()
    {
        /*
        string type = dropdownType.options[dropdownType.value].text;
        if (dropdownType.value != 0)
        {

            isTypeFilterOn = true;
        }
        else
        {
            isTypeFilterOn = false;
        }
        MultipleFilters();
        */
    }

    private void UpdateAttacks()
    {
        float asHeight = ASCamera.transform.position.y;
        float ipHeight = IPCamera.transform.position.y;
        float mapHeight = MapCamera.transform.position.y;

        float thicknessk = 50.0f;
        float radiusk = 140.0f;

        for (int i = 0; i < ASCurveLines.Count; i ++)
        {
            ASCurveLines[i].ParticlePathGO.GetComponent<ParticlePath>().Thickness = asHeight / thicknessk;
            /*
            GameObject GO = ASCurveLines[i].EffectA;
            for (int j = 0; j < GO.transform.childCount; j ++)
            {
                GO.transform.GetChild(j).localScale = new Vector3(asHeight / radiusk, asHeight / radiusk, 0.0f);
            }
            GO = ASCurveLines[i].EffectB;
            for (int j = 0; j < GO.transform.childCount; j++)
            {
                GO.transform.GetChild(j).localScale = new Vector3(asHeight / radiusk, asHeight / radiusk, 0.0f);
            }
            */
        }
        for (int i = 0; i < IPCurveLines.Count; i++)
        {
            IPCurveLines[i].ParticlePathGO.GetComponent<ParticlePath>().Thickness = ipHeight / thicknessk;
            /*
            GameObject GO = IPCurveLines[i].EffectA;
            for (int j = 0; j < GO.transform.childCount; j++)
            {
                GO.transform.GetChild(j).localScale = new Vector3(ipHeight / radiusk, ipHeight / radiusk, 0.0f);
            }
            GO = IPCurveLines[i].EffectB;
            for (int j = 0; j < GO.transform.childCount; j++)
            {
                GO.transform.GetChild(j).localScale = new Vector3(ipHeight / radiusk, ipHeight / radiusk, 0.0f);
            }
            */
        }
        for (int i = 0; i < MapCurveLines.Count; i++)
        {
            MapCurveLines[i].ParticlePathGO.GetComponent<ParticlePath>().Thickness = mapHeight / thicknessk;
            /*
            GameObject GO = MapCurveLines[i].EffectA;
            for (int j = 0; j < GO.transform.childCount; j++)
            {
                GO.transform.GetChild(j).localScale = new Vector3(mapHeight / radiusk, mapHeight / radiusk, 0.0f);
            }
            GO = MapCurveLines[i].EffectB;
            for (int j = 0; j < GO.transform.childCount; j++)
            {
                GO.transform.GetChild(j).localScale = new Vector3(mapHeight / radiusk, mapHeight / radiusk, 0.0f);
            }
            */
        }
        for (int i = 0; i < ASNavCurveLines.Count; i++)
        {
            ASNavCurveLines[i].ParticlePathGO.GetComponent<ParticlePath>().Thickness = asHeight / thicknessk;
        }
    }

    private void AddCurveLine(CurveLine CurveLineData, string tag, int layer)
    {
        GameObject CurveLine = Instantiate(CurveLinePrefab, Vector3.zero, Quaternion.identity);
        CurveLine.transform.SetParent(CurveLineParent.transform);
        CurveLine.tag = tag;
        CurveLine.layer = layer;
        CurveLine.GetComponent<ParticlePath>().Waypoints.Clear();
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.PosA);
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.GetPosT(0.15f));
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.GetPosT(0.3f));
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.GetPosT(0.5f));
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.GetPosT(0.7f));
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.GetPosT(0.85f));
        CurveLine.GetComponent<ParticlePath>().Waypoints.Add(CurveLineData.PosB);
        CurveLine.transform.position = CurveLineData.PosA;
        CurveLine.GetComponent<ParticlePath>().RemakeBezierPoints();
        CurveLineData.ParticlePathGO = CurveLine;
        /*
        GameObject EffectA = Instantiate(AttackEffectPrefab, CurveLineData.PosA, Quaternion.identity);
        GameObject EffectB = Instantiate(AttackEffectPrefab, CurveLineData.PosB, Quaternion.identity);
        for (int i = 0; i < EffectA.transform.childCount; i++)
        {
            EffectA.transform.GetChild(i).gameObject.layer = 11;
        }
        for (int i = 0; i < EffectB.transform.childCount; i++)
        {
            EffectB.transform.GetChild(i).gameObject.layer = 11;
        }
        CurveLineData.EffectA = EffectA;
        CurveLineData.EffectB = EffectB;
        */
    }


    private void AppendLines()
    {
        // Add curvelines on ip view

        for (int i = 0; i < IPCurveLines.Count; i++)
        {
            AddCurveLine(IPCurveLines[i], "IPCurve", 11);
        }

        // Add curvelines on map view

        for (int i = 0; i < MapCurveLines.Count; i ++)
        {
            AddCurveLine(MapCurveLines[i], "MapCurve", 12);
        }

        // Add curvelines on as view

        for (int i = 0; i < ASCurveLines.Count; i++)
        {
            AddCurveLine(ASCurveLines[i], "ASCurve", 10);
        }

        for (int i = 0; i < ASNavCurveLines.Count; i++)
        {
        }
    }

    void EnableAttacks()
    {
        AttackInfos = IPProxy.instance.GetAttackInfo();
        ShowAttacks();
        Debug.Log("Attack Info Fetched");
    }

    public void ShowAttacks()
    {
        int asNumberA, asNumberB;
        float latA, latB;
        float lngA, lngB;

        for (int i = 0; i < /*AttackInfos.Count*/30; i ++)
        {
            for (int j = 0; j < AttackInfos[i].srcInfo.Count; j ++)
            {
                IpDetail item = AttackInfos[i].srcInfo[j].srcIpInfo;
                IpDetail itemB = AttackInfos[i].destIpInfo;
                {
                    asNumberA = (int)item.ASNum;
                    latA = item.lat;
                    lngA = item.lng;
                    {
                        asNumberB = (int)itemB.ASNum;
                        latB = itemB.lat;
                        lngB = itemB.lng;

                        // Show in AS View
                        int asX = 0;
                        int asY = 0;
                        d2xy(256, asNumberA, out asX, out asY);
                        Vector3 posA = new Vector3(asX / 256.0f * ASHeight, 0.0f, asY / 256.0f * ASWidth);
                        d2xy(256, asNumberB, out asX, out asY);
                        Vector3 posB = new Vector3(asX / 256.0f * ASHeight, 0.0f, asY / 256.0f * ASWidth);
                        ASCurveLines.Add(new CurveLine(posA, posB));

                        // Show in AS Navigation View

                        //ASNavCurveLines.Add(new CurveLine(posA, posB));

                        // Show in Map View
                        posA = new Vector3(lngA / 180.0f * MapWidth, 0, latA / 90.0f * MapHeight);
                        posB = new Vector3(lngB / 180.0f * MapWidth, 0, latB / 90.0f * MapHeight);
                        MapCurveLines.Add(new CurveLine(posA, posB));

                        // Show in IP View
                        int length = 1 << 16;
                        uint ips = item.IP.Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);
                        ips = ips;
                        int xs = 0;
                        int ys = 0;
                        d2xy(length, ips, out xs, out ys);
                        uint ipe = itemB.IP.Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);
                        ipe = ipe;
                        int xe = 0;
                        int ye = 0;
                        d2xy(length, ipe, out xe, out ye);
                        posA = new Vector3(xs * 1.0f / length * IPWidth, 0, ys * 1.0f / length * IPHeight);
                        posB = new Vector3(xe * 1.0f / length * IPWidth, 0, ye * 1.0f / length * IPHeight);
                        IPCurveLines.Add(new CurveLine(posA, posB));
                    }
                }
            }

        }
        AppendLines();
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
}
