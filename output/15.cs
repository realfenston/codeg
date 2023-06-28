<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.GalaSports.Service;
public class LockRoom3DView : View3DBase
{
    private GameObject[] ReflectionProbes;

    private LightmapDataView lightmapDataView;

    private GameObject fitnessRoomFloor;
    private FitnessRoomPlayer_Hotfix fitnessRoomPlayer;


    const string EVENT_SetLockRoomReflectionProbe = "SetLockRoomReflectionProbe";
    const string EVENT_PlayerJerseyGenerator = "PlayerJerseyGenerator";
    const string EVENT_ShowPlayerDetail = "ShowPlayerDetail";
    const string EVENT_EquipJersey = "EquipJersey";
    const string EVENT_FitnessRoomPlayerShow = "FitnessRoomPlayerShow";
    //const string EVENT_SetDressingRoomCloth = "SetDressingRoomCloth";
    const string player_path = "3D/FunctionRoom/LockRoomPlayer";

    Vector4 characterSoftShadowDirection1 = new Vector4(0.4f, 4f, 2.5f, 0);
    Vector4 characterSoftShadowDirection2 = new Vector4(2.5f, 8f, -5, 0);

    private Vector3 pos;
    private string wallJerseyId = "";
    private string playerJerseyId = "";
    private string lastJereyId = "";
    private RenderTexture lastJerseyTex;
    private RenderTexture lastSockTex;

    private RenderTexture Detail_jerseytex;
    private RenderTexture Detail_socktex;

    private Material RoomClothMat;

    private Transform TeamPlayer_Tran;
    public static bool IsAtlasDetailView = false;

    public override async GTask Awake()
    {
        IsAtlasDetailView = false;
        GarenaSettings();

        lightmapDataView = this.transform.GetComponent<LightmapDataView>();
        lightmapDataView.RestoreLightMapData();

        ReflectionProbes = new GameObject[3];
        for (int i = 0; i < ReflectionProbes.Length; i++)
        {
            ReflectionProbes[i] = this.transform.Find(string.Format("Reflection Probe0{0}", i + 1)).gameObject;
        }

        fitnessRoomFloor = this.transform.Find("Static_Mesh/DressingRoom01/DressingRoomFloor").gameObject;
        
        RoomClothMat = this.transform.Find("Static_Mesh/Clothes_mesh/Clothes_mesh").GetComponent<MeshRenderer>().sharedMaterial;

        SceneCameraController.instance.EnterRoom(true);

        var quality = SettingConfig.GetGraphicsQualityLevel();
        var size = quality >= 3? 1024 : 512;
        PlayerJerseyGenerator.Instance.Initialize(size);

        if(quality>=3)
        {
           await  CreatePlayer();
        }

        //设置墙上的球衣
        ChangeDressingRoomCloth(TAGlobalData.Instance.jerseyTexture);

        Platform.EventDispatcher.AddEventListener<int>(EVENT_SetLockRoomReflectionProbe, SetReflectionProbe);
        Platform.EventDispatcher.AddEventListener<string, bool>(EVENT_PlayerJerseyGenerator, UpdateTexCallback);
        Platform.EventDispatcher.AddEventListener<PlayersInfoTransfer>(EVENT_ShowPlayerDetail, ChangePlayerDetail);
        Platform.EventDispatcher.AddEventListener(EVENT_EquipJersey, EquipJerseyHandler);
        Platform.EventDispatcher.AddEventListener<bool>(EVENT_FitnessRoomPlayerShow, FitnessRoomPlayerShowHandler);
    }
    //Garena特殊处理
    private void GarenaSettings()
    {
        if(ChannelInfo.CheckIsGarena())
        {
            var logo = this.transform.Find("UnStatic_Mesh/DressingRoom_Logo");
            if(logo!= null)
            {
                logo.gameObject.SetActive(false);
            }
        }
    }
    
    private async GTask CreatePlayer()
    {
        if(fitnessRoomPlayer == null)
        {
            var taskobj = await ResourceMgr.Instance.InstantiateAsync(player_path,this.transform);
            var playerObj = taskobj.result;
            fitnessRoomPlayer = RenderingHelper.Instance.GetComponent<FitnessRoomPlayer_Hotfix>(playerObj);
            TeamPlayer_Tran = fitnessRoomPlayer.transform.Find("TeamPlayer");
            pos = TeamPlayer_Tran.position;
            fitnessRoomPlayer.gameObject.SetActive(false);
            await fitnessRoomPlayer.Init_Hotfix(fitnessRoomFloor, characterSoftShadowDirection1, characterSoft<fim_suffix> override void OnEnable()
    {
        base.OnEnable();
        lightmapDataView?.RestoreLightMapData();
        if(fitnessRoomPlayer!= null && fitnessRoomPlayer.gameObject.activeSelf)
        {
            if (IsAtlasDetailView)
            {
                lightmapDataView?.ReconstructLightProbeData();
                FitnessRoomPlayerShowHandler(true);
                IsAtlasDetailView = false;
               // FitnessRoomPlayer_Hotfix.isResetShadow = true;
            }
               
            if (!string.IsNullOrEmpty(lastJereyId))
            {
                ResetPlayerInfo(lastJereyId);
            }
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        IsAtlasDetailView = false;
        lightmapDataView?.UnloadLightMapDatas();
        lightmapDataView?.ResetDefaultLightProbe();
    }
    public override void OnDestroy()
    {

        if (SceneCameraController.instance)
            SceneCameraController.instance.EnterRoom(false);
        
        lightmapDataView = null;

        if (lastJerseyTex!= null || lastSockTex!= null)
        {
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(lastJerseyTex);
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(lastSockTex);
            lastJerseyTex = null;
            lastSockTex = null;
        }

        TAGlobalData.Instance.ReleaseTexture(Detail_jerseytex, Detail_socktex);
        Detail_jerseytex = null;
        Detail_socktex = null;

        PlayerJerseyGenerator.Instance?.SetCameraActive(false);
        Platform.EventDispatcher.RemoveEvent(EVENT_PlayerJerseyGenerator);
        Platform.EventDispatcher.RemoveEvent(EVENT_ShowPlayerDetail);
        Platform.EventDispatcher.RemoveEvent(EVENT_EquipJersey);
        Platform.EventDispatcher.RemoveEvent(EVENT_FitnessRoomPlayerShow);
        Platform.EventDispatcher.RemoveEventListener<int>(EVENT_SetLockRoomReflectionProbe, SetReflectionProbe);
        //Platform.EventDispatcher.RemoveEvent(EVENT_SetDressingRoomCloth);

        if(fitnessRoomPlayer!= null)
        {
           ResourceMgr.Instance.UnloadGameObject(fitnessRoomPlayer.gameObject);
           fitnessRoomPlayer = null;
        }
    }

    public void SetReflectionProbe(int index)
    {
        if (index < ReflectionProbes.Length)
        {
            for (int i = 0; i < ReflectionProbes.Length; i++)
                ReflectionProbes[i].SetActive(false);
            ReflectionProbes[index].SetActive(true);
        }
    }

    void ChangeDressingRoomCloth(RenderTexture jerseyTexture)
    {
        RoomClothMat.SetTexture("_BaseMap", jerseyTexture);
    }

    private void FitnessRoomPlayerShowHandler(bool obj)
    {
        if (fitnessRoomPlayer!= null)
        {
            TeamPlayer_Tran.gameObject.SetActive(obj);
            if (obj)
            {
                fitnessRoomPlayer.PlayRandomIdleAnimation();
            }
        }

    }

    void EquipJerseyHandler()
    {
        if (lastJerseyTex == null || lastSockTex == null || string.IsNullOrEmpty(lastJereyId)) return;

        GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(TAGlobalData.Instance.jerseyTexture);
        GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(TAGlobalData.Instance.sockTexture);

        TAGlobalData.Instance.jerseyTexture = lastJerseyTex;
        TAGlobalData.Instance.sockTexture = lastSockTex;
        TAGlobalData.Instance.JereyId = lastJereyId;

        wallJerseyId = lastJereyId;
        ChangeDressingRoomCloth(lastJerseyTex);
        lastJerseyTex = null;
        lastSockTex = null;
    }

    int _headModelId;
    string _jerseyCode;
    PlayersInfoTransfer _info = null;
    public async void UpdateTexCallback(string jerseyId, bool isUpdateWallDress)
    {
        if (jerseyId == lastJereyId)
        {
            return;
        }
        await ResetPlayerInfo(jerseyId);
    }
    private async GTask ResetPlayerInfo(string jerseyId)
    {
        await CreatePlayer();

        fitnessRoomPlayer.gameObject.SetActive(true);


        if (_info == null)
        {
            if (GuideConfig.IsNewGuide())
            {
                _info = GuideConfig.JerseryPlayer;
            }
            else
            {
                _info = HomeScene3DPlayersController.GetRandomPlayerConfigs();
            }
                
        }

        _jerseyCode = PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(jerseyId, _info.PlayerNumber.ToString(), _info.PlayerName);
        PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(_jerseyCode);
        PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(_jerseyCode);
        await fitnessRoomPlayer.SetOneHighPolyPlayer(_info.Player_id,
                                                        int.Parse(_info.HeadModel_id), _info.IsKeeper,
                                                        (SkinColorForTexture)_info.SkinColor,
                                                        _info.SkinColorCorrectionValue,
                                                        (int)_info.BodyHeight,
                                                        _info.Shoe_id,
                                                        _info.BodyWeight,
                                                        _info.PlayerName,
                                                        _info.PlayerNumber, _jerseyCode, true);

        await PlayerJerseyGenerator.SetJerseyInfoAsyncStatic(jerseyId);

        PlayerJerseyGenerator.Instance.SetCameraActive(true);
        RenderTexture jerseytex = PlayerJerseyGenerator.Instance.GetDIYTex();
        RenderTexture socktex = PlayerJerseyGenerator.Instance.GetSockTex();
       

        //if (isUpdateWallDress)
        //{
        wallJerseyId = jerseyId;
        ChangeDressingRoomCloth(jerseytex);
        //}

        playerJerseyId = jerseyId;
        fitnessRoomPlayer.InitTeamClothesTexture(jerseytex, jerseytex, socktex, socktex);
        fitnessRoomPlayer.gameObject.SetActive(true);
        if (lastJerseyTex!= null)
        {//清除球衣贴图在接受球衣贴图
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(lastJerseyTex);
            lastJerseyTex = null;
        }
        if (lastSockTex!= null)
        {
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(lastSockTex);
            lastSockTex = null;
        }
        lastJereyId = jerseyId;
        lastJerseyTex = jerseytex;
        lastSockTex = socktex;

        //有球员后再设置
        lightmapDataView?.ReconstructLightProbeData();
        await GAsync.WaitNextFrame();
        PlayerJerseyGenerator.Instance.SetCameraActive(false);
    }

    public async void ChangePlayerDetail(PlayersInfoTransfer player)    
    {
        if (player!= null)
        {
            await CreatePlayer();
            //TAGlobalData.Instance.ReleaseTexture(Detail_jerseytex, Detail_socktex);
            //(Detail_jerseytex, Detail_socktex) = TAGlobalData.Instance.GetJerserTexture(player.PlayerNumber.ToString());
            //fitnessRoomPlayer.InitTeamClothesTexture(Detail_jerseytex, Detail_jerseytex, Detail_socktex, Detail_socktex);
            // var pos = fitnessRoomPlayer.transform.Find("TeamPlayer").GetComponent<Transform>().position;
            float offset = ((float)Screen.width / (float)Screen.height) - 1.777f;
            offset = offset < 0? 0 : offset / 1.3f;
            TeamPlayer_Tran.position = pos + new Vector3(offset - 0.1f, 0, -4.25f);
            //fitnessRoomPlayer.gameObject.SetActive(false);

            await PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(new string[] { player.HeadModel_id });
            await fitnessRoomPlayer.SetOneHighPolyPlayer(player.Player_id,
                                                        int.Parse(player.HeadModel_id), player.IsKeeper,
                                                        (SkinColorForTexture)player.SkinColor,
                                                        player.SkinColorCorrectionValue,
                                                        (int)player.BodyHeight,
                                                        player.Shoe_id,
                                                        player.BodyWeight,
                                                        player.PlayerName,
                                                        player.PlayerNumber, null, true);
            fitnessRoomPlayer.gameObject.SetActive(true);

            //有球员后再设置 
            lightmapDataView?.ReconstructLightProbeData();

            //fitnessRoomPlayer.gameObject.SetActive(true);
        }
        else
            fitnessRoomPlayer?.gameObject.SetActive(false);
    }
}
<fim_middle>ShadowDirection2);
            fitnessRoomPlayer.gameObject.SetActive(false);
        }
    }

    public<|endoftext|>