<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeMatch3DView : View3DBase
{

    //d1d1d1ff_d1d1d1ff_d1d1d1ff_0_20212022_22_538bc1ff_d8d8d8ff_538bc1ff
    //201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff
    static string[] JerseyIds = new string[] {
        "3_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff",
        "6_GALASPORTS_ffffffff_ffffffff_ffffffff_3_20202021_1_052134ff_d32f2fff_052134ff",
        "7_GALASPORTS_ffffffff_ffffffff_ffffffff_3_20202021_1_052134ff_d32f2fff_052134ff",
        "31_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff", //守门员
        "10_GALASPORTS_ffffffff_ffffffff_ffffffff_3_20202021_1_052134ff_d32f2fff_052134ff",
        "17_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff",

    };
    static Vector3 [] footBallPos = new Vector3[3]{
        new Vector3(-0.6f,0.18f,-1.3f),
        new Vector3(58.058f,1.833f,47.439f),
        new Vector3(20.5f,0.2f,3.33f) 
    };

     //队徽
    private Texture2D TeamLogoTex;
    [SerializeField]
    private Transform[] Players;
    private Texture[] Shoes;

    [SerializeField]
    private Transform footBallTrans;

    private PlayerTextures playerTextures;
    
    HighPolyPlayerAppearanceController[] playersAppearance;

    List<RenderTexture> JerseyTexList = new List<RenderTexture>();

     //守<fim_suffix>erseytex; //球衣
    private Texture2D GoalKeeper_Socktex; //袜子

    private BezierCurve curCurve;

    static string [] PlayersPath = new string []{"1A","1B","2A","2B","3A","3B"};
    public override async GTask Awake()
    {
        await base.Awake();
        playerTextures = this.gameObject.GetComponent<PlayerTextures>();
        TeamLogoTex = playerTextures.TeamLogoTex;
        Shoes = playerTextures.Shoes;
        GoalKeeper_Jerseytex = playerTextures.GoalKeeper_Jerseytex;
        GoalKeeper_Socktex = playerTextures.GoalKeeper_Socktex;

        footBallTrans = this.transform.Find("FootballRoot");

        Players = new Transform[PlayersPath.Length];
        for(int i = 0; i< Players.Length;i++)
        {
            Players[i] = this.transform.Find(PlayersPath[i]);
            Players[i].gameObject.SetActive(false);
        }
    }

    public override async GTask Start()
    {
        PlayerJerseyGenerator.Instance.Initialize();

        playersAppearance = new HighPolyPlayerAppearanceController[Players.Length];

        List<GTask> taskList = new List<GTask>(); 
        for (int i = 0; i < Players.Length; i++)
        {
            playersAppearance[i] = Players[i].GetComponent<HighPolyPlayerAppearanceController>();

            if (i == GoalKeeperId)
            {
                playersAppearance[i].IsKeeper = true;
                playersAppearance[i].SetGlove(true);
            }

            GTask task = CreateJersey(i);
            taskList.Add(task);
            // playersAppearance[i].HeadMR.material.SetFloat("_Smoothness", 1.42f);
            // playersAppearance[i].LimbGPUSKM.material.SetFloat("_Smoothness", 1.15f);
        }

         await GAsync.WaitUntil(() => {
            bool IsCompleted = true;
            for(int i = 0; i < taskList.Count;i++)
            {
                IsCompleted = IsCompleted & taskList[i].IsCompleted;
            }
            return IsCompleted;
         });
        PlayerJerseyGenerator.Instance.SetCameraActive(false);
    }

    Vector3 lightLocalEulerAngles;
    public override void OnEnable()
    {
        base.OnEnable();
        StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles = new Vector3(60,60,0);

    }

    public override void OnDisable()
    {
        base.OnDisable();
        StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles = HomeScene3DPlayersController.HomeSceneMainLightEuler;
    }


    async GTask CreateJersey(int playerid)
    {
        
        Material JerseyMat = playersAppearance[playerid].ClothGPUSKM.material;
        Material SockMat = playersAppearance[playerid].LegMR.materials[0];
        if (playerid == GoalKeeperId)
        {
            playersAppearance[playerid].HomeGKJerseyTex = GoalKeeper_Jerseytex;
            playersAppearance[playerid].HomeGKSockTex = GoalKeeper_Socktex;

            JerseyMat.SetTexture("_BaseMap", GoalKeeper_Jerseytex);
            SockMat.SetTexture("_BaseMap", GoalKeeper_Socktex);
        }
        else
        {
            RenderTexture jerseytex;
            RenderTexture socktex;
            PlayerJerseyGenerator.Instance.TeamLogoTex = TeamLogoTex;
            await PlayerJerseyGenerator_Hotfix.SetJerseyInfoAsync(JerseyIds[playerid]);

            jerseytex = PlayerJerseyGenerator_Hotfix.GetDIYTex();
            socktex = PlayerJerseyGenerator_Hotfix.GetSockTex();

            JerseyTexList.Add(jerseytex);
            JerseyTexList.Add(socktex);
            JerseyMat.SetTexture("_BaseMap", jerseytex);
            SockMat.SetTexture("_BaseMap", socktex);
        }

        Material ShoesMat = playersAppearance[playerid].LegMR.materials[1];
        ShoesMat.SetTexture("_BaseMap", Shoes[playerid]);
    }

   

    bool isFrist = true;
    public void Play(int index)
    {
        switch(index)
        {
            case 0:
                curCurve = PlatformHotfix.SceneZoomManager.Instance.PvPMatchCamCurve;
                //Platform.CameraAnimController.Instance.Cam.fieldOfView = 72;
                break;
            case 1:
                curCurve = PlatformHotfix.SceneZoomManager.Instance.PvPMatchCamCurve2;
                //Platform.CameraAnimController.Instance.Cam.fieldOfView = 40;
                break;
            case 2:
                curCurve = PlatformHotfix.SceneZoomManager.Instance.PvPMatchCamCurve3;
                //Platform.CameraAnimController.Instance.Cam.fieldOfView = 40;
                break;
        }

        if(isFrist){
            ReSetCamCurve();
            isFrist = false;
        }else{
            Platform.CameraAnimController.Instance.EaseCurve = curCurve;
            Platform.CameraAnimController.Instance.CurveEaseVal = 0f;
            Platform.CameraAnimController.Instance.SetCurveEaseVal(1,0.3f);
        }

        footBallTrans.localPosition = footBallPos[index];

        for(int i = 0; i< Players.Length;i++)
        {
            if(index * 2 == i || (index*2+1) == i)
            {
                Players[i].gameObject.SetActive(true);
            }else{
                Players[i].gameObject.SetActive(false);
            }
        }

        Platform.EventDispatcher.TriggerEvent("SetShadowForIncident");
    }

    public void ReSetCamCurve()
    {
        Platform.StackViewManager.Instance.SetCamEaseCurve(curCurve);
    }
    public override void OnDestroy()
    {
        foreach(var tex in JerseyTexList)
        {
            GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(tex);
        }
        JerseyTexList.Clear();
    }
}
//#endif<fim_middle>门员id
    private const int GoalKeeperId = 2;

    private Texture2D GoalKeeper_J<|endoftext|>