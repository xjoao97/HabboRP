using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class OnDutyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_staffduty_on"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ativa o modo de serviço da equipe."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetRoleplay().StaffOnDuty)
            {
                Session.SendWhisper("Você já está de plantão!", 1);
                return;
            }

            Session.GetRoleplay().StaffOnDuty = true;

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().CurrentEffect != 178)
                    Session.GetRoomUser().ApplyEffect(178);
            }

            PlusEnvironment.GetGame().GetClientManager().StaffWhisperAlert("Eu entrei em plantão!", Session);
        }
    }
}
