using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
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

    public bool isRegionFilterOn = false;
    public bool isASFilterOn = false;
    public bool isTypeFilterOn = false;

    private bool isShowPosOn = false;
    private bool isShowAttackOn = false;

    public bool[] asFilterFlag = new bool[65536];
    private IPPair[] ipPairs = new IPPair[1];

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
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    texas.SetPixel(i, j, new Color(1.0f, 0.0f, 0.0f));
                }
                else
                {
                    texas.SetPixel(i, j, asFilterFlag[i * 256 + j] ? new Color(0.1f, 0.0f, 0.0f) : new Color(1.0f, 0.0f, 0.0f));
                }
            }
        }
        texas.Apply();
        byte[] bytesas = texas.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/ashl.png", bytesas);
    }

    void MultipleFilters()
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
                        asFilterFlag[item.Value.ASNum] = true;
                        isHighlight = true;
                        // Highlight an IP

                        // Highlight an geo
                        float lat = item.Value.lat;
                        float lng = item.Value.lng;
                        GameObject newGeoPoint = Instantiate(GeoPointPrefab, new Vector3(lng / 180.0f * MapWidth, 0, lat / 90.0f * MapHeight), Quaternion.identity);
                        //newGeoPoint.transform.parent = GeoPointCollector.transform;
                    }
                }
            }
        }
        ModifyASHighlight(isHighlight);
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
                                CurveCal.AddLine(posA, posB, "MapCurve");

                                // Show in IP View
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
}
