using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class OffDutyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_staffduty_off"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desliga o modo de plantão da sua equipe."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Session.GetRoleplay().StaffOnDuty)
            {
                Session.SendWhisper("Você já está fora do plantão!", 1);
                return;
            }

            Session.GetRoleplay().StaffOnDuty = false;

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().CurrentEffect == 102)
                    Session.GetRoomUser().ApplyEffect(0);
            }

            PlusEnvironment.GetGame().GetClientManager().StaffWhisperAlert("Acabei de sair de serviço!", Session);
        }
    }
}
