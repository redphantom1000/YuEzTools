using AmongUs.GameOptions;
using Hazel;
using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using InnerNet;
using UnityEngine;
using YuEzTools;
using System.Net.Http;  
using System.Threading.Tasks;
using YuEzTools.Get;

namespace YuEzTools;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        Main.Logger.LogMessage("From " +__instance.GetRealName() + "'s RPC:" + callId);
        try
        {
            if (AntiCheatForAll.ReceiveRpc(__instance, callId, reader) || AUMCheat.ReceiveInvalidRpc(__instance, callId) ||
                SMCheat.ReceiveInvalidRpc(__instance, callId))
            {
                Main.Logger.LogInfo("Hacker " + __instance.GetRealName() + $"{"好友编号："+__instance.GetClient().FriendCode+"/名字："+__instance.GetRealName()+"/实验性ProductUserId获取："+__instance.GetClient().ProductUserId}");
                //Main.PlayerStates[__instance.GetClient().Id].IsHacker = true;
                SendChat.Prefix(__instance);
                if(!Toggles.SafeMode && !AmongUsClient.Instance.AmHost)
                {
                    Main.Logger.LogInfo("Try Murder" + __instance.GetRealName());
                    //__instance.RpcSendChat($"{Main.ModName}检测到我是外挂 并且正在尝试踢出我 [来自{AmongUsClient.Instance.PlayerPrefab.GetRealName()}的{Main.ModName}]");
                    //Try_to_ban(__instance);
                    MurderHacker.murderHacker(__instance,MurderResultFlags.Succeeded);
                    return false;
                }
                //PlayerControl Host = AmongUsClient.Instance.GetHost();
                else if (AmongUsClient.Instance.AmHost)
                {
                    Main.Logger.LogInfo("Host Try ban " + __instance.GetRealName());
                    //__instance.RpcSendChat($"{Main.ModName}检测到我是外挂 并且正在尝试踢出我 [来自房主{AmongUsClient.Instance.PlayerPrefab.GetRealName()}的{Main.ModName}]");
                    if (!Toggles.SafeMode)
                    {
                        Main.Logger.LogInfo("Host Try murder " + __instance.GetRealName());
                        MurderHacker.murderHacker(__instance,MurderResultFlags.Succeeded);
                    }
                    AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), true);
                        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                    return false;
                }
                return false;
            }
        }
        catch
        {
            SendInGamePatch.SendInGame(Translator.GetString("ERROR.CHECKRPC"));
        }
        return true;
    }

    public static void Try_to_ban(PlayerControl pc)
    {
        for(int i = 0;i <= 200;i++)
            MurderHacker.murderHacker(pc,MurderResultFlags.Succeeded);
    }
}
