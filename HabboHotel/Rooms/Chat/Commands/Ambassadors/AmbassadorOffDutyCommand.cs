using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Ambassadors
{
    class AmbassadorOffDutyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ambassadorduty_off"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desliga o modo de serviço do seu embaixador."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Session.GetRoleplay().AmbassadorOnDuty)
            {
                Session.SendWhisper("Você já está fora do serviço!", 1);
                return;
            }

            Session.GetRoleplay().AmbassadorOnDuty = false;

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().CurrentEffect == EffectsList.Ambassador)
                    Session.GetRoomUser().ApplyEffect(EffectsList.None);
            }

            PlusEnvironment.GetGame().GetClientManager().AmbassadorWhisperAlert("Sai do serviço de embaixador!", Session);
        }
    }
}
