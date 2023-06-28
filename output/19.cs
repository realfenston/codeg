<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Framework.GalaSports.Service;

public class RankUpgradeCup3DView : View3DBase
{
    const string path_root = "3D/RankMatchCups/Cup0{0}";
    const string Layer_Name = "UI3D";

    GameObject old_cup_asset;
    GameObject new_cup_asset;
    Transform old_cup;
    Transform new_cup;


    Vector3 lightAngles;

    Camera ray_Camera;

    float old_fieldOfView;
    CameraRenderPassSetting setting;
    float old_ShadowNear;
    float old_ShadowFar;

    //创建奖杯
    public async Task Init(int old_cup_id, int new_cup_id)
    {
        RemoveCup();
        string old_path = string.Format(path_root, old_cup_id);
        var obj = await ResourceMgr.Instance.LoadAssetAsync<GameObject>(old_path);
        if(obj!= null)
        {
            old_cup_asset = obj.result;
            GameObject gmObj = GameObject.Instantiate(old_cup_asset);
            old_cup = gmObj.transform;
            old_cup.SetParent(this.transform);
            old_cup.localPosition = Vector3.zero;
            if(old_cup_id == 7)
            {
                old_cup.localScale = Vector3.one * 4.4f;
            }else{
                old_cup.localScale = Vector3.one * 11;
            }
            
            SetLayer(old_cup, Layer_Name);
            old_cup.gameObject.SetActive(true);
        }
        string new_path = string.Format(path_root, new_cup_id);
        obj = await ResourceMgr.Instance.LoadAssetAsync<GameObject>(new_path);
        if(obj!=null)
        {
            new_cup_asset = obj.result;
            GameObject gmObj = GameObject.Instantiate(new_cup_asset);
            new_cup = gmObj.transform;
            new_cup.SetParent(this.transform);
            new_cup.localPosition = Vector3.zero;
             if(new_cup_id == 7)
            {
                new_<fim_suffix>
                new_cup.localScale = Vector3.one * 11;
            }
            SetLayer(new_cup, Layer_Name);
            new_cup.gameObject.SetActive(false);
        }
    }
    //升级动画
    public void RankUpgradeAnim()
    {
        old_cup.gameObject.SetActive(true);
        CupRotation(old_cup, 0.5f, 0, new Vector3(0, -90, 0), new Vector3(0, 0, 0), GoEaseType.SineIn, ()=>{
            old_cup.gameObject.SetActive(false);
            new_cup.gameObject.SetActive(true);
        });
        CupRotation(new_cup, 0.5f, 0.5f, new Vector3(0, 0, 0), new Vector3(0, 90, 0), GoEaseType.SineOut);
    }

    void CupRotation(Transform cup, float time, float delayTime, Vector3 startEulerAngles, Vector3 endEulerAngles, GoEaseType easeType, System.Action complete = null)
    {
        Go.killAllTweensWithTarget(cup);
        RotationCup rotationCup = cup.GetComponent<RotationCup>();
        if (rotationCup!= null)
        {
            rotationCup.deltaX = 0;
            rotationCup.isCanRotate = false;
        }
        cup.localRotation = Quaternion.Euler(startEulerAngles);
        Go.to(cup, time, new GoTweenConfig()
          .localRotation(Quaternion.Euler(endEulerAngles))
          .setDelay(delayTime)
          .setEaseType(easeType)
          .onComplete((obj) => {
               if (rotationCup!= null)
               {
                   rotationCup.deltaX = 0;
                   rotationCup.isCanRotate = true;
                   complete?.Invoke();
               }
           }));
    }

    void SetLayer(Transform cup, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        Renderer [] renders = cup.GetComponentsInChildren<Renderer>();
        if(renders!= null)
        {
            for(int i = 0; i< renders.Length;i++)
            {
                renders[i].gameObject.layer = layer;
            }
        }
    }

    void RemoveCup()
    {
        if(old_cup!=null)
        {
            Go.killAllTweensWithTarget(old_cup);
            GameObject.Destroy(old_cup.gameObject);
            old_cup = null;

            ResourceMgr.Instance.UnloadAsset<GameObject>(old_cup_asset);
            old_cup_asset = null;
        }
        if(new_cup!= null)
        {
            Go.killAllTweensWithTarget(new_cup);
            GameObject.Destroy(new_cup.gameObject);
            new_cup = null;

            ResourceMgr.Instance.UnloadAsset<GameObject>(new_cup_asset);
            new_cup_asset = null;
        }
    }
    public override void OnDestroy(){
        RemoveCup();
    }


   
    public override void OnEnable()
    {
        if(StadiumSceneController.Instance == null) return;
        lightAngles = StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles;
        StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles = new Vector3(40,123,115);
        if(ray_Camera == null)
        {
            ray_Camera = Platform.CameraAnimController.Instance.Cam.transform.Find("DummyCamera").GetComponent<Camera>();
            old_fieldOfView = ray_Camera.fieldOfView;
            ray_Camera.fieldOfView = 42;
        }
        setting = Platform.CameraAnimController.Instance.Cam.GetComponent<CameraRenderPassSetting>();
        if(setting!=null)
        {
            old_ShadowNear = setting.ShadowNear;
            old_ShadowFar = setting.ShadowFar;
            setting.ShadowNear = 5;
            setting.ShadowFar = 7;
        }
    }
    public override void OnDisable() {
        // StadiumSceneController.Instance.StadiumLight.transform.localEulerAngles = lightAngles;
        //  if(ray_Camera!= null)
        // {
        //     ray_Camera.fieldOfView = old_fieldOfView;
        // }
        // if(setting!=null)
        // {
        //     setting.ShadowNear = old_ShadowNear;
        //     setting.ShadowFar = old_ShadowFar;
        // }
    }
}
<fim_middle>cup.localScale = Vector3.one * 4.4f;
            }else{<|endoftext|>