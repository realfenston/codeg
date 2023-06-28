<fim_prefix>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlatformHotfix;
using UnityEngine.EventSystems;

public class RankMatchCup3DView :View3DBase
{
    private GameObject[] _cups;
    private MeshRenderer[] _cupRenders;
    private MeshRenderer[] _giftRenders;
    private SkinnedMeshRenderer[] _gift2Renders1;

    private Animator  _currentgiftBox2;
    private GameObject[] _cupPillars;
    private MeshRenderer[] _cupFlags;
    private GameObject _effectPrefab;
    private GameObject _effectObj;

    private static Vector3 _initialPos;
    private static int _selectedCupId = 0;

    [HideInInspector]
    public int SelectedCupId
    {
        get { return _selectedCupId; }
        set
        {
            if (value >= 0 && value < CUP_NUM)
            {
                _selectedCupId = value;
            }
            else
            {
                DebugEX.LogError("错误，奖杯ID超出 ：" + value);
            }
        }
    }

    private const int CUP_NUM = 7;
    private const string CUP = "Cup";
    private const string _GLIM_TIME = "_GlimTime";
    private const string _CENTER_POS = "_CenterPos";
    private const string _OUT_LINE = "_OutLine";
    private const string GIFT_NAME = "GiftBox";
    private const string GIFT2_NAME = "GiftBox02";
    private const string LAYER_NAME = "UI3D";
    private const string OPEN_SEASON_AWARD_VIEW = "OpenSeasonAwardView";
    static bool _isFirstShow = true;
    bool _show_effect = false;

    Transform pillar1;
    Transform pillar2;
    Transform pillar3;

    Vector3 pillar1_pos;
    Vector3 pillar2_pos;
    Vector3 pillar3_pos;

    Action pillar1Func;
    Action pillar2Func;
    Action pillar3Func;

    Vector3 lightAngles;

    Camera ray_Camera;

    float old_fieldOfView;
    CameraRenderPassSetting setting;
    float old_ShadowNear;
    float old_ShadowFar;

    bool isOpenGiftBox;
    bool showSingleCuping;

    static string [] CupRendersDic = new string[CUP_NUM]{"Cup_01/Cup/Beginner_Cup","Cup_02/Cup/Amateur_Cup02","Cup_03/Cup/Cup_03", "Cup_04/Cup/Top Events_Cup_04","Cup_05/Cup/SUPERSTAR","Cup_06/Cup/Legen_Cup","Cup_07/Cup/Trophy_Europe01"};
    public override async GTask Awake()
    {
        await base.Awake();
        _initialPos = this.transform.position;

        _cups = new GameObject[CUP_NUM];
        _cupRenders = new MeshRenderer[CUP_NUM];
        _giftRenders = new MeshRenderer[CUP_NUM];
        _gift2Renders1 = new SkinnedMeshRenderer[CUP_NUM];
        _cupPillars = new GameObject[CUP_NUM];
        _cupFlags = new MeshRenderer[CUP_NUM];

        for(int i = 0; i< CUP_NUM;i++)
        {
            _cups[i] = this.transform.Find(string.Format("Cup_0{0}", i+1)).gameObject;
        }

        for(int i = 0; i< CUP_NUM;i++)
        {
            _cupRenders[i] = this.transform.Find(CupRendersDic[i]).GetComponent<MeshRenderer>();
        }

        for(int i = 0; i< CUP_NUM;i++)
        {
            string path = string.Format("Cup_0{0}/Pillar/Pillar2/GiftBox", i+1);
            _giftRenders[i] = this.transform.Find(path).GetComponent<MeshRenderer>();
        } //Home/Scene3D/RankMatchCup(Clone)/Cup_01/Pillar/Pillar2/GiftBox
        
         for(int i = 0; i< CUP_NUM;i++)
        {
            string path = string.Format("Cup_0{0}/Pillar/Pillar4/GiftBox02/GiftBox02_02", i+1);
            _gift2Renders1[i] = this.transform.Find(path).GetComponent<SkinnedMeshRenderer>();
        }
        

        for(int i = 0; i< CUP_NUM;i++)
        {
            _cupPillars[i] = this.transform.Find(string.Format("Cup_0{0}/Pillar", i+1)).gameObject;
        }

        for(int i = 0; i< CUP_NUM;i++)
        {
            _cupFlags[i] = this.transform.Find(string.Format("Cup_0{0}/Flag", i+1)).GetComponent<MeshRenderer>();
        }

        for(int i = 0; i< CUP_NUM;i++)
        {
            Transform cup = this.transform.Find(string.Format("Cup_0{0}/Cup",i+1));
            DynamicBoneCollider giftCollider = _giftRenders[i].transform.GetComponent<DynamicBoneCollider>();
            DynamicBone [] dynamicBones = cup.GetComponents<DynamicBone>();
            for(int j = 0; j< dynamicBones.Length;j++)
            {
                dynamicBones[j].m_Colliders.Add(giftCollider);
            }
        }

        if (AppConfig.Instance.ChannelInfo == ChannelInfo.VIVO && GlobalDataManager.OpenVivoGiftRank)
        {
            for(int i = 0; i< CUP_NUM;i++)
            {
                try
                {
                    var gift = this.transform.Find(string.Format("Cup_0{0}/Pillar/Pillar3/Ball/Gifts", i+1)).gameObject;
                    gift.SetActive(true);
                }
                catch (Exception e)
                {
                
                }
            }
        }
       

        //OpenUpdate();
    }
    public override  void OnEnable()
    {
        if(StadiumSceneController.Instance!= null  && StadiumSceneController.Instance.StadiumLight!= null)
        {
            if(lightAngles == Vector3.zero)
            {
                lightAngles = StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles;
            }
            StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles = new Vector3(53, 107, 84);
            if (ray_Camera == null)
            {
                ray_Camera = Platform.CameraAnimController.Instance.Cam.transform.Find("DummyCamera").GetComponent<Camera>();
                old_fieldOfView = ray_Camera.fieldOfView;
                ray_Camera.fieldOfView = 42;
            }
        }
       if(Platform.CameraAnimController.Instance!= null)
       {
            setting = Platform.CameraAnimController.Instance.Cam.GetComponent<CameraRenderPassSetting>();
            if (setting!= null)
            {
                if((int)old_ShadowNear == 0)
                {
                    old_ShadowNear = setting.ShadowNear;
                    old_ShadowFar = setting.ShadowFar;
                }
                setting.ShadowNear = 3;
                setting.ShadowFar = 5;

                var FOVPlayerFeature = setting.getRendererFeatureByName("FOVCameraObjects");
                FOVPlayerFeature?.SetActive(true);
            }
       }
    }

    public void ResetCameraInfo()
    {
        if(Platform.CameraAnimController.Instance == null) return;
        Platform.CameraAnimController.Instance.SetFeatureFOV("FOVCameraObjects", 42, 0);
        Platform.CameraAnimController.Instance.SetFeatureFOV("FOVCameraForTransparentObjects", 42, 0);
        Platform.CameraAnimController.Instance.SetFeatureOffset("FOVCameraObjects", Vector4.zero, 0);
        Platform.CameraAnimController.Instance.SetFeatureOffset("FOVCameraFor<fim_suffix>SetFOV(70, 0);
        Platform.CameraAnimController.Instance.SetLensShift(new Vector2(0, 0.15f), 0);
    }
    public override void OnDisable()
    {
        if(StadiumSceneController.Instance!= null && StadiumSceneController.Instance.StadiumLight!= null){
            StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles = lightAngles;
        }
        
        if (ray_Camera!= null)
        {
            ray_Camera.fieldOfView = old_fieldOfView;
        }
        if (setting!= null)
        {
            setting.ShadowNear = old_ShadowNear;
            setting.ShadowFar = old_ShadowFar;
        }
    }

    bool isShow = false;
    public void ShowAllCups(bool show_effect = false)
    {
        if (isShow) return;
        isShow = true;
        float time = 0;
        if (_isFirstShow)
        {
            time = 1.68f;
            _isFirstShow = false;
        }

        _show_effect = show_effect;
        gameObject.SetActive(true);
        for (int i = 0; i < _cups.Length; i++)
        {
            _cups[i].SetActive(time > 0);
            _cups[i].transform.Find(CUP).GetComponent<Cup_DynamicBone_Setting>().Weight = 0;
            _cupFlags[i].material.SetFloat(_OUT_LINE, 0);
            _cupPillars[i].transform.localEulerAngles = Vector3.zero;
            _cupPillars[i].transform.GetChild(1).gameObject.SetActive(false);
            _cupPillars[i].transform.GetChild(2).gameObject.SetActive(false);
            _cupPillars[i].transform.GetChild(3).gameObject.SetActive(false);
            _cupRenders[i].material.SetFloat(_GLIM_TIME, -1);
            _gift2Renders1[i].material.SetFloat(_GLIM_TIME, -1);
            _cupFlags[i].transform.Find("3DText").GetComponent<TextMesh>().text = GetEachGradeName((GradeEnum)i);
            if (time > 0)
            {
                Go.to(_cupRenders[i].material, 1f, new GoTweenConfig()
               .materialFloat(1, _GLIM_TIME)
               .setDelay(.6f)
               .setEaseType(GoEaseType.ExpoOut));
            }
        }
        if (SelectedCupId >= 0)
        {
            _cupFlags[SelectedCupId].gameObject.SetActive(true);
            _cupFlags[SelectedCupId].material.SetFloat(_OUT_LINE, 1);
        }
        MoveTo(0, -0.04f, 0);
        HidenEffect();

        ShowSingleCup(time);
    }


    private void ShowSingleCup(float time)
    {
        showSingleCuping = true;
        Go.DelayCall(() =>
        {
            FocusOnSelectedCup();
            MoveTo(SelectedCupId - 2.5f, - (SelectedCupId -1)  * 0.05f, 0);
        }, time);
    }

    private void FocusOnSelectedCup()
    {
        gameObject.SetActive(true);
        SetSelectCupData(SelectedCupId);
        pillar1Func = () =>
        {
            if (pillar1!= null)
            {
                pillar1.gameObject.SetActive(true);
                Go.from(pillar1,.6f, new GoTweenConfig()
                   .localPosition(pillar1_pos - new Vector3(0,.5f, 0))
                   .setEaseType(GoEaseType.ExpoOut));
            }
        };
        Go.DelayCall(pillar1Func,.3f);

        pillar2Func = () =>
        {
            if (pillar2!= null)
            {
                pillar2.gameObject.SetActive(true);
                Go.from(pillar2,.6f, new GoTweenConfig()
                   .localPosition(pillar2_pos - new Vector3(0,.5f, 0))
                   .setEaseType(GoEaseType.ExpoOut));
            }
        };
        Go.DelayCall(pillar2Func,.6f);
        
        pillar3Func = () =>
        {
            if (pillar3!= null)
            {
                pillar3.gameObject.SetActive(true);
                Go.from(pillar3,.6f, new GoTweenConfig()
                   .localPosition(pillar3_pos - new Vector3(0,.5f, 0))
                   .setEaseType(GoEaseType.ExpoOut));
                GlimFocusedCup();
                isShow = false;
            }
        };
        Go.DelayCall(pillar3Func,.9f);
        Go.DelayCall(ShowCupComplete, 1.5f);
        if (_show_effect) //播放特效
        {
            Go.DelayCall(ShowEffect, 1.5f);
        }
    }
    private void ShowCupComplete()
    {
        showSingleCuping = false;
        if(isOpenGiftBox)
        {
            OpenGiftBox(isOpenGiftBox);
        }
    }

    private void SetSelectCupData(int selectid)
    {
        PillarsReset();
        for (int i = 0; i < _cups.Length; i++)
        {
            if (i!= selectid)
            {
                _cups[i].SetActive(false);
            }
        }
        _cups[selectid].SetActive(true);
        _cupPillars[selectid].transform.localEulerAngles = new Vector3(0, -55f, 0);
        pillar1 = _cupPillars[selectid].transform.GetChild(1);
        pillar2 = _cupPillars[selectid].transform.GetChild(2);
        pillar3 = _cupPillars[selectid].transform.GetChild(3);
        pillar1_pos = pillar1.localPosition;
        pillar2_pos = pillar2.localPosition;
        pillar3_pos = pillar3.localPosition;
        _cupFlags[selectid].gameObject.SetActive(false);
        _cups[selectid].transform.Find(CUP).GetComponent<Cup_DynamicBone_Setting>().Weight = 1;
        _currentgiftBox2 = _gift2Renders1[selectid].transform.parent.GetComponent<Animator>();
        CupRotation(2, selectid, new Vector3(0, -90, 0), new Vector3(0, 90, 0));
    }
    public void ShowRankCup(int selectid, float offset = 2.5f, float moveTime = 0, bool show_effect = false)
    {
        if (selectid < 0 || selectid >= CUP_NUM)
        {
            DebugEX.LogError("selectid is out");
            return;
        }
        SetSelectCupData(selectid);
        pillar1.gameObject.SetActive(true);
        pillar2.gameObject.SetActive(true);
        pillar3.gameObject.SetActive(true);
        MoveTo(selectid - offset, - (selectid -1) * 0.05f, moveTime);

        if (show_effect) //播放特效
        {
            Go.DelayCall(ShowEffect, 1.5f);
        }
    }

    public void GlimFocusedCup()
    {
        // _cupRenders[SelectedCupId].material.SetFloat("_GlimTime",-1);
        // Go.to(_cupRenders[SelectedCupId].material,1f,new GoTweenConfig()
        //        .materialFloat(1,"_GlimTime")
        //        .setEaseType(GoEaseType.ExpoOut));

        _cupRenders[SelectedCupId].material.SetFloat(_GLIM_TIME, -1);
        //_giftRenders[SelectedCupId].materials[0].SetFloat(_GLIM_TIME,-1);  
        _giftRenders[SelectedCupId].materials[1].SetFloat(_GLIM_TIME, -1);
        _giftRenders[SelectedCupId].materials[1].SetVector(_CENTER_POS, _giftRenders[SelectedCupId].transform.position);

        _gift2Renders1[SelectedCupId].material.SetFloat(_GLIM_TIME, -1);
        _gift2Renders1[SelectedCupId].material.SetVector(_CENTER_POS, _gift2Renders1[SelectedCupId].transform.parent.position);
        Go.to(this, 1f, new GoTweenConfig()
               .onUpdate((obj) =>
                {
                    float glimTime = (obj.totalElapsedTime - 0.5f) * 2f;
                    //_giftRenders[SelectedCupId].materials[0].SetFloat(_GLIM_TIME,glimTime);
                    _giftRenders[SelectedCupId].materials[1].SetFloat(_GLIM_TIME, glimTime);
                    _cupRenders[SelectedCupId].material.SetFloat(_GLIM_TIME, glimTime);
                    _gift2Renders1[SelectedCupId].material.SetFloat(_GLIM_TIME, glimTime);
                })
               .setEaseType(GoEaseType.ExpoOut));
    }



    public void MoveTo(float z, float y, float duation =.6f, GoEaseType easeType = GoEaseType.ExpoOut)
    {
        Vector3 pos = _initialPos;
        pos.y = y;
        pos.z = -z;
        if (duation == 0)
        {
            this.transform.position = pos;
        }
        else
        {
            Go.killAllTweensWithTarget(this.transform);
            Go.to(this.transform, duation, new GoTweenConfig()
               .position(pos)
               .setEaseType(easeType));
        }
    }

    public void CupRotation(float time, int selectid, Vector3 startEulerAngles, Vector3 endEulerAngles)
    {
        Transform cup = _cups[selectid].transform.Find(CUP);
        Go.killAllTweensWithTarget(cup.transform);
        RotationCup rotationCup = cup.GetComponent<RotationCup>();
        if (rotationCup!= null)
        {
            rotationCup.deltaX = 0;
            rotationCup.isCanRotate = false;
        }
        cup.localRotation = Quaternion.Euler(startEulerAngles);
        Go.to(cup.transform, time, new GoTweenConfig()
          .localRotation(Quaternion.Euler(endEulerAngles))
          .setDelay(.1f)
          .setEaseType(GoEaseType.CubicOut)
          .onComplete((obj) =>
           {
               if (rotationCup!= null)
               {
                   rotationCup.deltaX = 0;
                   rotationCup.isCanRotate = true;
               }
           }));
    }

    public Vector3 GetGiftBox2Pos()
    {
        Vector3 pos = Vector3.zero;
        if(SelectedCupId >= 0 && SelectedCupId < CUP_NUM)
        {
            pos = _gift2Renders1[SelectedCupId].transform.parent.position;
            pos.y += 0.08f;
        }
        return pos;
    }
    
    public void OpenGiftBox(bool isOpen)
    {
        this.isOpenGiftBox = isOpen;
        if(showSingleCuping) return;
        if(isOpen)
        {
            _currentgiftBox2.Play("Open", 0);
        }else{
            _currentgiftBox2.Play("Close", 0);
        }
    }
    private void ShowEffect()
    {
        if (_effectObj == null)
        {
            _effectObj = GameObject.Instantiate(_effectPrefab, _cups[SelectedCupId].transform);
        }
        _effectObj.SetActive(true);
        Go.DelayCall(() =>
        {
            HidenEffect();
        }, 4);
    }

    private void HidenEffect()
    {
        if (_effectObj!= null)
            _effectObj.SetActive(false);
    }

    private void PillarsReset()
    {   
        if (pillar1!= null)
        {
            pillar1.localPosition = pillar1_pos;
            Go.killAllTweensWithTarget(pillar1);
            pillar1 = null;
        }
        if(pillar2!= null)
        {
             pillar2.localPosition = pillar2_pos;
             Go.killAllTweensWithTarget(pillar2);
              pillar2 = null;
        }
        if(pillar3!= null)
        {
            pillar3.localPosition = pillar3_pos;
            Go.killAllTweensWithTarget(pillar3);
            pillar3 = null;
        }
        if (pillar1Func!= null)
        {
            Go.CancelDelayCall(pillar1Func);
            pillar1Func = null;
        }
        if(pillar2Func!= null)
        {
            Go.CancelDelayCall(pillar2Func);
            pillar2Func = null;
        }
        if(pillar3Func!= null)
        {
            Go.CancelDelayCall(pillar3Func);
            pillar3Func = null;
        }
    }
    public override void OnDestroy()
    {
        //CloseUpdate();
        if (_effectObj!= null)
        {
            GameObject.Destroy(_effectObj);
            _effectObj = null;
        }
        ray_Camera = null;
        Go.CancelDelayCall(ShowEffect);
        Go.killAllTweensWithTarget(this);
        PillarsReset();
        showSingleCuping = false;
        isOpenGiftBox = false; 
    }

    PointerEventData eventData = new PointerEventData(EventSystem.current);
    List<RaycastResult> results = new List<RaycastResult>();
    // public override void Update()
    // {
    //     if (IsPointerOverUIObject())
    //     {
    //         return;
    //     }
    //     if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
    //     {
    //         if (EventSystem.current.IsPointerOverGameObject())
    //         {  //阻断穿透UI的事件
    //             return;
    //         }
    //         Ray ray = ray_Camera.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hit;
    //         if (Physics.Raycast(ray, out hit, 1000, 1 << LayerMask.NameToLayer(LAYER_NAME)))
    //         {
    //             GameObject obj = hit.collider.gameObject;
                
    //         }
    //     }
    // }
    private bool IsPointerOverUIObject()
    {
        results.Clear();
        eventData.position = PlatformHotfix.Input.mousePosition;
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
    public static string GetEachGradeName(GradeEnum grade)
    {
        switch (grade)
        {
            case GradeEnum.BEGINNER:
                return LanguageKit.Get("Rank_Grade_Name1");
            case GradeEnum.AMATEUR:
                return LanguageKit.Get("Rank_Grade_Name2");
            case GradeEnum.PROFESSIONAL:
                return LanguageKit.Get("Rank_Grade_Name3");
            case GradeEnum.TOP:
                return LanguageKit.Get("Rank_Grade_Name4");
            case GradeEnum.SUPERSTAR:
                return LanguageKit.Get("Rank_Grade_Name5");
            case GradeEnum.LEGEND:
                return LanguageKit.Get("Rank_Grade_Name7");
            case GradeEnum.EPIC:
                return LanguageKit.Get("Rank_Grade_Name6");
        }
        return "";
    }
}
public enum GradeEnum
{
    BEGINNER = 0,
    AMATEUR = 1,
    PROFESSIONAL = 2,
    TOP = 3,
    SUPERSTAR = 4,
    LEGEND = 5,
    EPIC = 6,
}
<fim_middle>TransparentObjects", Vector4.zero, 0);
        Platform.CameraAnimController.Instance.<|endoftext|>