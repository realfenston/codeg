<fim_prefix>﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositonEnToCn 
{
	/// <summary>
	/// 球员位置转换
	/// </summary>
	/// <param name="en"></param>
	/// <returns></returns>
	public static string PlayerPositionEN2CN(string en)
	{
		string positionEN = String.Empty;
		string positionCN = String.Empty;
		switch (en)
		{
			case "LCF":
				positionEN = "CF";
				positionCN = "中锋";
				break;
			case "MCF":
				positionEN = "CF";
				positionCN = "中锋";
				break;
			case "RCF":
				positionEN = "CF";
				positionCN = "中锋";
				break;
			case "CF": // New
				positionEN = "CF";
				positionCN = "中锋";
				break;
			case "LW":
				positionEN = "LW";
				positionCN = "左边锋";
				break;
			case "LAMF":
				positionEN = "AMF";
				positionCN = "前腰";
				break;
			case "MAMF":
				positionEN = "AMF";
				positionCN = "前腰";
				break;
			case "RAMF":
				positionEN = "AMF";
				positionCN = "前腰";
				break;
			case "AMF": // New
				positionEN = "AMF";
				positionCN = "前腰";
				break;
			case "RW":
				positionEN = "RW";
				positionCN<fim_suffix>positionEN = "LMF";
				positionCN = "左前卫";
				break;
			case "LCMF":
				positionEN = "CMF";
				positionCN = "中前卫";
				break;
			case "MCMF":
				positionEN = "CMF";
				positionCN = "中前卫";
				break;
			case "RCMF":
				positionEN = "CMF";
				positionCN = "中前卫";
				break;
			case "CMF": // New
				positionEN = "CMF";
				positionCN = "中前卫";
				break;
			case "RMF":
				positionEN = "RMF";
				positionCN = "右前卫";
				break;
			case "LDMF":
				positionEN = "DMF";
				positionCN = "后腰";
				break;
			case "MDMF":
				positionEN = "DMF";
				positionCN = "后腰";
				break;
			case "RDMF":
				positionEN = "DMF";
				positionCN = "后腰";
				break;
			case "DMF": // New
				positionEN = "DMF";
				positionCN = "后腰";
				break;
			case "LB":
				positionEN = "LB";
				positionCN = "左后卫";
				break;
			case "LCB":
				positionEN = "CB";
				positionCN = "中后卫";
				break;
			case "MCB":
				positionEN = "CB";
				positionCN = "中后卫";
				break;
			case "RCB":
				positionEN = "CB";
				positionCN = "中后卫";
				break;
			case "CB": // New
 				positionEN = "CB";
				positionCN = "中后卫";
				break;
			case "RB":
				positionEN = "RB";
				positionCN = "右后卫";
				break;
			case "GK":
				positionEN = "GK";
				positionCN = "门将";
				break;

			default:
				break;
		}
		return positionCN;
	}
	
	public enum SoccerPositionRoleTag
	{
		BENCH = 0,
		LCF = 2,
		MCF = 3,
		RCF = 4,
		LW = 6,
		LAMF = 7,
		MAMF = 8,
		RAMF = 9,
		RW = 10,
		LMF = 11,
		LCMF = 12,
		MCMF = 13,
		RCMF = 14,
		RMF = 15,
		LDMF = 17,
		MDMF = 18,
		RDMF = 19,
		LB = 21,
		LCB = 22,
		MCB = 23,
		RCB = 24,
		RB = 25,
		GK = 26,
		SENTOFF = 27,
		LWB = 28,
		RWM = 29,
		LWM = 30,
		RWB = 31,
	}
}
<fim_middle> = "右边锋";
				break;
			case "LMF":
				<|endoftext|>