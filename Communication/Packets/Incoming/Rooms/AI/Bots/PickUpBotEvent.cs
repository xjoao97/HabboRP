using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.Communication.Packets.Outgoing.Inventory.Bots;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Bots.Manager;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Bots
{
    class PickUpBotEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            int BotId = Packet.PopInt();
            if (BotId == 0)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser BotUser = null;
            if (!Room.GetRoomUserManager().TryGetBot(BotId, out BotUser))
                return;

            if (Session.GetHabbo().Id != BotUser.BotData.ownerID && !Session.GetHabbo().GetPermissions().HasRight("bot_place_any_override"))
            {
                Session.SendWhisper("Você só pode pegar seus próprios bots!");
                return;
            }

            if (BotUser.GetBotRoleplay() == null)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", BotId);
                    dbClient.RunQuery();
                }

                Room.GetGameMap().RemoveUserFromMap(BotUser, new System.Drawing.Point(BotUser.X, BotUser.Y));

                Session.GetHabbo().GetInventoryComponent().TryAddBot(new Bot(Convert.ToInt32(BotUser.BotData.Id), Convert.ToInt32(BotUser.BotData.ownerID), BotUser.BotData.Name, BotUser.BotData.Motto, BotUser.BotData.Look, BotUser.BotData.Gender));
                Session.SendMessage(new BotInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetBots()));
                Room.GetRoomUserManager().RemoveBot(BotUser.VirtualId, false);
            }
            else
            {
                Room.GetGameMap().RemoveUserFromMap(BotUser, new System.Drawing.Point(BotUser.X, BotUser.Y));

                Session.GetHabbo().GetInventoryComponent().TryAddBot(new Bot(Convert.ToInt32(BotUser.BotData.Id), Convert.ToInt32(BotUser.BotData.ownerID), BotUser.BotData.Name, BotUser.BotData.Motto, BotUser.BotData.Look, BotUser.BotData.Gender));
                Session.SendMessage(new BotInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetBots()));
                RoleplayBotManager.EjectDeployedBot(BotUser, Room, true);
            }
        }
    }
}
