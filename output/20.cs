<fim_prefix>using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.GalaSports.Service;
using GalaRenderPipeline.URPPass;
using UnityEngine.UI;


public class BattlePass3DView : View3DBase
{
    HighPolyPlayerAppearanceController[] PlayersAppearance = new HighPolyPlayerAppearanceController[3];
    HighPolyPlayerAnimationController[] PlayersAnimation = new HighPolyPlayerAnimationController[3];
    Transform[] Players = new Transform[3];
    Transform[] Pelvis = new Transform[3];

    // Transform player;
    Material TeamJerseyMat;
    Material TeamShoesMat;
    Material TeamSockMat;
    Material SkinMatBlack;
    Material SkinMatWhite;
    Material SkinMatLatin;
    Material SkinMatAsian;
    Vector3 localPos = new Vector3(25f, 0f, 0f);
    Vector3 localEulerAngle = new Vector3(0, 90, 0);

    static Vector3[] PlayerLocalPosition = new Vector3[3] { new Vector3(0.535f, 0f, -0.04f), new Vector3(0.97f, 0f, -0.42f), new Vector3(0.26f, 0f, -0.42f) };
    static Vector3[] PlayerLocalEuler = new Vector3[3] { new Vector3(0f, -13.29f, 0), new Vector3(0, -16.848f, 0), new Vector3(0, -6.295f, 0) };
    static Vector3 defultEuler = new Vector3(0, -90, 0);
    GalaRenderObjectsFeature fovFeatrue;
    GalaRenderObjectsFeature transpartFovFeatrue;
    Vector3 oldCameraOffest;
    float oldCameraFOV;
    float shadowFar = 0;
    float shadowNear = 0;
    // Start is called before the first frame update
    bool isLoadCompeleted = false;

    static string[] clothDataNames = new string[] { "Idle01", "Idle02" };

    static string[] jerseyCodes = new string[]
    {
        "-1_Marcos Llorente_e0b244FF_deb043FF_e0b244FF_1_20222023_22_ab0425ff_232841FF_232841ff",
        "-1_Fabinho_28973bFF_d3cb4aFF_37362fFF_1_20222023_22_c9be44ff_28973bFF_4f3db5ff",
        "-1_Milinković_d7b475FF_d7b475FF_d7b475FF_1_20222023_22_c10729ff_d7b475FF_c10729ff"
    };
    static int[] headIds = new int[] { 72, 123, 981 };
    Texture2D teamLogo = null;
    public override async GTask Awake()
    {
        await base.Awake();
        await Instantiate3PlayerPrefab();
        ClothDataLoader.AsyncPreLoadMultiClotDataSet(clothDataNames);
        PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(headIds);
        for (int i = 0; i < jerseyCodes.Length; i++)
        {
            PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(jerseyCodes[i]);
            PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(jerseyCodes[i]);
        }
        this.transform.localPosition = localPos;
        this.transform.localEulerAngles = localEulerAngle;
        for (int i = 0; i < Players.Length; i++)
            Players[i].GetComponent<HighPolyPlayerAnimationController>().Hide();


        fovFeatrue = Camera.main.GetComponent<CameraRenderPassSetting>().getRendererFeatureByName("FOVCameraObjects") as GalaRenderObjectsFeature;
        transpartFovFeatrue = Camera.main.GetComponent<CameraRenderPassSetting>().getRendererFeatureByName("FOVCameraForTransparentObjects") as GalaRenderObjectsFeature;
        if (fovFeatrue)
        {
            oldCameraOffest = fovFeatrue.settings.cameraSettings.offset;
            oldCameraFOV = fovFeatrue.settings.cameraSettings.cameraFieldOfView;
            shadowFar = Camera.main.GetComponent<CameraRenderPassSetting>().ShadowFar;
            shadowNear = Camera.main.GetComponent<CameraRenderPassSetting>().ShadowNear;
        }
        if (teamLogo == null)
        {
            teamLogo = new Texture2D(2, 2);
            teamLogo.SetPixel(0, 0, Color.clear);
            teamLogo.SetPixel(0, 1, Color.clear);
            teamLogo.SetPixel(1, 0, Color.clear);
            teamLogo.SetPixel(1, 1, Color.clear);
            teamLogo.Apply();
        }
        Platform.EventDispatcher.AddEventListener("TempHideBattlePassPlayer", TempHide);
        Platform.EventDispatcher.AddEventListener("TempShowBattlePassPlayer", TempShow);
        Platform.EventDispatcher.AddEventListener("HideBattlePassPlayer", HidePlayer);
        Platform.EventDispatcher.AddEventListener<PlayersInfoTransfer[]>("ShowBattlePassPlayer", ShowPlayer);
        Platform.EventDispatcher.AddEventListener<PlayersInfoTransfer[],string[]>("ShowBattlePassPlayer1", ShowPlayer1);
    }

    GameObject _prefab;
    GTask<GameObject> _prefabTask;
    private async GTask Instantiate3PlayerPrefab()
    {
        _prefabTask = ResourceMgr.Instance.LoadAssetAsync<GameObject>("3D/Prefab/TeamPlayer");
        var prefabTaskResult = await _prefabTask;
        _prefab = prefabTaskResult.result;
        _prefab.transform.position = new Vector3(500,0,500);
        for (int i = 0; i < 3; i++)
        {
            GameObject instantiated = GameObject.Instantiate(_prefab, gameObject.transform);
            Players[i] = instantiated.transform;
            Players[i].name = "TeamPlayer" + i.ToString();
            PlayersAppearance[i] = instantiated.GetComponent<HighPolyPlayerAppearanceController>();
            PlayersAnimation[i] = instantiated.GetComponent<HighPolyPlayerAnimationController>();
            PlayersAnimation[i].SetUI3DLayer();
            PlayersAnimation[i].HideFootball();
            Pelvis[i] = PlayersAnimation[i].Pelvis;
            await GAsync.WaitNextFrame();
        }
    }


    //int player_id
    //int headModel_id
    //bool isKeeper,
    //SkinColorForTexture skinColor,
    //int bodyHeight,
    //int shoe_Id = 1,
    //int bodyWeight = 0

    //Texture clothTexture,
    //Texture keeperClothTexture,
    //Texture sockTexture,
    //Texture keeperSockTexture

    //public int id=0;
    //<fim_suffix>Setting galaShaderGlobalSetting;
    private async void ShowPlayer(PlayersInfoTransfer[] playersInfoTransfer)
    {

        try
        {
            if (fovFeatrue && transpartFovFeatrue)
            {
                fovFeatrue.settings.cameraSettings.offset = new Vector3(0f, 0.2f, 0);
                transpartFovFeatrue.settings.cameraSettings.offset = new Vector3(0f, 0.2f, 0);
                fovFeatrue.settings.cameraSettings.cameraFieldOfView = 28;
                transpartFovFeatrue.settings.cameraSettings.cameraFieldOfView = 28;
            }
            Camera.main.GetComponent<CameraRenderPassSetting>().ShadowNear = 2.8f;
            Camera.main.GetComponent<CameraRenderPassSetting>().ShadowFar = 3.5f;

            for (int i = 0; i < playersInfoTransfer.Length; i++)
            {
                string jerseyCode = "";
                string playerNumber = "10";
                string playerName = "GALA";
                if (playersInfoTransfer[i].PlayerNumber == 30077)
                {
                    playerName = "Marcos Llorente";
                    playerNumber = "-1";
                    jerseyCode = "-1_Marcos Llorente_e0b244FF_deb043FF_e0b244FF_1_20222023_22_ab0425ff_232841FF_232841ff";
                }
                else if (playersInfoTransfer[i].PlayerNumber == 30039)
                {
                    playerName = "Fabinho";
                    playerNumber = "-1";
                    jerseyCode = "-1_Fabinho_28973bFF_d3cb4aFF_37362fFF_1_20222023_22_c9be44ff_28973bFF_4f3db5ff";
                }
                else
                {
                    playerName = "Milinković";
                    playerNumber = "-1";
                    jerseyCode = "-1_Milinković_d7b475FF_d7b475FF_d7b475FF_1_20222023_22_c10729ff_d7b475FF_c10729ff";
                }

                PlayersAppearance[i].gameObject.SetActive(false);

                PlayersAppearance[i].SetPlayerAppearance(int.Parse(playersInfoTransfer[i].HeadModel_id), (int)playersInfoTransfer[i].BodyHeight, playersInfoTransfer[i].BodyWeight, (SkinColorForTexture)playersInfoTransfer[i].SkinColor, playersInfoTransfer[i].SkinColorCorrectionValue, jerseyCode, 0, playersInfoTransfer[i].Player_id, playersInfoTransfer[i].IsKeeper, playerName, playerNumber, false, teamLogo, true);

                InitPlayer(playersInfoTransfer[i].PlayerNumber, PlayersAppearance[i], PlayersAnimation[i]);

                PlayerComeOutAnimation(i, PlayersAnimation[i]);
                PlayersAppearance[i].gameObject.SetActive(true);
                await GAsync.WaitNextFrame();
            }
            Platform.EventDispatcher.TriggerEvent("OverrideShadowForHomeScenePlayer", true);
            isLoadCompeleted = true;
        }
        catch (Exception ex)
        {
            DebugEX.LogError("通行证生成三人出错");
        }

        galaShaderGlobalSetting = GameObject.FindObjectOfType<GalaShaderGlobalSetting>();
        if (galaShaderGlobalSetting == null || galaShaderGlobalSetting.GrassRender == null)
            return;
        Material[] grassMat = null;
        if (galaShaderGlobalSetting!= null)
            grassMat = galaShaderGlobalSetting.GrassRender.sharedMaterials;
        if (grassMat!= null)
        {
            for (int i = 0; i < grassMat.Length; i++)
            {
                if (grassMat[i]!= null)
                    grassMat[i].DisableKeyword("_RECEIVE_SHADOWS_ON");
            }

        }
    }
    private async void ShowPlayer1(PlayersInfoTransfer[] playersInfoTransfer, string[] jerseyCode)
    {
        try
        {
            await Instantiate3PlayerPrefab();
            for (int i = 0; i < jerseyCode.Length; i++)
            {
                PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(jerseyCode[i]);
                PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(jerseyCode[i]);
                headIds[i] = int.Parse(playersInfoTransfer[i].HeadModel_id);
            }
            ClothDataLoader.AsyncPreLoadMultiClotDataSet(clothDataNames);
            PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(headIds);
            for (int i = 0; i < Players.Length; i++)
                Players[i].GetComponent<HighPolyPlayerAnimationController>().Hide();
            if (fovFeatrue && transpartFovFeatrue)
            {
                fovFeatrue.settings.cameraSettings.offset = new Vector3(0f, 0.2f, 0);
                transpartFovFeatrue.settings.cameraSettings.offset = new Vector3(0f, 0.2f, 0);
                fovFeatrue.settings.cameraSettings.cameraFieldOfView = 28;
                transpartFovFeatrue.settings.cameraSettings.cameraFieldOfView = 28;
            }
            Camera.main.GetComponent<CameraRenderPassSetting>().ShadowNear = 2.8f;
            Camera.main.GetComponent<CameraRenderPassSetting>().ShadowFar = 3.5f;

            for (int i = 0; i < playersInfoTransfer.Length; i++)
            {
                PlayersAppearance[i].gameObject.SetActive(false);
                DebugEX.LogError(playersInfoTransfer[i].PlayerName);
                PlayersAppearance[i].SetPlayerAppearance(int.Parse(playersInfoTransfer[i].HeadModel_id), (int)playersInfoTransfer[i].BodyHeight, playersInfoTransfer[i].BodyWeight, (SkinColorForTexture)playersInfoTransfer[i].SkinColor, playersInfoTransfer[i].SkinColorCorrectionValue, jerseyCodes[i], 0, playersInfoTransfer[i].Player_id, playersInfoTransfer[i].IsKeeper, playersInfoTransfer[i].PlayerName, playersInfoTransfer[i].PlayerNumber.ToString(), false, teamLogo, true);

                InitPlayer(playersInfoTransfer[i].PlayerNumber, PlayersAppearance[i], PlayersAnimation[i]);

                PlayerComeOutAnimation(i, PlayersAnimation[i]);
                PlayersAppearance[i].gameObject.SetActive(true);
                await GAsync.WaitNextFrame();
            }

            Platform.EventDispatcher.TriggerEvent("OverrideShadowForHomeScenePlayer", true);
            isLoadCompeleted = true;
        }
        catch (Exception ex)
        {
            DebugEX.LogError("通行证生成三人出错");
        }

        galaShaderGlobalSetting = GameObject.FindObjectOfType<GalaShaderGlobalSetting>();
        if (galaShaderGlobalSetting == null || galaShaderGlobalSetting.GrassRender == null)
            return;
        Material[] grassMat = null;
        if (galaShaderGlobalSetting!= null)
            grassMat = galaShaderGlobalSetting.GrassRender.sharedMaterials;
        if (grassMat!= null)
        {
            for (int i = 0; i < grassMat.Length; i++)
            {
                if (grassMat[i]!= null)
                    grassMat[i].DisableKeyword("_RECEIVE_SHADOWS_ON");
            }

        }
    }
    private void InitPlayer(int playerId, HighPolyPlayerAppearanceController playerAppearance, HighPolyPlayerAnimationController playerAnimation)
    {
        playerAnimation.Init(true, false);
    }



    private void TempShow()
    {
        for (int i = 0; i < 3; i++)
        {
            if (PlayersAnimation[i] && isLoadCompeleted)
                PlayersAnimation[i].Show();
        }

        galaShaderGlobalSetting = GameObject.FindObjectOfType<GalaShaderGlobalSetting>();
        if (galaShaderGlobalSetting == null || galaShaderGlobalSetting.GrassRender == null)
            return;
        Material[] grassMat = null;
        if (galaShaderGlobalSetting!= null)
            grassMat = galaShaderGlobalSetting.GrassRender.sharedMaterials;
        if (grassMat!= null)
        {
            for (int i = 0; i < grassMat.Length; i++)
            {
                if (grassMat[i]!= null)
                    grassMat[i].DisableKeyword("_RECEIVE_SHADOWS_ON");
            }

        }
    }
    private void TempHide()
    {
        for (int i = 0; i < 3; i++)
        {
            if (PlayersAnimation[i])
                PlayersAnimation[i].Hide();
        }

        galaShaderGlobalSetting = GameObject.FindObjectOfType<GalaShaderGlobalSetting>();
        if (galaShaderGlobalSetting == null || galaShaderGlobalSetting.GrassRender == null)
            return;
        Material[] grassMat = null;
        if (galaShaderGlobalSetting!= null)
            grassMat = galaShaderGlobalSetting.GrassRender.sharedMaterials;
        if (grassMat!= null)
        {
            for (int i = 0; i < grassMat.Length; i++)
            {
                if (grassMat[i]!= null)
                    grassMat[i].EnableKeyword("_RECEIVE_SHADOWS_ON");
            }

        }
    }
    private void HidePlayer()
    {
        if (fovFeatrue && transpartFovFeatrue)
        {
            fovFeatrue.settings.cameraSettings.offset = oldCameraOffest;
            transpartFovFeatrue.settings.cameraSettings.offset = oldCameraOffest;
            fovFeatrue.settings.cameraSettings.cameraFieldOfView = oldCameraFOV;
            transpartFovFeatrue.settings.cameraSettings.cameraFieldOfView = oldCameraFOV;
        }
        for (int i = 0; i < 3; i++)
        {
            if (PlayersAnimation[i])
                PlayersAnimation[i].Hide();
        }
        Platform.EventDispatcher.TriggerEvent("OverrideShadowForHomeScenePlayer", false);
        if (Camera.main!= null)
        {
            Camera.main.GetComponent<CameraRenderPassSetting>().ShadowFar = shadowFar;
            Camera.main.GetComponent<CameraRenderPassSetting>().ShadowNear = shadowNear;
        }
        isLoadCompeleted = false;

        galaShaderGlobalSetting = GameObject.FindObjectOfType<GalaShaderGlobalSetting>();
        if (galaShaderGlobalSetting == null || galaShaderGlobalSetting.GrassRender == null)
            return;
        Material[] grassMat = null;
        if (galaShaderGlobalSetting!= null)
            grassMat = galaShaderGlobalSetting.GrassRender.sharedMaterials;
        if (grassMat!= null)
        {
            for (int i = 0; i < grassMat.Length; i++)
            {
                if (grassMat[i]!= null)
                    grassMat[i].EnableKeyword("_RECEIVE_SHADOWS_ON");
            }

        }
    }

    private void PlayerComeOutAnimation(int playerIndex, HighPolyPlayerAnimationController playerAnimation)
    {
        int poseId = 0;
        if (playerIndex == 0)
            poseId = 1;
        playerAnimation.PlayAnimation(ComeOutAndIdleAnimationConfigs.IdleStateNamesSet[poseId]);
        playerAnimation.transform.localPosition = PlayerLocalPosition[playerIndex];
        playerAnimation.transform.eulerAngles = PlayerLocalEuler[playerIndex] - defultEuler;
    }





    public override void OnDestroy()
    {
        if (TeamJerseyMat!= null)
        {
            ResourceMgr.Instance.UnloadAsset(TeamJerseyMat);
            TeamJerseyMat = null;
        }
        if (TeamShoesMat!= null)
        {
            ResourceMgr.Instance.UnloadAsset(TeamShoesMat);
            TeamShoesMat = null;
        }
        if (TeamSockMat!= null)
        {
            ResourceMgr.Instance.UnloadAsset(TeamSockMat);
            TeamSockMat = null;
        }
        if (SkinMatBlack!= null)
        {
            ResourceMgr.Instance.UnloadAsset(SkinMatBlack);
            SkinMatBlack = null;
        }
        if (SkinMatWhite!= null)
        {
            ResourceMgr.Instance.UnloadAsset(SkinMatWhite);
            SkinMatWhite = null;
        }
        if (SkinMatLatin!= null)
        {
            ResourceMgr.Instance.UnloadAsset(SkinMatLatin);
            SkinMatLatin = null;
        }
        if (SkinMatAsian!= null)
        {
            ResourceMgr.Instance.UnloadAsset(SkinMatAsian);
            SkinMatAsian = null;
        }

        HidePlayer();
        ClothDataLoader.UnloadMultiPreLoadedClothDatas(clothDataNames);
        teamLogo.DestroySelf();
        teamLogo = null;

        if (_prefab!= null)
        {
            ResourceMgr.Instance.UnloadAsset(_prefab);
        }
        galaShaderGlobalSetting = GameObject.FindObjectOfType<GalaShaderGlobalSetting>();
        if (galaShaderGlobalSetting == null || galaShaderGlobalSetting.GrassRender == null)
            return;
        Material[] grassMat = null;
        if (galaShaderGlobalSetting!= null)
            grassMat = galaShaderGlobalSetting.GrassRender.sharedMaterials;
        if (grassMat!= null)
        {
            for (int i = 0; i < grassMat.Length; i++)
            {
                if (grassMat[i]!= null)
                    grassMat[i].EnableKeyword("_RECEIVE_SHADOWS_ON");
            }

        }
        Platform.EventDispatcher.RemoveEventListener("TempHideBattlePassPlayer", TempHide);
        Platform.EventDispatcher.RemoveEventListener("TempShowBattlePassPlayer", TempShow);
        Platform.EventDispatcher.RemoveEventListener("HideBattlePassPlayer", HidePlayer);
        Platform.EventDispatcher.RemoveEventListener<PlayersInfoTransfer[]>("ShowBattlePassPlayer", ShowPlayer);
        Platform.EventDispatcher.RemoveEventListener<PlayersInfoTransfer[], string[]>("ShowBattlePassPlayer1", ShowPlayer1);
    }
}
<fim_middle>public string playerName = "";
    //public int playerNumber = 0;
    //public int headModelId = 0;
    //public int bodyHeight = 0;
    //public int bodyWeight = 0;
    //public