using System;
using System.Collections.Generic;

using Plus.HabboHotel.Items.Wired;
using System.Linq;

namespace Plus.HabboHotel.Items
{
    public class ItemData
    {
        public int Id { get; set; }
        public int SpriteId { get; set; }
        public string ItemName { get; set; }
        public string PublicName { get; set; }
        public char Type { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public double Height { get; set; }
        public bool Stackable { get; set; }
        public bool Walkable { get; set; }
        public bool IsSeat { get; set; }
        public bool AllowEcotronRecycle { get; set; }
        public bool AllowTrade { get; set; }
        public bool AllowMarketplaceSell { get; set; }
        public bool AllowGift { get; set; }
        public bool AllowInventoryStack { get; set; }
        public InteractionType InteractionType { get; set; }
        public int Modes { get; set; }
        public List<int> VendingIds { get; set; }
        public Dictionary<string, double> AdjustableHeights { get; set; }
        public int EffectId { get; set; }
        public WiredBoxType WiredType { get; set; }
        public bool IsRare { get; set; }
        public int ClothingId { get; set; }
        public bool ExtraRot { get; set; }

        public ItemData(int Id, int Sprite, string Name, string PublicName, string Type, int Width, int Length, double Height, bool Stackable, bool Walkable, bool IsSeat,
            bool AllowRecycle, bool AllowTrade, bool AllowMarketplaceSell, bool AllowGift, bool AllowInventoryStack, InteractionType InteractionType, int Modes,
            string VendingIds, List<double> AdjustableHeights, int EffectId, int WiredId, bool IsRare, int ClothingId, bool ExtraRot)
        {
            this.Id = Id;
            this.SpriteId = Sprite;
            this.ItemName = Name;
            this.PublicName = PublicName;
            this.Type = char.Parse(Type);
            this.Width = Width;
            this.Length = Length;
            this.Height = Height;
            this.Stackable = Stackable;
            this.Walkable = Walkable;
            this.IsSeat = IsSeat;
            this.AllowEcotronRecycle = AllowRecycle;
            this.AllowTrade = AllowTrade;
            this.AllowMarketplaceSell = AllowMarketplaceSell;
            this.AllowGift = AllowGift;
            this.AllowInventoryStack = AllowInventoryStack;
            this.InteractionType = InteractionType;
            this.Modes = Modes;
            this.VendingIds = new List<int>();
            if (VendingIds.Contains(","))
            {
                foreach (string VendingId in VendingIds.Split(','))
                {
                    try
                    {
                        this.VendingIds.Add(int.Parse(VendingId));
                    }
                    catch
                    {
                        Console.WriteLine("Erro com item " + ItemName + ".");
                        continue;
                    }
                }
            }
            else if (!String.IsNullOrEmpty(VendingIds) && (int.Parse(VendingIds)) > 0)
                this.VendingIds.Add(int.Parse(VendingIds));

            this.AdjustableHeights = new Dictionary<string, double>();

            int count = 0;
            foreach (double eHeight in AdjustableHeights)
            {
                this.AdjustableHeights.Add(count.ToString(), eHeight);
                count++;
            }

            this.EffectId = EffectId;
            this.WiredType = WiredBoxTypeUtility.FromWiredId(WiredId);
            this.IsRare = IsRare;
            this.ClothingId = ClothingId;
            this.ExtraRot = ExtraRot;
        }

        public bool IsBed()
        {
            if (InteractionType == InteractionType.BED)
                return true;

            if (InteractionType == InteractionType.BEDEFFECT)
                return true;

            if (InteractionType == InteractionType.TENT_SMALL)
                return true;

            return false;
        }
    }
}