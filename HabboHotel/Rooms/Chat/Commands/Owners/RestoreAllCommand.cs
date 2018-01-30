using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class RestoreAllCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_restore_all"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Revive todos os usuários online que estão mortos."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Execute
            int DeadUsers = 0;

            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (client == null)
                    continue;

                if (client.GetRoleplay() == null)
                    continue;

                if (!client.GetRoleplay().IsDead)
                    continue;

                if (client.GetRoomUser() == null)
                    continue;

                DeadUsers++;

                client.GetRoleplay().IsDead = false;
                client.GetRoleplay().DeadTimeLeft = 0;
                client.GetRoleplay().ReplenishStats(true);
                client.SendWhisper("Um administrador reviveu todos do hospital!");

            }

            Session.Shout("*Revive imediatamente todas as pessoas mortas no hotel*", 23);
            Session.SendWhisper("Você reviveu com sucesso " + DeadUsers + " pessoas do Hospital!", 1);

            #endregion

        }
    }
}
