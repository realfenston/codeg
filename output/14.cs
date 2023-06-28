<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StadiumSideConfig 
{

    public  const string SideLines = "3D/SideProps/Prefab/SideLines";
    public  const string SideProps_Fire = "3D/SideProps/Prefab/SideProps_Fire01";
    public  const string SideProps_Fireworks = "3D/SideProps/Prefab/SideProps_Fireworks01";
    //特效路径
    public static Dictionary<string, string> Eff_Path = new Dictionary<string, string>(){
       {"101", "UI/ParticleEffect/01_Prefab/Eff_Sprayfireworks_01"},
       {"102", "UI/ParticleEffect/01_Prefab/Eff_Sprayfireworks_02"},
       {"103", "UI/ParticleEffect/01_Prefab/Eff_Sprayfireworks_03"},
       {"201", "UI/ParticleEffect/01_Prefab/Eff_Spraysmoke_01"},
       {"202", "UI/ParticleEffect/01_Prefab/Eff_Spraysmoke_02"},
       {"203", "UI/ParticleEffect/01_Prefab/Eff_Spraysmoke_03"},
       {"301", "UI/ParticleEffect/01_Prefab/Eff_Spitsparks_01"},
       {"302", "UI/ParticleEffect/01_Prefab/Eff_Spitsparks_02"},
       {"303", "UI/ParticleEffect/01_Prefab/Eff_Spitsparks_03"},
       {"304", "UI/ParticleEffect/01_Prefab/Eff_Spitsparks_xinnian"},
       {"401", "UI/ParticleEffect/01_Prefab/Eff_penhuo_01"},
       {"402", "UI/ParticleEffect/01_Prefab/Eff_penhuo_02"},
       {"403", "UI/ParticleEffect/01_Prefab/Eff_penhuo_03"},
       {"501", "3D/SideProps/Prefab/SideProps_Balloon"},
    };

    //特效修改颜色路径
    public static Dictionary<string, List<string>> Eff_Color_Path = new  Dictionary<string, List<string>>(){
        {"101", new List<string>(){"eff/add","eff/liziguangyun","eff/xiaolizi01"}},
        {"102", new List<string>(){"eff/add2","eff/liziguangyun","eff/xia<fim_suffix>9","eff/liziguangyun","eff/xiaolizi01"}},
        {"201", new List<string>(){"eff/xiaolizi","eff/tuowei_smoke"}},
        {"202", new List<string>(){"eff/xiaolizi","eff/tuowei_smoke"}},
        {"203", new List<string>(){"eff/xiaolizi","eff/tuowei_smoke", "eff/smoke","eff/smoke2", "eff/xiaolizi2","eff/tuowei_smoke2"}},
        {"301", new List<string>(){"eff/smoke01","eff/lizi01","eff/lizi02"}},
        {"302", new List<string>(){"eff/Smoke","eff/lizi01","eff/lizi02","eff/lizi03"}},
        {"303", new List<string>(){"eff/Smoke","eff/lizi01","eff/lizi02","eff/lizi03","eff/guangyun02"}},
        {"401", new List<string>(){"eff/guangyun01","eff/guangyun02","eff/Fire"}},
        {"402", new List<string>(){"eff/guangyun01","eff/guangyun02","eff/Fire"}},
        {"403", new List<string>(){"eff/guangyun01","eff/guangyun02","eff/Fire"}},
    }; 

    //场边装饰
    public static Dictionary<int, string> SideOrnament = new Dictionary<int, string>()
    {
        {2, "3D/ChristmasProps/Prefab/ChristmasProps01"},  //圣诞场边
        {3, "3D/SideProps/Prefab/SideProps_BlueMoon"},  //蓝月亮场边
        {4, "3D/SideProps/Prefab/SideProps_Spray"},  //大帆船场边
        {5, "3D/SideProps/Prefab/SideProps_paintedEggshell"},//复活节蛋
        {6, "3D/SideProps/Prefab/SideProps_FigurInStone"},//复活节石像
    };  
    //圣诞装饰 特殊处理
    public static string ChristmasProps02 = "3D/ChristmasProps/Prefab/ChristmasProps02";
    public static string ChristmasLightMat_Path = "sds/shengdanshu1/shengdanshu_04";

    //场边横幅
    public static Dictionary<int, string> SideBanner = new Dictionary<int, string>(){
        {1,"3D/SideProps/Prefab/SideProps_Banner_Allanz"},
        {2,"3D/SideProps/Prefab/SideProps_Banner_ManCity"},
    };

    public static Color HexToColor(string hex)
    {
        if (Parse(hex, out Color color))
            return color;
        return Color.white;
    }

    static bool Parse(string str, out Color val)
    {
        if (str.Length!= 6 && str.Length!= 8)
        {
            val = Color.white;
            return true;
        }

        val = new Color();

        for (int i = 0; i < 6;)
        {
            if (!Parse(str[i], out int v1))
                return false;
            if (!Parse(str[i + 1], out int v2))
                return false;

            if (i == 0)
                val.r = (v1 * 16 + v2) * 1.0f / 255f;
            if (i == 2)
                val.g = (v1 * 16 + v2) * 1.0f / 255f;
            if (i == 4)
                val.b = (v1 * 16 + v2) * 1.0f / 255f;
            i += 2;
        }

        if (str.Length == 8)
        {
            if (!Parse(str[6], out int v1))
                return false;
            if (!Parse(str[7], out int v2))
                return false;
            val.a = (v1 * 16 + v2) * 1.0f / 255f;
        }
        else
            val.a = 1;

        return true;
    }

    static bool Parse(char c, out int val)
    {
        if (c >= '0' && c <= '9')
            val = c - '0';
        else if (c >= 'a' && c <= 'f')
            val = c - 'a' + 10;
        else if (c >= 'A' && c <= 'F')
            val = c - 'A' + 10;
        else
        {
            val = 0;
            return false;
        }

        return true;
    }


    public static  bool IsNormalScene()
    {
         Scene scene = SceneManager.GetActiveScene();
        if(scene.name.Contains("StadiumKhalifa") || scene.name.Contains("URPScene"))
        {
            return false;
        }
        return true;  
    }
}
<fim_middle>olizi01"}},
        {"103", new List<string>(){"eff/add3","eff/liziguangyun","eff/xiaolizi01"}},
        {"201", new List<string>(){"eff