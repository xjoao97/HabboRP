using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Cache;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorUserChatlogComposer : ServerPacket
    {
        public ModeratorUserChatlogComposer(int UserId)
            : base(ServerPacketHeader.ModeratorUserChatlogMessageComposer)
        {
            base.WriteInteger(UserId);
           base.WriteString(PlusEnvironment.GetGame().GetClientManager().GetNameById(UserId));
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `room_id`, `entry_timestamp`, `exit_timestamp` FROM `user_roomvisits` WHERE `user_id` = " + UserId + " ORDER BY `entry_timestamp` DESC LIMIT 5");
                DataTable Visits = dbClient.getTable();

                if (Visits != null)
                {
                    base.WriteInteger(Visits.Rows.Count);
                    foreach (DataRow Visit in Visits.Rows)
                    {
                        string RoomName = "Unknown";

                        Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(Visit["room_id"]), false);

                        if (Room != null)
                            RoomName = Room.Name;

                        base.WriteByte(1);
                        base.WriteShort(2);//Count
                       base.WriteString("roomName");
                        base.WriteByte(2);
                       base.WriteString(RoomName); // room name
                       base.WriteString("roomId");
                        base.WriteByte(1);
                        base.WriteInteger(Convert.ToInt32(Visit["room_id"]));

                        DataTable Chatlogs = null;
                        if ((Double)Visit["exit_timestamp"] <= 0)
                        {
                            Visit["exit_timestamp"] = PlusEnvironment.GetUnixTimestamp();
                        }

                        dbClient.SetQuery("SELECT `user_id`, `timestamp`, `message` FROM `chatlogs` WHERE `room_id` = " + Convert.ToInt32(Visit["room_id"]) + " AND `timestamp` > " + (Double)Visit["entry_timestamp"] + " AND `timestamp` < " + (Double)Visit["exit_timestamp"] + " ORDER BY timestamp DESC LIMIT 150");
                        Chatlogs = dbClient.getTable();

                        if (Chatlogs != null)
                        {
                            base.WriteShort(Chatlogs.Rows.Count);
                            foreach (DataRow Log in Chatlogs.Rows)
                            {
                                using (UserCache Habbo = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(Log["user_id"])))
                                {

                                    if (Habbo == null)
                                        continue;

                                    base.WriteString(UnixTimestamp.FromUnixTimestamp(Convert.ToInt32(Log["timestamp"])).ToShortTimeString());
                                    base.WriteInteger(Habbo.Id);
                                    base.WriteString(Habbo.Username);
                                    base.WriteString(string.IsNullOrWhiteSpace(Convert.ToString(Log["message"])) ? "*user sent a blank message*" : Convert.ToString(Log["message"]));
                                    base.WriteBoolean(false);
                                }
                            }
                        }
                        else
                            base.WriteInteger(0);
                    }
                }
                else
                    base.WriteInteger(0);
            }
        }
    }
}