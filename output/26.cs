<fim_prefix>using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SeasonCupWinningAnimationConfig
{
    public static List<string[]> PreLoadClothName;
    public static int[] TeamPlayerIndex;
    public static int[] TeamPlayerCount;
    public static int[] CameraFOV;
    public static int[] FarShadowDistance;
    public static int[] NearShadowDistance;
    public static int[][] RandomCloth = {
          new int[]{ 0,1,2 },
          new int[]{ 0,2,1 },
          new int[]{ 1,2,0 },
          new int[]{ 1,0,2 },
          new int[]{ 2,0,1 },
          new int[]{ 2,1,0 }
    };
    public static void InitPreLoad(int cupIndex)
    {
        PreLoadClothName = new List<string[]>();
        CameraFOV = new int[] {35,40,40,15 };
        FarShadowDistance = new int[] {40,6,6,87 };
        NearShadowDistance = new int[] {39,4,4,85 };
        if (cupIndex == 0)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_1", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_1", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
        else if (cupIndex == 1)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_2", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_2", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7,<fim_suffix>3, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
        else if (cupIndex == 2)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_3", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_3", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
        else if (cupIndex == 3)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_4", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_4", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
        else if (cupIndex == 4)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_5", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_5", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
        else if (cupIndex == 5)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_6", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_6", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
        else if (cupIndex == 6)
        {
            PreLoadClothName.Add(new string[] { "prizePlayer4_2", "prizePlayer5_2", "prizePlayer6_2", "prizePlayer7_2", "prizePlayer8_2", "prizePlayer9_2_7", "prizePlayer10_2", "prizePlayer11_2", "prizePlayer12_2", "prizePlayer13_2", "prizePlayer14_2", "prizePlayer15_2" });
            PreLoadClothName.Add(new string[] { "prizePlayer7_3", "prizePlayer8_3", "prizePlayer9_3_7", "prizePlayer10_3", "prizePlayer11_3", "prizePlayer12_3", "prizePlayer13_3", "prizePlayer14_3", "prizePlayer15_3", "prizePlayer16_3", "prizePlayer17_3", "prizePlayer18_3" });
            TeamPlayerIndex = new int[] { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            TeamPlayerCount = new int[] { 0, 12, 3, 0 };
        }
    }

  
}
<fim_middle> 8, 9, 10, 11, 12, 1<|endoftext|>