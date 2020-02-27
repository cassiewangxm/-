using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    public CurveLine(Vector3 A, Vector3 B)
    {
        PosA = A;
        PosB = B;
    }
}

public class Filters : MonoBehaviour
{
    public GameObject FilterRegion;
    public GameObject FilterAS;
    public GameObject FilterType;
    public GameObject IPProxy;
    private Dropdown dropdownRegion;
    private Dropdown dropdownAS;
    private Dropdown dropdownType;

    public test CurveCal;

    public GameObject GeoPointPrefab;
    public GameObject PosMarkerPrefab;
    public float MapWidth;
    public float MapHeight;
    public float IPHalfWidth;
    public float IPHalfHeight;

    public bool isRegionFilterOn = false;
    public bool isASFilterOn = false;
    public bool isTypeFilterOn = false;

    private bool isShowPosOn = false;
    private bool isShowAttackOn = false;

    public bool[] asFilterFlag = new bool[65536];
    private IPPair[] ipPairs = new IPPair[1];

    public Camera MapCamera;
    public Camera IPCamera;

    private List<CurveLine> MapCurveLines = new List<CurveLine>();
    private List<CurveLine> IPCurveLines = new List<CurveLine>();

    public InputField SearchBox;

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
        dropdownRegion = FilterRegion.GetComponent<Dropdown>();
        RegionData regionData = ReadJsonFile("UI/Config/RegionConfig.json");
        for (int i = 0; i < regionData.Regions.Length; i ++)
        {
            dropdownRegion.options.Add(new Dropdown.OptionData(regionData.Regions[i].country));
        }

        dropdownAS = FilterAS.GetComponent<Dropdown>();
        for (int i = 0; i < 65536; i++)
        {
            dropdownAS.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        dropdownType = FilterType.GetComponent<Dropdown>();
        for (int i = 0; i < 3; i++)
        {
            dropdownType.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        // Fake ddos
        ipPairs[0] = new IPPair("89.151.0.0", "89.151.176.13");

        ModifyASHighlight(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAttacks();
    }

    public void ShowPos()
    {
        isShowPosOn = !isShowPosOn;
        // Clear geopoints
        Destroy(GameObject.FindWithTag("PosPoint"));
        if (isShowPosOn)
        {
            string IP = "89.151.176.13";
            Dictionary<string, IpDetail> dictionary = IPProxy.GetComponent<IPProxy>().GetDictionary();
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
    }

    void MultipleFilters(bool isSelectedAS = false, int x = 0, int y = 0)
    {
        Dictionary<string, IpDetail> dictionary = IPProxy.GetComponent<IPProxy>().GetDictionary();
        string region = dropdownRegion.options[dropdownRegion.value].text;
        string asNumber = dropdownAS.options[dropdownAS.value].text;
        string type = dropdownType.options[dropdownType.value].text;

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
        }
        // as test
        {
            for (int i = 0; i < 256; i ++)
                asFilterFlag[128 * 256 + i] = true;
            isHighlight = true;
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
    }

    public void FilterByAS()
    {
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
    }

    public void FilterByType()
    {
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
    }

    float CalculateCurveLineThickness(Camera camera)
    {
        return camera.transform.position.y / 5.0f;
    }

    void UpdateAttacks()
    {
        if (isShowAttackOn)
        {
            CurveCal.DeleteLines();

            // Add curvelines on ip view
            float thickness = CalculateCurveLineThickness(IPCamera);

            for (int i = 0; i < IPCurveLines.Count; i++)
            {
                CurveCal.AddLine(IPCurveLines[i].PosA, IPCurveLines[i].PosB, "IPCurve", thickness);
            }

            // Add curvelines on map view
            thickness = CalculateCurveLineThickness(MapCamera);

            for (int i = 0; i < MapCurveLines.Count; i ++)
            {
                CurveCal.AddLine(MapCurveLines[i].PosA, MapCurveLines[i].PosB, "MapCurve", thickness);
            }
        }
    }

    public void ShowAttacks()
    {
        isShowAttackOn = !isShowAttackOn;
        if (isShowAttackOn)
        {
            Dictionary<string, IpDetail> dictionary = IPProxy.GetComponent<IPProxy>().GetDictionary();
            string asNumberA, asNumberB;
            float latA, latB;
            float lngA, lngB;

            for (int i = 0; i < ipPairs.Length; i++)
            {
                foreach (var item in dictionary)
                {
                    if (item.Value.IP == ipPairs[i].IPA)
                    {
                        asNumberA = item.Value.ASNum.ToString();
                        latA = item.Value.lat;
                        lngA = item.Value.lng;
                        foreach (var itemB in dictionary)
                        {
                            if (itemB.Value.IP == ipPairs[i].IPB)
                            {
                                asNumberB = itemB.Value.ASNum.ToString();
                                latB = itemB.Value.lat;
                                lngB = itemB.Value.lng;

                                // Show in AS View

                                // Show in Map View
                                Vector3 posA = new Vector3(lngA / 180.0f * MapWidth, 0, latA / 90.0f * MapHeight);
                                Vector3 posB = new Vector3(lngB / 180.0f * MapWidth, 0, latB / 90.0f * MapHeight);
                                MapCurveLines.Add(new CurveLine(posA, posB));

                                // Show in IP View
                                uint ips = item.Value.IP.Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);
                                ips = ips >> 12;
                                int xs = 0;
                                int ys = 0;
                                d2xy(256, ips, out xs, out ys);
                                xs -= 128;
                                ys -= 128;
                                uint ipe = itemB.Value.IP.Split('.').Select(uint.Parse).Aggregate((a, b) => a * 256 + b);
                                ipe = ipe >> 12;
                                int xe = 0;
                                int ye = 0;
                                d2xy(256, ipe, out xe, out ye);
                                xe -= 128;
                                ye -= 128;
                                posA = new Vector3(xs / 256.0f * IPHalfWidth, 0, ys / 256.0f * IPHalfHeight);
                                posB = new Vector3(xe / 256.0f * IPHalfWidth, 0, ye / 256.0f * IPHalfHeight);
                                IPCurveLines.Add(new CurveLine(posA, posB));
                            }
                        }
                    }
                }

            }
        }
        else
        {
            CurveCal.DeleteLines();
        }
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
