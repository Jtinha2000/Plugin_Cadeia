using JailPlugin.Models.Enums;
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

namespace JailPlugin.Models
{
    public class ReportSession
    {
        public Player Owner { get; set; }
        public List<Player> Officers { get; set; }
        public List<ulong> BannedOfficers { get; set; }
        public DateTime InitTime { get; set; }
        public SVector3 Location { get; set; }
        public EReportOrigin EOrigin { get; set; }
        public string Explanation { get; set; }
        public ReportSession()
        {

        }
        public ReportSession(Player owner, SVector3 location, string explanation, EReportOrigin Origin)
        {
            InitTime = DateTime.Now;
            Owner = owner;
            Officers = new List<Player>();
            BannedOfficers = new List<ulong>();
            Location = location;
            Explanation = explanation;
            EOrigin = Origin;

            List<SteamPlayer> OnlineOfficers = Provider.clients.FindAll(X => UnturnedPlayer.FromPlayer(X.player).GetPermissions().Any(XZ => XZ.Name == Main.Instance.Configuration.Instance.PermissaoDePolicial));
            foreach(SteamPlayer Officer in OnlineOfficers)
                ChatManager.serverSendMessage(Main.Instance.Translate("190Recieved"), Color.white, null, Officer, EChatMode.SAY, null, true);
        }

        public void AddOfficer(Player Officer)
        {
            if (Officers.Contains(Officer))
                return;

            Officers.Add(Officer);
        }
        public void RemoveOfficer(Player Officer) 
        {
            if (!Officers.Contains(Officer))
                return;

            Officers.Remove(Officer);
        }
    }
}
