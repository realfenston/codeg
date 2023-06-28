<fim_prefix>﻿using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Platform;

namespace PlatformHotfix
{
    public class SceneZoomManager
    {
         private static SceneZoomManager _instance;
        public static SceneZoomManager Instance
        {
            get {
                if(_instance == null)
                {
                    _instance = new SceneZoomManager();
                    _instance.Init();
                }
                return _instance; 
            }
        }

        Transform transform;

        SceneZoomData sceneZoomData;

        private Camera _cam => sceneZoomData._cam;
        public BezierCurve TeamViewCamCurve =>sceneZoomData.TeamViewCamCurve;
        public BezierCurve[] TrainingRoomCamCurves =>sceneZoomData.TrainingRoomCamCurves;
        public BezierCurve TransferEntranceCurve =>sceneZoomData.TransferEntranceCurve;
        public BezierCurve CollectionEntranceCurve =>sceneZoomData.CollectionEntranceCurve;
        public BezierCurve BossDesktopCamCurve =>sceneZoomData.BossDesktopCamCurve;
        public BezierCurve LockRoomCamCurve =>sceneZoomData.LockRoomCamCurve;
        public BezierCurve LockRoomCamCurve2 =>sceneZoomData.LockRoomCamCurve2;
        public BezierCurve PvPMatchCamCurve =>sceneZoomData.PvPMatchCamCurve;
        public BezierCurve PvPMatchCamCurve2 =>sceneZoomData.PvPMatchCamCurve2;
        public BezierCurve PvPMatchCamCurve3 =>sceneZoomData.PvPMatchCamCurve3;
        public BezierCurve RankMatchCamCurve =>sceneZoomData.RankMatchCamCurve;
        public BezierCurve RankMatchCamCurve_WS =>sceneZoomData.RankMatchCamCurve_WS;
        public BezierCurve RankUpgradeCamCurve => sceneZoomData.RankUpgradeCamCurve;
        public BezierCurve EliteMatchCamCurve =>sceneZoomData.EliteMatchCamCurve;
        public BezierCurve LiveEventMatchCamCurve =>sceneZoomData.LiveEventMatchCamCurve;
        public BezierCurve PvpWaitCamCurve =>sceneZoomData.PvpWaitCamCurve;
        public BezierCurve RoadOfGloryCamCurve =>sceneZoomData.RoadOfGloryCamCurve;
        public BezierCurve BattlePassCamCurve =>sceneZoomData.BattlePassCamCurve;
        public BezierCurve FullScreenUICurve =>sceneZoomData.FullScreenUICurve;
        public BezierCurve DreamLineupCurve =>sceneZoomData.DreamLineupCurve;
        //private Material _stadiumSkymat => sceneZoomData._stadiumSkymat;
        //private Material _indoorSkymat => sceneZoomData._stadiumSkymat;
        
        public int UpgradeCurveIndex = 0;

        private float _coverDepthDelta=.5f;
        private Color _backgroundColor = new Color(0.1982f,0,0.368f);

      

        public void Awake(Transform transform)
        {
            this.transform = transform;

            sceneZoomData = this.transform.GetComponent<SceneZoomData>();
        }
        private void Init() {
           
        }

        public void Destroy()
        {
            _instance = null;
            transform = null;
            sceneZoomData = null;
        }
        /*******************************************
        不同场景下镜头状态设置
    *******************************************/
        public void SetStadiumCamMode(bool isPush){
            //Scene3DManager.Instance.HideAll();
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,0f);
            Scene3DManager.Instance.SetSceneActive(true);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(72,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0, 0.15f),0);
            // CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            CameraAnimController.Instance.SetBloom(true, 0.8f);
            CameraAnimController.Instance.EnablePostProcess(true);
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta = 0.5f;
        }
        public void SetPvPMode(bool isPush){
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,0f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(72,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0.13f, 0.15f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta =  0.2f;
        }

        public void SetRankMatchMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(true);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,0f);
            _cam.gateFit = Camera.GateFitMode.Vertical;
            CameraAnimController.Instance.SetBlur(true);
            CameraAnimController.Instance.SetBloom(true, 0.8f);
            CameraAnimController.Instance.EaseCurve = null;

            if (isPush){
                CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0f),0);
                CameraAnimController.Instance.SetFOV(54, 0);
                CameraAnimController.Instance.SetFeatureFOV("FOVCameraObjects", 54, 0);
                CameraAnimController.Instance.SetFeatureFOV("FOVCameraForTransparentObjects", 54, 0);
                CameraAnimController.Instance.SetFeatureOffset("FOVCameraObjects", Vector4.zero, 0);
                CameraAnimController.Instance.SetFeatureOffset("FOVCameraForTransparentObjects", Vector4.zero, 0);
            }
            else{
                float fov = HomeScene3DPlayers3DView.FeatureFOV_Wide;
                if (!TAManager.IsWideScreen())
                {
                    //比较窄的屏幕设置
                    fov = HomeScene3DPlayers3DView.FeatureFOV_Slim;
                }
                CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0.15f),0);
                CameraAnimController.Instance.SetFeatureFOV("FOVCameraObjects", fov, 0);
                CameraAnimController.Instance.SetFeatureFOV("FOVCameraForTransparentObjects", fov, 0);
                CameraAnimController.Instance.SetFeatureOffset("FOVCameraObjects", new Vector4(0, 0,-1,0), 0);
                CameraAnimController.Instance.SetFeatureOffset("FOVCameraForTransparentObjects", new Vector4(0, 0, -1, 0), 0);
                CameraAnimController.Instance.SetFOV(70, 0);
            }
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta = 0.5f;
        }

         public void SetRankUpgradeMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(true);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,0f);
            _cam.gateFit = Camera.GateFitMode.Vertical;
            CameraAnimController.Instance.SetBlur(false);
            CameraAnimController.Instance.SetBloom(true, 0.8f);
            CameraAnimController.Instance.EaseCurve = null;
            CameraAnimController.Instance.SetLensShift(Vector2.zero,0);
            CameraAnimController.Instance.SetFeatureFOV("FOVCameraObjects", 54, 0);
            CameraAnimController.Instance.SetFeatureFOV("FOVCameraForTransparentObjects", 54, 0);
            CameraAnimController.Instance.SetFeatureOffset("FOVCameraObjects", Vector4.zero, 0);
            CameraAnimController.Instance.SetFeatureOffset("FOVCameraForTransparentObjects", Vector4.zero, 0);
            CameraAnimController.Instance.SetFOV(54, 0);
            
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta = 0.5f;
        }

        public void SetTeamManageMode(bool isPush){
            //Scene3DManager.Instance.HideAll();
            Scene3DManager.Instance.SetSceneActive(true);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,.6f,0f);
            _cam.gateFit = Camera.GateFitMode.Vertical;
            CameraAnimController.Instance.SetFOV( ((float)Screen.width / Screen.height < 1.5f? 80f : 67f),0);
            _cam.clearFlags = CameraClearFlags.Skybox;
            CameraAnimController.Instance.SetLensShift(new Vector2(-.075f, 0),0);
            CameraAnimController.Instance.SetBlur(false);
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta = 0.5f;
        }

        public void SetTrainingRoomMode(bool isPush){
            Scene3DManager.Instance.Set<fim_suffix>,.5f,0f,.0f,0f);

            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(45,0);
            CameraAnimController.Instance.SetLensShift(Vector2.zero,0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
        
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _indoorSkymat;
            _coverDepthDelta = 0.1f;
        }
    
        public void SetBossRoomMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,.0f,0f);

            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(45,0);
        
            CameraAnimController.Instance.SetLensShift(Vector2.zero,0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _indoorSkymat;
            Platform.EventDispatcher.TriggerEvent("SetBossRoomReflectionProbe",0);
            _coverDepthDelta = 0.1f;
        }

        public void SetBossDesktopMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,0f,0f,.0f,0f);

            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(70,0);
            CameraAnimController.Instance.SetLensShift(new Vector3(-0.14f,0),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
        
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _indoorSkymat;
            Platform.EventDispatcher.TriggerEvent("SetBossRoomReflectionProbe",1);
            _coverDepthDelta = 0.1f;
        }
        public void SetLockRoomMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,.0f,0f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(45,0);
            CameraAnimController.Instance.SetLensShift(Vector2.zero,0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
        
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _indoorSkymat;
            _coverDepthDelta = 0.1f;
        }
    
        public void SetEliteMatchMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(true);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,0f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(70,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0.16f, 0.15f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
        
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta = 0.5f;
        }

        public void SetLiveEventMatchMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,.3f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(40,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            CameraAnimController.Instance.EnablePostProcess(false);
        
            _cam.clearFlags = CameraClearFlags.Color;
            _coverDepthDelta = 0.5f;
        }

        public void SetChallengeRoadMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,1f,0f,0f,.3f);
            _cam.gateFit = Camera.GateFitMode.Vertical;
            CameraAnimController.Instance.SetFOV(40,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            CameraAnimController.Instance.EnablePostProcess(false);
        
            _cam.clearFlags = CameraClearFlags.Color;
            _coverDepthDelta = 0.5f;
        }

        public void SetRoadOfGloryMode(bool isPush){
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,0f,0f,0f,0f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
        
            CameraAnimController.Instance.SetFOV(40,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);

            _cam.clearFlags = CameraClearFlags.Color;
            _cam.backgroundColor = new Color(63f/255,29f/255,118/255f,0);

        }
        public void SetPVPWaitMode(bool isPush){
            //Scene3DManager.Instance.HideOthersExcept(Scene3DManager.Instance.PvpWaitPlayers3DView);
            Scene3DManager.Instance.SetSceneActive(true);
            LensMaskManager.Instance.SetMaskOpaques(0f,0f,0f,0f,.3f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(48,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);

            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            _coverDepthDelta = 0.5f;
        }

        public void SetFullScreenUIMode(bool isPush){
            //Scene3DManager.Instance.HideOthersExcept(Scene3DManager.Instance.FullScreenBgView);
            Scene3DManager.Instance.SetSceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,0f,0f,0f,0f);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(40f,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0f),0);
            CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            _cam.clearFlags = CameraClearFlags.Color;
            _cam.backgroundColor = _backgroundColor;
            _coverDepthDelta = 0.5f;
        }
         public void SetElevenGuyCamMode(bool isPush){
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,0f,0f);
            Scene3DManager.Instance.SetSceneActive(true);
            _cam.gateFit = Camera.GateFitMode.Horizontal;
            CameraAnimController.Instance.SetFOV(72,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0, 0.15f),0);
            // CameraAnimController.Instance.SetAddRotate(Vector3.zero,0);
            CameraAnimController.Instance.SetBlur(false);
            CameraAnimController.Instance.SetBloom(true, 0.8f);
            CameraAnimController.Instance.EnablePostProcess(true);
            _cam.clearFlags = CameraClearFlags.Skybox;
            //RenderSettings.skybox = _stadiumSkymat;
            Platform.EventDispatcher.TriggerEvent("ShowElevenGuyWithAni");
            Scene3DManager.Instance.Remove3DView<LockRoom3DView>();
            _coverDepthDelta = 0.5f;
        }

        public void SetDreamLineupCamMode(bool isPush)
        {
            Scene3DManager.Instance.SetSceneActive(true);
            _cam.gateFit = Camera.GateFitMode.Vertical;
            CameraAnimController.Instance.SetFOV(30,0);
            CameraAnimController.Instance.SetLensShift(new Vector2(0, -0.1f),0);
            CameraAnimController.Instance.SetBlur(true,doFBlurStart:0.4f);
            CameraAnimController.Instance.SetBloom(true, 0.8f);
        }
        /*******************************************
        镜头运动函数动画
    *******************************************/

        public void MatchEntranceAnim(Action onComplete)
        {
            CameraAnimController.Instance.EaseCurve = null;
            CameraAnimController.Instance.StopCurveEase();
            CameraAnimController.Instance.SetAnchorToHome();
            _cam.transform.position = GetCamPosAtDepth(0);
            _cam.transform.rotation = GetCamRot();
            Vector3 endPos = GetCamPosAtDepth(1);
            CameraAnimController.Instance.MoveTo(endPos, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut,false,onComplete);
        }
        public void RankMatchEnterAnim(Action onComplete){
            if(TAManager.IsWideScreen()) //屏幕适配
            {
                CameraAnimController.Instance.EaseCurve = RankMatchCamCurve;
            }else{
                CameraAnimController.Instance.EaseCurve = RankMatchCamCurve_WS;
            }
            CameraAnimController.Instance.CurveEaseVal = 0f;
            CameraAnimController.Instance.SetCurveEaseVal(1,StackViewManager.Instance.AnimSpeed*2);
            Go.DelayCall(()=>{
                CameraAnimController.Instance.EaseCurve = null;
                CameraAnimController.Instance.MoveTo(new Vector3(31.3f, 1.6f, 0), StackViewManager.Instance.AnimPushSpeed, GoEaseType.ExpoIn, true, () => {
                   RankMatchShowCupAnim(onComplete);
                });
            },1.4f);
        }

        public void RankMatchShowCupAnim(Action onComplete)
        {
            CameraAnimController.Instance.EaseCurve = null;
            CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0.15f), StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            CameraAnimController.Instance.Cam.transform.position = new Vector3(31.5f, 1.4f, 0);
            CameraAnimController.Instance.Cam.transform.eulerAngles = new Vector3(0, -90, 0);
            CameraAnimController.Instance.MoveTo(new Vector3(30.5f, 1.4f, 0), StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut, true, onComplete);
            CameraAnimController.Instance.SetFeatureFOV("FOVCameraObjects", 42, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            CameraAnimController.Instance.SetFeatureFOV("FOVCameraForTransparentObjects", 42, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            CameraAnimController.Instance.SetFeatureOffset("FOVCameraObjects", Vector4.zero, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            CameraAnimController.Instance.SetFeatureOffset("FOVCameraForTransparentObjects", Vector4.zero, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            CameraAnimController.Instance.SetFOV(70, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            CameraAnimController.Instance.SetLensShift(new Vector2(0, 0.15f), StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
        }

        // public void RankUpgradeShowCupAnim(Action onComplete)
        // {
        //     CameraAnimController.Instance.EaseCurve = null;
        //     CameraAnimController.Instance.SetLensShift(new Vector2(0f, 0.15f), StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
        //     CameraAnimController.Instance.Cam.transform.position = new Vector3(-6f, 1.4f, 500);
        //     CameraAnimController.Instance.Cam.transform.eulerAngles = new Vector3(0, 90, 0);
        //     CameraAnimController.Instance.MoveTo(new Vector3(-11f, 1.4f, 500f), StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut, true, onComplete);
        //     CameraAnimController.Instance.SetFeatureFOV("FOVCameraObjects", 42, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
        //     CameraAnimController.Instance.SetFeatureFOV("FOVCameraForTransparentObjects", 42, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
        //     CameraAnimController.Instance.SetFOV(70, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
        //     CameraAnimController.Instance.SetLensShift(new Vector2(0, 0.15f), StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
        // }

        public void LiveEventMatchEnterAnim(Action onComplete){
            CameraAnimController.Instance.EaseCurve = LiveEventMatchCamCurve;
            CameraAnimController.Instance.CurveEaseVal = 0f;
            CameraAnimController.Instance.SetCurveEaseVal(1f, StackViewManager.Instance.AnimSpeed,GoEaseType.ExpoOut);
            Go.DelayCall(()=>{
                CameraAnimController.Instance.EaseCurve = null;
                CameraAnimController.Instance.MoveTo(new Vector3(200,0,7), StackViewManager.Instance.AnimSpeed*2,GoEaseType.ExpoInOut, true,onComplete);
            },.5f);
        }

        public void ChallengeRoadEnterAnim(Action onComplete)
        {
            CameraAnimController.Instance.EaseCurve = LiveEventMatchCamCurve;
            CameraAnimController.Instance.CurveEaseVal = 0f;
            CameraAnimController.Instance.SetCurveEaseVal(1f, StackViewManager.Instance.AnimSpeed, GoEaseType.ExpoOut);
            Go.DelayCall(() =>
            {
                CameraAnimController.Instance.EaseCurve = null;
                CameraAnimController.Instance.MoveTo(new Vector3(200,0,10), StackViewManager.Instance.AnimSpeed*2,GoEaseType.ExpoInOut, true,onComplete);
            },.5f);
        }

        Vector3 GetCamPosAtDepth(float depth){
            if(StackViewManager.Instance.IsCovered){
                depth+=_coverDepthDelta;
            }
            Vector3 pos = CameraAnimController.Instance.CamAnchorPos - depth*2*CameraAnimController.Instance.CamAnchorDir;
            return  pos;
        }
        Quaternion GetCamRot(){
            Quaternion rot =  CameraAnimController.Instance.CamAnchorRot;
            rot = rot*Quaternion.Euler(CameraAnimController.Instance.AddRotation);
            return rot;
        }

        public BezierCurve GetRandomUpgradeViewEnterExitCurve()
        {
            UpgradeCurveIndex = UnityEngine.Random.Range(0, TrainingRoomCamCurves.Length);
            return TrainingRoomCamCurves[UpgradeCurveIndex];
        }
        public BezierCurve GetUpgradeViewEnterExitCurve()
        {
            return TrainingRoomCamCurves[UpgradeCurveIndex];
        }

#region  暂存摄像机数据
       
        const int Render_Cout = 10; 
        public CameraCacheData StorageCameraProperty()
        {
            CameraCacheData _camerData = new CameraCacheData();
            _camerData.position           = _cam.transform.position;
            _camerData.eulerAngles        = _cam.transform.eulerAngles;
            _camerData.localScale         = _cam.transform.localScale;
            _camerData.fieldOfView        = _cam.fieldOfView;
            _camerData.layerCullSpherical = _cam.layerCullSpherical;
            _camerData.lensShift          = _cam.lensShift;
            _camerData.gateFit            = _cam.gateFit;
            _camerData.clearFlags         = _cam.clearFlags;
            _camerData.renderIndex        = GetRenderIndex(_cam.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer);
            return _camerData;
        }

        public void RevertCameraProperty(CameraCacheData _camerData)
        {
            _cam.transform.position       = _camerData.position;
            _cam.transform.eulerAngles    = _camerData.eulerAngles;
            _cam.transform.localScale     = _camerData.localScale;
            _cam.fieldOfView              = _camerData.fieldOfView;
            _cam.layerCullSpherical       = _camerData.layerCullSpherical;
            _cam.lensShift                = _camerData.lensShift;
            _cam.gateFit                  = _camerData.gateFit;
            _cam.clearFlags               = _camerData.clearFlags;
            if(_camerData.renderIndex >=0)
            {
                _cam.GetComponent<UniversalAdditionalCameraData>().SetRenderer(_camerData.renderIndex);
            }
        }

        private int GetRenderIndex(ScriptableRenderer scriptableRenderer)
        {
             for(int i = 0; i< Render_Cout;i++)
            {
                var render = UniversalRenderPipeline.asset.GetRenderer(i);
                if(render == scriptableRenderer) 
                {
                   return i;
                }
            }
            return -1;
        }
#endregion
    }
}
<fim_middle>SceneActive(false);
            LensMaskManager.Instance.SetMaskOpaques(0f,.5f,0f,.0f,0f);

            _cam.gateFit = Camera.GateFitMode.Horizontal;
            Camera