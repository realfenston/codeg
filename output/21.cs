<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.GalaSports.Service;

namespace PlatformHotfix{
    public class Loading3DView{
        public bool isDestroy =false;
        public List<Action<View3DBase>> loadedCallBackList = new List<Action<View3DBase>>();
    }
    public class Scene3DManager
    {
        
        private static Scene3DManager _instance;
        public static Scene3DManager Instance
        {
            get {
                if(_instance == null)
                {
                    _instance = new Scene3DManager();
                    _instance.Awake();
                }
                return _instance; 
            }

        }
        public GameObject gameObject;
        public Transform transform;
        public HomeScene3DPlayers3DView Home3DPlayers3DView;
        // public Home3DPlayersLodView Home3DPlayersLodView;

        private Dictionary<Type, Scene3DConfig.Config3D> _assetPathDict = new Dictionary<Type, Scene3DConfig.Config3D>();

        private Dictionary<Type, View3DBase> _3DViewDict = new Dictionary<Type, View3DBase>();

        private List<View3DBase> _3DViewList = new List<View3DBase>();

        private Dictionary<Type, Loading3DView> _3DViewAsyncLoadingDict = new Dictionary<Type, Loading3DView>();

        private Dictionary<Type, int> addCountDic = new Dictionary<Type, int>();
        
        //private GameObject _mainStage;
        public void SetHome3DPlayers3DView(Transform transform)
        {
            //后续HomeScene3DPlayers3DView 移到热更层，再取消这个代码
            Home3DPlayers3DView = transform.Find("Home3DPlayers").GetComponent<HomeScene3DPlayers3DView>();
            // Home3DPlayersLodView = transform.Find("Home3DPlayers_lod").GetComponent<Home3DPlayersLodView>();

        }

         private void Awake()
        {
           GetMainStage();
           Scene3DConfig.InitConfig(_assetPathDict);
            if(gameObject == null)
            {
                gameObject = new GameObject("Scene3D");
                transform = gameObject.transform;
                GameObject.DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<UpdateManager>();
            }

            Platform.EventDispatcher.AddEventListener(UpdateManager.Home3DUpdate, Update);
            Platform.EventDispatcher.AddEventListener(UpdateManager.Home3DLateUpdate, LateUpdate);
            //兼容非热更接口， 
            Platform.EventDispatcher.AddEventListener<string>("Scene3DManager_Hide", Tmp_Hide); 
            Platform.EventDispatcher.AddEventListener<bool>("Scene3DManager_SetSceneActive",SetSceneActive);
        }

        public void Destroy()
        {
            if(_instance!= null)
            {
                foreach (var item in _3DViewDict)
                {
                    var cachedObj = item.Value;
                    cachedObj.OnDisable();
                    cachedObj.OnDestroy();
                    ResourceMgr.Instance.UnloadGameObject(cachedObj.gameObject);
                }
                _3DViewDict.Clear();
                _3DViewList.Clear();
                _3DViewAsyncLoadingDict.Clear();

                Platform.EventDispatcher.RemoveEventListener<string>("Scene3DManager_Hide", Tmp_Hide);
                Platform.EventDispatcher.RemoveEventListener<bool>("Scene3DManager_SetSceneActive",SetSceneActive);
                Platform.EventDispatcher.RemoveEventListener(UpdateManager.Home3DUpdate, Update);
                Platform.EventDispatcher.RemoveEventListener(UpdateManager.Home3DLateUpdate, LateUpdate);
            }
            _instance = null;
            if(this.gameObject)
            {
                GameObject.Destroy(this.gameObject);
                this.gameObject = null;
            }
            this.transform = null;
            this.Home3DPlayers3DView = null;
            // this.Home3DPlayersLodView = null;

            _assetPathDict.Clear();
        }

        private static GameObject _mainStage;
        public GameObject GetMainStage()
        {
             if (_mainStage == null)
            {
                _mainStage = GameObject.Find("StadiumScene");
                if (_mainStage == null)
                {
                    return new GameObject("MainStage");
                }
            }
            return _mainStage;
        }
        public void SetSceneActive(bool isActive)
        {
            GameObject sceneObj = GetMainStage();
            sceneObj?.SetActive(isActive);

            foreach( var item in _3DViewDict)
            {
                if(_assetPathDict.ContainsKey(item.Key) && _assetPathDict[item.Key].isInScene<fim_suffix>{
                        Show(item.Key);
                    }
                }
            }
            //场边特效特殊处理
            StadiumSideLines.Instance.gameObject?.SetActive(isActive);
        }

        public void Update()
        {
            for(int i = 0; i<_3DViewList.Count; i++)
            {
                if(_3DViewList[i].gameObject.activeInHierarchy && _3DViewList[i].isCanUpdate)
                {
                    _3DViewList[i].Update();
                } 
            }
        }
        public void LateUpdate()
        {
            for(int i = 0; i<_3DViewList.Count; i++)
            {
                if(_3DViewList[i].gameObject.activeInHierarchy && _3DViewList[i].isCanLateUpdate)
                {
                    _3DViewList[i].LateUpdate();
                } 
            }
        }

        public async GTask<T> Add3DView<T>()where T : View3DBase
        {
            T view = null;
            bool IsComplete = false;
            Action<View3DBase> loadedCallBack = (v)=>{
                view = v as T;
                IsComplete = true;
            };
            Add3DView<T>(loadedCallBack);
            await GAsync.WaitUntil(()=> IsComplete);
            return view;
        }
        public async void Add3DView<T>(Action<View3DBase> loadedCallBack) where T : View3DBase
        {
            Type type = typeof(T);
            if (addCountDic.ContainsKey(type))
            {
                addCountDic[type]++;
            }               
            else
                addCountDic.Add(type, 1);

            if(!_assetPathDict.ContainsKey(type))
            {
                DebugEX.LogError("没有配置 : "+ type.Name);
                loadedCallBack?.Invoke(null);
                return;
            }

            View3DBase cachedObj;
            bool isLoaded = _3DViewDict.TryGetValue(type, out cachedObj);
            if (isLoaded)
            {
                if(!cachedObj.gameObject.activeSelf)
                {
                    cachedObj.OnEnable();
                }
                cachedObj.gameObject.SetActive(true);
                loadedCallBack?.Invoke(cachedObj);
                return;
            }


            if(_3DViewAsyncLoadingDict.ContainsKey(type))
            {
                //正在加载中
                if(loadedCallBack!=null)
                {
                    _3DViewAsyncLoadingDict[type].loadedCallBackList.Add(loadedCallBack);
                }
                return;
            }
            if(loadedCallBack!=null)
            {
                Loading3DView loading3dview = new Loading3DView();
                _3DViewAsyncLoadingDict.Add(type, loading3dview);
                _3DViewAsyncLoadingDict[type].loadedCallBackList.Add(loadedCallBack);
            }
            string asset_path = GetABPath(_assetPathDict[type]);  //_assetPathDict[type].abpath;
            var obj = await ResourceMgr.Instance.InstantiateAsync(asset_path);
            T gameObj = Activator.CreateInstance<T>();
            if(obj!= null)
            {
                gameObj.gameObject = obj.result;
                gameObj.transform = gameObj.gameObject.transform;
                gameObj.transform.SetParent(this.transform);
                await gameObj.Awake();
                await gameObj.Start();
                gameObj.OnEnable();
                _3DViewDict.Add(type, gameObj);

                _3DViewList.Add(gameObj);
                
            }else{
                DebugEX.LogError (asset_path + " 加载失败 ！！ ");
            }
            if(_3DViewAsyncLoadingDict.ContainsKey(type))
            {
                var list = _3DViewAsyncLoadingDict[type];
                _3DViewAsyncLoadingDict.Remove(type);
                for(int i = 0;i< list.loadedCallBackList.Count;i++)
                {
                    list.loadedCallBackList[i]?.Invoke(gameObj);
                }
                
                if(list.isDestroy)
                {
                    Remove3DView<T>();
                }
            }
        }

        public T GetUI3DView<T>() where T : View3DBase
        {
            View3DBase cachedObj;
            _3DViewDict.TryGetValue(typeof(T), out cachedObj);
            if (cachedObj!= null)
            {
                return cachedObj as T;
            }
            else
            {
                return default(T);
            }
        }

        public bool IsHave3DView<T>()where T : View3DBase
        {
            return _3DViewDict.ContainsKey(typeof(T));
        }
        public void Remove3DView<T>(bool force = false)
        {
            View3DBase cachedObj;
            Type type = typeof(T);
            if (addCountDic.ContainsKey(type))
            {
                if(!force)
                {
                    addCountDic[type]--;
                    if (addCountDic[type] > 0)
                        return;
                    else
                        addCountDic[type] = 0;
                }else{
                    addCountDic[type] = 0;
                }
            }
            //如果已完成加载
            bool isLoaded = _3DViewDict.TryGetValue(type, out cachedObj);
            if (isLoaded)
            {
                cachedObj.OnDisable();
                cachedObj.OnDestroy();
                ResourceMgr.Instance.UnloadGameObject(cachedObj.gameObject);
                _3DViewDict.Remove(type);

                _3DViewList.Remove(cachedObj);
                return;
            }
            //如果已在加载中
            Loading3DView cachedAsyncHandler;
            bool isLoading = _3DViewAsyncLoadingDict.TryGetValue(type, out cachedAsyncHandler);
            if (isLoading)
            {
                cachedAsyncHandler.isDestroy = true;
            }
        }

        public void HideOthersExcept<T>()
        {
            Type type = typeof(T);
            foreach (KeyValuePair<Type, View3DBase> item in _3DViewDict)
            {
                item.Value.gameObject.SetActive(item.Key == type);
            }
        }

        public void Hide<T>()
        {
            Type type = typeof(T);
            Hide(type);
        }

        private void Hide(Type type)
        {
            View3DBase cachedObj;
            //如果已完成加载
            bool isLoaded = _3DViewDict.TryGetValue(type, out cachedObj);
            if (isLoaded)
            {
                cachedObj.gameObject.SetActive(false);
                cachedObj.OnDisable();
            }
        }

        public void Show<T>()
        {
            Type type = typeof(T);
            Show(type);
        }

        private void Show(Type type)
        {
            View3DBase cachedObj;
            //如果已完成加载
            bool isLoaded = _3DViewDict.TryGetValue(type, out cachedObj);
            if (isLoaded)
            {
                cachedObj.gameObject.SetActive(true);
                cachedObj.OnEnable();
            }
        }
        
        /// <summary>
        /// 这个接口慎用，别误删别的3Dview
        /// </summary>
        public void HideAll() 
        {
            foreach (KeyValuePair<Type, View3DBase> item in _3DViewDict)
            {
                item.Value.gameObject.SetActive(false);
                item.Value.OnDisable();
            }
        }

        //兼容非热更接口，修改成所有热更代码后就删掉
        private void Tmp_Hide(string ClassName)
        {
            Type type = Type.GetType(ClassName);
            View3DBase cachedObj;
            //如果已完成加载
            bool isLoaded = _3DViewDict.TryGetValue(type, out cachedObj);
            if (isLoaded)
            {
                cachedObj.gameObject.SetActive(false);
                cachedObj.OnDisable();
            }
        }
        public Dictionary<string,bool> GetView3DObjState()
        {
            Dictionary<string,bool> _viewStateDic = new Dictionary<string,bool>();
            foreach(var item in _3DViewList)
            {
                _viewStateDic.Add(item.gameObject.name,item.gameObject.activeSelf);
            }
            return _viewStateDic;
        }
        public void SetView3DObjState(Dictionary<string,bool> _viewStateDic)
        {
            foreach(var item in _3DViewList)
            {
                if(_viewStateDic.ContainsKey(item.gameObject.name))
                item.gameObject.SetActive(_viewStateDic[item.gameObject.name]); 
            }
        }

        const string Medium = "_Medium";
        const string Low = "_Low";
        private string GetABPath(Scene3DConfig.Config3D config)
        {
            if(config.supportQuality)
            {
                int level = SettingConfig.CurQualityLevel; 
                if(level <=2)
                {
                    return config.abpath+ Low;
                }else if(level <= 4)
                {
                    return config.abpath+ Medium;
                }else{
                    return config.abpath;
                }
            }else{
                return config.abpath;
            }
        }
    }
    
}<fim_middle>)
                {
                    if(isActive)
                    <|endoftext|>