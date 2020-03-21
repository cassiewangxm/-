using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASPool : MonoBehaviour
{
    public static ASPool Instance
    {
        get {
                if(_applicationIsQuitting)
                    return null;
                    
                if(m_instance == null)
                {
                    m_instance = new GameObject("ASPool").AddComponent<ASPool>();
                }
                return m_instance;
            }
    }

    public static bool _applicationIsQuitting = false;

    private int m_cacheCount = 200;
    private static ASPool m_instance;
    private Stack<SingleASV2> m_FreeASStack = new Stack<SingleASV2>();
    private GameObject m_prefab;

    void Awake()
    {
        _applicationIsQuitting = false;
        Debug.Log("On ASPool's Awake....");
    }
    void OnDestroy()
    {
        if(m_instance != null)
            _applicationIsQuitting = true;
        Debug.Log("On ASPool's Destroy...." + m_instance);
    }

    public void Prepare()
    {
        m_prefab = Resources.Load("ViewSingleAS") as GameObject;
        transform.position = new Vector3(0,1000,0);
        StartCoroutine(CreateASCubes());
    }

    IEnumerator CreateASCubes()
    {
        for(int i = 0; i < m_cacheCount; i++)
        {
            SingleASV2 a = Instantiate(m_prefab, transform).GetComponent<SingleASV2>();
            a.IsInUse = false;
            m_FreeASStack.Push(a);
           // counter++;
            yield return 0;
        }
    }

    public SingleASV2 GetASSave()
    {
        if(m_FreeASStack.Count>0)
            return m_FreeASStack.Pop();
        
        SingleASV2 a = Instantiate(m_prefab, transform).GetComponent<SingleASV2>();
        a.IsInUse = true;
        //    counter++;

        return a;
    }

    public void ReturnBackAS(SingleASV2 a)
    {
        //a.transform.SetParent(transform);
        a.transform.position = transform.position;
        a.IsInUse = false;
        a.ClearFilterMode();
        a.AppearRoot = null;
        m_FreeASStack.Push(a);
    }

    // int counter = 0;
    // void OnGUI()
    // {
    //     GUIStyle s = new GUIStyle();
    //     s.fontSize = 36;
    //     s.normal.textColor = Color.yellow;
    //     GUI.Label(new Rect(100,200,200,100), string.Format("{0}/{1}", m_FreeASStack.Count, counter),s);
    // }
}
