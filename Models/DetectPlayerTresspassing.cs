using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Models
{
    public class DetectPlayerTresspassing : MonoBehaviour
    {
        public JailModel Jail { get; set; }
        public void OnTriggerExit(Collider Other)
        {
            List<Player> Players = new List<Player>();
            if (Other.gameObject.CompareTag("Player"))
            {
                Players.Add(Other.GetComponent<Player>());
            }
            else if (Other.gameObject.CompareTag("Vehicle"))
            {
                InteractableVehicle Vehicle = Other.gameObject.GetComponent<InteractableVehicle>();
                foreach (Passenger Pass in Vehicle.passengers)
                {
                    if (Pass.player != null)
                        Players.Add(Pass.player.player);
                }
            }
            else
                return;

            foreach (Player Player in Players)
            {
                if (!Jail.Sessions.Any(X => X.PlayerCSteamID == Player.channel.owner.playerID.steamID.m_SteamID))
                    return;

                ChatManager.serverSendMessage(Main.Instance.Translate("CantEscape"), Color.white, null, Player.channel.owner, EChatMode.SAY, null, true);
                Player.teleportToLocationUnsafe(Jail.JailPosition.ToVector3(), Player.look.yaw);
            }
        }
    }
}
