using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboRoleplay.Houses
{
    public class HouseSign
    {
        public int RoomId;
        public int X;
        public int Y;
        public int Z;
        public bool Spawned;
        public Item Item;

        public HouseSign(int RoomId, int X, int Y, int Z)
        {
            this.RoomId = RoomId;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Spawned = false;
            this.Item = null;
        }
    }
}
