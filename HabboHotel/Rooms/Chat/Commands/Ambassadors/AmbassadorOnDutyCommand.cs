using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Ambassadors
{
    class AmbassadorOnDutyCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ambassadorduty_on"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Transforma o modo de serviço do seu embaixador."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetRoleplay().AmbassadorOnDuty)
            {
                Session.SendWhisper("Você já está de plantão.", 1);
                return;
            }

            Session.GetRoleplay().AmbassadorOnDuty = true;

            if (Session.GetRoomUser() != null)
            {
                if (Session.GetRoomUser().CurrentEffect != EffectsList.Ambassador)
                    Session.GetRoomUser().ApplyEffect(EffectsList.Ambassador);
            }

            PlusEnvironment.GetGame().GetClientManager().AmbassadorWhisperAlert("Entrei no serviço de Plantão!", Session);
        }
    }
}
