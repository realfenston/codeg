<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.GalaSports.Service;

public class FitnessRoomPlayer_Hotfix : RenderingBase
{
    public GameObject gameObject;
    public Transform transform;
    FitnessRoomPlayer fitnessRoomPlayer;
    public static bool isResetShadow = false;

    public override void Init(GameObject go)
    {
        base.Init(go);
        gameObject = go;
        transform = go.transform;
        fitnessRoomPlayer = go.GetComponent<FitnessRoomPlayer>();
        fitnessRoomPlayer.onEnable = OnEnable;
        fitnessRoomPlayer.onDisable = OnDisable;
        fitnessRoomPlayer.onDestroy = OnDestroy;

        fitnessAnimatorCtrl = fitnessRoomPlayer.fitnessAnimatorCtrl;
        Awake();
    }

    public HighPolyPlayerAppearanceController playerAppearance;
    public HighPolyPlayerAnimationController playerAnimation;

    GameObject fitnessRoomFloor;

    PlayerShadowAnimation playerShadow;
    Material playerMaterial;
    PlayerReflectionController playerRefl;
    GameObject playerObj;

    Texture2D ShoeTex;

    public bool isDressingRoom = false;


    FitnessAnimatorCtrl fitnessAnimatorCtrl;
    int graphicsLevel;
    RenderTexture softshadow_RT;
    // Start is called before the first frame update
    void Awake()
    {
        isResetShadow = false;
        playerAppearance = gameObject.GetComponentInChildren<HighPolyPlayerAppearanceController>();
        playerAnimation = gameObject.GetComponentInChildren<HighPolyPlayerAnimationController>();
        playerRefl = gameObject.GetComponentInChildren<PlayerReflectionController>();


        string clothDataNameToPreLoad;

        clothDataNameToPreLoad = CultivateAnimationConfig.IdleStateEntryNamesForClothData[(CultivateAnimationConfig.AnimationType)0][0];

        ClothDataLoader.AsyncPreLoadOneClothDataSet(clothDataNameToPreLoad);

        clothDataNameToPreLoad = CultivateAnimationConfig.IdleStateEntryNamesForClothData[(CultivateAnimationConfig.AnimationType)1][0];

        ClothDataLoader.AsyncPreLoadOneClothDataSet(clothDataNameToPreLoad);
    }

    private void OnEnable()
    {
        GameObject go = GameObject.Find("Gala Global Setting");
        if (go!= null)
        {
            GalaShaderGlobalSetting setting = go.GetComponent<GalaShaderGlobalSetting>();
            if (setting!= null)
            {
                if (GalaShaderGlobalSetting.CurQualitySetting.PostProcess <= 2)
                {
                    setting.m_Setting.m_SHAmbientIntensity = 1;
                    setting.m_Setting.m_LightMapIntensity = 1;
                    setting.UpdateSetting();
                }
                else
                {
                    setting.m_Setting.m_SHAmbientIntensity = 0;
                    setting.m_Setting.m_LightMapIntensity = 0;
                    setting.UpdateSetting();
                }

            }
        }
        if (isResetShadow)
        {
            playerShadow.IsShadowEnabled = true;
            playerRefl.IsReflectionEnabled = true;
        }
          
        if (isResetShadow && playerShadow.IsShadowEnabled && fitnessRoomFloor!= null && graphicsLevel > 1)
        {
            if (shadowDir1!= Vector4.zero && shadowDir2!= Vector4.zero)
            {
                playerShadow.EnableSoftShadow(fitnessRoomFloor.GetComponent<MeshRenderer>().material, shadowDir1, shadowDir2, groundY);
            }
            else
            {
                playerShadow.EnableSoftShadow(fitnessRoomFloor.GetComponent<MeshRenderer>().material);

            }

        }
        if (isResetShadow && playerRefl.IsReflectionEnabled && fitnessRoomFloor!= null && SceneCameraController.instance.reflectionRT!= null)
        {
            fitnessRoomPlayer.getPlayerRefTex.SetRefPlayerTex();
            playerRefl.EnableReflection(fitnessRoomFloor.GetComponent<MeshRenderer>().material, fitnessRoomFloor.transform.position.y, SceneCameraController.instance.reflectionRT);
        }
    }


    Vector4 shadowDir1 = Vector4.zero;
    Vector4 shadowDir2 = Vector4.zero;
    float groundY = 0;
    public async GTask Init_Hotfix(GameObject aFitnessRoomFloor, Vector4 shadowDir1, Vector4 shadowDir2, float groundY = 0)
    {
        await Init_Hotfix(aFitnessRoomFloor);
        this.shadowDir1 = shadowDir1;
        this.shadowDir2 = shadowDir2;
        this.groundY = groundY;
    }

    private async GTask Init_Hotfix(GameObject aFitnessRoomFloor)
    {
        playerObj = playerAnimation.gameObject;

        //#if USE_RENDER_ASYNCLOAD
        //        playerAnimation.SetHighPolyPlayerAppearanceController(playerAppearance);
        //#endif

        InitSoftShadow();
#if UNITY_IOS &&!UNITY_EDITOR
        /*if (Device.generation == DeviceGeneration.iPhone11
            || Device.generation == DeviceGeneration.iPhone11Pro 
            || Device.generation == DeviceGeneration.iPhone11ProMax 
            || Device.generation == DeviceGeneration.iPhone12
            || Device.generation == DeviceGeneration.iPhone12Mini
            || Device.generation == DeviceGeneration.iPhone12Pro
            || Device.generation == DeviceGeneration.iPhone12ProMax)
        {
            playerAnimation.Init(false, true);
        }
        else
        {
            playerAnimation.Init(true, false);
        }*/
        playerAnimation.Init(true, false);
#else
        playerAnimation.Init(true, false);
#endif<fim_suffix>alaShaderGlobalSetting.CurQualitySetting.ShaderQuality;
        /*GameObject go = GameObject.Find("/Home/CameraRoot/PlayerSoftShadowCamera");
        if (go!= null)
        {
            shadowCamera = go.GetComponent<Camera>();
            if (graphicsLevel >= 2)
            {
                rt = GalaRenderPipeline.GalaRenderManager.CreateTemporaryRT(640, 360, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, "FitnessRoom_RT");          
                shadowCamera.targetTexture = rt;
            }
        }
        GameObject go2 = GameObject.Find("/Home/CameraRoot/PlayerReflectionCamera");
        if (go2!= null)
        {
            reflCamera = go2.GetComponent<Camera>();
        }*/
        fitnessRoomFloor = aFitnessRoomFloor;
    }

    Texture _clothTex;
    Texture _keeperClothTex;
    Texture _socksTex;
    Texture _keeperSocksTex;


    //这些图片不能释放，都是 TAGlobalData.Instance.jerseyTexture 存储图片
    public void DestroyGeneratedTextures()
    {
        if (_clothTex!= null)
        {
            //DebugEX.LogError("Destroy " + _clothTex.GetInstanceID());
            _clothTex.DestroySelf();

            _clothTex = null;
        }
        if (_keeperClothTex!= null)
        {
            _keeperClothTex.DestroySelf();
            _keeperClothTex = null;
        }
        if (_socksTex!= null)
        {
            _socksTex.DestroySelf();
            _socksTex = null;
        }
        if (_keeperSocksTex!= null)
        {
            _keeperSocksTex.DestroySelf();
            _keeperSocksTex = null;
        }
    }

    public void InitTeamClothesTexture(Texture clothTexture, Texture keeperClothTexture, Texture sockTexture, Texture keeperSockTexture)
    {
        if (isDressingRoom)
        {
            DestroyGeneratedTextures();
            _clothTex = clothTexture;
            _keeperClothTex = keeperClothTexture;
            _socksTex = sockTexture;
            _keeperSocksTex = keeperClothTexture;


            /*            //playerAppearance.ClothSKM.material.SetTexture("_BaseMap", clothTexture);
                        playerAppearance.ClothGPUSKM.material.SetTexture("_BaseMap", clothTexture);
                        //DebugEX.LogError(playerAppearance.ClothGPUSKM.material.GetInstanceID());
                        //TeamKeeperJerseyMat.SetTexture("_BaseMap", keeperClothTexture);
                        playerAppearance.LegMR.material.SetTexture("_BaseMap", sockTexture);*/
            if (fitnessRoomPlayer.getPlayerRefTex && this.fitnessRoomPlayer.isActiveAndEnabled)
                fitnessRoomPlayer.getPlayerRefTex.SetRefPlayerTex();

        }
        else
        {
            if (!playerAppearance.IsKeeper)
            {
                /*playerAppearance.ClothGPUSKM.material.SetTexture("_BaseMap", clothTexture);
                playerAppearance.LegMR.material.SetTexture("_BaseMap", sockTexture);*/
            }
            /*TeamJerseyMat.SetTexture("_BaseMap", clothTexture);
            TeamKeeperJerseyMat.SetTexture("_BaseMap", keeperClothTexture);
            TeamSockMat.SetTexture("_BaseMap", sockTexture);
            TeamKeeperSockMat.SetTexture("_BaseMap", keeperSockTexture);*/
        }

        //
        //TeamKeeperSockMat.SetTexture("_BaseMap", sockTexture);
    }

    
    public void InitSoftShadow()
    {
        //设置软阴影材质
        playerShadow = this.gameObject.GetComponentInChildren<PlayerShadowAnimation>();
        playerMaterial = playerShadow.Shadow.material;
        playerMaterial.SetFloat("_baseScale", 1.3f);
        playerMaterial.SetFloat("_yScaleMul", 1);
        playerMaterial.SetFloat("_baseBlur", 0.5f);
        playerMaterial.SetFloat("_yBlurFade", 2);
        playerMaterial.SetFloat("_yFade", 1.5f);
        playerMaterial.SetFloat("_transparent", 0.3f);

    }

    //设置的球员数量
    int playerSetCount = 0;
    //多少次后清理一下资源
    readonly int maxSetCount = 5;

    //player_id:配置文件中的player_id，可以用来查配置
    //headModel_id:配置文件中的headmodel值，用来表示头部3D模型的文件名
    //isKeeper:是守门员吗?
    //skinColor:肤色枚举值，取值规则与PlayerCfg相同
    //bodyHeight:身高
    //shoe_Id:配置文件中的球鞋id
    public async GTask SetOneHighPolyPlayer(int player_id, int headModel_id, bool isKeeper, SkinColorForTexture skinColor, Vector3 skinColorCorrectionValue, int bodyHeight, int shoe_Id = 1, int bodyWeight = 0, string playerName = "GALA", int playerNumber = 0, string jerseyCode = null, bool isReserveClothMat = false)
    {
        DebugEX.LogError("  Fitness: " + player_id);
        if (shoe_Id < 1 || shoe_Id > 6)
        {
            DebugEX.LogError("无效的 shoe_id ！！！");
            shoe_Id = 1;
        }

        playerAppearance.player_id = player_id;

        if (jerseyCode!= null)
        {
            playerAppearance.SetPlayerAppearance(headModel_id, bodyHeight, bodyWeight, skinColor, skinColorCorrectionValue,PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(jerseyCode, playerNumber.ToString(), playerName),1, player_id, isKeeper, "", "0", false, null, true, isReserveClothMat);
        }
        else
        {
            playerAppearance.SetPlayerAppearance(headModel_id, bodyHeight, bodyWeight, skinColor, skinColorCorrectionValue, PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(TAGlobalData.Instance.JereyId, playerNumber.ToString(), playerName), 1, player_id, isKeeper, "", "0", false, null, true, isReserveClothMat);
        }

        fitnessRoomPlayer.getPlayerRefTex.SetRefPlayerTex();
        if (!playerRefl.IsReflectionEnabled && fitnessRoomFloor!= null && SceneCameraController.instance.reflectionRT!= null)
        {
            playerRefl.EnableReflection(fitnessRoomFloor.GetComponent<MeshRenderer>().material, fitnessRoomFloor.transform.position.y, SceneCameraController.instance.reflectionRT);
        }


        //playerAppearance.PrecalculateBodyShape(bodyHeight, bodyWeight); 
        //playerAppearance.SetPlayerBodyShape(bodyHeight, bodyWeight);

        //PlayRandomComeOutAnimation();

        if (!playerShadow.IsShadowEnabled && fitnessRoomFloor!= null && graphicsLevel > 1)
        {
            if (shadowDir1!= Vector4.zero && shadowDir2!= Vector4.zero)
            {
                playerShadow.EnableSoftShadow(fitnessRoomFloor.GetComponent<MeshRenderer>().material, shadowDir1, shadowDir2, groundY);
            }
            else
            {
                playerShadow.EnableSoftShadow(fitnessRoomFloor.GetComponent<MeshRenderer>().material);

            }

        }

        //playerSetCount++;


        if (playerSetCount++ > maxSetCount)
        {
            Resources.UnloadUnusedAssets();
            playerSetCount = 0;
        }

        Platform.EventDispatcher.TriggerEvent("SetShadowForLowQ", 0.04f);

        string fileNameSuffix = headModel_id.ToString();
        //playerAppearance.SetPlayerHead(fileNameSuffix, skinColor);
        this.gameObject.SetActive(true);
        PlayRandomIdleAnimation();

        //await SetShoesTex(ShoesMat, shoe_Id);
    }

    async GTask SetShoesTex(Material ShoesMat, int shoe_Id)
    {
        string path = "3D/PlayerJersey/PlayerShoes/";
        var shoeTexTask = await TAManager.LoadAssetAsync<Texture2D>(path + "Shoes_" + shoe_Id.ToString() + "_tex", "FitnessRoom_Player_ShoeTex");
        if (ShoeTex!= null)
        {
            TAManager.UnloadAsset(ShoeTex);
        }
        ShoeTex = shoeTexTask.result;
        ShoesMat.SetTexture("_BaseMap", ShoeTex);
    }
    public void PlayerUpgrade()
    {
        PlayRandomUpgradeAnimation();
    }

    CultivateAnimationConfig.AnimationType animationType;
    public void PlayRandomIdleAnimation()
    {
        //int stateID;
        //stateID = Random.Range(0, CultivateAnimationConfig.CultivateIdleStateEntryNamesSet.Length);
        //playerAnimation.PlayAnimationWhenFocus(CultivateAnimationConfig.CultivateIdleStateEntryNamesSet[stateID]);
        playerAnimation.HideFootball();


        if (Random.Range(0, 10f) < 6.4f)
        {
            animationType = (CultivateAnimationConfig.AnimationType)0;
        }
        else
        {
            animationType = (CultivateAnimationConfig.AnimationType)1;
        }



        fitnessAnimatorCtrl.PlayIdle(playerAnimation, isDressingRoom, animationType);
    }

    void PlayRandomUpgradeAnimation()
    {
        playerAnimation.HideFootball();

        fitnessAnimatorCtrl.PlayUpgrade(playerAnimation);
    }

    private void OnDisable()
    {
        if (playerShadow!= null)
        {
            playerShadow.DisableSoftShadow();
        }
        if (playerRefl!= null)
        {
            playerRefl.DisableReflection();
        }
        if (softshadow_RT!= null)
        {
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(softshadow_RT);
            softshadow_RT = null;
        }
    }



    public void OnDestroy()
    {
        isResetShadow = false;
        ClothDataLoader.UnloadMultiPreLoadedClothDatas(CultivateAnimationConfig.IdleStateEntryNamesForClothData[(CultivateAnimationConfig.AnimationType)0]);
        ClothDataLoader.UnloadMultiPreLoadedClothDatas(CultivateAnimationConfig.IdleStateEntryNamesForClothData[(CultivateAnimationConfig.AnimationType)1]);
        ClothDataLoader.UnloadMultiPreLoadedClothDatas(CultivateAnimationConfig.UpgradeStateNamesForClothData[(CultivateAnimationConfig.AnimationType)0]);
        ClothDataLoader.UnloadMultiPreLoadedClothDatas(CultivateAnimationConfig.UpgradeStateNamesForClothData[(CultivateAnimationConfig.AnimationType)1]);


        DestroyGeneratedTextures();
        
        Resources.UnloadUnusedAssets();
    }
}
<fim_middle>
        playerAnimation.SetHighPolyPlayerAppearanceController(playerAppearance);
        playerAnimation.SetHighPolyPlayerAnimationController(playerAnimation);
        playerAnimation.SetHighPolyPlayerReflectionController(playerRefl);
        playerAnimation.SetHighPoly