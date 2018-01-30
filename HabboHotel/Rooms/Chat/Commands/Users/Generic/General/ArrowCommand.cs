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
    class ArrowCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_arrow"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ativa as setas para caminhar."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode usar esse comando enquanto estiver dentro de um evento!", 1);
                return;
            }

            if (!Session.GetRoleplay().ArrowEnabled)
            {
                Session.SendWhisper("Você agora está caminhando com setas!");
                Session.GetRoleplay().ArrowEnabled = true;
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_arrowmovement:yes");
            }
            else
            {
                Session.SendWhisper("Você parou de caminhar com setas!");
                Session.GetRoleplay().ArrowEnabled = false;
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_arrowmovement:no");
            }
            
        }
    }
}