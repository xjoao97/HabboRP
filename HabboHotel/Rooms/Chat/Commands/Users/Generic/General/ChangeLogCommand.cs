using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Notifications;


using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class ChangeLogCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_change_log"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Exibe as atualizações mais recentes."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {

            if (string.IsNullOrWhiteSpace(PlusEnvironment.GetDBConfig().DBData["welcome_message"]))
            {
                Session.SendWhisper("Opa, não há registro de mudanças!", 1);
                return;
            }
            
            Session.SendMessage(new MOTDNotificationComposer(PlusEnvironment.GetDBConfig().DBData["welcome_message"].Replace("\\r\\n", "\n")));
        }
    }
}