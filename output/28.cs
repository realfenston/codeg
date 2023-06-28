<fim_prefix>using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.GalaSports.Service;
using UnityEngine;
using UnityEngine.AddressableAssets;
//[ExecuteInEditMode]
public class FitnessRoom3DView : View3DBase
{

    public GameObject[] fitnessRoomFloors;
    public Light CharacterLight;
    const string FitnessRoomPlayer = "FitnessRoomPlayer";
    const string FitnessRoomPlayerUpgrade = "FitnessRoomPlayerUpgrade";
    bool isPlayerSetStarted = false;
    bool isSetting = false;
    FitnessRoomPlayer_Hotfix fitnessRoomPlayer;

    Vector3[] localPos = new Vector3[] {
        new Vector3(7.814f, -0.005f, -6.97f),
        new Vector3(-8.457f, 0, 1.232f),
        new Vector3(-12.6f, 0.055f, -0.816f)
    };

    Vector3[] localEulerAngles = new Vector3[] {
        Vector3.zero,
        new Vector3(0, 178f, 0),
        new Vector3(0, 2.12f, 0)
    };

    Vector3[] characterLightLocalEuler = new Vector3[]
    {
        new Vector3(38f, -150f, 0),
        new Vector3(38f, -175f, 0),
        new Vector3(38f, -160f, 0),
    };

    float[] characterLightIntensity = new float[]
    {
       1.35f,1.7f, 0.9f,
    };

    Vector4[] characterSoftShadowDirection1 = new Vector4[]
    {
        new Vector4(-1.3f, 4f, 1f, 0f),
        new Vector4(0.5f, 5f, 1f, 0f),
        new Vector4(-1.1f, 3.5f, 1.125f, 0f),
    };

    Vector4[] characterSoftShadowDirection2 = new Vector4[]
    {
        new Vector4(1.75f, 1.5f, 0.75f, 0f),
        new Vector4(-1.25f, 1.5f, 0.4f, 0f),
        new Vector4(1.1f, 1.75f, 1.25f, 0f),
    };
    public GameObject[] ReflectionProbes;

    int[] fitnessRoomFloorIndex = new int[]
    {
        0,0,1
    };
    float[] fitnessRoomFloorGroundY = new float[]
    {
        0,0,0.05f
    };


    public RenderTexture _jerseyTexture;
    RenderTexture _socksTexture;
    RenderTexture _keeperJerseyTexture;
    RenderTexture _keeperSocksTexture;
    private GameObject playerObj;
    float oldBloom;

    private LightmapDataView lightmapDataView;
    public override async GTask Awake()
    {
        //await base.Awake();
        fitnessRoomFloors = new GameObject[2];
        fitnessRoomFloors[0] = this.transform.Find("StaticModel/FitnessRoom01/FitnessRoom_Floor").gameObject;<fim_suffix>01/FitnessRoom_Grass").gameObject;

        CharacterLight = this.transform.Find("Lights_player/Directional Light").GetComponent<Light>();

        ReflectionProbes = new GameObject[3];
        for (int i = 0; i < ReflectionProbes.Length; i++)
        {
            ReflectionProbes[i] = this.transform.Find(string.Format("ReflectionProbe0{0}", i + 1)).gameObject;
        }


        lightmapDataView = this.transform.GetComponent<LightmapDataView>();

        oldBloom = Camera.main.GetComponent<CameraRenderPassSetting>().BloomIntensity;
        Camera.main.GetComponent<CameraRenderPassSetting>().BloomIntensity = 0.5f;

        SceneCameraController.instance.EnterRoom(true);
        isPlayerSetStarted = false;

        await InitPlayer();

        Platform.EventDispatcher.AddEventListener<PlayersInfoTransfer, bool, string>(FitnessRoomPlayer, SetFitnessRoomPlayer);
        Platform.EventDispatcher.AddEventListener(FitnessRoomPlayerUpgrade, delegate
        {
            fitnessRoomPlayer.PlayerUpgrade();
        });
    }

    public override async void OnEnable()
    {
        base.OnEnable();
        await GAsync.WaitNextFrame();
        lightmapDataView?.ReconstructLightProbeData();
        lightmapDataView?.RestoreLightMapData();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        lightmapDataView?.ResetDefaultLightProbe();
        lightmapDataView?.UnloadLightMapDatas();
    }


    private async void SetFitnessRoomPlayer(PlayersInfoTransfer player, bool isKeeper, string playerName)
    {
        //player_id:配置文件中的player_id，可以用来查配置
        //headModel_id:配置文件中的headmodel值，用来表示头部3D模型的文件名
        //isKeeper:是守门员吗?
        //skinColor:肤色枚举值，取值规则与PlayerCfg相同
        //bodyHeight:身高
        if (fitnessRoomPlayer!= null &&!isSetting)
        {
            isSetting = true;
            //fitnessRoomPlayer.gameObject.SetActive(false);
            await PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(new string[] { player.HeadModel_id.ToString() });
            await fitnessRoomPlayer.SetOneHighPolyPlayer(player.Player_id, int.Parse(player.HeadModel_id), isKeeper, (SkinColorForTexture)player.SkinColor, player.SkinColorCorrectionValue,(int)player.BodyHeight, player.Shoe_id, player.BodyWeight, playerName, player.PlayerNumber, null, true);
     


            fitnessRoomPlayer.gameObject.SetActive(true);
            isSetting = false;
        }


    }

    private async GTask InitPlayer()
    {
        if (fitnessRoomPlayer == null)
        {
            var req = await ResourceMgr.Instance.InstantiateAsync("3D/Upgrade/HighPolyPlayerRoot");
            //if (this == null)
            //{ //防止来回切换健身房创建两个球员
            //    ResourceMgr.Instance.UnloadGameObject(req.result);
            //    return;
            //}
            playerObj = req.result;
            playerObj.transform.SetParent(this.transform);
            int index = PlatformHotfix.SceneZoomManager.Instance.UpgradeCurveIndex;
            SetReflectionProbe(index);
            playerObj.transform.localPosition = localPos[index];
            playerObj.transform.localEulerAngles = localEulerAngles[index];
            playerObj.SetActive(false);
            CharacterLight.transform.localEulerAngles = characterLightLocalEuler[index];
            if (GalaShaderGlobalSetting.CurQualitySetting.ShaderQuality > 2)
                CharacterLight.intensity = characterLightIntensity[index];
            else
                CharacterLight.intensity = 1.4f;

            CharacterLight.intensity = characterLightIntensity[index];
            fitnessRoomPlayer = RenderingHelper.Instance.GetComponent<FitnessRoomPlayer_Hotfix>(playerObj);
            Vector4 shadowDir1 = characterSoftShadowDirection1[index];
            Vector4 shadowDir2 = characterSoftShadowDirection2[index];
            await fitnessRoomPlayer.Init_Hotfix(fitnessRoomFloors[fitnessRoomFloorIndex[index]], shadowDir1, shadowDir2, fitnessRoomFloorGroundY[index]);
            
        }
        //await SetPlayerClothesTexture(DefaultPlayerNumber);
    }

    // private async GTask SetPlayerClothesTexture(string playerNum)
    // {
    //     //设置球衣贴图
    //     if (string.IsNullOrEmpty(playerNum))
    //     {
    //         playerNum = DefaultPlayerNumber;
    //     }
    //     if (string.IsNullOrEmpty(PlayerNumber) ||!PlayerNumber.Equals(playerNum))
    //     {
    //         PlayerNumber = playerNum;

    //         System.Tuple<RenderTexture, RenderTexture> t = (await PlayerJerseyGenerator_Hotfix.GetJerserTexture(playerNum)).result;
    //         TAGlobalData.Instance.ReleaseTexture(_jerseyTexture, _socksTexture);
    //         _jerseyTexture = t.Item1;
    //         _socksTexture = t.Item2;
    //         _keeperJerseyTexture = _jerseyTexture;
    //         _keeperSocksTexture = _socksTexture;
    //         fitnessRoomPlayer.InitTeamClothesTexture(_jerseyTexture, _keeperJerseyTexture, _socksTexture, _keeperSocksTexture);
    //     }
    // }

    const string DefaultPlayerNumber = "10";
    string PlayerNumber;

    void SetReflectionProbe(int index)
    {
        for (int i = 0; i < ReflectionProbes.Length; i++)
            ReflectionProbes[i].SetActive(false);
        ReflectionProbes[index].SetActive(true);
    }

    public override void OnDestroy()
    {
        if (SceneCameraController.instance!= null)
        {
            SceneCameraController.instance.EnterRoom(false);
        }

        //if (fitnessRoomPlayer!= null)
        //{
        //    fitnessRoomPlayer.gameObject.DestroySelf();
        //}

        TAGlobalData.Instance.ReleaseTexture(_jerseyTexture, _socksTexture);
        _socksTexture = null;
        _jerseyTexture = null;
        _keeperSocksTexture = null;
        _keeperJerseyTexture = null;
        fitnessRoomPlayer = null;
        if (playerObj!= null)
        {
            //playerObj.DestroySelf();
            ResourceMgr.Instance.UnloadGameObject(playerObj);
            playerObj = null;
        }
        Platform.EventDispatcher.RemoveEvent(FitnessRoomPlayer);
        Platform.EventDispatcher.RemoveEvent(FitnessRoomPlayerUpgrade);
        if (Camera.main!= null)
        {
            var cameraRenderPassSetting = Camera.main.GetComponent<CameraRenderPassSetting>();
            if (cameraRenderPassSetting!= null)
                cameraRenderPassSetting.BloomIntensity = oldBloom;
        }
    }
}
<fim_middle>
        fitnessRoomFloors[1] = this.transform.Find("StaticModel/FitnessRoom<|endoftext|>