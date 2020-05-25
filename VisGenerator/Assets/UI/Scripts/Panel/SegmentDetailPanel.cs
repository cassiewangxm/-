using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RTG;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SegmentDetailPanel : UIBasePanel, IPointerClickHandler
{
    [SerializeField]
    private Text m_TimeText;
    [SerializeField]
    private RawImage m_IPMap;
    [SerializeField]
    private RectTransform m_validClickRect;
    [SerializeField]
    private IPDetailPanel m_IpDetailPanel;
    [SerializeField]
    private RawImage m_focusMark;
    [SerializeField]
    private UIline m_line;

    private Texture2D m_texture;
    private ASSegmentInfo m_segmentData;
    private int m_originalWidth;
    private Vector2Int m_originalTexSize;
    private int m_maxWidth;
    private int m_curLevel;
    private int m_maxLevel;
    //private int //m_IpBlockWith = 1;
    private int m_hCount = 1;
    private int m_vCount = 1;
    private bool m_lastIpShowFinished = true;
    void Awake()
    {
        m_originalTexSize = new Vector2Int((int)m_validClickRect.rect.width, (int)m_validClickRect.rect.height);
        Debug.Log(m_validClickRect.rect+" .......... ");
        m_texture = new Texture2D(m_originalTexSize.x, m_originalTexSize.y); //((int)m_originalWidth, (int)m_originalWidth);
        m_IPMap.texture = m_texture;
    }

    void OnDisable()
    {
        if(RTFocusCamera.Get != null)
        {
            RTFocusCamera.Get.LookAroundSettings.IsLookAroundEnabled = true;
            RTFocusCamera.Get.ZoomSettings.IsZoomEnabled = true;
            RTFocusCamera.Get.PanSettings.IsPanningEnabled = true;
        }
    }
    public void SetUIData(ASSegmentInfo info)
    {
        m_lastIpShowFinished = true;
        m_segmentData = info;
        m_curLevel = 1;
        int pixelsize = (int)Mathf.Sqrt((float)m_originalTexSize.x * m_originalTexSize.y / info.IPCount); 
        //int maxp = info.IPCount * 16;
        //int maxw = Mathf.CeilToInt( Mathf.Sqrt(info.IPCount) ) * 16;
        //m_maxLevel = Mathf.CeilToInt((float)maxw / m_originalWidth);
        m_maxLevel = Mathf.CeilToInt(16.0f/pixelsize);
        m_maxWidth = pixelsize > 16 ? m_originalTexSize.x : (int)(16.0f/pixelsize*m_originalTexSize.x);//m_maxLevel * m_originalTexSize.x;//m_originalWidth;

        Debug.LogFormat("IPcount : {0} ; MaxWidth : {1}; pixelsize : {2}; MaxLevel : {3}", info.IPCount, m_maxWidth,pixelsize, m_maxLevel);

        Clean();
        StopAllCoroutines();
        //StartCoroutine(GenerateTextureByLevel(m_curLevel));
        GenerateTextureByLevel(m_curLevel);
        UpdateUI();
        //m_IpBlockWith = (int)(m_IPMap.rectTransform.rect.width / m_hCount);

        RTFocusCamera.Get.LookAroundSettings.IsLookAroundEnabled = false;
        RTFocusCamera.Get.ZoomSettings.IsZoomEnabled = false;
        RTFocusCamera.Get.PanSettings.IsPanningEnabled = false;
    }

    void GenerateTextureByLevel(int level)
    {
        int width = level * m_originalTexSize.x;
        width = Mathf.Min(m_maxWidth, width);
        int height = (int)((float)width * (float)m_originalTexSize.y / m_originalTexSize.x);
        if(m_texture.width != width)
        {
            m_texture.Resize(width, height);
        }

        int pixelsize = (int)Mathf.Sqrt( (width * height) / m_segmentData.IPCount);
        pixelsize = pixelsize > 0 ? pixelsize : 1;
        int lineCount = width/pixelsize;
        int rowCount = height/pixelsize;
        m_hCount = lineCount;
        m_vCount = rowCount;
        Debug.LogFormat("IPMap Resize (width , height, pixel, level) = ({0},{1},{2},{3}) ", width, height, pixelsize, level);

        int curCount = 0;
        for(int i = 0; i < width; i++)
        {  
            for(int j = 0; j < height; j++)
            {
                if(j/pixelsize >= rowCount || i/pixelsize >= lineCount)
                {
                    m_texture.SetPixel(i, j, Color.black);
                }
                else
                {
                    curCount = j/pixelsize * lineCount + i/pixelsize;
                    m_texture.SetPixel(i, j, m_segmentData.GetIPColor(curCount));
                    //Debug.LogFormat("({0},{1}) : {2}", i, j, m_segmentData.GetIPColor(curCount));
                }
            }

            // if(curCount % 1024 == 0)
            // {
            //     m_texture.Apply();
            //     yield return null;
            // }
        }
        m_texture.Apply();
        //yield return 0;
    }

    public void UpdatePos(Vector2 screenPos)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 offset = screenPos - new Vector2(Screen.width/2, Screen.height/2);

        if(offset.x > 0)
            offset.x -= rect.sizeDelta.x/2;
        else
            offset.x += rect.sizeDelta.x/2;
        
        //越界检测
        if(offset.x - rect.sizeDelta.x/2 < - Screen.width/2)
            offset.x = rect.sizeDelta.x/2 - Screen.width/2;
        if(offset.x + rect.sizeDelta.x/2 > Screen.width/2)
            offset.x = Screen.width/2 - rect.sizeDelta.x/2;
        if(offset.y - rect.sizeDelta.y/2 < - Screen.height/2)
            offset.y = rect.sizeDelta.y/2 - Screen.height/2;
        if(offset.y + rect.sizeDelta.y/2 > Screen.height/2)
            offset.y = Screen.height/2 - rect.sizeDelta.y/2;
        
        rect.anchoredPosition = offset;
    }

    private void UpdateUI()
    {
        if (m_segmentData == null) 
            return;
        m_IPMap.rectTransform.sizeDelta = new Vector2(m_originalTexSize.x, m_originalTexSize.y);//(m_originalWidth, m_originalWidth);
        m_focusMark.gameObject.SetActive(false);
        HideIpDetail();
        m_TimeText.text = m_segmentData.Time;
    }

    private void Clean()
    {
        m_TimeText.text = string.Empty;
    }

    void Update()
    {
        if(Input.mouseScrollDelta != Vector2.zero)
        {
            HideFocusMark();
            HideIpDetail();

            //Debug.Log(Input.mouseScrollDelta); 
            Vector2 targetSize = m_IPMap.rectTransform.sizeDelta * (1.0f + Input.mouseScrollDelta.y / 100);
            float scale = targetSize.x / m_originalTexSize.x;
            //Debug.Log(scale+" , "+m_curLevel);
            if(scale >= m_curLevel + 1 && m_curLevel < m_maxLevel)
            {
                m_curLevel ++;
                StopAllCoroutines();
                //StartCoroutine(GenerateTextureByLevel(m_curLevel));
                GenerateTextureByLevel(m_curLevel);
            }
            else if(scale <= m_curLevel - 1 && m_curLevel > 1)
            {
                m_curLevel --;
                StopAllCoroutines();
                //StartCoroutine(GenerateTextureByLevel(m_curLevel));
                GenerateTextureByLevel(m_curLevel);
            }

            if(targetSize.x <= m_maxWidth && targetSize.x >= m_originalTexSize.x)
            {
                m_IPMap.rectTransform.sizeDelta = targetSize;
                //m_IpBlockWith = (int)(targetSize.x / m_hCount);
            }
        }
    }
    public void OnPointerClick(PointerEventData data)
    {
        if(!m_lastIpShowFinished)
        {
            ShowWarningOnTitle(true);
            return;
        }
        if(CheckClickPos(data.position, m_validClickRect) && CheckClickPos(data.position, m_IPMap.rectTransform))
        {
            m_lastIpShowFinished = false;
            Vector2 offset = GetOffset(data.position, m_IPMap.rectTransform);

            int y = (int)(offset.y/m_IPMap.rectTransform.rect.height * m_vCount);
            int x = (int)(offset.x/m_IPMap.rectTransform.rect.width * m_hCount);

            //需要判断x y 越界
            int index = y * m_hCount + x;

            UpdateMarkPos(x,y, new Vector2(data.position.x - m_IPMap.rectTransform.position.x, data.position.y - m_IPMap.rectTransform.position.y));

            Debug.LogFormat("Clicked IP : {0},{1} ; {2}/{3}", x, y, index, m_segmentData.IPCount);

            HideIpDetail();
            ASProxy.instance.GetSegmentIPInfo(m_segmentData, index, ShowIPDetail);
        }
        
    }
    void UpdateMarkPos(int x, int y, Vector2 pos)
    {
        Vector2 start = new Vector2(-m_IPMap.rectTransform.pivot.x * m_IPMap.rectTransform.rect.width, -m_IPMap.rectTransform.pivot.y * m_IPMap.rectTransform.rect.height);
        Debug.Log(start);
        m_focusMark.gameObject.SetActive(true);
        m_focusMark.rectTransform.localPosition = new Vector3(pos.x, pos.y, 0);//new Vector3((x + 0.5f) * //m_IpBlockWith + start.x, (y + 0.5f) * //m_IpBlockWith + start.y, 0);
    }

    void ShowWarningOnTitle(bool warning)
    {
        if(warning)
        {
            m_TimeText.text = m_segmentData.Time + " <color=#f7cd46ff>(Waiting for net response !!!)</color>";
        }
        else
        {
            m_TimeText.text = m_segmentData.Time;
        }
    }

    public void HideFocusMark()
    {
        if(m_focusMark.gameObject.activeSelf)
            m_focusMark.gameObject.SetActive(false);
    }

    public void HideIpDetail()
    {
        m_IpDetailPanel.gameObject.SetActive(false);
        m_line.gameObject.SetActive(false);
    }

    void ShowIPDetail(IpDetail ipDetail)
    {
        if(!gameObject.activeSelf)
            return;
            
        ShowWarningOnTitle(false);
        m_lastIpShowFinished = true;
        if(m_IpDetailPanel != null && ipDetail != null)
        {
            float offsetx = 300;
            float offsety = 100;
            if(m_focusMark.transform.position.x > Screen.width/2)
                offsetx = -offsetx;
            if(m_focusMark.transform.position.y > Screen.height/2)
                offsety = -offsety;

            m_IpDetailPanel.transform.position = new Vector3(m_focusMark.transform.position.x + offsetx,m_focusMark.transform.position.y + offsety, 0);
            m_IpDetailPanel.SetUIData(ipDetail);
            m_IpDetailPanel.gameObject.SetActive(true);

            m_line.gameObject.SetActive(true);
            m_line.SetTarget(m_IpDetailPanel.GetLineEndPos(offsetx > 0), m_focusMark.transform.position);
        }
    }

    bool CheckClickPos(Vector2 pos, RectTransform rect)
    {
        Vector2 offset = GetOffset(pos, rect);
        
        if(offset.x >= 0 && offset.y >= 0 && offset.x <= rect.rect.width && offset.y <= rect.rect.height)
        {
            return true;
        }
        return false;
    }

    Vector2 GetOffset(Vector2 pos, RectTransform rect)
    {
        float x = pos.x - (rect.position.x - rect.pivot.x * rect.rect.width);
        float y = pos.y - (rect.position.y - rect.pivot.y * rect.rect.height);

        return new Vector2(x,y);
    }

}
