<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HonorRoom3DView : View3DBase
{

    [System.NonSerialized]
    public RenderTexture rt;

    #region 私有成员变量
    string PlayerNumber;
    const string DefaultPlayerNumber = "10";
    RenderTexture _jerseyTexture;
    RenderTexture _socksTexture;

    private FitnessRoomPlayer_Hotfix fitnessRoomPlayer;
    private Camera playerCamera;

    private LightProbesData ProbesData;
    #endregion

    // playerShowData[0] = curPlayerNum;
    // playerShowData[1] = headModel_id;
    // playerShowData[2] = bodyHeight;
    // playerShowData[3] = color;
    // playerShowData[4] = boots;
    // playerShowData[5] = bodyWeight;
    // playerShowData[6] = shirtNumber;

    public override async GTask Awake()
    {
        await base.Awake();
        ProbesData = this.transform.GetComponent<LightProbesData>();
        playerCamera = this.transform.Find("PlayerCamera").GetComponent<Camera>();
        fitnessRoomPlayer = RenderingHelper.Instance.GetComponent<FitnessRoomPlayer_Hotfix>(this.transform.Find("HighPolyPlayerRoot").gameObject);

        await fitnessRoomPlayer.Init_Hotfix(null, new Vector4(-1.3f, 4f, 1f, 0f), new Vector4(1.75f, 1.5f, 0.75f, 0f), 0);
        ReconstructLightProbeData();

        StadiumSceneController.Instance<fim_suffix>Level();
        //var size = quality >= 3? Screen.height : Screen.height/2; // 1024 : 512
        var size = Screen.height;
        rt = GalaRenderPipeline.GalaRenderManager.CreateTemporaryRT(size, size, 16, RenderTextureFormat.ARGB32, "honor_room_player");

        playerCamera.targetTexture = rt;
    }

    bool isSetting = false;

    public async void SetPlayerData(PlayersInfoTransfer player, bool isKeeper = false, string PlayerName = "")
    {
        if (fitnessRoomPlayer!= null &&!isSetting)
        {
            isSetting = true;

            await fitnessRoomPlayer.SetOneHighPolyPlayer(player.Player_id, int.Parse(player.HeadModel_id), isKeeper, (SkinColorForTexture)player.SkinColor, player.SkinColorCorrectionValue, (int)player.BodyHeight, player.Shoe_id, player.BodyWeight, "GALA", player.PlayerNumber);

            fitnessRoomPlayer.gameObject.SetActive(true);

            isSetting = false;
        }
    }

    private async GTask SetPlayerJerseyTexture(string playerNum)
    {
        //设置球衣贴图
        if (string.IsNullOrEmpty(playerNum))
        {
            playerNum = DefaultPlayerNumber;
        }
        if (string.IsNullOrEmpty(PlayerNumber) ||!PlayerNumber.Equals(playerNum))
        {
            PlayerNumber = playerNum;
            TAGlobalData.Instance.ReleaseTexture(_jerseyTexture, _socksTexture);

            //System.Tuple<Texture2D, Texture2D> t = (await TAGlobalData.Instance.GetJerserTexture(playerNum)).result;
            System.Tuple<RenderTexture, RenderTexture> t = (await PlayerJerseyGenerator_Hotfix.GetJerserTexture(playerNum)).result;
            _jerseyTexture = t.Item1;
            _socksTexture = t.Item2;

            fitnessRoomPlayer.InitTeamClothesTexture(_jerseyTexture, _jerseyTexture, _socksTexture, _socksTexture);
        }
    }


    private void ReconstructLightProbeData()
    {
        if (ProbesData.Probes!= null &&!isSetLightProbe)
        {
            isSetLightProbe = true;
            defaultLightProbe = LightmapSettings.lightProbes;
            LightmapSettings.lightProbes = ProbesData.Probes;
        }
    }

    private LightProbes defaultLightProbe;
    private bool isSetLightProbe;
    private void ReSetLightProbeData()
    {
        if(defaultLightProbe!= null && isSetLightProbe)
        {
            LightmapSettings.lightProbes = defaultLightProbe;
            isSetLightProbe =false;
        }
    }
    public override void OnDestroy()
    {
        TAGlobalData.Instance.ReleaseTexture(_jerseyTexture, _socksTexture);
        _jerseyTexture = null;
        _socksTexture = null;
        if (StadiumSceneController.Instance!= null && StadiumSceneController.Instance.StadiumLight!= null)
        {
            StadiumSceneController.Instance.StadiumLight.SetActive(true);
        }

        GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(rt);
        rt = null;
    }
}
<fim_middle>.StadiumLight.SetActive(false);

        await SetPlayerJerseyTexture(DefaultPlayerNumber);

        //设置球员位置
        fitnessRoomPlayer.SetPlayerPosition(new Vector3(0, 0, 0));

        //