using JailPlugin.Commands;
using JailPlugin.Models.Sub_Models;
using Newtonsoft.Json;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Models
{
    public class JailSession
    {
        public string JailName { get; set; }
        public string SavedName { get; set; }
        public bool Arrested { get; set; }
        public ulong PlayerCSteamID { get; set; }
        public uint TotalTime { get; set; }
        public uint RemainingTime { get; set; }
        public List<string> Causes { get; set; }
        public int JailFine { get; set; }
        public List<SItem> KeepedInventory { get; set; }

        [JsonIgnore]
        public Coroutine Timer { get; set; }

        public JailSession()
        {

        }
        public JailSession(string jailName, ulong playerCSteamID, uint remainingTime, List<string> causes, int jailfine)
        {
            Arrested = false;
            JailName = jailName;
            SavedName = "Desconhecido";
            PlayerCSteamID = playerCSteamID;
            RemainingTime = remainingTime;
            TotalTime = remainingTime;
            JailFine = jailfine;
            Causes = causes;
            KeepedInventory = new List<SItem>();
            Arrest(true);
        }

        public string ConvertCausesToString()
        {
            string Cause = "";
            foreach (string Part in Causes)
            {
                Cause += Part + "; ";
            }
            Cause.Remove(Cause.Length - 1, 1);
            return Cause;
        }
        public void Arrest(bool FirstTime)
        {
            Player Robber = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerCSteamID));
            if (Robber == null)
                return;
            SavedName = Robber.channel.owner.playerID.characterName;
            JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.JailName == JailName);

            if (Main.Instance.Configuration.Instance.AlgemarPreso)
            {
                if (Robber.animator.gesture != EPlayerGesture.ARREST_START)
                    Robber.animator.sendGesture(EPlayerGesture.ARREST_START, true);
                Robber.animator.onGestureUpdated += OnGestureUpdated;
            }

            if (FirstTime)
            {
                ChatManager.serverSendMessage(Main.Instance.Translate("BeingArrested", TimeSpan.FromSeconds(RemainingTime).ToString("hh:mm:ss"), ConvertCausesToString()), Color.white, null, Robber.channel.owner, EChatMode.SAY, null, true);
                #region ItemManager
                //Items:
                for (int Page = 6; Page >= 0; Page--)
                {
                    Items PageClass = Robber.inventory.items[Page];
                    if (PageClass == null)
                        continue;

                    while (PageClass.getItemCount() > 0)
                    {   
                        ItemJar SelectedItem = PageClass.items[0];
                        KeepedInventory.Add(new SItem(Page < 2, false, SelectedItem.item.id, SelectedItem.item.state, SelectedItem.item.amount, SelectedItem.item.durability, SelectedItem.x, SelectedItem.y, SelectedItem.rot, (byte)Math.Max(0, Page)));
                        PageClass.removeItem(0);
                    }
                }

                //Roupas:
                if (Robber.clothing.mask != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.mask, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearMask(Main.Instance.Configuration.Instance.Vestimenta.MascaraID, byte.MaxValue, new byte[1] { 0 }, false);

                if (Robber.clothing.backpack != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.backpack, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearBackpack(Main.Instance.Configuration.Instance.Vestimenta.MochilaID, byte.MaxValue, new byte[0], false);

                if (Robber.clothing.hat != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.hat, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearHat(Main.Instance.Configuration.Instance.Vestimenta.CapaceteID, byte.MaxValue, new byte[0], false);

                if (Robber.clothing.glasses != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.glasses, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearGlasses(Main.Instance.Configuration.Instance.Vestimenta.OculosID, byte.MaxValue, new byte[0], false);

                if (Robber.clothing.vest != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.vest, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearVest(Main.Instance.Configuration.Instance.Vestimenta.ColeteID, byte.MaxValue, new byte[0], false);

                if (Robber.clothing.shirt != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.shirt, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearShirt(Main.Instance.Configuration.Instance.Vestimenta.CamisaID, byte.MaxValue, new byte[0], false);

                if (Robber.clothing.pants != 0)
                    KeepedInventory.Add(new SItem(false, true, Robber.clothing.pants, Robber.clothing.maskState, 1, Robber.clothing.maskQuality, 0, 0, 0, 0));
                Robber.clothing.askWearPants(Main.Instance.Configuration.Instance.Vestimenta.CalçaID, byte.MaxValue, new byte[0], false);
                //Removendo Sobras:
                for (int Page = 6; Page >= 0; Page--)
                {
                    Items PageClass = Robber.inventory.items[Page];
                    if (PageClass == null)
                        continue;

                    while (PageClass.getItemCount() > 0)
                    {
                        PageClass.removeItem(0);
                    }
                }
                #endregion

                //Adicionando items dados:
                foreach (ushort ItemID in Main.Instance.Configuration.Instance.ItemsDados)
                    Robber.inventory.forceAddItemAuto(new Item(ItemID, true), true, true, true);
            }

            Robber.teleportToLocationUnsafe(Jail.JailPosition.ToVector3(), Robber.look.yaw);
            if (Timer == null)
                Timer = Main.Instance.StartCoroutine(JailSession.ReleaseCountdown(this));
            Arrested = true;
        }
        public void Release(bool Definitely)
        {
            Rocket.Core.Logging.Logger.Log($"JailPlugin > Releasing Start > IsDefinitly: {Definitely}");
            Player Robber = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerCSteamID));
            if (Robber == null)
                return;
            Rocket.Core.Logging.Logger.Log($"JailPlugin > Identified Criminal > Criminal Name: {Robber.channel.owner.playerID.characterName}");
            JailModel Jail = Main.Instance.Jails.FirstOrDefault(X => X.JailName == JailName);
            Rocket.Core.Logging.Logger.Log($"JailPlugin > Finded Jail > Jail Name: {Jail.JailName}");
            Robber.animator.onGestureUpdated -= OnGestureUpdated;
            if (Robber.animator.gesture == EPlayerGesture.ARREST_START)
                Robber.animator.sendGesture(EPlayerGesture.ARREST_STOP, true);
            Rocket.Core.Logging.Logger.Log($"JailPlugin > Managed Gesture");
            if (Definitely)
            {
                Rocket.Core.Logging.Logger.Log($"JailPlugin > DefCode Running Now");
                ChatManager.serverSendMessage(Main.Instance.Translate("Released", JailName), Color.white, null, Robber.channel.owner, EChatMode.SAY, null, true);
                Rocket.Core.Logging.Logger.Log($"JailPlugin > DefCode Chatted");
                Jail.Sessions.Remove(this);
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Sessions Wiped from List");
                if (Timer != null)
                    Main.Instance.StopCoroutine(Timer);
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Timer Reseted");
                #region Clothing Manager
                //Removendo Items:
                for (int Page = 6; Page >= 0; Page--)
                {
                    Items PageClass = Robber.inventory.items[Page];
                    while (PageClass.getItemCount() > 0)
                    {
                        PageClass.removeItem(0);
                    }
                }
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Clothing Phase 1 Passed");
                //Roupas:
                if (Robber.clothing.mask != 0)
                    Robber.clothing.askWearMask(0, byte.MaxValue, new byte[1] { 0 }, false);
                if (Robber.clothing.backpack != 0)
                    Robber.clothing.askWearBackpack(0, byte.MaxValue, new byte[0], false);
                if (Robber.clothing.hat != 0)
                    Robber.clothing.askWearHat(0, byte.MaxValue, new byte[0], false);
                if (Robber.clothing.glasses != 0)
                    Robber.clothing.askWearGlasses(0, byte.MaxValue, new byte[0], false);
                if (Robber.clothing.vest != 0)
                    Robber.clothing.askWearVest(0, byte.MaxValue, new byte[0], false);
                if (Robber.clothing.shirt != 0)
                    Robber.clothing.askWearShirt(0, byte.MaxValue, new byte[0], false);
                if (Robber.clothing.pants != 0)
                    Robber.clothing.askWearPants(0, byte.MaxValue, new byte[0], false);
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Clothing Phase 2 Passed");
                //Removendo Sobras:
                for (int Page = 6; Page >= 0; Page--)
                {
                    Items PageClass = Robber.inventory.items[Page];
                    while (PageClass.getItemCount() > 0)
                    {
                        PageClass.removeItem(0);
                    }
                }
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Clothing Phase 3 Passed");
                //Devolvendo Roupas:
                KeepedInventory.FindAll(X => X.IsClothing).ForEach(X =>
                {
                    Robber.inventory.forceAddItemAuto(new Item(X.ItemID, X.Amount, X.Durability), false, false, true);
                });
                KeepedInventory.RemoveAll(X => X.IsClothing);
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Clothing Phase 4 Passed");
                //Devolvendo Items:
                for (int Count = KeepedInventory.Count - 1; Count >= 0; Count--)
                {
                    SItem SerializableItem = KeepedInventory[Count];
                    Robber.inventory.items[SerializableItem.Page].addItem(SerializableItem.X, SerializableItem.Y, SerializableItem.Rot, new Item(SerializableItem.ItemID, SerializableItem.Amount, SerializableItem.Durability));
                }
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Clothing Phase 5 Passed");
                KeepedInventory.Clear();
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Clothing Phase 6 Passed");
                #endregion
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Teleporting...");
                Robber.teleportToLocationUnsafe(Jail.ReturnPosition.ToVector3(), Robber.look.yaw);
                if (Main.Instance.Configuration.Instance.ResetarStatusAoLiberar)
                    Robber.life.ReceiveLifeStats(100, 100, 100, 100, 100, false, false);
                Rocket.Core.Logging.Logger.Log($"JailPlugin > Teleported");
            }
            Rocket.Core.Logging.Logger.Log($"JailPlugin > Releasing Ending");
            Arrested = false;
        }

        public void OnGestureUpdated(EPlayerGesture NewGesture)
        {
            if (NewGesture != EPlayerGesture.ARREST_START)
                return;

            Player Robber = PlayerTool.getPlayer(new Steamworks.CSteamID(PlayerCSteamID));
            if (Robber == null)
                return;

            Robber.animator.sendGesture(EPlayerGesture.ARREST_START, true);
        }

        public static IEnumerator ReleaseCountdown(JailSession Session)
        {
            uint DisplayCooldown = Main.Instance.Configuration.Instance.DelayParaExibirUIdePreso;
            while (DisplayCooldown > 0)
            {
                yield return new WaitForSeconds(1f);
                DisplayCooldown--;
            }
            if (Provider.clients.Any(X => X.playerID.steamID.m_SteamID == Session.PlayerCSteamID))
                JailTimeCommand.OpenVisualizeUI(PlayerTool.getPlayer(new Steamworks.CSteamID(Session.PlayerCSteamID)), Session);

            while (Session.RemainingTime > 0)
            {
                Session.RemainingTime--;
                yield return new WaitForSeconds(1f);
            }

            Session.Release(true);
        }
    }
}
