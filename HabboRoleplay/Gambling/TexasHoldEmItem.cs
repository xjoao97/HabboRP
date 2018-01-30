using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Core;

namespace Plus.HabboRoleplay.Gambling
{
    public class TexasHoldEmItem
    {
        public int RoomId;
        public int ItemId;     
        public int X;
        public int Y;
        public double Z;
        public int Rotation;

        public Item Furni;
        public bool Rolled;
        public int Value;

        public TexasHoldEmItem(int RoomId, int ItemId, int X, int Y, double Z, int Rotation)
        {
            this.RoomId = RoomId;
            this.ItemId = ItemId;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Rotation = Rotation;
            this.Furni = null;
            this.Rolled = false;
            this.Value = 0;
        }

        public void SpawnDice()
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);
                this.Furni = RoleplayManager.PlaceItemToRoom(null, this.ItemId, 0, this.X, this.Y, this.Z, this.Rotation, false, this.RoomId, false, "0", false, "", null, null, this);
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in SpawnDice() void: " + e);
            }
        }
    }
}
