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
    class InvisibleCommand : IChatCommand
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
            get { return "Faz você invisível"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (Session.GetRoleplay().Invisible)
            {
                Session.SendWhisper("Você já está invisível!", 1);
                return;
            }

            foreach (RoomUser roomUser in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
            {
                if (roomUser == null)
                    continue;
                if (roomUser.GetClient() == null)
                    continue;
                if (roomUser.GetClient().GetHabbo() == null)
                    continue;
                if (roomUser.GetClient().GetRoleplay().Invisible && roomUser.GetClient().GetHabbo().Username != Session.GetHabbo().Username)
                {
                    roomUser.GetClient().SendWhisper(Session.GetHabbo().Username + " também ficou insivível, agora vocês podem se ver!", 1);
                    continue;
                }
                if (roomUser.GetClient().GetHabbo().Username == Session.GetHabbo().Username)
                {
                    string cansee = "";


                    foreach (RoomUser invisibleuser in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                    {

                        if (invisibleuser.IsBot)
                            continue;

                        if (invisibleuser.GetClient().GetHabbo().Username != Session.GetHabbo().Username && invisibleuser.GetClient().GetRoleplay().Invisible)
                        {
                            cansee += invisibleuser.GetClient().GetHabbo().Username + ", ";
                            Session.SendMessage(new UsersComposer(invisibleuser));
                        }
                    }

                    Session.SendWhisper((cansee == "" ? "Não há pessoas invisíveis na sala!" : "Agora você pode ver: " + cansee + " pois ele também está invisivel!"), 1);

                    continue;
                }



                roomUser.GetClient().SendMessage(new UserRemoveComposer(Session.GetRoomUser().VirtualId));
            }

            Session.SendWhisper("Você está agora invisível!", 1);
            Session.GetRoleplay().Invisible = true;



        }
    }
}