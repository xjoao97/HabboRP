using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Weapons
{
    /// <summary>
    /// Structure for weapons
    /// </summary>
    public class Weapon
    {
        #region Variables
        public uint ID;
        public string Name;
        public string PublicName;
        public string FiringText;
        public string EquipText;
        public string UnEquipText;
        public string ReloadText;
        public int Energy;
        public int EffectID;
        public int HandItem;

        public int Range;
        public int MinDamage;
        public int MaxDamage;
        public int ClipSize;
        public int ReloadTime;
        
        public int Cost;
        public int CostFine;
        public int Stock;
        public int LevelRequirement;

        public bool CanUse;

        #endregion

        /// <summary>
        /// Weapon constructor
        /// </summary>
        public Weapon(uint ID, string Name, string PublicName, string FiringText, string EquipText, string UnEquipText, string ReloadText, int Energy, int EffectID, int HandItem, int Range, int MinDamage, int MaxDamage, int ClipSize, int ReloadTime, int Cost, int CostFine, int Stock, int LevelRequirement, bool CanUse)
        {
            this.ID = ID;
            this.Name = Name;
            this.PublicName = PublicName;
            this.FiringText = FiringText;
            this.EquipText = EquipText;
            this.UnEquipText = UnEquipText;
            this.ReloadText = ReloadText;
            this.Energy = Energy;
            this.EffectID = EffectID;
            this.HandItem = HandItem;

            this.Range = Range;
            this.MinDamage = MinDamage;
            this.MaxDamage = MaxDamage;
            this.ClipSize = ClipSize;
            this.ReloadTime = ReloadTime;

            this.Cost = Cost;
            this.CostFine = CostFine;
            this.Stock = Stock;
            this.LevelRequirement = LevelRequirement;

            this.CanUse = CanUse;
        }

        /// <summary>
        /// Reloads the weapon
        /// </summary>
        public bool Reload(GameClient Client, GameClient TargetClient = null)
        {
            Client.GetRoleplay().GunShots = 0;

            if (Client.GetRoleplay().Bullets > 0 || Client.GetRoleplay().Game != null)
            {
                if (TargetClient != null)
                    RoleplayManager.Shout(Client, "*Tenta atirar em " + TargetClient.GetHabbo().Username + " mas estou sem balas no pente*", 4);

                this.ReloadMessage(Client, this.ClipSize);
                return true;
            }
            else
            {
                if (TargetClient != null)
                    RoleplayManager.Shout(Client, "*Tenta atirar em " + TargetClient.GetHabbo().Username + " mas percebe que acabaram completamente as balas*", 4);
                else
                    Client.SendWhisper("Você acabou com balas para recarregar sua arma!", 1);
                return false;
            }
        }

        /// <summary>
        /// Provides the reload text depending on bool
        /// </summary>
        /// <param name="Client"></param>
        public void ReloadMessage(GameClient Client, int Bullets)
        {
            string Text = this.ReloadText;
            Text = Text.Replace("[NAME]", PublicName);
            Text = Text.Replace("[BULLETS]", Bullets.ToString());

            RoleplayManager.Shout(Client, Text, 4);
        }
    }
}
