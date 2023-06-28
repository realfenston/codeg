<fim_prefix>﻿using Framework.GalaSports.Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_RENDER_ASYNCLOAD
public class LoginScene3DPlayersController : RenderingBase
{

    public AudienceCamController audience;
    public string[] playersHeadName;

    public int[] playersBodyHeight;

    public int[] playersABodyWeight;

    public Material[] playersSkinMat;

    public Material ClothMat;
    public Material SocksMat;
    public Material ShoesMat;

    public Material[] playersPCSkinMat;
    public Material PCClothMat;
    public Material PCSocksMat;
    public Material PCShoesMat;

    public Transform[] Players;
    public Animator ballAnimator;
    public Animator cameraAnimator;

    //队徽
    public Texture2D TeamLogoTex;

    //守门员
    int GoalKeeperId = 1;
    public Texture2D GoalKeeper_Jerseytex; //球衣
    public Texture2D GoalKeeper_Socktex; //袜子

    public Camera camera;

    public GalaShaderGlobalSetting galaShaderGlobalSetting;

    public Light mainLight;

    public MeshRenderer[] DofMeshRenderers;

    List<RenderTexture> JerseyTexList = new List<RenderTexture>();

    public float DOFDistance = 15f;

    static string[] JerseyIds = new string[] {
        "7_GALASPORTS_ffffffff_ffffffff_ffffffff_3_20202021_1_052134ff_d32f2fff_052134ff",
        "31_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff",//守门员数据无效
        
        "3_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff",
        "6_GALASPORTS_ffffffff_ffffffff_ffffffff_3_20202021_1_052134ff_d32f2fff_052134ff",
        "10_GALASPORTS_ffffffff_ffffffff_ffffffff_3_20202021_1_052134ff_d32f2fff_052134ff",
        "17_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff",
    };
    static int[] ShoesId = new int[] {
        1,2,6,1,3,4
    };
    static string[] PlayerAnimationNames = new string[] {
        "Login1A",
        "Login1B",
        "Login2A",
        "Login2B",
        "Login3A",
        "Login3B",
    };
    const string BallAnimationName = "Football1";
    const string CameraAnimationName = "LoginCamera";

    //public  GameObject DebutWarmUPController;
    //GameObject debutWarmUpController;

    LoginPlayersController loginPlayersController;

    public override async void Init(GameObject go)
    {
        loginPlayersController = go.GetComponent<LoginPlayersController>();
        if (loginPlayersController!= null)
        {
            RenderingHelper.PublicFieldCopy<LoginPlayersController, LoginScene3DPlayersController>(loginPlayersController, this);
            //loginPlayersController.onUpdate = Update;
            loginPlayersController.onDestroy = OnDestroy;
            loginPlayersController.onStart = Start;
        }

        Platform.EventDispatcher.AddEventListener<Camera, Material[], float, bool>("PlayerIncidentSoftShadowInit", SoftShadowInit);
        loginPlayersController.cameraTransform = camera.transform;
        galaShaderGlobalSetting.UpdateSetting();

        loginPlayersController.setting = camera.GetComponent<CameraRenderPassSetting>();
        loginPlayersController.setting.DoFEnable = true;
        loginPlayersController.setting.DoFBlurIteration = 2;
        loginPlayersController.setting.DoFBlurIntensity = 1f;
        loginPlayersController.setting.DoFBlurStart = 0.34f;
        loginPlayersController.setting.DofBlurSoftness = 0.125f;
        loginPlayersController.setting.HDRExposureKey = 0.2f;
        loginPlayersController.setting.HDRMinLum = 0.08f;
        loginPlayersController.setting.HDRMaxLum = 0.8f;

        ResourceMgr.Instance.SetProvider(new CustomAddressableProvider());

        PlayerJerseyGenerator.Instance.Initialize();
    }

    async void Start()
    {
        RenderingHelper.Instance.AddLoadingTime();
        try
        {
            await StartAsync();
        }
        catch { }
        RenderingHelper.Instance.RemoveLoadingTime();
    }

    private async GTask StartAsync()
    {
        cameraAnimator.enabled = false;
        ballAnimator.enabled = false;

        //设置状态
        for (int i = 0; i < Players.Length; i++)
        {
            loginPlayersController.playersAnimation[i] = Players[i].GetComponent<HighPolyPlayerAnimationController>();

            loginPlayersController.playersAppearance[i] = Players[i].GetComponent<HighPolyPlayerAppearanceController>();
            loginPlayersController.playerShoes[i] = Players[i].GetComponent<Playershoes>();
            if (i == GoalKeeperId)
            {
                loginPlayersController.playersAppearance[i].IsKeeper = true;
            }

            loginPlayersController.playersAnimation[i].Hide(); //隐藏避免加载中被看到
        }

        //加载球员贴图
        for (int i = 0; i < Players.Length; i++)
        {

#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && (!UNITY_ANDROID) && (!UNITY_IOS)
            //playersAppearance[i].SetPlayerMaterials(PCClothMat, playersPCSkinMat[i], PCSocksMat, PCShoesMat);
             HighPolyPlayerAppearance_Hotfix.SetPlayerMaterials(loginPlayersController.playersAppearance[i],PCClothMat,playersPCSkinMat[i], PCSocksMat, PCShoesMat);
#else
            //playersAppearance[i].SetPlayerMaterials(ClothMat, playersSkinMat[i], SocksMat, ShoesMat);
            loginPlayersController.playersAppearance[i].SetPlayerMaterials(ClothMat, playersSkinMat[i], SocksMat, ShoesMat);
#endif

            //playersAppearance[i].PrecalculateBodyShape(playersBodyHeight[i], playersABodyWeight[i]);
            loginPlayersController.playersAppearance[i].PrecalculateBodyShape(playersBodyHeight[i], playersABodyWeight[i]);
            loginPlayersController.playersAnimation[i].Hide(); //隐藏避免加载中被看到

            //playersAppearance[i].SetPlayerHead(playersHeadName[i], SkinColorForTexture.WHITE);
            //playersAppearance[i].SetPlayerBodyShapeOnPelvis(playersBodyHeight[i], playersABodyWeight[i], 0);
            await loginPlayersController.playersAppearance[i].SetPlayerHeadAsync(playersHeadName[i], SkinColorForTexture.WHITE);
            loginPlayersController.playersAppearance[i].SetPlayerBodyShapeOnPelvis(playersBodyHeight[i], playersABodyWeight[i], 0);

            loginPlayersController.playersAnimation[i].Init(true, false);
            loginPlayersController.playersAnimation[i].HideFootball();

            await CreateJersey(i);

            loginPlayersController.playersAppearance[i].HeadMR.material.SetFloat("_Smoothness", 1.42f);
            loginPlayersController.playersAppearance[i].LimbGPUSKM.material.SetFloat("_Smoothness", 1.15f);
            loginPlayersController.playersPelvis[i] = loginPlayersController.playersAnimation[i].Pelvis;
        }

        loginPlayersController.football = ballAnimator.GetComponentInChildren<MeshRenderer>().transform;

        //Go.DelayCall(()=>{
        //    PlayerJerseyGenerator.Instance.SetCameraActive(false);
        //},2);
        //

        //预读取动画
        for (int i = 0; i < loginPlayersController.playersAnimation.Length; i++)
        {
            await loginPlayersController.playersAnimation[i].PreloadAnimationAsync(PlayerAnimationNames[i], false);
        }

        //播放动画
        for (int i = 0; i < loginPlayersController.playersAnimation.Length; i++)
        {
            loginPlayersController.playersAnimation[i].PlayAnimation(PlayerAnimationNames[i]);
            PlayerShadow(loginPlayersController.playersAnimation[i].transform);
        }

        loginPlayersController.ResourceInit = true;

        audience.SetSheerUpLevel(2);

        cameraAnimator.enabled = true;
        cameraAnimator.Play(CameraAnimationName);

        ballAnimator.enabled = true;
        ballAnimator.Play(BallAnimationName);


        for (int i = 0; i < DofMeshRenderers.Length; i++)
        {
            DofMeshRenderers[i].gameObject.layer = LayerMask.NameToLayer("DOF");
            if (i == 1)
            {
                DofMeshRenderers[i].material.SetColor("_EmissionColor", new Color(0.7f, 0.7f, 0.7f, 1));
                DofMeshRenderers[i].material.EnableKeyword("_EMISSION");
            }
        }

        Platform.EventDispatcher.TriggerEvent("RenderDetailMode", true);
    }

    async GTask CreateJersey(int playerid)
    {
        Texture2D jerseytex;
        Texture2D socktex;
        if (playerid == GoalKeeperId)
        {
            jerseytex = GoalKeeper_Jerseytex;
            socktex = GoalKeeper_Socktex;
            loginPlayersController.playersAppearance[player<fim_suffix>playerid].HomeGKSockTex = socktex;
        }
        else
        {
            await GAsync.WaitNextFrame();
            PlayerJerseyGenerator.Instance.TeamLogoTex = TeamLogoTex;
            //await PlayerJerseyGenerator.Instance.SetJerseyInfo(JerseyIds[playerid]);
            //PlayerJerseyGenerator.Instance.UpdateJerseyProperties();

            //jerseytex = PlayerJerseyGenerator.Instance.GetDIYTex();
            //socktex = PlayerJerseyGenerator.Instance.GetSockTex();

            await PlayerJerseyGenerator.Instance.SetJerseyInfoAsync(JerseyIds[playerid]);

            jerseytex = PlayerJerseyGenerator.Instance.GetDIYTex();
            socktex = PlayerJerseyGenerator.Instance.GetSockTex();


            JerseyTexList.Add(jerseytex);
            JerseyTexList.Add(socktex);
        }

        Material JerseyMat = loginPlayersController.playersAppearance[playerid].ClothGPUSKM.material;
        JerseyMat.SetTexture("_BaseMap", jerseytex);
        Material SkinMat = loginPlayersController.playersAppearance[playerid].LimbGPUSKM.material;
        Material SockMat = loginPlayersController.playersAppearance[playerid].LegMR.materials[0];
        SockMat.SetTexture("_BaseMap", socktex);
        Material ShoesMat = loginPlayersController.playersAppearance[playerid].LegMR.materials[1];
        loginPlayersController.playerShoes[playerid].id = ShoesId[playerid];

        loginPlayersController.playersAppearance[playerid].SetPlayerMaterials(JerseyMat, SkinMat, SockMat, ShoesMat);
    }

    PlayerShadowAnimation playerShadowAnimation;
    Material celeMaterial;
    Camera _shadowCamera;
    Material[] _materials;
    void PlayerShadow(Transform targetPlayer)
    {
        Platform.EventDispatcher.TriggerEvent("SetShadowForIncident");
        playerShadowAnimation = targetPlayer.GetComponent<PlayerShadowAnimation>();
        celeMaterial = playerShadowAnimation.Shadow.material;
        celeMaterial.SetFloat("_baseScale", 1.5f);
        celeMaterial.SetFloat("_yScaleMul", 5);
        celeMaterial.SetFloat("_baseBlur", 1);
        celeMaterial.SetFloat("_yBlurFade", 0.5f);
        celeMaterial.SetFloat("_yFade", 0.67f);
        celeMaterial.SetFloat("_transparent", 0.7f);

        if (_materials!= null)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i]!= null && _shadowCamera!= null)
                {
                    playerShadowAnimation.EnableSoftShadow(_materials[i], _shadowCamera, 0.025f);
                }
            }
        }
    }

    void SoftShadowInit(Camera shadowCamera, Material[] materials, float fitnessRoomFloorGroundY, bool isShadowEnabled)
    {
        _shadowCamera = shadowCamera;
        _materials = materials;
    }

    private void Update()
    {
        
    }
    

    private void OnDestroy()
    {
        Platform.EventDispatcher.RemoveEventListener<Camera, Material[], float, bool>("PlayerIncidentSoftShadowInit", SoftShadowInit);

        foreach (var tex in JerseyTexList)
        {
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(tex);
        }

        Platform.EventDispatcher.TriggerEvent("RenderDetailMode", false);

        // if(debutWarmUpController!=null)
        // {
        //    Destroy(debutWarmUpController);
        //    debutWarmUpController=null;
        // }
    }

}
#else
public class LoginScene3DPlayersController : RenderingBase
{

    public AudienceCamController audience;
    public string[] playersHeadName;

    public int[] playersBodyHeight;

    public int[] playersABodyWeight;

    public Material[] playersSkinMat;

    public Material ClothMat;
    public Material SocksMat;
    public Material ShoesMat;

    Material[] playersPCSkinMat;
    Material PCClothMat;
    Material PCSocksMat;
    Material PCShoesMat;

    public string[] playersPCSkinsMatPath;
    public string PCClothMatPath;
    public string PCSocksMatPath;
    public string PCShoesMatPath;

    public Transform[] Players;
    public Animator ballAnimator;
    public Animator cameraAnimator;

    //队徽
    public Texture2D TeamLogoTex;

    //守门员
    int GoalKeeperId = 1;
    public Texture2D GoalKeeper_Jerseytex; //球衣
    public Texture2D GoalKeeper_Socktex; //袜子

    public Camera camera;

    public GalaShaderGlobalSetting galaShaderGlobalSetting;

    public Light mainLight;

    public MeshRenderer[] DofMeshRenderers;

    List<RenderTexture> JerseyTexList = new List<RenderTexture>();

    public float DOFDistance = 15f;

    static string[] JerseyIds = new string[] {
        "7_GALASPORTS_28973bFF_d3cb4aFF_37362fFF_1_20222023_22_c9be44ff_28973bFF_4f3db5ff",
        "31_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff",//守门员数据无效
        
        "3_GALASPORTS_c19a6eFF_c19a6eFF_c19a6eFF_1_20222023_22_27324fff_202738FF_27324fff",
        "6_GALASPORTS_151515FF_d9d9d9FF_151515FF_1_20222023_22_ddddddff_1f2326FF_0f0f0fff",
        "10_GALASPORTS_1e57a0FF_f0f0f0FF_1e57a0FF_1_20222023_22_b6cadaff_74a1c8FF_1255a5ff",
        "17_GALASPORTS_cd8b3cFF_c6893bFF_cd8b3cFF_1_20222023_22_7a111cff_0b4623FF_0b4623ff",
    };
    static int[] ShoesId = new int[] {
        1,2,6,1,3,4
    };
    static string[] PlayerAnimationNames = new string[] {
        "Login1A",
        "Login1B",
        "Login2A",
        "Login2B",
        "Login3A",
        "Login3B",
    };
    const string BallAnimationName = "Football";
    const string CameraAnimationName = "LoginCamera";

    //public  GameObject DebutWarmUPController;
    //GameObject debutWarmUpController;

    LoginPlayersController loginPlayersController;

    static string[] stateNamesToPreLoadClothData = new string[]
    {
        "Player1","Player2","Player3","Player4","Player5","Player6"
    };

    public override async void Init(GameObject go)
    {
        ClothDataLoader.AsyncPreLoadMultiClotDataSet(stateNamesToPreLoadClothData);

        for (int i = 0; i < JerseyIds.Length; i++)
        {
            PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(JerseyIds[i]);
            PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(JerseyIds[i]);
        }

        loginPlayersController = go.GetComponent<LoginPlayersController>();
        if (loginPlayersController!= null)
        {
            RenderingHelper.PublicFieldCopy<LoginPlayersController, LoginScene3DPlayersController>(loginPlayersController, this);
            //loginPlayersController.onUpdate = Update;
            loginPlayersController.onDestroy = OnDestroy;
            loginPlayersController.onStart = Start;
        }

        PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(playersHeadName);

        Platform.EventDispatcher.AddEventListener<Material[], float, bool>("PlayerIncidentSoftShadowInit", SoftShadowInit);
        loginPlayersController.cameraTransform = camera.transform;
        galaShaderGlobalSetting.UpdateSetting();

        loginPlayersController.setting = camera.GetComponent<CameraRenderPassSetting>();
        loginPlayersController.setting.DoFEnable = true;
        loginPlayersController.setting.DoFBlurIteration = 2;
        loginPlayersController.setting.DoFBlurIntensity = 1f;
        loginPlayersController.setting.DoFBlurStart = 0.34f;
        loginPlayersController.setting.DofBlurSoftness = 0.125f;
        loginPlayersController.setting.HDRExposureKey = 0.2f;
        loginPlayersController.setting.HDRMinLum = 0.08f;
        loginPlayersController.setting.HDRMaxLum = 0.8f;

        ResourceMgr.Instance.SetProvider(new CustomAddressableProvider());

        PlayerJerseyGenerator.Instance.Initialize();
    }

    private void Start()
    {
        try
        {
            cameraAnimator.enabled = false;
            ballAnimator.enabled = false;

            //设置状态
            for (int i = 0; i < Players.Length; i++)
            {
                loginPlayersController.playersAnimation[i] = Players[i].GetComponent<HighPolyPlayerAnimationController>();

                loginPlayersController.playersAppearance[i] = Players[i].GetComponent<HighPolyPlayerAppearanceController>();
                loginPlayersController.playerShoes[i] = Players[i].GetComponent<Playershoes>();
                if (i == GoalKeeperId)
                {
                    loginPlayersController.playersAppearance[i].IsKeeper = true;
                }

                loginPlayersController.playersAnimation[i].Hide(); //隐藏避免加载中被看到
            }

//#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && (!UNITY_ANDROID) && (!UNITY_IOS)
//                        //加载球员材质
//                        int skinCount = playersPCSkinsMatPath.Length;
//                        playersPCSkinMat = new Material[skinCount];
//                        for (int i = 0; i < skinCount; i++)
//                        {
//                            playersPCSkinMat[i] = ResourceMgr.Instance.LoadAsset<Material>(playersPCSkinsMatPath[i]);
//                        }

//                        PCClothMat = ResourceMgr.Instance.LoadAsset<Material>(PCClothMatPath);
//                        PCSocksMat = ResourceMgr.Instance.LoadAsset<Material>(PCSocksMatPath);
//                        PCShoesMat = ResourceMgr.Instance.LoadAsset<Material>(PCShoesMatPath);
//#endif


            //加载球员贴图
            for (int i = 0; i < Players.Length; i++)
            {
                if (i == 1)
                {
                    loginPlayersController.playersAppearance[i].HomeGKJerseyTex = loginPlayersController.GoalKeeper_Jerseytex;
                    loginPlayersController.playersAppearance[i].HomeGKSockTex = loginPlayersController.GoalKeeper_Socktex;
                }

                HighPolyPlayerAppearance_Hotfix.SetPlayerAppearance(loginPlayersController.playersAppearance[i], int.Parse(playersHeadName[i]), playersBodyHeight[i], playersABodyWeight[i], loginPlayersController.skinColors[i], loginPlayersController.skinColorsCorrectionValue[i], JerseyIds[i], ShoesId[i], loginPlayersController.playersID[i], i == 1, "", "0", false, null, true);

                /*#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && (!UNITY_ANDROID) && (!UNITY_IOS)
                                //playersAppearance[i].SetPlayerMaterials(PCClothMat, playersPCSkinMat[i], PCSocksMat, PCShoesMat);
                                HighPolyPlayerAppearance_Hotfix.SetPlayerMaterials(loginPlayersController.playersAppearance[i],PCClothMat, playersPCSkinMat[i], PCSocksMat, PCShoesMat);
                #else
                                //playersAppearance[i].SetPlayerMaterials(ClothMat, playersSkinMat[i], SocksMat, ShoesMat);
                                HighPolyPlayerAppearance_Hotfix.SetPlayerMaterials(loginPlayersController.playersAppearance[i], ClothMat, playersSkinMat[i], SocksMat, ShoesMat);
                #endif

                                //playersAppearance[i].PrecalculateBodyShape(playersBodyHeight[i], playersABodyWeight[i]);
                                HighPolyPlayerAppearance_Hotfix.PrecalculateBodyShape(loginPlayersController.playersAppearance[i], playersBodyHeight[i], playersABodyWeight[i]);
                                loginPlayersController.playersAnimation[i].Hide(); //隐藏避免加载中被看到

                                //playersAppearance[i].SetPlayerHead(playersHeadName[i], SkinColorForTexture.WHITE);
                                //playersAppearance[i].SetPlayerBodyShapeOnPelvis(playersBodyHeight[i], playersABodyWeight[i], 0);
                                HighPolyPlayerAppearance_Hotfix.SetPlayerHead(loginPlayersController.playersAppearance[i], playersHeadName[i], SkinColorForTexture.WHITE);
                                HighPolyPlayerAppearance_Hotfix.SetPlayerBodyShapeOnPelvis(loginPlayersController.playersAppearance[i], playersBodyHeight[i], playersABodyWeight[i], 0);*/

                loginPlayersController.playersAnimation[i].Init(true, false);
                loginPlayersController.playersAnimation[i].HideFootball();

                //CreateJersey(i);

                loginPlayersController.playersAppearance[i].HeadMR.material.SetFloat("_Smoothness", 1.42f);
                loginPlayersController.playersAppearance[i].LimbGPUSKM.material.SetFloat("_Smoothness", 1.15f);
                loginPlayersController.playersPelvis[i] = loginPlayersController.playersAnimation[i].Pelvis;
            }

            loginPlayersController.football = ballAnimator.GetComponentInChildren<MeshRenderer>().transform;

            //Go.DelayCall(()=>{
            //    PlayerJerseyGenerator.Instance.SetCameraActive(false);
            //},2);
            //

            //播放动画
            for (int i = 0; i < loginPlayersController.playersAnimation.Length; i++)
            {
                //playersAnimation[i].PlayAnimation(PlayerAnimationNames[i]);
                HighPolyPlayerAnimation_Hotfix.PlayAnimation(loginPlayersController.playersAnimation[i], PlayerAnimationNames[i]);
                PlayerShadow(loginPlayersController.playersAnimation[i].transform);
            }

            loginPlayersController.ResourceInit = true;

            //if(audience!= null)
            //{
            //    audience.SetSheerUpLevel(2);
            //}
            AudienceStatus.Instance.SetAudienceSheerUpLevel(2);
            //加载世界杯氛围(世界杯过去了，不加载)
            //WorldCupLogin.Instance.Init();

            cameraAnimator.enabled = true;
            cameraAnimator.Play(CameraAnimationName, 0, 0);
            cameraAnimator.Update(0);

            ballAnimator.enabled = true;
            ballAnimator.Play(BallAnimationName, 0, 0);
            ballAnimator.Update(0);


            for (int i = 0; i < DofMeshRenderers.Length; i++)
            {
                DofMeshRenderers[i].gameObject.layer = LayerMask.NameToLayer("DOF");
                if (i == 1)
                {
                    DofMeshRenderers[i].material.SetColor("_EmissionColor", new Color(0.7f, 0.7f, 0.7f, 1));
                    DofMeshRenderers[i].material.EnableKeyword("_EMISSION");
                }
            }

            Platform.EventDispatcher.TriggerEvent("RenderDetailMode", true);

        }
        catch { }
    }

    void CreateJersey(int playerid)
    {

        Material JerseyMat = loginPlayersController.playersAppearance[playerid].ClothGPUSKM.material;
        Material SockMat = loginPlayersController.playersAppearance[playerid].LegMR.materials[0];
        if (playerid == GoalKeeperId)
        {
            loginPlayersController.playersAppearance[playerid].HomeGKJerseyTex = GoalKeeper_Jerseytex;
            loginPlayersController.playersAppearance[playerid].HomeGKSockTex = GoalKeeper_Socktex;
            JerseyMat.SetTexture("_BaseMap", GoalKeeper_Jerseytex);
            SockMat.SetTexture("_BaseMap", GoalKeeper_Socktex);
        }
        else
        {
            GAsync.WaitNextFrame();
            RenderTexture jerseytex;
            RenderTexture socktex;
            PlayerJerseyGenerator.Instance.TeamLogoTex = TeamLogoTex;
            //PlayerJerseyGenerator.Instance.SetJerseyInfo(JerseyIds[playerid]);
            //PlayerJerseyGenerator.Instance.UpdateJerseyProperties();

            //jerseytex = PlayerJerseyGenerator.Instance.GetDIYTex();
            //socktex = PlayerJerseyGenerator.Instance.GetSockTex();

            PlayerJerseyGenerator_Hotfix.SetJerseyInfo(JerseyIds[playerid]);

            jerseytex = PlayerJerseyGenerator_Hotfix.GetDIYTex();
            socktex = PlayerJerseyGenerator_Hotfix.GetSockTex();
            JerseyTexList.Add(jerseytex);
            JerseyTexList.Add(socktex);

            JerseyMat.SetTexture("_BaseMap", jerseytex);
            SockMat.SetTexture("_BaseMap", socktex);
        }
        Material SkinMat = loginPlayersController.playersAppearance[playerid].LimbGPUSKM.material;
        Material ShoesMat = loginPlayersController.playersAppearance[playerid].LegMR.materials[1];
        loginPlayersController.playerShoes[playerid].id = ShoesId[playerid];

        loginPlayersController.playersAppearance[playerid].SetPlayerMaterials(JerseyMat, SkinMat, SockMat, ShoesMat);
    }

    PlayerShadowAnimation playerShadowAnimation;
    Material celeMaterial;
    Material[] _materials;
    void PlayerShadow(Transform targetPlayer)
    {
        Platform.EventDispatcher.TriggerEvent("SetShadowForIncident");
        playerShadowAnimation = targetPlayer.GetComponent<PlayerShadowAnimation>();
        celeMaterial = playerShadowAnimation.Shadow.material;
        celeMaterial.SetFloat("_baseScale", 1.5f);
        celeMaterial.SetFloat("_yScaleMul", 5);
        celeMaterial.SetFloat("_baseBlur", 1);
        celeMaterial.SetFloat("_yBlurFade", 0.5f);
        celeMaterial.SetFloat("_yFade", 0.67f);
        celeMaterial.SetFloat("_transparent", 0.7f);

        if (_materials!= null)
        {
            for (int i = 0; i < _materials.Length; i++)
            {
                if (_materials[i]!= null)
                {
                    playerShadowAnimation.EnableSoftShadow(_materials[i], 0.025f);
                }
            }
        }
    }

    void SoftShadowInit(Material[] materials, float fitnessRoomFloorGroundY, bool isShadowEnabled)
    {
        _materials = materials;
    }


    private void Update()
    {

    }


    private void OnDestroy()
    {
        Platform.EventDispatcher.RemoveEventListener<Material[], float, bool>("PlayerIncidentSoftShadowInit", SoftShadowInit);

        foreach (var tex in JerseyTexList)
        {
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(tex);
        }

        Platform.EventDispatcher.TriggerEvent("RenderDetailMode", false);

        ClothDataLoader.UnloadAllPreLoadedClothDatas();
        PlayerHeadDataLoader.UnloadAllPreLoadedHeadDatas();
        PlayerJerseyDataLoader.UnloadAllPreLoadedJerseyDatas();
        PlayerSockDataLoader.UnloadAllPreLoadedSockDatas();
        //WorldCupLogin.Instance.OnDestory();

        // if(debutWarmUpController!=null)
        // {
        //    Destroy(debutWarmUpController);
        //    debutWarmUpController=null;
        // }
    }

}
#endif
<fim_middle>id].HomeGKJerseyTex = jerseytex;
            loginPlayersController.playersAppearance[<|endoftext|>