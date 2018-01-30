using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class VisibleCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_invisible"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Faz você ficar visível."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (!Session.GetRoleplay().Invisible)
            {
                Session.SendWhisper("Você já está visível!", 1);
                return;
            }

            Session.GetHabbo().CurrentRoom.SendMessage(new UsersComposer(Session.GetRoomUser()));
            Session.SendWhisper("Você está agora visível!", 1);
            Session.GetRoleplay().Invisible = false;

            string cantsee = "";

            foreach (RoomUser invisibleuser in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
            {
                if (invisibleuser.IsBot)
                    continue;

                if (invisibleuser.GetClient().GetHabbo().Username != Session.GetHabbo().Username && invisibleuser.GetClient().GetRoleplay().Invisible)
                {
                    invisibleuser.GetClient().SendWhisper(Session.GetHabbo().Username + " ficou visível, então ele não pode mais ver você!", 1);
                    cantsee += invisibleuser.GetClient().GetHabbo().Username + ", ";
                    Session.SendMessage(new UserRemoveComposer(invisibleuser.VirtualId));
                }
            }


            Session.SendWhisper((cantsee == "" ? "Não há pessoas invisíveis na sala!" : "Você não pode mais ver: " + cantsee + " pois ele está invisível!"), 1);

        }
    }
}