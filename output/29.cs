<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platform;

public class PVPWaitPlayers3DView : View3DBase
{
    HighPolyPlayerAnimationController LeftController;
    HighPolyPlayerAnimationController RightController;

    HighPolyPlayerAppearanceController leftAppearance;
    HighPolyPlayerAppearanceController rightAppearance;


    string firstTimeLeftJerserCode = "Empty";

    public const string MATCH_PLAYER_STAND_ANIMATION_COMPLETE = "MatchPlayerStandAnimationComplete";

    private Vector3 LeftPos;
    private Vector3 RightPos;

    private PvpWaitPlayersData pvpWaitPlayersData;
    public override async GTask Awake()
    {
        await base.Awake();

        ClothDataLoader.AsyncPreLoadMultiClotDataSet(new string[] { "Idle08", "ComeOut08" });

        pvpWaitPlayersData = this.transform.GetComponent<PvpWaitPlayersData>();

        LeftController = this.transform.Find("LeftPlayer").GetComponent<HighPolyPlayerAnimationController>();
        RightController = this.transform.Find("RightPlayer").GetComponent<HighPolyPlayerAnimationController>();

        leftAppearance = LeftController.GetComponent<HighPolyPlayerAppearanceController>();
        rightAppearance = RightController.GetComponent<HighPolyPlayerAppearanceController>();

        LeftPos = LeftController.transform.localPosition;
        RightPos = RightController.transform.localPosition;
    }

    bool isStandAnimationCompleted = false;
    public override void OnDestroy()
    {
        TAManager.Instance.SetUI3DMode(false);

        if (Camera.main!= null)
        {
            if (Camera.main.GetComponent<CameraRenderPassSetting>()!= null)
            {
                Camera.main.GetComponent<CameraRenderPassSetting>().SetWithTime(1f, 2.9f, 3f, 3f, false, 0, 0, 0, 0.0f);
            }
        }
        ClothDataLoader.UnloadOnePreLoadedClothData("ComeOut08");
        if (isStandAnimationCompleted)
        {
            ClothDataLoader.UnloadOnePreLoadedClothData("Idle08");
        }
    }


    bool isLastFrameRightStateComeOut08 = false;
    // Update is called once per frame
    public override void Update()
    {
        if (isLastFrameRightStateComeOut08)
        {
            if (RightController.MyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle08"))
            {
                Platform.EventDispatcher.TriggerEvent(PVPWaitPlayers3DView.MATCH_PLAYER_STAND_ANIMATION_COMPLETE);
                isStandAnimationCompleted = true;
            }
        }

        if (RightController.MyAnimator.GetCurrentAnimatorStateInfo(0).IsName("ComeOut08"))
        {
            isLastFrameRightStateComeOut08 = true;
        }
        else
        {
            isLastFrameRightStateComeOut08 = false;
        }

        if (_cameraRenderPassSetting!= null)
        {
            _cameraRenderPassSetting.SetWithTime(1f, 4.25f, 3f, 3f, false, 0, 0, 0, 0.0f);
        }

    }

    public void Reset()
    {
        LeftController.gameObject.SetActive(false);
        RightController.gameObject.SetActive(false);

    }

    CameraRenderPassSetting _cameraRenderPassSetting;

#if USE_RENDER_ASYNCLOAD
    public async GTask ShowPlayer(int playerID, string ClothTextureID, int playerHeadModel, bool isKeeper, SkinColorForTexture skinColor, int playerHeight, int playerWeight, int playerShoes, bool isLeftPlayer, Texture2D teamLogoTex = null)
    {
        if (Camera.main!= null)
        {
            if (Camera.main.GetComponent<CameraRenderPassSetting>()!= null)
            {
                Camera.main.GetComponent<CameraRenderPassSetting>().SetWithTime(1f, 4.25f, 3f, 3f, false, 0, 0, 0, 0.0f);
                _cameraRenderPassSetting = Camera.main.GetComponent<CameraRenderPassSetting>();
            }
        }

        HighPolyPlayerAnimationController animation;
        HighPolyPlayerAppearanceController appearance;
        if (isLeftPlayer)
        {
            animation = LeftController;
            appearance = leftAppearance;
            if (ClothTextureID.Equals(firstTimeLeftJerserCode))
            {
                //闂佽法鍠愰弸濠氬箯瀹勬澘娓愰梺璺ㄥ枑閺嬪骞忛悜鑺ユ櫢闁哄倶鍊栫€氬綊鏌ㄩ悢鍛婄伄闁归鍏橀弫鎾诲棘閵堝棗锟藉綊鏌ㄩ悢铏瑰彄濞村吋鐟︾€氬綊鏌ㄩ悢�<fim_suffix>�诲棘閵堝棗锟藉綊鏌ㄩ悢鍛婄伄闁圭兘锟藉海鍩犻梺璺ㄥ枑閺嬪骞忛柨瀣拷鎺楋拷鏍ュ€栫€氬綊鏌ㄩ悢鍛婄伄闁归鍏橀弫鎾诲棘閵堝棗锟藉綊鏌ㄩ悢鍛婄伄闁瑰嘲鍢查妵宥夋煥閻斿憡鐏柟椋庡厴閺佹捇寮妶鍡楋拷褰掓煥閻曞倹瀚�
                return;
            }
            firstTimeLeftJerserCode = ClothTextureID;
        }
        else
        {

            animation = RightController;
            appearance = rightAppearance;
        }

        animation.gameObject.SetActive(true);

        Material BodyMat;
        if (skinColor == SkinColorForTexture.WHITE)
        {
            BodyMat = pvpWaitPlayersData.SkinW;
        }
        else if (skinColor == SkinColorForTexture.BLACK)
        {
            BodyMat = pvpWaitPlayersData.SkinB;
        }
        else if (skinColor == SkinColorForTexture.LATIN)
        {
            BodyMat = pvpWaitPlayersData.SkinL;
        }
        else
        {
            BodyMat = pvpWaitPlayersData.SkinA;
        }

        //appearance.SetPlayerMaterialWithTemplate(pvpWaitPlayersData.ClothMat, BodyMat, pvpWaitPlayersData.SockMat, pvpWaitPlayersData.ShoeMat, ClothTextureID, playerShoes, playerID, isKeeper, "GALA", "10", false, teamLogoTex);
        await HighPolyPlayerAppearance_Hotfix.SetPlayerMaterialWithTemplate(appearance, pvpWaitPlayersData.ClothMat, BodyMat, pvpWaitPlayersData.SockMat, pvpWaitPlayersData.ShoeMat, ClothTextureID, playerShoes, playerID, isKeeper, "GALA", "10", false, teamLogoTex);
        appearance.PrecalculateBodyShape(playerHeight, playerWeight);
        //appearance.SetPlayerHead(playerHeadModel.ToString(), skinColor);
        await HighPolyPlayerAppearance_Hotfix.SetPlayerHeadAsync(appearance, playerHeadModel.ToString(), skinColor);
        appearance.SetPlayerBodyShapeOnPelvis(playerHeight, playerWeight, 0);


        animation.Init(true, false);
        animation.SetDOFLayer();
        animation.PlayAnimation("ComeOut08", 1, 0, true, false);
    }
#else

    public async void ShowPlayer(int playerID, string playerName, string playerNumber, string ClothTextureID, int playerHeadModel, bool isKeeper, SkinColorForTexture skinColor, Vector3 skinColorCorrectionValue, int playerHeight, int playerWeight, int playerShoes, bool isLeftPlayer, Texture2D teamLogoTex = null)
    {
        DebugEX.Log("_____________________PVPWaitPlayers ShowPlayer: " +  skinColor);
        ClothTextureID = PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(ClothTextureID, playerNumber, playerName);
        GTask task1 = PlayerHeadDataLoader.AsyncPreLoadOneHeadDataSet(playerHeadModel);
        GTask task2 = PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(ClothTextureID, false, teamLogoTex);
        GTask task3 = PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(ClothTextureID);

        await GAsync.WaitUntil(() =>
        {
            bool isLoadCompleted = true;

            if (task1!= null)
            {
                isLoadCompleted = isLoadCompleted && task1.IsCompleted;
            }
            if (task2!= null)
            {
                isLoadCompleted = isLoadCompleted && task2.IsCompleted;
            }
            if (task3!= null)
            {
                isLoadCompleted = isLoadCompleted && task3.IsCompleted;
            }

            return isLoadCompleted;
        });

        if (Camera.main!= null)
        {
            if (Camera.main.GetComponent<CameraRenderPassSetting>()!= null)
            {
                Camera.main.GetComponent<CameraRenderPassSetting>().SetWithTime(1f, 4.25f, 3f, 3f, false, 0, 0, 0, 0.0f);
                _cameraRenderPassSetting = Camera.main.GetComponent<CameraRenderPassSetting>();
            }
        }

        HighPolyPlayerAnimationController animation;
        HighPolyPlayerAppearanceController appearance;
        if (isLeftPlayer)
        {
            animation = LeftController;
            appearance = leftAppearance;
            if (ClothTextureID.Equals(firstTimeLeftJerserCode))
            {
                //闂佽法鍠愰弸濠氬箯瀹勬澘娓愰梺璺ㄥ枑閺嬪骞忛悜鑺ユ櫢闁哄倶鍊栫€氬綊鏌ㄩ悢鍛婄伄闁归鍏橀弫鎾诲棘閵堝棗锟藉綊鏌ㄩ悢铏瑰彄濞村吋鐟︾€氬綊鏌ㄩ悢鍛婄伄闁归鍏橀弫鎾诲棘閵堝棗锟藉綊鏌ㄩ悢鍛婄伄闁圭兘锟藉海鍩犻梺璺ㄥ枑閺嬪骞忛柨瀣拷鎺楋拷鏍ュ€栫€氬綊鏌ㄩ悢鍛婄伄闁归鍏橀弫鎾诲棘閵堝棗锟藉綊鏌ㄩ悢鍛婄伄闁瑰嘲鍢查妵宥夋煥閻斿憡鐏柟椋庡厴閺佹捇寮妶鍡楋拷褰掓煥閻曞倹瀚�
                return;
            }
            firstTimeLeftJerserCode = ClothTextureID;
        }
        else
        {

            animation = RightController;
            appearance = rightAppearance;
        }

        animation.gameObject.SetActive(true);

        /* Material BodyMat;
         if (skinColor == SkinColorForTexture.WHITE)
         {
             BodyMat = pvpWaitPlayersData.SkinW;
         }
         else if (skinColor == SkinColorForTexture.BLACK)
         {
             BodyMat = pvpWaitPlayersData.SkinB;
         }
         else if (skinColor == SkinColorForTexture.LATIN)
         {
             BodyMat = pvpWaitPlayersData.SkinL;
         }
         else
         {
             BodyMat = pvpWaitPlayersData.SkinA;
         }
         appearance.SetPlayerMaterialWithTemplate(pvpWaitPlayersData.ClothMat, BodyMat, pvpWaitPlayersData.SockMat, pvpWaitPlayersData.ShoeMat, ClothTextureID, playerShoes, playerID, isKeeper,"GALA","10",false,teamLogoTex);
         appearance.PrecalculateBodyShape(playerHeight, playerWeight);
         appearance.SetPlayerHead(playerHeadModel.ToString(), skinColor);
         appearance.SetPlayerBodyShapeOnPelvis(playerHeight, playerWeight, 0);*/

        appearance.SetPlayerAppearance(playerHeadModel, playerHeight, playerWeight, skinColor,skinColorCorrectionValue, ClothTextureID, 1, playerID, isKeeper, playerName, playerNumber, false, teamLogoTex, true, true);


        animation.Init(true, false);
        animation.SetDOFLayer();
        animation.PlayAnimation("ComeOut08", 1, 0, true, false);
    }


#endif

    //閻犱礁澧介悿鍡涙偠閸愩劍鍠呴柛瀣箳浜�
    public void SetOffsetPosZ(float offsetz)
    {
        Vector3 _leftPos = Vector3.zero;
        _leftPos.x = LeftPos.x;
        _leftPos.y = LeftPos.y;
        _leftPos.z = LeftPos.z - offsetz;
        LeftController.transform.localPosition = _leftPos;

        Vector3 _rightPos = Vector3.zero;
        _rightPos.x = RightPos.x;
        _rightPos.y = RightPos.y;
        _rightPos.z = RightPos.z + offsetz;
        RightController.transform.localPosition = _rightPos;
    }

    public override void OnEnable()
    {
        //LeftController.Init(true, false);
        //RightController.Init(true, false);

        TAManager.Instance.SetUI3DMode(true, 0.14f);

        OpenUpdate();
    }

    public override void OnDisable()
    {
        TAManager.Instance.SetUI3DMode(false);
        CloseUpdate();
    }
}
//#endif<fim_middle>鍛婄伄闁归鍏橀弫鎾诲棘閵堝棗锟藉綊鏌ㄩ悢鍛