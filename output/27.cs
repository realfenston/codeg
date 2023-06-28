<fim_prefix>﻿using UnityEngine;

public class Snow3DView : View3DBase
{
    #region Field

    /// <summary>
    /// 降雪数量。
    /// </summary>
    private int SNOW_NUM = 1500;  //10000;

    /// <summary>
    /// 雪頂点。
    /// </summary>
    private Vector3[] snowMeshVertices;

    /// <summary>
    /// 雪三角形。
    /// </summary>
    private int[] snowMeshTriangleIndexes;

    /// <summary>
    /// 雪 UV.
    /// </summary>
    private Vector2[] snowMeshUvs;

    private float range;
    private float rangeR;
    private Vector3 move = Vector3.zero;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    #endregion Field

    public override async GTask Awake()
    {
        await base.Awake();

        this.meshFilter = transform.GetComponent<MeshFilter>();
        this.meshRenderer = transform.GetComponent<MeshRenderer>();

        CreateSnowMesh();
        
        
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        this.meshFilter = null;
        this.meshRenderer = null;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GalaRenderPipeline.GalaRenderManager.AddEventListener("QualityChanged", OnQualityChanged);
        OpenLateUpdate();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        GalaRenderPipeline.GalaRenderManager.RemoveEventListener("QualityChanged", OnQualityChanged);
        CloseLateUpdate();
    }

    private void OnQualityChanged()
    {
        CreateSnowMesh();
    }

    private void SetSnowNum()
    {
        int quality = SettingConfig.CurQualityLevel;
        switch(quality)
        {
            case 1:
            case 2:
                SNOW_NUM = 300;
                break;
            case 3:
                SNOW_NUM = 800;
                break;
            case 4:
                SNOW_NUM = 1000;
                break;
            case 5:
                SNOW_NUM = 1200;
                break;
            case 6:
                SNOW_NUM = 1500;
                break;
        }
    }
    private void CreateSnowMesh()
    {
        if(this.meshFilter == null) return;
        
        SetSnowNum();<fim_suffix>.0f / range;
        this.snowMeshVertices = new Vector3[SNOW_NUM * 4];

        #region Generate Vertices

        for (var i = 0; i < SNOW_NUM; i++)
        {
            float x = Random.Range (-this.range, this.range);
            float y = Random.Range (-this.range, this.range);
            float z = Random.Range (-this.range, this.range);

            Vector3 point = new Vector3(Random.Range(-this.range, this.range),
                                        Random.Range(-this.range, this.range),
                                        Random.Range(-this.range, this.range));

            this.snowMeshVertices [i * 4 + 0] = point;
            this.snowMeshVertices [i * 4 + 1] = point;
            this.snowMeshVertices [i * 4 + 2] = point;
            this.snowMeshVertices [i * 4 + 3] = point;
        }

        #endregion Generate Vertices

        #region Generate Indexes

        this.snowMeshTriangleIndexes = new int[SNOW_NUM * 6];

        for (int i = 0; i < SNOW_NUM; i++)
        {
            this.snowMeshTriangleIndexes[i * 6 + 0] = i * 4 + 0;
            this.snowMeshTriangleIndexes[i * 6 + 1] = i * 4 + 1;
            this.snowMeshTriangleIndexes[i * 6 + 2] = i * 4 + 2;

            this.snowMeshTriangleIndexes[i * 6 + 3] = i * 4 + 2;
            this.snowMeshTriangleIndexes[i * 6 + 4] = i * 4 + 1;
            this.snowMeshTriangleIndexes[i * 6 + 5] = i * 4 + 3;
        }

        #endregion Generate Indexes

        #region Generate UVs

        this.snowMeshUvs = new Vector2[SNOW_NUM * 4];

        for (var i = 0; i < SNOW_NUM; i++)
        {
            snowMeshUvs [i * 4 + 0] = new Vector2 (0f, 0f);
            snowMeshUvs [i * 4 + 1] = new Vector2 (1f, 0f);
            snowMeshUvs [i * 4 + 2] = new Vector2 (0f, 1f);
            snowMeshUvs [i * 4 + 3] = new Vector2 (1f, 1f);
        }

        #endregion Generate UVs

        Mesh mesh      = new Mesh();
        mesh.name      = "SnowMeshes";
        mesh.vertices  = this.snowMeshVertices;
        mesh.triangles = this.snowMeshTriangleIndexes;
        mesh.uv        = this.snowMeshUvs;
        mesh.bounds    = new Bounds(Vector3.zero, Vector3.one * 9999);  //99999999

        meshFilter.sharedMesh = mesh;

    }
    

    public override void LateUpdate()
    {
        Camera cam = Camera.main;
        if(cam == null) return;
        Vector3 target_position = cam.transform.TransformPoint(Vector3.forward * this.range);

        this.meshRenderer.material.SetFloat("_Range",  this.range);
        this.meshRenderer.material.SetFloat("_RangeR", this.rangeR);
        this.meshRenderer.material.SetFloat("_Size",   0.2f);

        this.meshRenderer.material.SetVector("_MoveTotal",      this.move);
        this.meshRenderer.material.SetVector("_CamUp",          cam.transform.up);
        this.meshRenderer.material.SetVector("_TargetPosition", target_position);

        float x = (Mathf.PerlinNoise(0f, Time.time * 0.1f) - 0.5f) * 10f;
        float y = -2f;
        float z = (Mathf.PerlinNoise(Time.time * 0.1f, 0f) - 0.5f) * 10f;
        
        //if(cam.transform.localPosition.y > 180)
        //{
        //    y = -10f;
        //}
        move += new Vector3(x, y, z) * Time.deltaTime;
        move.x = Mathf.Repeat(move.x, range * 2f);
        move.y = Mathf.Repeat(move.y, range * 2f);
        move.z = Mathf.Repeat(move.z, range * 2f);
    }
}<fim_middle>

        this.range = 2000f;
        this.rangeR = 1<|endoftext|>