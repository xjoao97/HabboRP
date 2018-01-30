using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class ToggleVIPAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_toggle_vip_alerts"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite ignorar alertas vip."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetRoleplay().DisableVIPA = !Session.GetRoleplay().DisableVIPA;
            Session.SendWhisper("Você " + (Session.GetRoleplay().DisableVIPA ? "agora está" : "não está mais mais") + " ignorando alertas vip!", 1);
        }
    }
}
