using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Patches
{
    [HarmonyPatch(typeof(PlayerLife), "doDamage")]
    public static class PDamage
    {
        public static void Prefix(ref byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, ref EPlayerKill kill, bool trackKill, ERagdollEffect newRagdollEffect, ref bool canCauseBleeding, PlayerLife __instance)
        {
            if (!Main.Instance.Configuration.Instance.TornarPresoImortal || !Main.Instance.Jails.Any(X => X.Sessions.Any(Y => Y.PlayerCSteamID == __instance.player.channel.owner.playerID.steamID.m_SteamID)))
                return;

            amount = 0;
        }
    }
}
