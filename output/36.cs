<fim_prefix>﻿using Framework.GalaSports.GameScene;
using Framework.GalaSports.MVC.Core;
using Framework.GalaSports.MVC.Core.Events;
using GalaSports;
using GalaSports.FootballNG;
using Platform;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    partial class BattleTeamInfoTip : MonoBehaviour
    {
        Dictionary<int, float> ShowTimeDic = new Dictionary<int, float>() { { 1, 2 }, { 2, 2 }, { 3, 1 }, { 4, 0.5f }, { 5, 0.1f } };
        public void ShowTip(ISoccerStatisticManager ssm, int type, TeamConfigurationData teamA, TeamConfigurationData teamB, string matchTime)
        {
            gameObject.SetActive(true);

            BattleTipConfig.TeamInfoSmallTipStayTimeL = GetShowTime(ssm.teamAStatistic.sgsList.Count);
            BattleTipConfig.TeamInfoSmallTipStayTimeR = GetShowTime(ssm.teamBStatistic.sgsList.Count);

            SetData(ssm, teamA, teamB, matchTime, type);
            if (type!= 2)
            {
                StartCoroutine(ShowTipItemAIEnumerator(ssm.teamAStatistic.sgsList,BattleTipConfig.TeamInfoSmallTipStayTimeL));
                StartCoroutine(ShowTipItemBIEnumerator(ssm.teamBStatistic.sgsList,BattleTipConfig.TeamInfoSmallTipStayTimeR));
            }
            int maxItemNum = Mathf.Max(ssm.teamAStatistic.sgsList.Count, ssm.teamBStatistic.sgsList.Count);
            float maxItemTime = Mathf.Max(BattleTipConfig.TeamInfoSmallTipStayTimeL, BattleTipConfig.TeamInfoSmallTipStayTimeR);
            float itemWaitTime = Mathf.FloorToInt(maxItemNum / 3f) * (maxItemTime + BattleTipConfig.ShowTipTime + BattleTipConfig.HideTipTime);
            StartCoroutine(ShowTipIEnumerator(ssm, itemWaitTime, type, teamA, teamB));
            float time = 0f;
            if (type == 2)
            {
                time = BattleTipConfig.TeamInfoTipGoalStayTime;
            }
            else
            {
                time = itemWaitTime;
                time += Mathf.Max(itemWaitTime + BattleTipConfig.TeamInfoTipLeftStayTime) < 4? 4 - (itemWaitTime) : BattleTipConfig.TeamInfoTipLeftStayTime;
                time += BattleTipConfig.Hide<fim_suffix> && type!= 2 && type!= 6)
                {
                    if (transform!= null)
                    {
                        transform.GetComponent<CanvasGroup>().alpha = 0f;
                        EventDispatcher.TriggerEvent("SetOffsetState", true, true);
                    }
                }
            }, time);
        }
        private float GetShowTime(int ballCount)
        {
            int groupNum = Mathf.CeilToInt(ballCount / 3f);
            if(ShowTimeDic.ContainsKey(groupNum))
                return ShowTimeDic[groupNum];
            else
                return 0.1f;
        }

        private void SetData(ISoccerStatisticManager ssm, TeamConfigurationData teamA, TeamConfigurationData teamB, string matchTime, int type)
        {
            Text_TeamAName.text = ssm.teamAStatistic.teamName;
            Text_TeamBName.text = ssm.teamBStatistic.teamName;
            Text_TeamAUnionShortName.text = teamA.union_shortname;
            Text_TeamBUnionShortName.text = teamB.union_shortname;
            if (type!= 6)
            {
                Text_TeamAScore.text = ssm.teamAStatistic.score.ToString();
                Text_TeamBScore.text = ssm.teamBStatistic.score.ToString();
                if(GameDataHelper.PveCupMatchType == 4)
                {
                    Text_TeamATotalScore.text = (ssm.teamAStatistic.score + GameDataHelper.TeamALastScore).ToString();
                    Text_TeamBTotalScore.text = (ssm.teamBStatistic.score + GameDataHelper.TeamBLastScore).ToString();
                }
            }
            else
            {
                Text_TeamAScore.text = ssm.teamAStatistic.penaltyScore.ToString();
                Text_TeamBScore.text = ssm.teamBStatistic.penaltyScore.ToString();
            }
            Text_MatchTime.text = matchTime;
            SetSprite(Image_TeamALogo, Image_TeamABg, true, ssm, teamA.teamLogo);
            SetSprite(Image_TeamBLogo, Image_TeamBBg, false, ssm, teamB.teamLogo);
        }

        private void SetSprite(Image imageLogo, Image imageBg, bool teamA, ISoccerStatisticManager ssm, Sprite logo)
        {
            int logoId = teamA? 1 : 0;
            //if (imageBg!= null &&!GuideConfig.isGuideMatch()) imageBg.color = BattleTipConfig.GetColor(logoId);
            imageLogo.sprite = logo;
            if (imageLogo.sprite == null || imageLogo.sprite.texture == null)
                return;
            if (imageLogo.sprite.texture.name == "NationFlag")
            {
                imageLogo.GetComponent<RectTransform>().localScale = new Vector2(0.8f, 0.8f);
            }
        }

        IEnumerator ShowTipIEnumerator(ISoccerStatisticManager ssm, float itemWaitTime, int type, TeamConfigurationData teamA, TeamConfigurationData teamB)
        {
            // transform.GetComponent<CanvasGroup>().alpha = 0;
            transform.GetComponent<CanvasGroup>().alpha = 1;
            if (type == 2)
            {
                yield return new WaitForSeconds(BattleTipConfig.TeamInfoTipGoalStayTime);
            }
            else
            {
                yield return new WaitForSeconds(itemWaitTime);
                yield return new WaitForSeconds(Mathf.Max(itemWaitTime + BattleTipConfig.TeamInfoTipLeftStayTime) < 4? 4 - (itemWaitTime) : BattleTipConfig.TeamInfoTipLeftStayTime);
            }

            if (transform!= null)
            {
                Go.to(transform.GetComponent<CanvasGroup>(), BattleTipConfig.HideTipTime, new GoTweenConfig().floatProp("alpha", 0));
            }
        }

        IEnumerator ShowTipItemAIEnumerator(List<SoccerGoalStatistic> sgsList,float showTime)
        {
            int itemMo = 0;
            for (int i = 0; i < List_ItemTeamA.Count; i++)
            {
                List_ItemTeamA[i].gameObject.SetActive(false);
            }
            for (int i = 0, num = sgsList.Count; i < num; i++)
            {
                itemMo = (i + 1) % 3;
                List_ItemTeamA[i % 3].ShowTip(sgsList[i],showTime);
                if (itemMo == 0)
                {
                    yield return new WaitForSeconds(showTime + BattleTipConfig.ShowTipTime + BattleTipConfig.HideTipTime);
                }
            }
        }

        IEnumerator ShowTipItemBIEnumerator(List<SoccerGoalStatistic> sgsList, float showTime)
        {
            int itemMo = 0;
            for (int i = 0; i < List_ItemTeamB.Count; i++)
            {
                List_ItemTeamB[i].gameObject.SetActive(false);
            }
            for (int i = 0, num = sgsList.Count; i < num; i++)
            {
                itemMo = (i + 1) % 3;
                List_ItemTeamB[i % 3].ShowTip(sgsList[i], showTime);
                if (itemMo == 0)
                {
                    yield return new WaitForSeconds(showTime + BattleTipConfig.ShowTipTime + BattleTipConfig.HideTipTime);
                }
            }
        }
    }
}
<fim_middle>TipTime;
            }
            Go.DelayCall(() =>
            {
                if (type != 1<|endoftext|>