using HarmonyLib;
using SDG.Unturned;
using System;
using System.Linq;

namespace JailPlugin.Patches
{
  [HarmonyPatch(typeof (PlayerClothing), "askWearPants", new Type[] {typeof (ItemPantsAsset), typeof (byte), typeof (byte[]), typeof (bool)})]
  public static class PClothesPantsW
  {
    public static bool Prefix(PlayerClothing __instance) => !Main.Instance.Jails.Any(X => X.Sessions.Any(Y => Y.PlayerCSteamID == __instance.player.channel.owner.playerID.steamID.m_SteamID));
  }
}
