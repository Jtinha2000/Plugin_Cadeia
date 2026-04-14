using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JailPlugin.Models.Sub_Models
{
    public class SItem
    {
        public bool IsEquipped { get; set; }
        public bool IsClothing { get; set; }

        public ushort ItemID { get; set; }
        public byte[] State { get; set; }
        public byte Amount { get; set; }
        public byte Durability { get; set; }

        public byte Page { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Rot { get; set; }
        public SItem()
        {
            
        }
        public SItem(bool isEquipped, bool isClothing, ushort itemID, byte[] state, byte amount, byte durability, byte x, byte y, byte rot, byte page)
        {
            IsEquipped = isEquipped;
            IsClothing = isClothing;
            ItemID = itemID;
            State = state;
            Amount = amount;
            Durability = durability;
            X = x;
            Y = y;
            Rot = rot;
            Page = page;
        }
    }
}
