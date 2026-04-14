using JailPlugin.Commands;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Models.Sub_Models
{
    public class CooldownInfo
    {
        public Player Owner { get; set; }
        public Coroutine Identifier { get; set; }
        public int RemainingTime { get; set; }
        public CooldownInfo()
        {
            
        }
        public CooldownInfo(Player owner, int remainingTime)
        {
            Owner = owner;
            RemainingTime = remainingTime;
            Identifier = Main.Instance.StartCoroutine(Counter());
        }
        public void ForceRemove()
        {
            if (Identifier != null)
                Main.Instance.StopCoroutine(Identifier);
            _190Command.Cooldowns.Remove(this);
        }

        public IEnumerator Counter()
        {
            while(RemainingTime > 0)
            {
                yield return new WaitForSeconds(1);
                RemainingTime -= 1;
            }

            ForceRemove();
        }
    }
}
