using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class ReleaseAllCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_release_all"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Liberta todos os usuários que estão presos."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Execute
            int JailedUsers = 0;

            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (client == null)
                    continue;

                if (client.GetRoleplay() == null)
                    continue;

                if (!client.GetRoleplay().IsJailed)
                    continue;

                if (client.GetRoomUser() == null)
                    continue;

                JailedUsers++;

                client.GetRoleplay().IsJailed = false;
                client.GetRoleplay().JailedTimeLeft = 0;
                client.SendWhisper("Um administrador liberou todos da prisão!");
            }

            Session.Shout("*Liberta todas pessoas presas no hotel*", 23);
            Session.SendWhisper("Você libertou com sucesso " + JailedUsers + " da prisão!", 1);

            #endregion

        }
    }
}
