using JailPlugin.Models;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Commands
{
    public class JailCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Jail";

        public string Help => "";

        public string Syntax => "/Jail <Arrest/Release/List>";

        public List<string> Permissions => new List<string> { "Jail.Police" };

        public List<string> Aliases => new List<string> { "cadeia", "prisao" };
        public List<string> JailAliases => new List<string> { "prender" };
        public List<string> ReleaseAliases => new List<string> { "libertar", "soltar" };
        public List<string> ListAliases => new List<string> { "listar", "lista" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Caller = (UnturnedPlayer)caller;

            if (command.Length < 1)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NoOption", "/Jail <Arrest/Release/List>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (JailAliases.Contains(command[0].ToLower()))
            {
                if (Main.Instance.Sessions.Any(X => X.Owner == Caller.Player))
                    Main.Instance.Sessions.First(X => X.Owner == Caller.Player).Delete();

                Main.Instance.Sessions.Add(new UiSession(Caller.Player));
            }
            else if (ReleaseAliases.Contains(command[0].ToLower()))
            {
                if (command.Length < 2)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("UseReleaseCorrectly", "/Jail Release <Player Name>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                Player Target = PlayerTool.getPlayer(command[1]);
                if(Target == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("PlayerNotFinded", command[1]), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.Sessions.Any(Y => Y.PlayerCSteamID == Target.channel.owner.playerID.steamID.m_SteamID));
                if(Jail == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("OtherNotJailed", Target.channel.owner.playerID.characterName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                JailSession Session = Jail.Sessions.First(Y => Y.PlayerCSteamID == Target.channel.owner.playerID.steamID.m_SteamID);
                Session.Release(true);
                ChatManager.serverSendMessage(Main.Instance.Translate("PlayerReleased", Target.channel.owner.playerID.characterName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
            }
            else if (ListAliases.Contains(command[0].ToLower()))
            {
                if (Main.Instance.Jails.Count == 0)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("NoJails"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                JailModel PotentialJail = null;
                if (command.Length > 1)
                {
                    PotentialJail = Main.Instance.Jails.FirstOrDefault(X => X.JailName.ToLower() == command[1].ToLower());
                    if (PotentialJail == null && int.TryParse(command[1], out int Index))
                    {
                        Index--;
                        if (Index >= 0 || Index < Main.Instance.Jails.Count())
                            PotentialJail = Main.Instance.Jails[Index];
                    }
                }

                if (PotentialJail == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("NullJailListStart", Main.Instance.Jails.Count), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    foreach (JailModel Jail in Main.Instance.Jails)
                    {
                        ChatManager.serverSendMessage(Main.Instance.Translate("NullJailListed", (Main.Instance.Jails.IndexOf(Jail) + 1).ToString(), Jail.JailName, Jail.Sessions.Count()), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    }
                    ChatManager.serverSendMessage(Main.Instance.Translate("NullJailListFinal"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                if (PotentialJail.Sessions.Count == 0)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("NoPlayers"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                ChatManager.serverSendMessage(Main.Instance.Translate("JailListStart", PotentialJail.Sessions.Count, PotentialJail.MaxPlayers), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                foreach (JailSession Session in PotentialJail.Sessions)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("JailListed", (PotentialJail.Sessions.IndexOf(Session) + 1).ToString(), Session.SavedName, Session.RemainingTime, Session.ConvertCausesToString()), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                }
                ChatManager.serverSendMessage(Main.Instance.Translate("JailListFinal", (Main.Instance.Jails.IndexOf(PotentialJail) +1).ToString()), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
            }
            else
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NoOption", "/Jail <Arrest/Release/List>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
            }
        }
    }
}
