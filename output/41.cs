<fim_prefix>using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GraphProcessor;
using Platform;
using UniRx;
using Framework.GalaSports.Service;


namespace NodeGraphProcessor
{
    [Serializable, NodeMenuItem("Button/MaskButton")]
    public class MaskButtonNode : WaitableNode
    {
        [SerializeField, Input(name = "In")]
        public GameObject btn;
        public string Desc;
        float Speed = 0.3f;
        public float AutoCloseAfter = 0f;
        public bool ShowArrow = true;
        public bool needMask = true;
        public bool ClickScreenClose;
        public Vector2 BtnPosOffset;
        public Vector2 BtnSizeOffset;
        public Vector2 DescOffSet;
        public bool NeedShowSecondHightLight;
        [VisibleIf(nameof(NeedShowSecondHightLight), true)]
        [SerializeField, Input(name = "SecondIn")]
        public GameObject SecondHightLight =null;
        [VisibleIf(nameof(NeedShowSecondHightLight), true)]
        public Vector2 SecondPosOffset;
        [VisibleIf(nameof(NeedShowSecondHightLight), true)]
        public Vector2 SecondSizeOffset;

        private GameObject instantFocus;
        private System.IDisposable disposable;
        TimeItem timeItem;

        //重新强制引导10次，每次间隔0.5秒，如果都未成功，则跳过这一步新手，防止卡死
        private int retryNum = 10;

        protected override void Enable()
        {
            base.Enable();
            retryNum = 10;
        }

        //TODO  修复 界面未刷新 新手加载完的情况导致 新手不引导的异常
        protected override async void Process()
        {

            if (btn == null)
            {
                if (retryNum > 0)
                {
                    await GAsync.WaitSeconds(0.5f);
                    retryNum--;
                    Process();
                    return;
                }
                DebugEX.LogError("<fim_suffix>ProcessFinished();
                return;
            }

            var canvas = btn.GetComponentInParent<Canvas>();
            Vector2 descoffset = DescOffSet * canvas.scaleFactor;

            var realDesc = string.IsNullOrEmpty(Desc)?"":LanguageKit.Get(Desc);
            Action clickAction = null;
            MaskParameter maskParameter = new MaskParameter();
            maskParameter.HighLight1 = new HighLightParameter()
            {
                Pos = GetRealPos(btn,BtnPosOffset),
                Size = GetSize(btn,BtnSizeOffset),
                Desc = realDesc,
                DescPos = descoffset,
                IsNeedFocas = ShowArrow,
                IsNeedMask = needMask
            };
            if(NeedShowSecondHightLight)
            {
                maskParameter.HighLight2 = new HighLightParameter()
                {
                    Pos = GetRealPos(SecondHightLight,SecondPosOffset),
                    Size = GetSize(SecondHightLight,SecondSizeOffset),
                    IsNeedFocas = false,
                    IsNeedMask = needMask
                };
            }
            maskParameter.Delay = Speed;
            maskParameter.BackAction = clickAction;
            if (ClickScreenClose)
            {
                maskParameter.BackAction = () => { this.ProcessFinished(); };
            }
            else
            {
                UGUIExecuteEventMask.ShieldingClickEvent(() =>
                {
                    GuideMaskController.Instance.ShowMask(maskParameter);
                });
            }
            GuideMaskController.Instance.ShowMask(maskParameter);

            if (AutoCloseAfter > 0)
            {
                UGUIExecuteEventMask.ShieldingClickEvent();
                timeItem = Timer.Instance.Post2Scale((x) =>
                {
                    this.ProcessFinished();
                }, AutoCloseAfter);
            }
            else
            {
                var uguiPassCom = btn.GetComponent<ExecutePassComponent>();
                if (uguiPassCom == null)
                    uguiPassCom = btn.AddComponent<ExecutePassComponent>();
                uguiPassCom.SetPassCallBackEvent(this.ProcessFinished);
            }
        }
        private Vector2 GetRealPos(GameObject btn,Vector2 posOffset)
        {
            Vector2 realPos = Vector2.zero;
            var canvas = btn.GetComponentInParent<Canvas>();
            if (canvas.worldCamera!= null)
            {
                realPos = canvas.worldCamera.WorldToScreenPoint(btn.transform.position);
            }
            else
                realPos = btn.transform.position;
            UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold = 10000;
            realPos += posOffset * canvas.scaleFactor;
            return realPos;
        }
        private Vector2 GetSize(GameObject btn,Vector2 sizeOffset)
        {
            var canvas = btn.GetComponentInParent<Canvas>();
            var rect = btn.GetComponent<RectTransform>().rect;
            var width = rect.width * canvas.scaleFactor + sizeOffset.x * canvas.scaleFactor;
            var height = rect.height * canvas.scaleFactor + sizeOffset.y * canvas.scaleFactor;
            Vector2 size = new Vector2(width,height);
            return size;
        }

        protected override void ProcessFinished()
        {
            if (onProcessFinished == null)
            {
                DebugEX.Log("Button onProcessFinished null");
                return;
            }
            Dispose();
            base.ProcessFinished();
        }

        public override void Dispose()
        {
            //Platform.EventDispatcher.RemoveEventListener("ReCalculateGuide", ReCalculateButtonPos);
            UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold = 10;
            if (instantFocus!= null)
            {
                ResourceMgr.Instance.UnloadGameObject(instantFocus);
            }
            if (btn!= null)
            {
                var uguiPassCom = btn.GetComponent<ExecutePassComponent>();
                if (uguiPassCom)
                {
                    GameObject.Destroy(uguiPassCom);
                }
            }
            UGUIExecuteEventMask.RecoverClickEvent();
            GuideMaskController.Instance.HideMask();
            if (timeItem!= null)
                timeItem.Cancel();
            if (disposable!= null)
                disposable.Dispose();
        }
    }
}
<fim_middle>btn is null");
                this.<|endoftext|>