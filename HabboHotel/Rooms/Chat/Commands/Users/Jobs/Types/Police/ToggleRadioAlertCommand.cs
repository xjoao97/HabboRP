using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ToggleRadioAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_toggle_radio_alerts"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ignorar alertas vip."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Group Job = GroupManager.GetJob(Session.GetRoleplay().JobId);

            if (Job == null)
            {
                Session.SendWhisper("Você está desempregado!", 1);
                return;
            }

            if (Job.Id <= 0)
            {
                Session.SendWhisper("Você está desempregado!", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "radio") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Apenas policiais podem usar esse comando!", 1);
                return;
            }

            Session.GetRoleplay().DisableRadio = !Session.GetRoleplay().DisableRadio;
            Session.SendWhisper("Você agora " + (Session.GetRoleplay().DisableRadio ? "está" : "não está mais") + " ignorando alertas de rádio!", 1);
        }
    }
}
