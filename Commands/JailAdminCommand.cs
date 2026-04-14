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
    public class JailAdminCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "JailAdmin";

        public string Help => "";

        public string Syntax => "/JailAdmin <Add/Rem/Set> <Parameters>";

        public List<string> Aliases => new List<string> { "AdminJail", "AdminJ", "JAdmin", "JailAdm", "AdmJail", "JailA", "AJail"};

        public List<string> Permissions => new List<string> { "Jail.Admin" };

        public List<string> AddAliases => new List<string> { "add", "new", "create" };
        public List<string> RemAliases => new List<string> { "rem", "delete", "del", "remove" };
        public List<string> SetAliases => new List<string> { "set", "define" };
        public List<string> SetJailAliases => new List<string> { "jail", "point" };
        public List<string> SetReturnAliases => new List<string> { "return", "returnpoint", "pointreturn" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Caller = (UnturnedPlayer)caller;

            if (command.Length < 1)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NoOption", "/JailAdmin <Add/Rem/Set>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if (AddAliases.Contains(command[0].ToLower()))
            {
                if (command.Length < 4)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("UseAddCorrectly", "/JailAdmin Add <Jail Name> <Max Range> <Max Players>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                string NewJailName = command[1];
                if (Main.Instance.Jails.Any(X => X.JailName.ToLower() == NewJailName.ToLower()))
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("AlreadyHaveJailNamed", NewJailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                if (!float.TryParse(command[2], out float WalkDistance) || WalkDistance < 1)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("InsertMaxDistance", "/JailAdmin Add <Jail Name> <color=red><Max Range></color> <Max Players>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }
                if (!int.TryParse(command[3], out int MaxPlayers) || MaxPlayers < 1)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("InsertMaxPlayers", "/JailAdmin Add <Jail Name> <Max Range> <color=red><Max Players></color>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                JailModel NewJail = new JailModel(NewJailName, new Models.Sub_Models.SVector3(), WalkDistance, MaxPlayers, new List<JailSession>(), new Models.Sub_Models.SVector3());
                Main.Instance.Jails.Add(NewJail);
                NewJail.SetupColissors();
                ChatManager.serverSendMessage(Main.Instance.Translate("JailCreated", NewJailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
            }
            else if (RemAliases.Contains(command[0].ToLower()))
            {
                if (command.Length < 2)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("UseRemCorrectly", "/JailAdmin Rem <Jail Name>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                string JailName = command[1].ToLower();
                JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.JailName.ToLower() == JailName);
                if (Jail == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("JailNotFound", JailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                while(Jail.Sessions.Count() > 0)
                {
                    Jail.Sessions[0].Release(true);
                }
                Jail.RemoveColissors();
                Main.Instance.Jails.Remove(Jail);
                ChatManager.serverSendMessage(Main.Instance.Translate("JailRemoved", Jail.JailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
            }
            else if (SetAliases.Contains(command[0].ToLower()))
            {
                if (command.Length < 3)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("UseSetCorrectly", "/JailAdmin Set <Jail Name> <Return/Jail>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                string JailName = command[1].ToLower();
                JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.JailName.ToLower() == JailName);
                if (Jail == null)
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("JailNotFound", JailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    return;
                }

                if (SetJailAliases.Contains(command[2].ToLower()))
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("JailSettedJail", Jail.JailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    Jail.JailPosition = new Models.Sub_Models.SVector3(Caller.Player.transform.position);
                    Jail.RestartColissors();
                }
                else if (SetReturnAliases.Contains(command[2].ToLower()))
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("JailSettedReturn", Jail.JailName), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                    Jail.ReturnPosition = new Models.Sub_Models.SVector3(Caller.Player.transform.position);
                }
                else
                {
                    ChatManager.serverSendMessage(Main.Instance.Translate("UseSetCorrectly", "/JailAdmin Set <Jail Name> <Return/Jail>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                }
            }
            else
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NoOption", "/JailAdmin <Add/Rem/Set>"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
            }
        }
    }
}
