using Rocket.Unturned.Player;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace JailPlugin.Models
{
    public class UiSession
    {
        public Player Owner { get; set; }
        public Dictionary<string, bool> SelectedCrimes { get; set; }
        public List<Player> AvaliablePlayers { get; set; }
        public List<Player> SelectedPlayers { get; set; }
        public int ExtraFine { get; set; }
        public int ActualPage { get; set; }
        public UiSession()
        {

        }
        public UiSession(Player owner)
        {
            Owner = owner;
            SelectedCrimes = new Dictionary<string, bool>
            {
                {"Assedio", false},
                {"Vandalismo", false},
                {"Posse Ilegal", false},
                {"Assalto", false},
                {"Homicidio", false},
                {"Trafico", false},
                {"Briga", false},
                {"Desacato", false},
                {"Lavagem de Dinheiro", false},
                {"Direção Perigosa", false},
            };
            AvaliablePlayers = new List<Player>();
            SelectedPlayers = new List<Player>();
            ExtraFine = 0;
            ActualPage = 0;
            Instantiate();
        }

        public void MakeAvaliablePlayers() =>
            AvaliablePlayers = Provider.clients.Where(X => X.player != Owner && !UnturnedPlayer.FromPlayer(X.player).IsAdmin && Vector3.Distance(X.player.transform.position, Owner.transform.position) <= Main.Instance.Configuration.Instance.RaioMaximoParaAlgemar).ToList().ConvertAll(X => X.player);

        public void Instantiate()
        {
            EffectManager.sendUIEffect(15894, 15894, Owner.channel.owner.transportConnection, true);
            MakeAvaliablePlayers();
            Owner.setPluginWidgetFlag(EPluginWidgetFlags.Modal, true);
            Remake();
            RemakeFine();
        }
        public void Delete()
        {
            EffectManager.askEffectClearByID(15894, Owner.channel.owner.transportConnection);
            Owner.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
            Main.Instance.Sessions.Remove(this);
        }

        public void RemovePlayer(Player Player)
        {
            if (!AvaliablePlayers.Contains(Player))
                return;

            if (SelectedPlayers.Contains(Player))
                SelectedPlayers.Remove(Player);
            AvaliablePlayers.Remove(Player);

            if (AvaliablePlayers.Count == 0)
            {
                Delete();
                ChatManager.serverSendMessage(Main.Instance.Translate("LastPlayerDisconnected"), Color.white, null, Owner.channel.owner, EChatMode.SAY, null, true);
                return;
            }

            ActualPage = 0;
            Remake();
            CheckConclude();
        }
        public void Remake()
        {
            EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, "Until", ActualPage != 0);
            EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, "Next", ActualPage != (GetTotalPages() - 1));
            EffectManager.sendUIEffectText(15894, Owner.channel.owner.transportConnection, true, "Progress", $"{ActualPage + 1}/{GetTotalPages()}");
            for (int i = 0; i < 4; i++)
            {
                int ListIndex = i + (ActualPage * 5);
                int UiTreatmentIndex = i + 1;
                if (AvaliablePlayers.Count < ListIndex)
                {
                    EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, $"Player{UiTreatmentIndex} (2)", true);
                    continue;
                }

                Player Target = AvaliablePlayers[ListIndex];
                EffectManager.sendUIEffectText(15894, Owner.channel.owner.transportConnection, true, $"Player{UiTreatmentIndex}Text", Target.channel.owner.playerID.characterName.Length > 14 ? (Target.channel.owner.playerID.characterName.Substring(0, 11) + "...") : Target.channel.owner.playerID.characterName);
                if (SelectedPlayers.Contains(Target))
                {
                    EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, $"Player{UiTreatmentIndex}", false);
                    continue;
                }

                EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, $"Player{UiTreatmentIndex}", true);
            }
        }
        public void RemakeFine()
        {
            List<CrimeModel> Crimes = GetCrimes();
            int JailTime = Crimes.Sum(X => X.TempoDeCustodia);
            float Jail = Crimes.Sum(X => X.Multa);
            float Soma = Jail + ExtraFine;
            int ScrapAmount = (JailTime / Main.Instance.Configuration.Instance.SegundosPorMetal);

            EffectManager.sendUIEffectText(15894, Owner.channel.owner.transportConnection, true, "TotalFine", $"Valor da Multa (Total): <color=#B7583F><i>{Soma} R$</i></color>");
            EffectManager.sendUIEffectText(15894, Owner.channel.owner.transportConnection, true, "JailFine", $"Valor da Multa (Jail): <color=#B7583F><i>{Jail} R$</i></color>");
            EffectManager.sendUIEffectText(15894, Owner.channel.owner.transportConnection, true, "JailTime", $"Tempo total (Cadeia): <color=#B7583F>{TimeSpan.FromSeconds(JailTime).ToString("hh:mm:ss")}</color>");
            EffectManager.sendUIEffectText(15894, Owner.channel.owner.transportConnection, true, "ScrapAmount", ScrapAmount == 0 ? $"Ou <color=#B7583F>nenhum</color> metal coletado." : (ScrapAmount > 1 ? $"Ou <color=#B7583F>{ScrapAmount}</color> metais coletados." : $"Ou <color=#B7583F>{ScrapAmount}</color> metal coletado."));
            CheckConclude();
        }
        public void CheckConclude()
        {
            EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, $"Conclude (1)",
                SelectedPlayers.Count == 0 || (ExtraFine == 0 && SelectedCrimes.Count == 0));
        }

        public void OnButtonClicked(string ButtonName)
        {
            if (Main.Instance.Configuration.Instance.Crimes.Any(X => X.Nome == ButtonName))
            {
                SelectedCrimes[ButtonName] = !SelectedCrimes[ButtonName];
                EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, ButtonName + " (1)", SelectedCrimes[ButtonName]);
                RemakeFine();
            }
            else if (ButtonName == "Conclude")
            {
                List<CrimeModel> Crimes = GetCrimes();
                int Tempo = Crimes.Sum(X => X.TempoDeCustodia);
                int Multa = Crimes.Sum(X => X.Multa) + ExtraFine;

                foreach (Player Player in SelectedPlayers)
                {
                    if (Multa > 0)
                    {
                        uint ActualBalance = XPconomy.XPconomy.Database.GetBalance(Player.channel.owner.playerID.steamID.m_SteamID);
                        XPconomy.XPconomy.Database.UpdateBalance(Player.channel.owner.playerID.steamID.m_SteamID, (ulong)Math.Max(0, (ActualBalance - Multa)));
                        ChatManager.serverSendMessage(Main.Instance.Translate("Multado", Owner.channel.owner.playerID.characterName, Multa), Color.white, null, Player.channel.owner, EChatMode.SAY, null, true);
                    }
                    if (Tempo > 0)
                    {
                        JailModel Jail = Main.Instance.Jails.OrderByDescending(X => X.Sessions.Count).FirstOrDefault();
                        if (Jail != null)
                            Jail.Sessions.Add(new JailSession(Jail.JailName, Player.channel.owner.playerID.steamID.m_SteamID, (uint)Tempo, SelectedCrimes.Keys.ToList(), Multa));
                    }
                }
                ChatManager.serverSendMessage(Main.Instance.Translate("Arrested", SelectedPlayers.Count), Color.white, null, Owner.channel.owner, EChatMode.SAY, null, true);
            }
            else if (ButtonName == "Until")
            {
                ActualPage -= 1;
                Remake();
            }
            else if (ButtonName == "Next")
            {
                ActualPage += 1;
                Remake();
            }
            else if (ButtonName == "Exit")
                Delete();
            else if (ButtonName.StartsWith("Player"))
            {
                int UiTreatmentIndex = ButtonName[6];
                int RealTreatmentIndex = UiTreatmentIndex - 1;
                int ListTreatmentIndex = UiTreatmentIndex + (ActualPage * 5);
                Player Target = AvaliablePlayers[ListTreatmentIndex];

                if (ButtonName.EndsWith("(1)"))
                {
                    EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, ButtonName.Substring(0, 7), true);
                    SelectedPlayers.Remove(Target);
                }
                else
                {
                    EffectManager.sendUIEffectVisibility(15894, Owner.channel.owner.transportConnection, true, ButtonName, false);
                    SelectedPlayers.Add(Target);
                }
            }
        }
        public void OnInputField(string FieldName, string FieldContent)
        {
            if (FieldName != "FineValue")
                return;

            ExtraFine = int.Parse(FieldContent);
            RemakeFine();
        }

        public List<CrimeModel> GetCrimes()
        {
            List<CrimeModel> Crimes = new List<CrimeModel>();
            foreach (var Doublete in SelectedCrimes)
            {
                CrimeModel Crime = Main.Instance.Configuration.Instance.Crimes.FirstOrDefault(X => X.Nome == Doublete.Key);
                if (Doublete.Value)
                    Crimes.Add(Crime);
            }
            return Crimes;
        }
        public int GetTotalPages() => ((AvaliablePlayers.Count - 1) / 5) + 1;
        public int GetPlayerPage(Player Player)
        {
            if (Player == null || !AvaliablePlayers.Contains(Player))
                return -1;

            int PlayerIndex = AvaliablePlayers.IndexOf(Player);
            int PlayerPage = PlayerIndex / 5;
            return PlayerPage;
        }
        public bool PlayerInActualPage(Player Player) => ActualPage == GetPlayerPage(Player);
    }
}
