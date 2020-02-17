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

public class Filters : MonoBehaviour
{
    public GameObject FilterRegion;
    public GameObject FilterAS;
    public GameObject FilterType;
    public GameObject IPProxy;
    private Dropdown dropdownRegion;
    private Dropdown dropdownAS;
    private Dropdown dropdownType;

    public GameObject GeoPointCollector;
    public GameObject PosPointCollector;
    public GameObject GeoPointPrefab;
    public GameObject PosMarkerPrefab;
    public float MapWidth;
    public float MapHeight;

    public bool isRegionFilterOn = false;
    public bool isASFilterOn = false;
    public bool isTypeFilterOn = false;

    public bool[] asFilterFlag = new bool[65536];

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPos()
    {
        // Clear geopoints
        Destroy(GameObject.FindWithTag("PosPoint"));

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

        // Clear geopoints
        Destroy(GameObject.FindWithTag("GeoPoint"));

        // Clear geo points

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
}
