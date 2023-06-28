<fim_prefix>﻿using Framework.GalaSports.MVC.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlatformHotfix
{
    public class TransformDataSource : LoopScrollDataSource
    {
        Action<Transform, int> provideAction;

        public TransformDataSource(Action<Transform, int> updateAction)
        {
            this.provideAction = updateAction;
        }


        public override void ProvideData(Transform transform, int idx)
        {
            provideAction(transform, idx);
        }

        public override void Clear()
        {

        }
    }


    public class ScrollViewSource : LoopScrollDataSource
    {
        Action<View,int> provideAction;
        Func<GameObject, View> func;
        Dictionary<Transform, View> scrollDict = new Dictionary<Transform, View>();

        public ScrollViewSource(Func<GameObject, View> createfunc, Action<View,int> updateAction)
        {
            this.func = createfunc;
            this.provideAction = updateAction;
        }

        private View Create(GameObject gameObject)
        {
            return this.func?.Invoke(gameObject);
        }

        public override void ProvideData(Transform transform, int idx)
        {
            View view;
            if (!scrollDict.TryGetValue(transform, out view))
            {
                view = Create(transform.gameObject);
                scrollDict.Add(transform, view);
            }

            if (provideAction!= null)
            {
                provideAction.Invoke(view, idx);
            }
        }

        public override void Clear()
        {
            provideAction = null;
            func = null;
            scrollDict.Clear();
        }
    }


    public class ScrollAnimationSource<fim_suffix>;
        public bool onlyFirstAnimation;
        public GoEaseType easeType = GoEaseType.BackOut;
        public float awaitTime = 0.1f;
        public bool hasData;

        public ScrollAnimationSource(Func<GameObject, View> createfunc, Func<int, IModel> updateAction, bool onlyFirstAnimation = false) : base(createfunc, updateAction)
        {
            Animation = true;
            hasData = false;
            this.onlyFirstAnimation = onlyFirstAnimation;
        }

        public override async void ProvideData(Transform transform, int idx)
        {
            base.ProvideData(transform, idx);
            hasData = true;
            if (Animation)
            {
                var group = transform.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = transform.gameObject.AddComponent<CanvasGroup>();
                }
                group.alpha = 0;
                var animationChild = transform.GetChild(0);
                if (IsShake)
                {
                    animationChild.LocalPositionY(-40);
                }

                //2帧之后再处理动画，因为有个排序的过程 排序是再下一帧进行，需要排序之后再处理动画
                await GAsync.WaitNextFrame();
                await GAsync.WaitNextFrame();
                int siblingIndex = transform.GetSiblingIndex();
                await GAsync.WaitSeconds(siblingIndex * awaitTime);

                if (animationChild == null)
                    return;


                animationChild.localPositionTo(0.3f, Vector3.zero).easeType = easeType;
                group.alpha = 1;
            }
        }

        public override void ProvideEnd()
        {
            if (this.onlyFirstAnimation && hasData)
                Animation = false;
        }
    }


    public class ScrollDataSource : LoopScrollDataSource 
    {
        Func<int, IModel> provideAction;
        Func<int, View, IModel> provideAction2;
        Func<GameObject, View> func;
        Dictionary<Transform, View> scrollDict = new Dictionary<Transform, View>();

        public ScrollDataSource(Func<GameObject, View> createfunc,Func<int, IModel> updateAction)
        {
            this.func = createfunc;
            this.provideAction = updateAction;
        }

        public ScrollDataSource(Func<GameObject, View> createfunc, Func<int,View, IModel> updateAction)
        {
            this.func = createfunc;
            this.provideAction2 = updateAction;
        }

        private View Create(GameObject gameObject)
        {
            return this.func?.Invoke(gameObject);
        }

        public override void ProvideData(Transform transform, int idx)
        {
            View view;
            if (!scrollDict.TryGetValue(transform, out view))
            {
                view = Create(transform.gameObject);
                scrollDict.Add(transform, view);
            }

            if (provideAction!= null)
                view.ViewModelHandleBase(provideAction.Invoke(idx));
            if (this.provideAction2!= null)
                view.ViewModelHandleBase(this.provideAction2.Invoke(idx,view));          
        }

        public override void Clear()
        {
            provideAction = null;
            func = null;
            scrollDict.Clear();
        }
    }
}<fim_middle> : ScrollDataSource
    {
        public bool IsShake;
        public bool Animation<|endoftext|>