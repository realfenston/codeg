<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchData
{
    public bool IsPlayerSkinned;
    public List<Vector3> mTeam1 = new List<Vector3>();
    public List<Vector3> mTeam2 = new List<Vector3>();
    float orthographicSize = 5.0f;
    float orthographicHeightSize = 3.0f;

    public int PlayerCount = 11;
    readonly float StadiumWidth = 54.47f;
    readonly float StadiumHeight = 34.26f;
    Dictionary<int, GameObject> playersDic = new Dictionary<int, GameObject>();
    public void SetOrthographicSize(float size)
    {
        orthographicSize = size;
        orthographicHeightSize = size * (StadiumHeight / StadiumWidth);
    }

    Vector3 world2Camera(Vector3 wPos)
    {
        Vector3 cPos = Vector3.zero;

        cPos.x = wPos.x / StadiumWidth * orthographicSize;
        cPos.y = wPos.z / StadiumHeight * orthographicHeightSize;
        cPos.z = 1;

        return cPos;
    }

    public void SetPlayerData(Dictionary<int, GameObject> players)
    {
        playersDic = players;
    }

    public void GetPlayerPos()
    {
        mTeam1.Clear();
        mTeam2.Clear();

        foreach(var item in playersDic)
        {
            Vector3 pos;
            if (IsPlayerSkinned)
            {
                pos = item.Value.transform.Find("Dummy<fim_suffix>Value.transform.position;
            }
            var cPos = world2Camera(pos);
            if (item.Key < 11)
            {
                mTeam1.Add(cPos);
            }
            else
            {
                mTeam2.Add(cPos);
            }
        }        
    }
}

public class MatchHeatMapControl : MonoBehaviour
{
    public bool IsPlayerSkinned;
    List<GameObject> mTeam1 = new List<GameObject>();
    List<GameObject> mTeam2 = new List<GameObject>();

    public MatchData md = new MatchData();
    

    List<GameObject> pool = new List<GameObject>();
    int objCount = 0;

    Color TagColor1 = new Color(1.0f, 0.0f, 0.0f);
    Color TagColor2 = new Color(0.0f, 1.0f, 0.0f);

    Material mat1;
    Material mat2;
    GameObject GetObject()
    {
        int index = objCount;
        GameObject go = null;
        if (index < pool.Count)
        {
            objCount++;
            go = pool[index];
            go.SetActive(true);
        }
        else
        {
            var childTran = gameObject.transform.GetChild(0);
            if (childTran!= null)
            {
                GameObject newObj = GameObject.Instantiate(childTran.gameObject);
                newObj.name = "HeatQuad";
                newObj.transform.SetParent(gameObject.transform);
                pool.Add(newObj);
                objCount = pool.Count;
                go = newObj;
                go.SetActive(true);
            }
        }

        return go;
    }

    Material GetMat(bool bRed)
    {
        if (mat1 == null || mat2 == null)
        {
            var childTran = gameObject.transform.GetChild(0);
            if (childTran!= null)
            {
                var mr = childTran.gameObject.GetComponent<MeshRenderer>();
                if(mr!= null)
                {
                    mat1 = new Material(mr.sharedMaterial);
                    mat1.name = "BlendMaterial1";
                    mat2 = new Material(mr.sharedMaterial);
                    mat2.name = "BlendMaterial2";

                    mat1.SetColor("_TagCol", TagColor1);
                    mat2.SetColor("_TagCol", TagColor2);
                }
            }
        }

        if (bRed)
            return mat1;

        return mat2;
    }

    public bool inVisualizeView = false;

    void Update()
    {
        if (!inVisualizeView) return;
        md.GetPlayerPos();
        var camera = GetComponent<Camera>();
        if(camera!= null)
        {
            md.SetOrthographicSize(camera.orthographicSize);
        }

        objCount = 0;
        foreach (var pos in md.mTeam1)
        {
            var go = GetObject();
            go.GetComponent<MeshRenderer>().sharedMaterial = GetMat(true);
            go.transform.localPosition = pos;
        }

        foreach (var pos in md.mTeam2)
        {
            var go = GetObject();
            go.GetComponent<MeshRenderer>().sharedMaterial = GetMat(false);
            go.transform.localPosition = pos;
        }
    }
}
<fim_middle>").transform.position;
            }
            else
            {
                pos = item.<|endoftext|>