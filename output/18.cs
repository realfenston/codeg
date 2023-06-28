<fim_prefix>using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Challenge3DView : View3DBase
{
    private List<int> _solidIntLine = new List<int>();
    private List<int> _growIntLine = new List<int>();

    // 全部关卡线段，目前写死，后续新玩法开放
    private List<int> _dotIntLine = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
    private List<int> _mainIntLine = new List<int>() { 1, 3, 5, 8, 11, 14, 17 };

    private float Angle;
    [Range(0, 5)] public float Speed = 1;

    [Space]
    public MeshFilter Filter;
    public Material LineMat;

    private float _delta;
    private Action _action;
    private float _progress;
    private bool _grow;

    public override async GTask Awake()
    {
        await base.Awake();
        // for test
        // List<int> test_solid = new List<int>() { 1, 2, 3, 4,5, 6,};
        // List<int> test_grow = new List<int>() {  7 };
        // UpdateLines(test_solid, test_grow, 0f);
        // Grow(0.05f, null);

        Filter = transform.Find("MapLine").GetComponent<MeshFilter>();
        LineMat = transform.Find("MapLine").GetComponent<MeshRenderer>().material;

        UpdateLines();
        OpenUpdate();
    }


    public override void Update()
    {
        // #if UNITY_EDITOR
        //         this.UpdateLines();
        // #endif

        Angle += 0.001f * Speed;
        LineMat.SetFloat("_Angle", Angle);

        if (this._grow)
        {
            SetProgress(this._progress + this._delta);
        }
    }

    private void UpdateLines()<fim_suffix> Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        Vector2[] uv2s = mesh.uv2;

        for (int i = 0; i < vertices.Length; i++)
        {
            // calculate color
            int curUVValue = Mathf.RoundToInt(uv2s[i].y);
            bool isMainLine = _mainIntLine.IndexOf(curUVValue)!= -1;
            colors[i] =  isMainLine? new Color(1, 0, 0, 1) : new Color(1, 0, 1, 1);
            if (_dotIntLine.IndexOf(curUVValue)!= -1)//未挑战
            {
                colors[i].r = 1f;
                colors[i].g = 0f;
                colors[i].a = 0.5f;
            }
            if (_solidIntLine.IndexOf(curUVValue)!= -1)//已挑战
            {
                colors[i].r = 1f;
                colors[i].g = 0f;
                colors[i].a = 1f;
            }
            if (_growIntLine.IndexOf(curUVValue)!= -1)//可挑战
            {
                colors[i].r = 0f;
                colors[i].g = 1f;
                colors[i].a = 1f;
            }

        }
        mesh.colors = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    private void SetProgress(float progress)
    {
        if (progress >= 1)
        {
            this._grow = false;
            var action = this._action;
            this._action = null;

            action?.Invoke();
        }

        this._progress = Mathf.Clamp01(progress);
        if (this.LineMat!= null)
            this.LineMat.SetFloat("_Progress", progress);
    }

    public void Grow(float delta, Action action)
    {
        SetProgress(0);
        this._delta = delta;
        this._action = action;
        this._grow = true;
    }

    public void SetMapLine(Mesh curMapLine)
    {
        if (curMapLine!= null)
        {
            Filter.mesh = curMapLine;
        }
    }

    /// <summary>
    /// 更新挑战线路状态
    /// </summary>
    /// <param name="solidLines">已经挑战的关卡</param>
    /// <param name="growLines">可挑战的关卡</param>
    /// <param name="progress">动画参数默认是 1</param>
    public void UpdateLines(List<int> solidLines, List<int> growLines, float progress = 1f)
    {
        if(solidLines == null || growLines == null)
        {
            UpdateLines();
            return;
        }

        _solidIntLine.Clear();
        _growIntLine.Clear();
        for (int i = 0; i < solidLines.Count; i++)
        {
            _solidIntLine.Add(solidLines[i]);
        }
        for (int i = 0; i < growLines.Count; i++)
        {
            _growIntLine.Add(growLines[i]);
        }

        SetProgress(progress);
        UpdateLines();
    }

    /// <summary>
    /// 已经挑战的关卡
    /// </summary>
    /// <param name="solidLines">已经挑战的关卡</param>
    public void AddSolidLine(int[] solidLine)
    {
        for (int i = 0; i < solidLine.Length; i++)
        {
            _growIntLine.Remove(solidLine[i]);
        }
        _solidIntLine.AddRange(solidLine);
        SetProgress(0f);
        UpdateLines();
    }
    /// <summary>
    /// 可挑战的关卡
    /// </summary>
    /// <param name="solidLines">已经挑战的关卡</param>
    public void AddGrowIntLine(int[] growIntLine, Action callback)
    {
        _growIntLine.Clear();
        _growIntLine.AddRange(growIntLine);
        SetProgress(0f);
        UpdateLines();
        callback += ()=>
        {
            AddSolidLine(growIntLine);
        };
        Grow(.05f, callback);
    }
}
<fim_middle>
    {
        Mesh mesh = Filter.mesh;
       <|endoftext|>