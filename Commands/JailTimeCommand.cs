using JailPlugin.Models;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace JailPlugin.Commands
{
    public class JailTimeCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "JailTime";

        public string Help => "";

        public string Syntax => "/JailTime";

        public List<string> Aliases => new List<string> { "Tempo", "TempoCadeia", "CadeiaTempo", "TempoPrisao", "PrisaoTempo", "TimeJail", "TempoPreso", "PresoTempo", "TempoRestante", "RestanteTempo" };

        public List<string> Permissions => new List<string> { "Jail.Normal" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Caller = (UnturnedPlayer)caller;
            JailModel Jail = null;
            JailSession Session = null;
        Recovery:
            if (command.Length > 0)
            {
                Player Target = PlayerTool.getPlayer(command[0]);
                if (Target == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("PlayerNotFinded", command[0]), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                if (Target.channel.owner.playerID.steamID.m_SteamID == Caller.CSteamID.m_SteamID)
                {
                    command = new string[0];
                    goto Recovery;
                }
                Jail = Main.Instance.Jails.FirstOrDefault(X => X.Sessions.Any(Y => Y.PlayerCSteamID == Target.channel.owner.playerID.steamID.m_SteamID));
                if (Jail == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("OtherNotJailed", Target.channel.owner.playerID.characterName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                Session = Jail.Sessions.FirstOrDefault(Y => Y.PlayerCSteamID == Target.channel.owner.playerID.steamID.m_SteamID);
                ChatManager.serverSendMessage(Main.Instance.Translate("OtherJailTime", Target.channel.owner.playerID.characterName, TimeSpan.FromSeconds(Session.RemainingTime).ToString("hh:mm:ss")), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                OpenVisualizeUI(Target, Session, Caller.Player);
            }
            else
            {
                Jail = Main.Instance.Jails.FirstOrDefault(X => X.Sessions.Any(Y => Y.PlayerCSteamID == Caller.CSteamID.m_SteamID));
                if (Jail == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("NotJailed"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                Session = Jail.Sessions.FirstOrDefault(Y => Y.PlayerCSteamID == Caller.CSteamID.m_SteamID);
                ChatManager.serverSendMessage(Main.Instance.Translate("JailTime", Jail.JailName, TimeSpan.FromSeconds(Session.RemainingTime).ToString("hh:mm:ss")), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                OpenVisualizeUI(Caller.Player, Session);
            }
        }

        public static void OpenVisualizeUI(Player Player, JailSession Session, Player ToPlayer = null)
        {
            if (Player == null)
                return;
            if (ToPlayer == null)
                ToPlayer = Player;
            if (Session == null)
                return;

            ToPlayer.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
            EffectManager.sendUIEffect(15894, 15894, ToPlayer.channel.owner.transportConnection, true);
            EffectManager.sendUIEffectText(15894, ToPlayer.channel.owner.transportConnection, true, "RemainingTime", $"Tempo restante: <color=#B7583F><i>{TimeSpan.FromSeconds(Session.RemainingTime).ToString("hh:mm:ss")}</i></color>");
            EffectManager.sendUIEffectText(15894, ToPlayer.channel.owner.transportConnection, true, "TotalTime", $"Tempo total: <color=#B7583F><i>{TimeSpan.FromSeconds(Session.TotalTime).ToString("hh:mm:ss")}</i></color>");
            EffectManager.sendUIEffectText(15894, ToPlayer.channel.owner.transportConnection, true, "JailFine", $"Valor da multa: <color=#B7583F><i>{Session.JailFine} R$</i></color>");
            EffectManager.sendUIEffectText(15894, ToPlayer.channel.owner.transportConnection, true, "info", $"! <color=#B7583F>1</color> Metal = -<color=#B7583F>{Main.Instance.Configuration.Instance.SegundosPorMetal}</color> segundos !");
            EffectManager.sendUIEffectVisibility(15894, ToPlayer.channel.owner.transportConnection, true, "info", Main.Instance.Configuration.Instance.SegundosPorMetal != 0);
            Session.Causes.ForEach(X => EffectManager.sendUIEffectVisibility(15894, ToPlayer.channel.owner.transportConnection, true, X, false));
        }
    }
}
