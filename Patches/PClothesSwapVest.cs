using HarmonyLib;
using SDG.Unturned;
using System;
using System.Linq;

namespace JailPlugin.Patches
{
  [HarmonyPatch(typeof (PlayerClothing), "ReceiveSwapVestRequest")]
  public static class PClothesSwapVest
  {
    public static bool Prefix(PlayerClothing __instance) => !Main.Instance.Jails.Any(X => X.Sessions.Any(Y => Y.PlayerCSteamID == __instance.player.channel.owner.playerID.steamID.m_SteamID));
  }
}
