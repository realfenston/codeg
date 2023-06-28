<fim_prefix>﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.GalaSports.MVC.Core;
using Framework.GalaSports.MVC.Core.Events;
using Framework.GalaSports.Service;
using GalaSports.FootballNG;
using Platform;
using UnityEngine;
using UnityEngine.UI;
using static GalaSDKBase.GalaSDKBaseFunction;

namespace Assets.Scripts
{
    [Module("BattleTip")]
    public partial class BattleTipView : FootballBaseView
    {
        // GameGrassScoreEffectController grassController;
        private ISoccerStatisticManager _statisticManager;
        private TeamConfigurationData _teamAConfigurationData;
        private TeamConfigurationData _teamBConfigurationData;
        int quarter = 1;
        int lastState = -1;
        // bool isShowGrassContent = false;
        // GoTween tween;
        // int _gameType;
        bool guideMatchKickOffLog = true;
        Dictionary<int, string> halfTimeTextDict;
        private void Awake()
        {
            Platform.EventDispatcher.AddEventListener<ISoccerStatisticManager, TeamConfigurationData, TeamConfigurationData>("ONInitStaticManager", InitStaticManager);
        }

        public void InitStaticManager(ISoccerStatisticManager manager, TeamConfigurationData teamA, TeamConfigurationData teamB)
        {
            _statisticManager = manager;
            _teamAConfigurationData = teamA;
            _teamBConfigurationData = teamB;
            if (GamePlayConfig.GamePlayType!= GameType.Normal)
            {
                return;
            }
            SoccerGameEvents.Instance.onGameTipFoul.AddListener(OnFoulHandler);
            SoccerGameEvents.Instance.onGoalEvent.AddListener(OnScoreHandler);
            SoccerGameEvents.Instance.onShowGameStateTip.AddListener(ShowGameStateTip);

            SoccerGameEvents.Instance.onUpdateSubstitutePlayersEvent.AddListener(OnUpdateSubstitutePlayersEvent);
            SoccerGameEvents.Instance.onMatchQuarterStartKickOffEvent.AddListener(OnTriggerQuarterStartKickOffTipView);
            SoccerGameEvents.Instance.onUpdateMatchStateEvent.AddListener(onUpdateMatchStateEvent);
            // Platform.EventDispatcher.AddEventListener("GameEndStatus", HideGrassContent);
            // Platform.EventDispatcher.AddEventListener("HideGrassContent", HideGrassContent);
            SpiecalScreen();
            halfTimeTextDict = new Dictionary<int, string>();
            halfTimeTextDict.Add(1, LanguageKit.Get("GamePlayUIPanel/HalfTime/SecondHalf/Text"));//中场休息
            halfTimeTextDict.Add(2, LanguageKit.Get("GamePlayUIPanel/SecondHalf/SecondHalf/Text_light"));//下半场
            halfTimeTextDict.Add(3, LanguageKit.Get("Label_1660727720395"));//常规加时
            halfTimeTextDict.Add(4, LanguageKit.Get("Label_1660727933049"));//点球大战
            halfTimeTextDict.Add(5, LanguageKit.Get("Label_1661483427069"));//加时赛上半场
            halfTimeTextDict.Add(6, LanguageKit.Get("Label_1661483435197"));//加时赛下半场
            halfTimeTextDict.Add(7, LanguageKit.Get("Label_1661916314138"));//常规比赛结束
            halfTimeTextDict.Add(8, LanguageKit.Get("Label_1661916314139"));//加时赛中场
        }
        private void SpiecalScreen()
        {
            if (AppConfig.Instance.IsSpecialScreen)
            {
                var exchangeParent = transform.Find("ExchangeParent");
                float offset = AppConfig.Instance.BandHeight;
                Vector2 offsetLeft = new Vector2(offset, 0);
                exchangeParent.GetComponent<RectTransform>().anchoredPosition = offsetLeft;
            }
        }

        private void OnUpdateSubstitutePlayersEvent(string teamId, List<SoccerPlayerAttributes> playerAttributes, Dictionary<int, int> dic)
        {
            // if (GlobalConfigManager.matchGamePlayConfig.IsHomeTeam&&_teamBConfigurationData.teamId== teamId||!GlobalConfigManager.matchGamePlayConfig.IsHomeTeam&&_teamAConfigurationData.teamId== teamId)
            // {
            //     return;
            // }
            if (dic == null)
            {
                return;
            }
            var IsTeamA = _teamAConfigurationData.teamId == teamId;
            var config = IsTeamA? _teamAConfigurationData : _teamBConfigurationData;
            List<ball_player> players = new List<ball_player>();
            players.AddRange(config.teamStartPlayers);
            players.AddRange(config.teamBenchPlayers);
            foreach (var item in dic)
            {
                var downPlayer = item.Key;

                var player1 = players.Find((player) => { return player.case_id == downPlayer; });

                var upPlayer = item.Value;
                var player2 = players.Find((player) => { return player.case_id == upPlayer; });
#if OPEN_DEBUG_LOG
                DebugEX.Log("UpdateSubstitutePlayer:   " + teamId + "   " + player1.shirt_number + "  " + PlayerNameCfg.GetItem(player1.tm_id).name + "   " + player2.shirt_number + "  " + PlayerNameCfg.GetItem(player2.tm_id).name);
#endif
                if (player1!= null && player2!= null)
                {
                    if (IsTeamA)
                        SC_BattleExchangePlayerTip_Left.ShowTip(player1, player2, config.teamLogo);
                    else
                        SC_BattleExchangePlayerTip_Right.ShowTip(player1, player2, config.teamLogo);
                }
            }
        }<fim_suffix>ScoreTip.ShowTip(sgs, sgs.isTeamA? _teamAConfigurationData : _teamBConfigurationData);
            Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, false, false);
        }

        private void OnFoulHandler(SoccerFoulStatistic foul)
        {

            SoccerPlayerStatistic playerStatistic = _statisticManager.GetOnPitchPlayerStatisticByName(foul.playerName);
            EnumShowTipType enumType;
            switch (foul.foulType)
            {
                case EnumFoulStatisticType.YellowGoodForAttack:
                case EnumFoulStatisticType.YellowCardDirectedFreeKick:
                case EnumFoulStatisticType.YelloCardPenaltyKick:
                    enumType = EnumShowTipType.Yellow;
                    break;
                case EnumFoulStatisticType.RedCardDirectedFreeKick:
                case EnumFoulStatisticType.RedCardGoodForAttack:
                case EnumFoulStatisticType.RedCardPenaltyKick:
                    enumType = EnumShowTipType.Red;
                    break;
                default:
                    enumType = EnumShowTipType.TeamInfo;
                    return;
            }

            var teamCfg = "TeamA" == playerStatistic.teamUid? _teamAConfigurationData : _teamBConfigurationData;
            SC_BattleWarningTip.ShowTip(enumType, playerStatistic, teamCfg);
            Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, false, false);
        }

        // 展示转场UI动画和界面
        private void ShowGameStateTip(int type)
        {
            // Platform.EventDispatcher.TriggerEvent<System.Action<int>>("GetGamePlayType", (value) =>
            // {
            //     _gameType = value;
            // });
            GlobalConfigManager.matchGamePlayConfig.IsHalfTime = false;
            GlobalConfigManager.matchGamePlayConfig.IsExtraHalfTime = false;
            GlobalConfigManager.matchGamePlayConfig.IsPenaltyHalfTime = false;
            switch (type)
            {
                case -2: //特殊比赛下半开场
                    if (footballController.IsKickOff())
                    {
                        lastState = 8;
                        // ShowGrassContent();
                        Platform.EventDispatcher.TriggerEvent<ISoccerStatisticManager, TeamConfigurationData, TeamConfigurationData>("OnInitSceneLogoData", _statisticManager, _teamAConfigurationData, _teamBConfigurationData);
                    }
                    if (GameDataHelper.IsPvPOnLine)
                    {
                        UpdateUtils.Instance.DelayCall(60, () => Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true));
                    }
                    else
                    {
                        Go.DelayCall(() => Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true), 2f);
                    }
                    SC_BattleTeamInfoTip.ShowTip(_statisticManager, type, _teamAConfigurationData, _teamBConfigurationData, halfTimeTextDict[2]);
                    break;
                case -1://正常开场
                    lastState = 8;
                    // ShowGrassContent();
                    Platform.EventDispatcher.TriggerEvent<ISoccerStatisticManager, TeamConfigurationData, TeamConfigurationData>("OnInitSceneLogoData", _statisticManager, _teamAConfigurationData, _teamBConfigurationData);
                    if (GameDataHelper.IsPvPOnLine)
                    {
                        UpdateUtils.Instance.DelayCall(75, () => Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true));
                    }
                    else
                    {
                        Go.DelayCall(() => Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true), 2.5f);
                    }

                    // if (GameDataHelper.IsPvPOnLine)//pvp没有自动隐藏水印
                    // {
                    //     HideGrassContentGradually(8f);
                    // }
                    break;
                case 0://中场休息
                    GlobalConfigManager.matchGamePlayConfig.IsHalfTime = true;
                    GlobalConfigManager.matchGamePlayConfig.IsPvpHalfTime = GameDataHelper.IsPvPOnLine;
                    ShowHalfTimeAni(halfTimeTextDict[1]);
                    Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, false, false);
                    break;
                case 1://常规比赛的终场结束
                    Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, false, false);
                    SC_BattleTeamInfoTip.ShowTip(_statisticManager, type, _teamAConfigurationData, _teamBConfigurationData, halfTimeTextDict[2]);
                    break;
                case 2:
                    if (GameDataHelper.IsPvPOnLine)
                    {
                        UpdateUtils.Instance.DelayCall(75, () =>
                        {
                            Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true);
                        });
                    }
                    else
                    {
                        Go.DelayCall(() =>
                        {
                            Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true);
                        }, 2.5f);
                    }
                    break;
                case 3://常规比赛休息到加时赛
                    GlobalConfigManager.matchGamePlayConfig.IsExtraHalfTime = true;
                    Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true);
                    ShowHalfTimeAni(halfTimeTextDict[7]);
                    break;
                case 4://加时赛中场休息
                    GlobalConfigManager.matchGamePlayConfig.IsExtraHalfTime = true;
                    Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true);
                    ShowHalfTimeAni(halfTimeTextDict[8]);
                    break;
                case 5://点球大战开始
                    //GamePlayConfig.IsPenaltyShootOut = true;
                    GlobalConfigManager.matchGamePlayConfig.IsPenaltyHalfTime = true;
                    Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true);
                    ShowHalfTimeAni(halfTimeTextDict[4]);
                    break;
                case 6://点球大战结束
                    Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, false, false);
                    SC_BattleTeamInfoTip.ShowTip(_statisticManager, type, _teamAConfigurationData, _teamBConfigurationData, halfTimeTextDict[4]);
                    break;
            }
        }

        private void onUpdateMatchStateEvent(MatchStateData obj)
        {
            int curState = (int)obj.matchState;
            if (curState == (int)MatchStateData.MatchState.KickOff && lastState!= -1)
            {
                if (GameDataHelper.IsPvPOnLine)
                {
                    UpdateUtils.Instance.DelayCall(90, ShowMainUIAfterKickOff);
                }
                else
                {
                    Go.DelayCall(ShowMainUIAfterKickOff, 3f);
                }
                // ShowGrassContent();
                Platform.EventDispatcher.TriggerEvent<ISoccerStatisticManager, TeamConfigurationData, TeamConfigurationData>("OnInitSceneLogoData", _statisticManager, _teamAConfigurationData, _teamBConfigurationData);
            }
            if (lastState == (int)MatchStateData.MatchState.KickOff && curState == (int)MatchStateData.MatchState.Playing)
            {
                if (GuideConfig.isGuideMatch() && guideMatchKickOffLog)
                {
                    guideMatchKickOffLog = false;
                    TGALog.Instance.Log(GuideStepType.CLICK_PASS_KICK_OFF);
                }
                // HideGrassContentGradually(2f);
            }
            lastState = curState;
        }

        private void ShowMainUIAfterKickOff()
        {
            if (footballController.IsExitPitch() || footballController.frameId == 0 || FootballManager.IsInRecord || FootballManager.IsInCelebrate)
                return;
            Platform.EventDispatcher.TriggerEvent(Events.SetPlayingMainUIState, true, true);
        }

        private void OnTriggerQuarterStartKickOffTipView(MatchStateData obj)
        {
            string kickOffStartTxt = halfTimeTextDict[2];
            bool MayKickOff = false;
            quarter = obj.quarter;
            if (obj.quarter == 2)
            {
                kickOffStartTxt = halfTimeTextDict[2];
                MayKickOff = true;
            }
            else if (obj.quarter == 3)
            {
                kickOffStartTxt = halfTimeTextDict[5];
                MayKickOff = true;
            }
            else if (obj.quarter == 4)
            {
                kickOffStartTxt = halfTimeTextDict[6];
                MayKickOff = true;
            }
            if (MayKickOff)
            {
                ShowHalfTimeAni(kickOffStartTxt);
            }
        }

        void ShowHalfTimeAni(string text)
        {
            Go.CancelDelayCall(HideHalfTimeAni);
            Go.DelayCall(() =>
            {
                HalfTimeAniText.text = text;
                HalfTimeAni.SetActive(true);
                Go.DelayCall(HideHalfTimeAni, 2f);
            }, 1f);
        }

        void HideHalfTimeAni()
        {
            HalfTimeAni.SetActive(false);
        }

        // private async void ShowGrassContent()
        // {
        //     if (_gameType == 16 || isShowGrassContent)
        //     {
        //         return;
        //     }
        //     if (grassController == null)
        //     {
        //         GameObject go = ResourceMgr.Instance.Instantiate("3D/GameGrassScore/GameGrassScoreEffect");
        //         grassController = go.GetComponent<GameGrassScoreEffectController>();
        //     }
        //     if (grassController == null)
        //     {
        //         DebugEX.LogError("GameGrassScoreEffectController Empty");
        //         await GAsync.WaitNextFrame();
        //         return;
        //     }

        //     if (quarter == 2 || quarter == 4)
        //         grassController.SetGameScoreData(_teamBConfigurationData.teamLogo.texture, _teamAConfigurationData.teamLogo.texture, _statisticManager.teamBStatistic.score, _statisticManager.teamAStatistic.score);
        //     else
        //         grassController.SetGameScoreData(_teamAConfigurationData.teamLogo.texture, _teamBConfigurationData.teamLogo.texture, _statisticManager.teamAStatistic.score, _statisticManager.teamBStatistic.score);

        //     grassController.SetGameScoreAlpha(GameGrassScoreEffectController.DEFAULT_ALPHA_VALUE);
        //     isShowGrassContent = true;
        //     await GAsync.WaitNextFrame();
        // }

        // void HideGrassContent()
        // {
        //     if (tween!= null)
        //         Go.removeTween(tween);
        //     isShowGrassContent = false;
        //     grassController?.SetGameScoreAlpha(0);
        // }

        // void HideGrassContentGradually(float delay)
        // {
        //     if (isShowGrassContent)
        //     {
        //         isShowGrassContent = false;
        //         float time = 2f;
        //         tween = Go.to(this.transform, time, new GoTweenConfig()
        //            .setDelay(delay)
        //            .onUpdate((obj) =>
        //             {
        //                 float curalpha = (1 - obj.totalElapsedTime / time) * GameGrassScoreEffectController.DEFAULT_ALPHA_VALUE;
        //                 grassController?.SetGameScoreAlpha(curalpha);
        //             }));
        //     }
        // }

        private void LogGuideStep(string step)
        {
            if (GuideConfig.isGuideMatch())
            {
                TGALog.Instance.Log(step);
            }
        }

        private void OnDestroy()
        {
            _teamAConfigurationData = null;
            _teamBConfigurationData = null;
            _statisticManager = null;
            // Platform.EventDispatcher.RemoveEventListener("GameEndStatus", HideGrassContent);
            // Platform.EventDispatcher.RemoveEventListener("HideGrassContent", HideGrassContent);
            Platform.EventDispatcher.RemoveEventListener<ISoccerStatisticManager, TeamConfigurationData, TeamConfigurationData>("ONInitStaticManager", InitStaticManager);
        }

        protected override void OnBaseViewInit()
        {

        }
    }
}<fim_middle>

        private void OnScoreHandler(SoccerGoalStatistic sgs)
        {
            if (sgs.isTeamA)
            {
                _statisticManager.teamAStatistic.score = sgs.score;
                _statisticManager.teamAStatistic