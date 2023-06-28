<fim_prefix>using System.Collections.Generic;
using UnityEngine;
using Framework.GalaSports.Service;
public struct HotFixLowPlayerInfo
{
    public Texture jerseyTex;
    public bool isGK;
    public string playerName;
}
public struct HotFixHighPlayerInfo
{
    public string teamId;
    public int playerId;
    public int headId;
    public string playerName;
    public string playerNumber;
    public int playerHeight;
    public int playerWeight;
    public SkinColorForTexture playerSkinColor;
    public string jerseyCode;
    public int shoesId;
    public bool isGK;
    public int position;
    public bool isStarter; // 是否是首发:true:是   false:替补
    public bool isCaptial;
    public Vector3 skinColorCorrectionValue;
}
public class HotFixHighPolyPlayerInstantiate
{
    public List<GameObject> highPolyPlayers;
    public List<string> jerseyCodes;
    public List<int> headIds;
    public GameObject gameObject;
    public Transform transform;
    bool isCharactersDestroyed = false;
    // Start is called before the first frame update

    public void Init()
    {
        highPolyPlayers = new List<GameObject>();
        jerseyCodes = new List<string>();
        headIds = new List<int>();
    }
    void InitData()
    {
        for(int i=0;i<1;i++)
        { //以下数据在正式生产环境中应该由前端传给TA
            int playerHeadModelId = playerHeadModelIds3[i]; //脸模3D资源的id
            int playerHeight = playerHeights3[i]; //身高
            int playerWeight = playerWeights3[i]; //体重，影响球员体型胖瘦
            SkinColorForTexture playerSkinColor = playerSkinColors3[i]; //肤色
            string jerseyCode = PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(jerseyId, playerShirtNumbers3[i].ToString(), playerNames3[i]); //球衣Id字符串，这里使用ApplyNameAndNumberToJerseyId方法把球员名字和球员号码拼到球衣Id字符串中，这样就可以生成具有对应名字和号码的球衣贴图
            int playerShoesType = PlayerShoeTypes3[i]; //球员鞋子样式
            int playerId = playerCfgIds3[i]; //球员在配置表里的id
            bool isGK = playerIsGKs3[i]; //是不是守门员，守门员会戴手套，球衣也不一样
            string playerName = playerNames3[i]; //球员名字
            string playerShirtNumber = playerShirtNumbers3[i].ToString(); //球衣号码
            //以上数据在正式生产环境中应该由前端传给TA   
        }
       

    }
    //1.a
    //预加载人物相关资源（同步执行）,预加载完成后，实例化人物就不会再产生加载资源的耗时
    //这里使用的仍然是异步的LoadAssetAsync，只是外部调用是同步的，人数多的时候会造成主线程卡顿。人数<=3人时，或者有读条界面时可以调用这个方法
    public void PreloadCharacterResources(List<HotFixHighPlayerInfo> playerInfos, string[] clothNames)
    {
        isCharactersDestroyed = false;
        headIds.Clear();
        jerseyCodes.Clear();
        for (int i = 0; i < playerInfos.Count; i++)
            headIds.Add(playerInfos[i].headId);
        PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(headIds.ToArray());
        ClothDataLoader.AsyncPreLoadMultiClotDataSet(clothNames);
        for (int i = 0; i <playerInfos.Count; i++)
        {
            if (playerInfos[i].isGK)
            {
                //守门员目前是写死的球衣，不需要额外生成
                continue;
            }
            string jerseyCode = PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(playerInfos[i].jerseyCode, playerInfos[i].playerNumber, playerInfos[i].playerName);
            jerseyCodes.Add(jerseyCode);
            PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(jerseyCode);
            PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(jerseyCode);
        }
    }

    //2.a
    //实例化人物（同步执行），人数<=3人时可以调用这个方法
    //如果没有执行预加载资源的步骤，就会在实例化人物的时候执行加载资源，产生耗时
    //初始化阶段的设置材质属性耗时较长，如果人数较多，会导致主线程卡顿
    public void InstantiateCharacters(List<HotFixHighPlayerInfo> playerInfos,string playerInstantiatePath, RuntimeAnimatorController runtimeAnimator)
    {
        isCharactersDestroyed = false;
        for (int i = 0; i < playerInfos.Count; i++)
        {
            GameObject player =ResourceMgr.Instance.Instantiate(playerInstantiatePath) ;

            #region 定义球员外观
            HighPolyPlayerAppearanceController playerAppearance = player.GetComponent<HighPolyPlayerAppearanceController>();
            playerAppearance.SetPlayerAppearance(playerInfos[i].headId, playerInfos[i].playerHeight, playerInfos[i].playerWeight, playerInfos[i].playerSkinColor, playerInfos[i].skinColorCorrectionValue, playerInfos[i].jerseyCode, playerInfos[i].shoesId, playerInfos[i].playerId, playerInfos[i].isGK, playerInfos[i].playerName,
               playerInfos[i].playerNumber);
            HighPolyPlayerAnimationController playerAnimation = player.GetComponent<HighPolyPlayerAnimationController>();
            //如果GameObject player上没有挂载默认Animator，需要另外指定animator，就用下面这行代码指定
            playerAnimation.MyAnimator.runtimeAnimatorController = runtimeAnimator;
            playerAnimation.Init(true, false);
            playerAnimation.HideFootball();
            #endregion
            highPolyPlayers.Add(player);
        }
    }


    //1.b
    //预加载人物相关资源（异步执行）,预加载完成后，实例化人物就不会再产生加载资源的耗时
    //基本上不会引起主线程卡顿，但实际加载耗时会比同步执行更长，按需选择。人数>3人且没有读条界面时建议调用这个方法
    public async GTask PreloadCharacterResourcesAsync(List<HotFixHighPlayerInfo> playerInfos, string[] clothNames)
    {
        isCharactersDestroyed = false;
        headIds.Clear();
        jerseyCodes.Clear();
        for (int i = 0; i < playerInfos.Count; i++)
            headIds.Add(playerInfos[i].headId);
        await PlayerHeadDataLoader.AsyncPreLoadMultiHeadDataSet(headIds.ToArray());
        await<fim_suffix> i = 0; i < playerInfos.Count; i++)
        {
            if (playerInfos[i].isGK)
            {
                //守门员目前是写死的球衣，不需要额外生成
                continue;
            }
            string jerseyCode = PlayerJerseyGenerator.ApplyNameAndNumberToJerseyId(playerInfos[i].jerseyCode, playerInfos[i].playerNumber, playerInfos[i].playerName);
            jerseyCodes.Add(jerseyCode);
            await PlayerJerseyDataLoader.AsyncPreLoadOneJerseyDataSet(jerseyCode);
            await PlayerSockDataLoader.AsyncPreLoadOneSockDataSet(jerseyCode);
            if (isCharactersDestroyed)
            {
                //防止人物销毁后，资源预加载的线程继续执行
                return;
            }
        }
    }

    //2.b
    //实例化人物（异步执行），人数>3人时建议调用这个方法
    //如果没有执行预加载资源的步骤，就会在实例化人物的时候执行加载资源，产生耗时
    //初始化阶段的设置材质属性耗时较长，因此每实例化一个人物都WaitForNextFrame，将这个开销平摊到不同的帧，防止主线程卡顿
    public async void InstantiateCharactersAsync(List<HotFixHighPlayerInfo> playerInfos, string playerInstantiatePath,Transform parent)
    {
        isCharactersDestroyed = false;
        for (int i = 0; i < playerInfos.Count; i++)
        {
            GameObject player = (await ResourceMgr.Instance.InstantiateAsync(playerInstantiatePath)).result;
            player.transform.name = "TeamPlayer" + i.ToString();
            player.SetActive(false);
            #region 定义球员外观
            HighPolyPlayerAppearanceController playerAppearance = player.GetComponent<HighPolyPlayerAppearanceController>();
            playerAppearance.SetPlayerAppearance(playerInfos[i].headId, playerInfos[i].playerHeight, playerInfos[i].playerWeight, playerInfos[i].playerSkinColor, playerInfos[i].skinColorCorrectionValue, playerInfos[i].jerseyCode, playerInfos[i].shoesId, playerInfos[i].playerId, playerInfos[i].isGK, playerInfos[i].playerName,
                playerInfos[i].playerNumber);
            #endregion
            HighPolyPlayerAnimationController playerAnimation = player.GetComponent<HighPolyPlayerAnimationController>();
            //如果GameObject player上没有挂载默认Animator，需要另外指定animator，就用下面这行代码指定
            playerAnimation.Init(true, false);
            playerAnimation.HideFootball();
            //把已经实例化的人物挪到视线外
            player.transform.localPosition = new Vector3(0, 0, 500);
            player.transform.localEulerAngles = Vector3.zero;
            player.transform.parent = parent;
            highPolyPlayers.Add(player);
          //  await GAsync.WaitNextFrame();

            if (isCharactersDestroyed)
            {
                //防止异步执行时，在调用销毁方法后仍然有人物实例化出来
                GameObject.Destroy(player);
                highPolyPlayers.Clear();
                return;
            }
        }
    }

    public async GTask InstantiateCharactersAsyncGTask(List<HotFixHighPlayerInfo> playerInfos, string playerInstantiatePath, Transform parent)
    {
        isCharactersDestroyed = false;
        for (int i = 0; i < playerInfos.Count; i++)
        {
            GameObject player = (await ResourceMgr.Instance.InstantiateAsync(playerInstantiatePath)).result;
            player.transform.name = "TeamPlayer" + i.ToString();
            player.SetActive(false);
            #region 定义球员外观
            HighPolyPlayerAppearanceController playerAppearance = player.GetComponent<HighPolyPlayerAppearanceController>();
            playerAppearance.SetPlayerAppearance(playerInfos[i].headId, playerInfos[i].playerHeight, playerInfos[i].playerWeight, playerInfos[i].playerSkinColor, playerInfos[i].skinColorCorrectionValue, playerInfos[i].jerseyCode, playerInfos[i].shoesId, playerInfos[i].playerId, playerInfos[i].isGK, playerInfos[i].playerName,
                playerInfos[i].playerNumber);
            #endregion
            HighPolyPlayerAnimationController playerAnimation = player.GetComponent<HighPolyPlayerAnimationController>();
            //如果GameObject player上没有挂载默认Animator，需要另外指定animator，就用下面这行代码指定
            playerAnimation.Init(true, false);
            playerAnimation.HideFootball();
            //把已经实例化的人物挪到视线外
            player.transform.localPosition = new Vector3(0, 0, 500);
            player.transform.localEulerAngles = Vector3.zero;
            player.transform.parent = parent;
            highPolyPlayers.Add(player);
            //  await GAsync.WaitNextFrame();

            if (isCharactersDestroyed)
            {
                //防止异步执行时，在调用销毁方法后仍然有人物实例化出来
                GameObject.Destroy(player);
                highPolyPlayers.Clear();
                return;
            }
        }
    }
    //一个镜头的布料会一起卸载，不需要在卸载球员的时候单独处理
    public void UnloadCharaters(int startIndex,int endIndex)
    {
        for (int i =startIndex; i <endIndex; i++)
        {
            if (highPolyPlayers[i]!= null&&endIndex<highPolyPlayers.Count)
            {
                ResourceMgr.Instance.UnloadGameObject(highPolyPlayers[i].gameObject);
                GameObject.Destroy(highPolyPlayers[i]);
                highPolyPlayers[i] = null;
            }
        }
    }
    //3.
    //销毁人物并卸载资源的方法，可以在OnDestroy时调用，也可以手动调用
    public void DestroyCharaters()
    {
        for (int i = 0; i < highPolyPlayers.Count; i++)
        {
            if (highPolyPlayers[i]!= null)
            {
                ResourceMgr.Instance.UnloadGameObject(highPolyPlayers[i].gameObject);
                GameObject.Destroy(highPolyPlayers[i]);
                highPolyPlayers[i] = null;
            }
        }
        isCharactersDestroyed = true;
        if(highPolyPlayers!=null)
          highPolyPlayers.Clear();
        ClothDataLoader.UnloadAllPreLoadedClothDatas();
        //Destroy所有player GameObject后，就已经把所有人物相关的资源卸载干净了，如果没有，找黄智晖修bug


        //异步实例化人物的时候，可能人物没实例化出来，就调用了销毁人物的方法。这时可能会有预加载的资源未被人物引用到，自然不会随着人物销毁而卸载。因此我们要手动卸载引用计数为0的资源
        PlayerHeadDataLoader.UnloadUnreferencedHeadData();
        PlayerJerseyDataLoader.UnloadUnreferencedJerseyDatas();
        PlayerSockDataLoader.UnloadUnreferencedSockDatas();

        PlayerHeadDataLoader.UnloadMultiPreLoadedHeadDatas(headIds.ToArray());
        PlayerJerseyDataLoader.UnloadMultiPreLoadedJerseyDatas(jerseyCodes.ToArray());
        //下面两句会强制卸载所有人物相关的资源，如果你确定一个阶段结束了（比如退出登录场景进入主界面、退出主界面进入比赛场景、热身动画结束等等），可以调用以下方法，避免有人物资源没卸载掉。
        //但是一般情况下，只要player的GameObject被销毁，就会把资源一并卸载干净。以下的方法只是一种保底措施。
        //如果在场景内还有其他高模的情况下调用，会导致这些高模的资源丢失。
        //PlayerHeadDataLoader.UnloadAllPreLoadedHeadDatas();
        //PlayerJerseyDataLoader.UnloadAllPreLoadedJerseyDatas();
        if (headIds!=null)
          headIds.Clear();
        if(jerseyCodes!=null)
          jerseyCodes.Clear();
       // GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        DestroyCharaters();
    }



    // Update is called once per frame


    #region 写死的球员配置数据
    static int[] playerCfgIds = new int[11]
    {
        1,2,3,4,8,10,26,5,19,7,13
    };
    static int[] playerHeadModelIds = new int[11]
    {
        1,2,4,5,19,8,22,6,35,7,11
    };
    static SkinColorForTexture[] playerSkinColors = new SkinColorForTexture[11]
    {
        SkinColorForTexture.WHITE,
        SkinColorForTexture.ASIAN,
        SkinColorForTexture.ASIAN,
        SkinColorForTexture.WHITE,
        SkinColorForTexture.ASIAN,
        SkinColorForTexture.LATIN,
        SkinColorForTexture.WHITE,
        SkinColorForTexture.WHITE,
        SkinColorForTexture.BLACK,
        SkinColorForTexture.ASIAN,
        SkinColorForTexture.WHITE
    };
    static string[] playerNames = new string[11]
    {
        "MESSI",
        "RONALDO",
        "NEYMAR",
        "DE BRUYNE",
        "SALAH",
        "MBAPPE",
        "BENZEMA",
        "OBLAK",
        "KANTE",
        "VAN DIJK",
        "KANE",
    };
    static int[] playerShirtNumbers = new int[11]
    {
        30,7,10,17,11,7,9,13,7,4,10
    };
    static int[] PlayerShoeTypes = new int[11]
    {
        1,2,3,4,3,1,2,1,2,2,3
    };
    static int[] playerHeights = new int[11]
    {
        170,187,175,180,175,177,185,187,167,193,187
    };
    static int[] playerWeights = new int[11]
    {
        72,83,68,69,71,73,81,87,69,92,88
    };
    static bool[] playerIsGKs = new bool[11]
    {
        false,false,false,false,false,false,false,true,false,false,false
    };
    static string[] playerAnimationNames = new string[11]
    {
        "Cultivate_Idle1",
        "Cultivate_Idle1_SmallMotion1",
        "Cultivate_Idle1_SmallMotion2",
        "Cultivate_Idle1_SmallMotion3",
        "Cultivate_Idle1_SmallMotion4",
        "Cultivate_Idle1_SmallMotion5",
        "Cultivate_Idle1_Upgrade1",
        "Cultivate_Idle1_Upgrade2",
        "Cultivate_Idle1_Upgrade3",
        "Cultivate_Idle1_Upgrade4",
        "Cultivate_Idle1_Upgrade5"
    };
    static string[] clothDataNames = new string[11]
    {
        "Idle1",
        "Idle1_SmallMotion1",
        "Idle1_SmallMotion2",
        "Idle1_SmallMotion3",
        "Idle1_SmallMotion4",
        "Idle1_SmallMotion5",
        "Idle1_Upgrade1",
        "Idle1_Upgrade2",
        "Idle1_Upgrade3",
        "Idle1_Upgrade4",
        "Idle1_Upgrade5"
    };
    static string jerseyId = "10_GALASPORTS_201f40FF_201f40FF_201f40FF_0_20222023_22_5289beff_5a191fFF_dbdbdbff";



    static int[] playerCfgIds3 = new int[3]
{
        1,2,3
};
    static int[] playerHeadModelIds3 = new int[3]
    {
        1,2,4
    };
    static SkinColorForTexture[] playerSkinColors3 = new SkinColorForTexture[3]
    {
        SkinColorForTexture.WHITE,
        SkinColorForTexture.ASIAN,
        SkinColorForTexture.ASIAN,
    };
    static string[] playerNames3 = new string[3]
    {
        "MESSI",
        "RONALDO",
        "NEYMAR",
    };
    static int[] playerShirtNumbers3 = new int[3]
    {
        30,7,10,
    };
    static int[] PlayerShoeTypes3 = new int[3]
    {
        1,2,3,
    };
    static int[] playerHeights3 = new int[3]
    {
        170,187,175
    };
    static int[] playerWeights3 = new int[3]
    {
        72,83,68,
    };
    static bool[] playerIsGKs3 = new bool[3]
    {
        false,false,false,
    };
    static string[] playerAnimationNames3 = new string[3]
    {
        "Cultivate_Idle1",
        "Cultivate_Idle1_SmallMotion1",
        "Cultivate_Idle1_SmallMotion2",
    };
    static string[] clothDataNames3 = new string[3]
    {
        "Idle1",
        "Idle1_SmallMotion1",
        "Idle1_SmallMotion2",
    };
    #endregion
}
<fim_middle> ClothDataLoader.AsyncPreLoadMultiClotDataSet(clothNames);
        for (int<|endoftext|>