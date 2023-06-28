<fim_prefix>﻿using Framework.GalaSports.Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GalaSports.FootballNG;
using UnityEngine.AddressableAssets;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class HighPlayerController
{
    GameObject highPlayerTemplate;

    public class HighPlayerInfo
    {
        public int index;
        public string team_id;
        public int player_id;
        public int headModel_id;
        public bool isKeeper;
        public bool isHome;
        public SkinColorForTexture skinColor;
        public Vector3 skinColorCorrectionValue;
        public int bodyHeight;
        public Vector3 worldPosition;
        public float worldEulerY;
        public int bodyWeight = 0;
        public int shoe_id;

        public HighPlayerInfo(int index, string team_id, int player_id, int headModel_id, bool isKeeper, bool isHome, SkinColorForTexture skinColor, Vector3 skinColorCorrectionValue, int shoeId, int bodyHeight, Vector3 worldPosition, float worldEulerY, bool needReloadRes, int bodyWeight)
        {
            this.index = index;
            this.team_id = team_id;
            this.player_id = player_id;
            this.headModel_id = headModel_id;
            this.isKeeper = isKeeper;
            this.isHome = isHome;
            this.skinColor = skinColor;
            this.skinColorCorrectionValue = skinColorCorrectionValue;
            this.bodyHeight = bodyHeight;
            this.worldPosition = worldPosition;
            this.worldEulerY = worldEulerY;
            this.bodyWeight = bodyWeight;
            this.shoe_id = shoeId;
        }
    }

    public GameObject gameObject;
    public Transform transform;

    List<HighPlayerInfo> playerInfos;

    //场上22个球员的高模
    public List<Transform> HighPolyPlayers = new List<Transform>();

    class HighPolyPlayersAsyncStatus
    {
        public int index;
        public bool loaded;
    }

    public void Awake()
    {
        playerInfos = new List<HighPlayerInfo>();
        Platform.EventDispatcher.AddEventListener("ClearEnterPlayerHighPlayers", ClearHighPolyPlayers);
        highPlayerTemplate = ResourceMgr.Instance.LoadAsset<GameObject>("3D/MatchEnterPlayer");
    }

    public void InitOneHighPolyPlayerInfo(int index, string team_id, int player_id, int headModel_id, bool<fim_suffix>Value, int shoe_id, int bodyHeight, Vector3 worldPosition, float worldEulerY, int bodyWeight = 0)
    {
        HighPlayerInfo info = new HighPlayerInfo(index, team_id, player_id, headModel_id, isKeeper, isHome, skinColor, skinColorCorrectionValue, shoe_id, bodyHeight, worldPosition, worldEulerY, true, bodyWeight);
        playerInfos.Add(info);
    }

    public HighPlayerInfo GetPlayerInfoFromPlayerIndex(int index)
    {
        HighPlayerInfo info = null;
        for (int i = 0; i < playerInfos.Count; i++)
        {
            if (index == playerInfos[i].index)
            {
                info = playerInfos[i];
            }
        }
        return info;
    }

    Transform PrepareOneHighPolyPlayerRes(int index, bool isAdjustStepSize = false)
    {
        HighPlayerInfo info = null;
        for (int i = 0; i < playerInfos.Count; i++)
        {
            if (index == playerInfos[i].index)
            {
                info = playerInfos[i];
            }
        }

        GameObject tempPlayerObj = GameObject.Instantiate(highPlayerTemplate);
        tempPlayerObj.transform.parent = transform;
        HighPolyPlayers.Add(tempPlayerObj.transform);

        tempPlayerObj.transform.localPosition = new Vector3(info.worldPosition.x, 0, info.worldPosition.z);
        tempPlayerObj.transform.localEulerAngles = new Vector3(0, info.worldEulerY, 0);

        tempPlayerObj.name = "TeamPlayer" + index.ToString();

        TeamConfigurationData teamdata = TeamDataManager.Instance.GetTeamDataByTeamId(info.team_id);
        GameJerseyData jerseyData = TeamDataManager.Instance.GetJerseyData(info.team_id, info.player_id);
        //DebugEX.LogError(index + " " + jerseyData.PlayerName + " " + jerseyData.PlayerNumber);
        //DebugEX.LogError(jerseyData.JerseyId);
        //PlayerJerseyGenerator.Instance.Initialize();
        if (teamdata.teamLogo!= null && teamdata.teamLogo.texture!= null)
        {
            PlayerJerseyGenerator.Instance.TeamLogoTex = teamdata.teamLogo.texture;
        }

        HighPolyPlayerAppearanceController playerAppearanceController = tempPlayerObj.GetComponent<HighPolyPlayerAppearanceController>();
        HighPolyPlayerAnimationController playerAnimationController = tempPlayerObj.GetComponent<HighPolyPlayerAnimationController>();

        playerAppearanceController.index = index;
        playerAppearanceController.SetPlayerAppearance(info.headModel_id, info.bodyHeight, info.bodyWeight, info.skinColor, info.skinColorCorrectionValue, jerseyData.JerseyId, info.shoe_id, info.player_id, info.isKeeper, jerseyData.PlayerName, jerseyData.PlayerNumber.ToString(), false, null, info.isHome);
        playerAnimationController.Init(true, false);

        tempPlayerObj.transform.localPosition = new Vector3(0, 0, 500);
        tempPlayerObj.SetActive(false);
        tempPlayerObj.transform.parent = transform;
        HighPolyPlayers.Add(tempPlayerObj.transform);

        return tempPlayerObj.transform;
    }


    //index:场上22个人的唯一标记id，取值0~21
    public Transform GetPlayerByIndex(int index, bool isAdjustStepSize = false)
    {
        for (int i = 0; i < playerInfos.Count; i++)
        {
            if (index == playerInfos[i].index)
            {
                return PrepareOneHighPolyPlayerRes(index, isAdjustStepSize);
            }
        }
        return null;
    }

    void ClearHighPolyPlayers()
    {
        for (int i = 0; i < HighPolyPlayers.Count; i++)
        {
            if (HighPolyPlayers[i]!= null && HighPolyPlayers[i].gameObject!= null)
                GameObject.Destroy(HighPolyPlayers[i].gameObject);
        }
        playerInfos.Clear();
        HighPolyPlayers.Clear();

#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && (!UNITY_ANDROID) && (!UNITY_IOS)
        //PC、Mac平台在比赛内使用高模，因此不能调用UnloadAll方法
#else
        PlayerJerseyDataLoader.UnloadAllPreLoadedJerseyDatas();
        PlayerSockDataLoader.UnloadAllPreLoadedSockDatas();
        PlayerHeadDataLoader.UnloadAllPreLoadedHeadDatas();       
#endif
    }


    public void OnDestroy()
    {
        ClearHighPolyPlayers();
        Platform.EventDispatcher.RemoveEventListener("ClearEnterPlayerHighPlayers", ClearHighPolyPlayers);
        ResourceMgr.Instance.UnloadAsset(highPlayerTemplate);
    }
}
//#endif<fim_middle> isKeeper, bool isHome, SkinColorForTexture skinColor, Vector3 skinColorCorrection<|endoftext|>