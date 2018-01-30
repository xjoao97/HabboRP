using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Outgoing.Rooms.Settings
{
    class RoomRightsListComposer : ServerPacket
    {
        public RoomRightsListComposer(Room Instance)
            : base(ServerPacketHeader.RoomRightsListMessageComposer)
        {
            base.WriteInteger(Instance.Id);

            base.WriteInteger(Instance.UsersWithRights.Count);
            foreach (int Id in Instance.UsersWithRights.ToList())
            {
                using (UserCache Data = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Id))
                {
                    if (Data == null)
                    {
                        base.WriteInteger(0);
                        base.WriteString("Erro desconhecido");
                    }
                    else
                    {
                        base.WriteInteger(Data.Id);
                        base.WriteString(Data.Username);
                    }
                }
            }
        }
    }
}
