using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

using Plus.Utilities;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorRoomChatlogComposer : ServerPacket
    {
        public ModeratorRoomChatlogComposer(Room Room)
            : base(ServerPacketHeader.ModeratorRoomChatlogMessageComposer)
        {
            base.WriteByte(1);
            base.WriteShort(2);//Count
            base.WriteString("roomName");
            base.WriteByte(2);
            base.WriteString(Room.Name);
            base.WriteString("roomId");
            base.WriteByte(1);
            base.WriteInteger(Room.Id);

            DataTable Table = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `chatlogs` WHERE `room_id` = @rid ORDER BY `id` DESC LIMIT 250");
                dbClient.AddParameter("rid", Room.Id);
                Table = dbClient.getTable();
            }

            base.WriteShort(Table.Rows.Count);
            if (Table != null)
            {
                foreach (DataRow Row in Table.Rows)
                {
                    using (UserCache Habbo = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(Row["user_id"])))
                    {

                        if (Habbo == null)
                        {
                            base.WriteString(UnixTimestamp.FromUnixTimestamp(Convert.ToInt32(Row["timestamp"])).ToShortTimeString());
                            base.WriteInteger(-1);
                            base.WriteString("Usuário desconhecido");
                            base.WriteString(string.IsNullOrWhiteSpace(Convert.ToString(Row["message"])) ? "*o usuário enviou uma mensagem em branco*" : Convert.ToString(Row["message"]));
                            base.WriteBoolean(false);
                        }
                        else
                        {
                            base.WriteString(UnixTimestamp.FromUnixTimestamp(Convert.ToInt32(Row["timestamp"])).ToShortTimeString());
                            base.WriteInteger(Habbo.Id);
                            base.WriteString(Habbo.Username);
                            base.WriteString(string.IsNullOrWhiteSpace(Convert.ToString(Row["message"])) ? "*o usuário enviou uma mensagem em branco*" : Convert.ToString(Row["message"]));
                            base.WriteBoolean(false);
                        }
                    }
                }
            }
        }
    }
}
