using System.ComponentModel;
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
    [Serializable, NodeMenuItem("Button/TwoMaskButton")]
    public class TwoMaskButtonNode : WaitableNode
    {
        [SerializeField, Input(name = "FirstIn")]
        public GameObject FirstBtn;
        public string Desc;
        public string Desc2;
        float Speed = 0.3f;
        public float AutoCloseAfter = 0f;
        public bool ClickScreenClose;
        public Vector2 BtnPosOffset;
        public Vector2 BtnSizeOffset;
        public Vector2 DescOffSet;
        [SerializeField, Input(name = "SecondIn")]
        public GameObject SecondBtn =null;
        public Vector2 SecondPosOffset;
        public Vector2 SecondSizeOffset;
        public Vector2 SecondDescOffSet;
        [SerializeField,Output(name = "ClickFirst")]
        bool ClickFirstBtn;
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

            if (FirstBtn == null)
            {
                if (retryNum > 0)
                {
                    await GAsync.WaitSeconds(0.5f);
                    retryNum--;
                    Process();
                    return;
                }
                DebugEX.LogError("新手引导强制引导失败,跳出当前引导");
                // this.ProcessFinished();
                return;
            }
            //计算按钮真实位置尺寸
            var canvas = FirstBtn.GetComponentInParent<Canvas>();
            Vector2 descoffset = Desc OffSet * canvas.scaleFactor;
            Vector2 descoffset2 = SecondDescOffSet * canvas.scaleFactor;
            var maskParameter = new GuideMaskParameter();
            maskParameter.HighLight1 = new HighLightParameter()
            {
                Pos = GetRealPos(FirstBtn,BtnPosOffset),
                Size = GetSize(FirstBtn,BtnSizeOffset),
                Desc = string.IsNullOrEmpty(this.Desc)? "" : LanguageKit.Get(this.Desc),
                DescPos = descoffset
            };
            maskParameter.HighLight2 = new HighLightParameter()
            {
                Pos = GetRealPos(SecondBtn,SecondPosOffset),
                Size = GetSize(SecondBtn,SecondSizeOffset),
                Desc = string.IsNullOrEmpty(this.Desc2)? "" : LanguageKit.Get(this.Desc2),
                DescPos = descoffset2
            };
            maskParameter.Delay = Speed;
            maskParameter.BackAction = clickAction;
            if (ClickScreenClose)
            {
                maskParameter.BackAction = () => { this.ProcessFinished(); };
            }
            else
            {
                UGUIExecuteEventMask.ShieldingClickEvent();
            }
            GuideMaskController.Instance.ShowMask(maskParameter);

            if (AutoCloseAfter > 0)
            {
                timeItem = Timer.Instance.Post2Scale((x) =>
                {
                    this.ProcessFinished();
                }, AutoCloseAfter);
            }

            var uguiPassCom = FirstBtn.GetComponent<ExecutePassComponent>();
            if (uguiPassCom == null)
                uguiPassCom = FirstBtn.AddComponent<ExecutePassComponent>();
            var uguiPassCom2 = SecondBtn.GetComponent<ExecutePassComponent>();
            if (uguiPassCom2 == null)
                uguiPassCom2 = SecondBtn.AddComponent<ExecutePassComponent>();
            uguiPassCom.SetPassCallBackEvent(() =>
            {
                ClickFirstBtn = true;
                this.ProcessFinished();
            });
            uguiPassCom2.SetPassCallBackEvent(() =>
            {
                ClickFirstBtn = false;
                this.ProcessFinished();
            });
        }
        private Vector2 GetRealPos(GameObject btn,Vector2 posOffset)
        {
            Vector2 realPos = Vector2.zero;
            var canvas = btn.GetComponentInParent<Canvas>();
            Vector2 descoffset = DescOffSet * canvas.scaleFactor;
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
            UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold = 10;
            if (FirstBtn!= null)
            {
                var uguiPassCom = FirstBtn.GetComponent<ExecutePassComponent>();
                if (uguiPassCom)
                {
                    GameObject.Destroy(uguiPassCom);
                }
            }
            if (SecondBtn!= null)
            {
                var uguiPassCom = SecondBtn.GetComponent<ExecutePassComponent>();
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
