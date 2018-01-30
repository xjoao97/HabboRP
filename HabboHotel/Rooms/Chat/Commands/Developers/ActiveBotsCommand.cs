using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Misc;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class ActiveBotsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_active_bots"; }
        }

        public string Parameters
        {
            get { return "%comando%"; }
        }

        public string Description
        {
            get { return "Testando bots"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            ConcurrentBag<RoomUser> ActiveBots = new ConcurrentBag<RoomUser>();

            foreach (Room LoadedRoom in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList())
            {
                if (LoadedRoom == null)
                    continue;

                foreach (RoomUser Bot in LoadedRoom.GetRoomUserManager().GetBotList().ToList())
                {
                    ActiveBots.Add(Bot);
                }

            }

            string String = null;
            String += "Bots ativos:\n------------------\n\n";
            foreach (RoomUser Bot in ActiveBots)
            {
                if (Bot == null)
                    continue;

                if (Bot.GetBotRoleplay() == null)
                    continue;

                if (Bot.GetBotRoleplayAI() == null)
                    continue;

                Room BotRoom = Bot.GetBotRoleplayAI().GetRoom();

                if (BotRoom == null)
                    continue;

                String += "Nome do BOT: " + Bot.GetBotRoleplay().Name + "\n";
                String += "Nome do Quarto: " + BotRoom.Name + "\n";
                String += "Quarto ID: " + BotRoom.Id + "\n";
                String += "Usuários atuais: " + BotRoom.UsersNow + "\n\n";
            }

            Session.SendMessage(new MOTDNotificationComposer(String));
        }
    }
}
