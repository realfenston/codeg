<fim_prefix>using Framework.GalaSports.Service;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ARPlayerAnimState
{
    None,
    ShoulderHook,//勾肩
    FingerSky,//手指天
    Juggle,//颠球
    ShakeFist,//挥拳
    ThumbUp,//比大拇指
    Akimbo,//叉腰
    DropHand,//双手下垂
    CrossChest,//抱胸
    BackHand,//手背后
}

public class ARShare3DView : View3DBase
{
    GameObject _player;
    Animator _playerAnimator;
    RuntimeAnimatorController _runtimeAnimatorController;
    HighPolyPlayerAppearanceController _playerAppearance;
    HighPolyPlayerAnimationController _playerAnimation;
    Animator _ballAnimator;

    RenderTexture _jerseyTexture;
    RenderTexture _socksTexture;

    Material _TeamJerseyMat;
    Material _TeamShoesMat;
    Material _TeamSockMat;
    Material _SkinMat;
    Material _leftEyeMat;
    Material _rightEyeMat;
    Material _hairMat;
    Material _headMat;
    Material _footballMat;
    Texture2D _ShoeTex;
    Texture _GlowTexture;

    int _headId;
    SkinnedMeshRenderer _headMesh;
    MeshRenderer _leftEyeMesh;
    MeshRenderer _rightEyeMesh;
    MeshRenderer _hairMesh;
    MeshRenderer _clothMesh;
    MeshRenderer _skinMesh;
    SkinnedMeshRenderer _sockMesh;
    MeshRenderer _footballMesh;

   
    ARPlayerAnimState _beforeAnimState;
    ARPlayerAnimState _curAnimState;
    bool _isInit = true;
    bool _isChangePos = false;
    GameObject _TipsObj;
    bool _isOneAnimEnd=false;
    bool _isChangeAnim=false;
    int _hash = -1;
    int _currentHash = -1;
    List<string> _dynamicClothName;
    ARPlayerData aRPlayerData;
    public override async GTask Awake()
    {
        _player = this.transform.Find("ARPlayer").gameObject;
        aRPlayerData = this.gameObject.GetComponent<ARPlayerData>();
        _GlowTexture = aRPlayerData._GlowTex;
        _runtimeAnimatorController = (await ResourceMgr.Instance.LoadAssetAsync<RuntimeAnimatorController>("3D/Controller/ARShare/ARShareController")).result;
        _TipsObj = aRPlayerData._Tips;
        _TipsObj.SetActive(false);
        _dynamicClothName = new List<string>();
      /*  HotFixHighPlayerInfo highPlayerInfo = new HotFixHighPlayerInfo();
        highPlayerInfo.playerId = 1;
        highPlayerInfo.headId = 1;
        highPlayerInfo.playerName = "MESSI";
        highPlayerInfo.playerNumber = "30";
        highPlayerInfo.playerHeight = 180;
        highPlayerInfo.playerWeight = 150;
        highPlayerInfo.playerSkinColor = SkinColorForTexture.WHITE;
        highPlayerInfo.jerseyCode = TAGlobalData.Instance.JereyId;
        highPlayerInfo.isGK = false;
        ShowARPlayer(highPlayerInfo);*/
       
    }
    //显示球员接口
    public async GTask ShowARPlayer(HotFixHighPlayerInfo highPlayerInfo)
    {
        _headId = highPlayerInfo.headId;
        await PlayerHeadDataLoader.AsyncPreLoadOneHeadDataSet(_headId);
        await InitPlayer(highPlayerInfo);
        _playerAnimation.Show();
        _playerAnimator = _player.GetComponent<Animator>();
        _playerAnimator.runtimeAnimatorController = _runtimeAnimatorController;
         HighPolyPlayerAnimation_Hotfix.PlayAnimation(_playerAnimation, "Idle05",0);
         _beforeAnimState = ARPlayerAnimState.DropHand;
        _curAnimState = ARPlayerAnimState.DropHand;
        SwitchARPlayerAnimState(_curAnimState);
        // _playerAnimation.StartLookAtCamera(Camera.main.transform, _playerAnimation.MyAnimator.GetCurrentAnimatorStateInfo(0).length - 1.5f);
        OpenUpdate();
    }
   
    async GTask LoadMaterial(HotFixHighPlayerInfo highPlayerInfo)
    {

        if (highPlayerInfo.playerSkinColor == SkinColorForTexture.BLACK)
        {
            _SkinMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Body_B")).result;
        }
        else if (highPlayerInfo.playerSkinColor == SkinColorForTexture.WHITE)
        {
            _SkinMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Body_W")).result;
        }
        else if (highPlayerInfo.playerSkinColor == SkinColorForTexture.LATIN)
        {
            _SkinMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Body_L")).result;
        }
        else if (highPlayerInfo.playerSkinColor == SkinColorForTexture.ASIAN)
        {
            _SkinMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Body_A")).result;
        }
        else
        {
            _SkinMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Body_L")).result;
        }
        _TeamJerseyMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Cloth")).result;
        _TeamShoesMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Shoe")).result;
        _TeamSockMat = (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/Mat_Sock")).result;
        _footballMat= (await ResourceMgr.Instance.LoadAssetAsync<Material>("3D/Material/ARPlayerMat/ARShareFootball")).result;
    }
    async GTask InitPlayer(HotFixHighPlayerInfo highPlayerInfo)
    {

        //设置材质球参数
        await LoadMaterial(highPlayerInfo);
        //球衣
        PlayerJerseyGenerator.Instance.Initialize();
        await PlayerJerseyGenerator.Instance.SetJerseyInfoAsync(highPlayerInfo.jerseyCode, false);
        PlayerJerseyGenerator.Instance.PlayerName = highPlayerInfo.playerName;
        PlayerJerseyGenerator.Instance.PlayerNumber = highPlayerInfo.playerNumber;
        PlayerJerseyGenerator.Instance.UpdateJerseyProperties();
        _jerseyTexture = PlayerJerseyGenerator.Instance.GetDIYTex();
        _socksTexture = PlayerJerseyGenerator.Instance.GetSockTex();
        PlayerJerseyGenerator.Instance.SetCameraActive(false);
        if (_TeamJerseyMat)
            _TeamJerseyMat.SetTexture("_Base<fim_suffix>SetTexture("_BaseMap", _socksTexture);

        if (_TeamJerseyMat)
            _TeamJerseyMat.SetTexture("_BaseMap", _jerseyTexture);
        if (_TeamSockMat)
            _TeamSockMat.SetTexture("_BaseMap", _socksTexture);
        //
        string path = "3D/PlayerJersey/PlayerShoes/";
        _ShoeTex = (await ResourceMgr.Instance.LoadAssetAsync<Texture2D>(path + "Shoes_" + highPlayerInfo.shoesId.ToString() + "_tex")).result;
        if (_ShoeTex)
            _TeamShoesMat.SetTexture("_BaseMap", _ShoeTex);

        Vector3 skinColorCorrectionValue = highPlayerInfo.skinColorCorrectionValue;
        _SkinMat.SetColor("_BaseColor", new Color(skinColorCorrectionValue.x, skinColorCorrectionValue.y, skinColorCorrectionValue.z, 1));

        //配置球员数据
        string fileNameSuffix = highPlayerInfo.headId.ToString();
        _playerAppearance = _player.GetComponent<HighPolyPlayerAppearanceController>();
        _playerAppearance.PrecalculateBodyShape(highPlayerInfo.playerHeight, highPlayerInfo.playerWeight);
        _playerAppearance.SetPlayerHead(highPlayerInfo.playerId, fileNameSuffix, highPlayerInfo.playerSkinColor);
        _playerAppearance.SetPlayerMaterials(_TeamJerseyMat, _SkinMat, _TeamSockMat, _TeamShoesMat);
        _playerAppearance.SetPlayerBodyShape(highPlayerInfo.playerHeight, highPlayerInfo.playerWeight);
        _playerAppearance.SetShadowMask(false);

        _playerAnimation = _player.GetComponent<HighPolyPlayerAnimationController>();
#if UNITY_IOS &&!UNITY_EDITOR
        _playerAnimation.Init(true, false);
#else
        _playerAnimation.Show();
        _playerAnimation.Init(true, false);
#endif

        SetPlayerHead();
    }
    void SetPlayerHead()
    {

        Transform head = _playerAnimation.Head;
        _headMesh = _playerAppearance.HeadMR;
        _headMat = _headMesh.material;
        _leftEyeMesh = head.Find("LeftEye").GetComponent<MeshRenderer>();
        _leftEyeMat = _leftEyeMesh.material;
        _rightEyeMesh = head.Find("RightEye").GetComponent<MeshRenderer>();
        _rightEyeMat = _rightEyeMesh.material;
        _hairMesh = head.Find("Hair").GetComponent<MeshRenderer>();
        if (_hairMesh)
            _hairMat = _hairMesh.material;

        _headMat.renderQueue = 3000;
        _headMat.EnableKeyword("_ARGLOW_ON");
        _headMat.SetFloat("_GlowWidth", 0.5f);
        _headMat.SetTexture("_GlowTex", _GlowTexture);
        if (_leftEyeMat)
        {
            _leftEyeMat.renderQueue = 3000;
            _leftEyeMat.EnableKeyword("_ARGLOW_ON");
            _leftEyeMat.SetFloat("_GlowWidth", 0.5f);
            _leftEyeMat.SetTexture("_GlowTex", _GlowTexture);
        }
        if (_rightEyeMat)
        {
            _rightEyeMat.renderQueue = 3000;
            _rightEyeMat.EnableKeyword("_ARGLOW_ON");
            _rightEyeMat.SetFloat("_GlowWidth", 0.5f);
            _rightEyeMat.SetTexture("_GlowTex", _GlowTexture);
        }
        if (_hairMat)
        {
            _hairMat.renderQueue = 3000;
            _hairMat.EnableKeyword("_ARGLOW_ON");
            _hairMat.SetFloat("_GlowWidth", 0.5f);
            _hairMat.SetTexture("_GlowTex", _GlowTexture);
        }
    }
    //更改球员动画接口
    public void SwitchARPlayerAnimState(ARPlayerAnimState playerAnim)
    {
        string stateName = "DropHand";
        switch (playerAnim)
        {
            case ARPlayerAnimState.FingerSky:
                stateName = "FingerSky";
                break;
            case ARPlayerAnimState.Juggle:
                stateName = "Juggle";
                break;
            case ARPlayerAnimState.ShakeFist:
                stateName = "ShakeFist";
                break;
            case ARPlayerAnimState.ShoulderHook:
                stateName = "ShoulderHook";
                break;
            case ARPlayerAnimState.ThumbUp:
                stateName = "ThumbUp";
                break;
            case ARPlayerAnimState.DropHand:
                stateName = "Idle05";
                break;
            case ARPlayerAnimState.CrossChest:
                stateName = "Idle08";
                break;
            case ARPlayerAnimState.BackHand:
                stateName = "Idle06";
                break;
            case ARPlayerAnimState.Akimbo:
                stateName = "Idle07";
                break;
        }
        _curAnimState = playerAnim;
        if (!_dynamicClothName.Contains(stateName))
        {
            ClothDataLoader.AsyncPreLoadOneClothDataSet(stateName);
            _dynamicClothName.Add(stateName);
        }
        float nmTime = 0.2f / _playerAnimator.GetCurrentAnimatorStateInfo(0).length;
        _playerAnimator.CrossFade(stateName, nmTime);
        _playerAnimation.MyAnimator.Update(0);
        _playerAnimation.OnStateNameOrMaterialChanged();
        _playerAnimation.UpdateManually();
        if (stateName == "Juggle")
        {
            _playerAnimation.ShowFootball();
            if (_ballAnimator == null)
            {
                Transform ball = _playerAnimation.Football.Find("ball");
                _ballAnimator = ball.GetComponent<Animator>();
                _footballMesh = ball.GetComponent<MeshRenderer>();
                _footballMesh.material = _footballMat;
                _ballAnimator.enabled = true;
            }
            _ballAnimator.Play("JuggleFootball");
        }
        else
            _playerAnimation.HideFootball();

        if ((_beforeAnimState == ARPlayerAnimState.Juggle && _curAnimState!= ARPlayerAnimState.Juggle) ||
          (_beforeAnimState!= ARPlayerAnimState.Juggle && _curAnimState == ARPlayerAnimState.Juggle) || _isInit||_isChangePos)
        {
            ShowPlayerGlow();
            _isInit = false;
            _isChangePos = false;
        }
        _beforeAnimState = _curAnimState;
    }
    void ShowPlayerGlow()
    {
         SetPlayerGlowPosY(_player.transform.position.y - 0.5f);
         float time = 1f;
         Go.to(this, time, new GoTweenConfig()
               .onUpdate
                ((AbstractGoTween t) =>
                {
                    float glowPosY = (t.totalElapsedTime / time) * 3f - 0.5f + _player.transform.position.y;
                    SetPlayerGlowPosY(glowPosY);
                }));
    }
    void SetPlayerGlowPosY(float glowPosY)
    {
        if (_skinMesh == null)
            _skinMesh = _playerAppearance.LimbGPUSKM;
        if (_clothMesh == null)
            _clothMesh = _playerAppearance.ClothGPUSKM;
        if (_sockMesh == null)
            _sockMesh = _playerAppearance.LegMR;
        _skinMesh.material.SetFloat("_GlowPosY", glowPosY);
        _clothMesh.material.SetFloat("_GlowPosY", glowPosY);
        _headMat.SetFloat("_GlowPosY", glowPosY);
        _leftEyeMat.SetFloat("_GlowPosY", glowPosY);
        _rightEyeMat.SetFloat("_GlowPosY", glowPosY);
        if (_hairMat)
            _hairMat.SetFloat("_GlowPosY", glowPosY);
        for (int i = 0; i < _sockMesh.materials.Length; i++)
            _sockMesh.materials[i].SetFloat("_GlowPosY", glowPosY);
        if (_playerAnimation.Football.gameObject.active)
            _footballMesh.material.SetFloat("_GlowPosY", glowPosY);
    }

    //更改球员位置接口
    public void ChangeARPlayerPos(Vector3 pos)
    {
        SetPlayerGlowPosY(_player.transform.position.y - 1 + 3);
        float time = 1f;
        Go.to(this, time, new GoTweenConfig()
              .onUpdate
               ((AbstractGoTween t) =>
               {
                   float glowPosY = _player.transform.position.y - 1 + 3 - (t.totalElapsedTime / time) * 3f;
                   SetPlayerGlowPosY(glowPosY);
               })
               .onComplete((AbstractGoTween t) => {
                    Debug.LogError("ARpos:"+ pos);
                    this.transform.localPosition = pos;
                    _isChangePos = true;
                    SwitchARPlayerAnimState(_curAnimState);
                }));
    }
    //提示Obj接口
    public void ShowARTips()
    {
        _TipsObj.SetActive(true);
        Go.DelayCall(()=> { _TipsObj.SetActive(false); },4f);
    }
   
    public override void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.S))
             SwitchARPlayerAnimState(ARPlayerAnimState.FingerSky);
         else if (Input.GetKeyDown(KeyCode.W))
             SwitchARPlayerAnimState(ARPlayerAnimState.ShoulderHook);
         else if (Input.GetKeyDown(KeyCode.A))
             SwitchARPlayerAnimState(ARPlayerAnimState.Juggle);
         else if (Input.GetKeyDown(KeyCode.D))
             SwitchARPlayerAnimState(ARPlayerAnimState.ShakeFist);
         else if (Input.GetKeyDown(KeyCode.Z))
             SwitchARPlayerAnimState(ARPlayerAnimState.ThumbUp);
         else if (Input.GetKeyDown(KeyCode.Q))
             SwitchARPlayerAnimState(ARPlayerAnimState.Akimbo);
         else if (Input.GetKeyDown(KeyCode.E))
             SwitchARPlayerAnimState(ARPlayerAnimState.BackHand);
         else if (Input.GetKeyDown(KeyCode.R))
             SwitchARPlayerAnimState(ARPlayerAnimState.CrossChest);
         else if (Input.GetKeyDown(KeyCode.T))
             SwitchARPlayerAnimState(ARPlayerAnimState.DropHand);
         else if (Input.GetKeyDown(KeyCode.M))
             ChangeARPlayerPos(new Vector3(27.9f, 0, 13f));
        else if (Input.GetKeyDown(KeyCode.N))
            ChangeARPlayerPos(new Vector3(27.9f, -3, 13f));*/
         _currentHash = _playerAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash;
          if(_currentHash!= _hash)
          {
              _hash = _currentHash;
              _isChangeAnim = true;
              _isOneAnimEnd = false;
          }
          if (_isChangeAnim && _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f)
          {
              _isOneAnimEnd = true;
              _isChangeAnim = false;
          }

    }
    public override void OnDestroy()
    {
        if (_headMat)
        {
            _headMat.renderQueue = 2000;
            _headMat.DisableKeyword("_ARGLOW_ON");
        }
        if (_leftEyeMat)
        {
            _leftEyeMat.renderQueue = 2000;
            _leftEyeMat.DisableKeyword("_ARGLOW_ON");
        }
        if (_rightEyeMat)
        {
            _rightEyeMat.renderQueue = 2000;
            _rightEyeMat.DisableKeyword("_ARGLOW_ON");
        }
        if (_hairMat)
        {
            _hairMat.renderQueue = 2000;
            _hairMat.DisableKeyword("_ARGLOW_ON");
        }
         
        _isChangeAnim = false;
        _isOneAnimEnd = false;
        //PlayerHeadDataLoader.UnloadUnreferencedHeadData();
        //PlayerHeadDataLoader.UnloadOnePreLoadedHeadData(_headId);
        ClothDataLoader.UnloadMultiPreLoadedClothDatas(_dynamicClothName.ToArray());
        GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(_jerseyTexture);
        GalaRenderPipeline.GalaRenderManager.ReleaseTemporaryRT(_socksTexture);
        PlayerJerseyGenerator.Instance.ClearCache();
        if (_runtimeAnimatorController)
        {
            ResourceMgr.Instance.UnloadAsset<RuntimeAnimatorController>(_runtimeAnimatorController);
            _runtimeAnimatorController = null;
        }
        if (_SkinMat)
        {
            ResourceMgr.Instance.UnloadAsset<Material>(_SkinMat);
            _SkinMat = null;
        }
        if (_TeamJerseyMat)
        {
            ResourceMgr.Instance.UnloadAsset<Material>(_TeamJerseyMat);
            _TeamJerseyMat = null;
        }
        if (_TeamShoesMat)
        {
            ResourceMgr.Instance.UnloadAsset<Material>(_TeamShoesMat);
            _TeamShoesMat = null;
        }
        if (_TeamSockMat)
        {
            ResourceMgr.Instance.UnloadAsset<Material>(_TeamSockMat);
            _TeamSockMat = null;
        }
        if (_ShoeTex)
        {
            ResourceMgr.Instance.UnloadAsset<Texture>(_ShoeTex);
            _ShoeTex = null;
        }
        if (_footballMat)
        {
            ResourceMgr.Instance.UnloadAsset<Material>(_footballMat);
            _footballMat = null;
        }
        _isInit = true;
        _isChangePos = false;
        FitnessRoomPlayer_Hotfix.isResetShadow= true;
        base.OnDestroy();
    }

}
<fim_middle>Map", _jerseyTexture);
        if (_TeamSockMat)
            _TeamSockMat.SetTexture("_BaseMap", _socksTexture);
        //球鞋
        PlayerShoesGenerator.Instance.Initialize();
        await PlayerSh