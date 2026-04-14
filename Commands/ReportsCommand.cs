using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JailPlugin.Commands
{
    public class ReportsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Report";

        public string Help => "";

        public string Syntax => "/Report <List/Enter/Exit>";

        public List<string> Aliases => new List<string> { "Ocorrencia", "Qth",  "Denuncia" };

        public List<string> Permissions => new List<string> { "Jail.Police" };
        public List<string> EnterAliases => new List<string> { "" };
        public List<string> ExitAliases => new List<string> { "" };
        public List<string> ListAliases => new List<string> { "" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Caller = (UnturnedPlayer)caller;
            if(command.Length == 0)
            {
                return;
            }

                
        }
    }
}
