using JailPlugin.Models.Sub_Models;
using Rocket.API;
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
    internal class _190Command : IRocketCommand
    {
        public static List<CooldownInfo> Cooldowns { get; set; } = new List<CooldownInfo>();

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "190";

        public string Help => "";

        public string Syntax => "/190 <Explanation>";

        public List<string> Aliases => new List<string> { "Denunciar" };

        public List<string> Permissions => new List<string> { "Jail.Normal" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Caller = (UnturnedPlayer)caller;
            if(Main.Instance.Configuration.Instance.Cancelar190QuandoSemPoliciais && 0 == Provider.clients.Count(X => UnturnedPlayer.FromPlayer(X.player).GetPermissions().Any(Z=> Z.Name == Main.Instance.Configuration.Instance.PermissaoDePolicial)))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("NoOfficerOnline"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }
            if(Cooldowns.Any(X => X.Owner == Caller.Player))
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("CommandCooldown"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            string Explanation = "";
            if (command.Length > 0)
            {
                for (int Index = 0; Index < command.Length; Index++)
                    Explanation += command[Index] + " ";
                Explanation = Explanation.Remove(Explanation.Length - 1) + ".";
            }
            else
                Explanation = "Undefined";

            Main.Instance.Reports.Add(new Models.ReportSession(Caller.Player, new SVector3(Caller.Player.transform.position), Explanation, Models.Enums.EReportOrigin.CommandOrigin));
            Cooldowns.Add(new CooldownInfo(Caller.Player, Main.Instance.Configuration.Instance.CooldownComando190));
            ChatManager.serverSendMessage(Main.Instance.Translate("190Sended"), Color.white, null, Caller.Player.channel.owner, EChatMode.SAY, null, true);
        }
    }
}
