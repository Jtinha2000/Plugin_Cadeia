using HarmonyLib;
using JailPlugin.Models;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace JailPlugin
{
    public class Main : RocketPlugin<Configuration>
    {
        /*
         * 1. Testar comprimento dos nomes.
         * 2. Testar se está alterando os nomes corretamente.
         * 3. Testar se o calculo de quantidade de scraps está correto (Float result)
         * 4. 
         */
        public List<ReportSession> Reports { get; set; }
        public List<UiSession> Sessions { get; set; }
        public List<JailModel> Jails { get; set; }
        public static Main Instance { get; set; }
        public Harmony HarmonyManager { get; set; }
        protected override void Load()
        {
            Instance = this;
            Sessions = new List<UiSession>();
            Reports = new List<ReportSession>();

            HarmonyManager = new Harmony("Agiota.JailPlugin");
            HarmonyManager.PatchAll();

            if (!File.Exists(this.Directory + @"\Jails.json"))
                File.Create(this.Directory + @"\Jails.json");
            else
                Jails = JsonConvert.DeserializeObject<List<JailModel>>(File.ReadAllText(this.Directory + @"\Jails.json"));
            if (Jails is null)
                Jails = new List<JailModel>();

            if (!Level.isLoaded)
                Level.onPostLevelLoaded += new PostLevelLoaded(OnLevelLoaded);
            Jails.ForEach(X =>
            {
                if (Level.isLoaded)
                    X.SetupColissors();
                X.Sessions.ForEach(Y =>
                {
                    if (Main.Instance.Configuration.Instance.ContarTempoOffline)
                        Y.Timer = Main.Instance.StartCoroutine(JailSession.ReleaseCountdown(Y));
                    if (Level.isLoaded)
                        Y.Arrest(false);
                });
            });

            UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            EffectManager.onEffectButtonClicked += EffectManager_OnButtonClicked;
            EffectManager.onEffectTextCommitted += EffectManager_onEffectText;
            UnturnedPlayerEvents.OnPlayerDead += UnturnedPlayerEvents_OnPlayerDead;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
        }
        protected override void Unload()
        {
            while (Sessions.Count > 0)
                Sessions[0].Delete();

            UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            EffectManager.onEffectButtonClicked -= EffectManager_OnButtonClicked;
            EffectManager.onEffectTextCommitted -= EffectManager_onEffectText;
            UnturnedPlayerEvents.OnPlayerDead -= UnturnedPlayerEvents_OnPlayerDead;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            Level.onPostLevelLoaded -= new PostLevelLoaded(OnLevelLoaded);

            using (StreamWriter Writer = new StreamWriter(this.Directory + @"\Jails.json", false))
            {
                Writer.Write(JsonConvert.SerializeObject(Jails));
            }
            Jails.ForEach(X =>
            {
                X.RemoveColissors();
                X.Sessions.ForEach(Z =>
                {
                    Z.Release(false);
                    if (Z.Timer != null)
                        StopCoroutine(Z.Timer);
                });
            });

            HarmonyManager.UnpatchAll();

            HarmonyManager = null;
            Instance = null;
        }

        public void OnLevelLoaded(int __)
        {
            Jails.ForEach(X => X.SetupColissors());
        }
        public void EffectManager_onEffectText(Player Player, string ButtonName, string Content)
        {
            UiSession Session = Sessions.FirstOrDefault(X => X.Owner == Player);
            if (Session == null)
                return;

            Session.OnInputField(ButtonName, Content);
        }
        public void EffectManager_OnButtonClicked(Player Player, string ButtonName)
        {
            UiSession Session = Sessions.FirstOrDefault(X => X.Owner == Player);
            if (Session == null)
            {
                if(ButtonName == "Exit")
                {
                    Player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, false);
                    EffectManager.askEffectClearByID(15894, Player.channel.owner.transportConnection);
                }
                return;
            }

            Session.OnButtonClicked(ButtonName);
        }
        private void UnturnedPlayerEvents_OnPlayerDead(UnturnedPlayer player, Vector3 position)
        {
            UiSession SessionUi = Sessions.FirstOrDefault(X => X.Owner == player.Player);
            if (SessionUi != null)
                SessionUi.Delete();
        }
        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (Main.Instance.Configuration.Instance.CausasParaIgnorar.Contains(cause) || (Main.Instance.Configuration.Instance.Cancelar190QuandoSemPoliciais && 0 == Provider.clients.Count(X => UnturnedPlayer.FromPlayer(X.player).GetPermissions().Any(Z => Z.Name == Main.Instance.Configuration.Instance.PermissaoDePolicial))))
                return;

            Reports.Add(new ReportSession(player.Player, new Models.Sub_Models.SVector3(player.Player.transform.position), "", Models.Enums.EReportOrigin.DeathOrigin));
        }
        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            UiSession SessionUi = Sessions.FirstOrDefault(X => X.Owner == player.Player);
            if (SessionUi != null)
                SessionUi.Delete();
            Sessions.FindAll(Y => Y.AvaliablePlayers.Contains(player.Player)).ForEach(X => X.RemovePlayer(player.Player));

            JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.Sessions.Any(Y => Y.PlayerCSteamID == player.CSteamID.m_SteamID));
            if (Jail == null)
                return;
            JailSession Session = Jail.Sessions.FirstOrDefault(Y => Y.PlayerCSteamID == player.CSteamID.m_SteamID);

            if (!Main.Instance.Configuration.Instance.ContarTempoOffline)
            {
                Main.Instance.StopCoroutine(Session.Timer);
                Session.Timer = null;
            }
            Session.Release(false);
        }
        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.Sessions.Any(Y => Y.PlayerCSteamID == player.CSteamID.m_SteamID));
            if (Jail == null)
                return;
            JailSession Session = Jail.Sessions.FirstOrDefault(Y => Y.PlayerCSteamID == player.CSteamID.m_SteamID);

            Session.Arrest(false);
        }

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"NoOfficerOnline", "<color=blue>[190]</color> Não há nenhum policial online para receber sua denuncia!" },
            {"CommandCooldown", "<color=blue>[190]</color> Aguarde antes de fazer outro boletim de ocorrência..." },
            {"190Sended", "<color=blue>[190]</color> Sua denúncia foi registrada!" },
            {"190Recieved", "<color=blue>[190]</color> Uma nova dénuncia foi recebida! Para atende-la use /Report" },
            {"LastPlayerDisconnected", "<color=red>[JailSystem]</color> O último jogador disponível para multar ou prender foi desconectado..." },
            {"JailSettedJail", "<color=red>[JailSystem]</color> A cadeia '{0}' teve seu <color=red>ponto de cárcere</color> definido com sucesso!" },
            {"JailSettedReturn", "<color=red>[JailSystem]</color> A cadeia '{0}' teve seu <color=red>ponto de retorno</color> definido com sucesso!" },
            {"JailRemoved", "<color=red>[JailSystem]</color> A cadeia '{0}' foi removida com sucesso!" },
            { "NoOption", "<color=red>[JailSystem]</color> Ao utilizar o comando escolha uma ação: <color=green>{0}</color>." },
            { "UseRemCorrectly", "<color=red>[JailSystem]</color> Ao utilizar o comando de <color=red>remover</color> insira os parâmetros: <color=green>{0}</color>." },
            { "UseSetCorrectly", "<color=red>[JailSystem]</color> Ao utilizar o comando de <color=red>definir ponto</color> insira os parâmetros: <color=green>{0}</color>." },
            { "UseAddCorrectly", "<color=red>[JailSystem]</color> Ao utilizar o comando de <color=red>adicionar</color> insira os parâmetros: <color=green>{0}</color>." },
            { "AlreadyHaveJailNamed", "<color=red>[JailSystem]</color> Ja existe outra cadeia com o nome <color=red>{0}</color>." },
            { "InsertMaxPlayers", "<color=red>[JailSystem]</color> Insira um número máximo de players válido (Inteiro e Positivo): <color=green>{0}</color>" },
            { "InsertMaxDistance", "<color=red>[JailSystem]</color> Insira uma distância máxima válida (Inteira e Positiva): <color=green>{0}</color>" },
            { "JailCreated", "<color=red>[JailSystem]</color> A cadeia '{0}' foi criada com sucesso!"},
            { "JailNotFound" , "<color=red>[JailSystem]</color> Nenhuma cadeia de nome '{0}' foi encontrada..."},
            { "Arrested", "<color=red>[JailSystem]</color> Você aplicou as devidas punições para os {0} jogadore(s)." },
            { "Multado", "<color=red>[JailSystem]</color> Você foi multado por {0}! -<color=red>{1}</color> R$" },
            { "BeingArrested", "<color=red>[JailSystem]</color> Você foi preso por <color=red>{0}</color>, por infracionar: <color=red>{1}</color>. Caso queira vêr seu tempo restante em cárcere utilize o comando: <color=red>/JailTime</color>" },
            { "Released", "<color=red>[JailSystem]</color> Finalmente você foi liberto do carcere de <color=red>{0}</color>!" },
            { "OtherJailTime", "<color=red>[JailSystem]</color> {0} está preso por, ainda, <color=red>{1}</color> segundos de custodia..." },
            { "JailTime", "<color=red>[JailSystem]</color> Você está preso na cadeia <color=red>{0}</color>. Ainda lhe restam <color=red>{1}</color> de custodia..." },
            { "NotJailed", "<color=red>[JailSystem]</color> Você está em liberdade!"},
            { "OtherNotJailed", "<color=red>[JailSystem]</color> {0} está em liberdade!"},
            { "PlayerNotFinded", "<color=red>[JailSystem]</color> Não foi encontrado nenhum jogador com o nome '{0}' online..."},
            {"CantEscape", "<color=red>[JailSystem]</color> Você não pode escapar! Espere o fim de sua pena... [Caso queira verifica-la, utilize /JailTime]"},
            {"JailListStart", "<color=red>[JailSystem]</color> Listando agora todas as cadeias do servidor ({0}/{1})..."},
            {"JailListed", "<color=red>[{0}]</color> - {1} - {2}s - {3}"},
            {"JailListFinal", "Para soltar algum desses presos relembre da posição da cadeia (<color=red>{0}</color>) e do infrator!"},
            {"NullJailListStart", "<color=red>[JailSystem]</color> Listando agora todas as cadeias do servidor ({0})..."},
            {"NullJailListed", "<color=red>[{0}]</color> - {1} - {2}"},
            {"NullJailListFinal", "Para consultar especificamente alguma dessas cadeias utilize: /Jail Listar <Número ou Nome>"},
            {"PlayerReleased", "<color=red>[JailSystem]</color> Você soltou com sucesso <color=red>{0}</color>." },
            {"NoJails", "<color=red>[JailSystem]</color> Não há nenhuma cadeia no servidor... ):" },
            {"NoPlayers", "<color=red>[JailSystem]</color> Não há nenhum jogador nessa cadeia..." },
            { "UseReleaseCorrectly", "<color=red>[JailSystem]</color> Ao utilizar o comando de <color=red>liberar detento</color> insira os parâmetros: <color=green>{0}</color>." },
        };
    }
}
