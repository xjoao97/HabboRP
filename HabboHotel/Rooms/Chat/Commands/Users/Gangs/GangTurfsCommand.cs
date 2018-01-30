using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangTurfsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_turfs"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista de todos os territórios de gangues capturáveis."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("<----- Territórios de Gangue ----->\n\n");

            if (TurfManager.TurfList.Count == 0)
                Message.Append("Não há territórios de gangues disponíveis!\n");

            lock (TurfManager.TurfList)
            {
                foreach (Turf Turf in TurfManager.TurfList.Values)
                {
                    if (Turf == null)
                        continue;

                    Room TurfRoom = RoleplayManager.GenerateRoom(Turf.RoomId, false);
                    Groups.Group Gang = Groups.GroupManager.GetGang(Turf.GangId);

                    if (TurfRoom != null)
                    {
                        Message.Append(TurfRoom.Name + " [Quarto ID: " + Turf.RoomId + "] --- Controlado por: " + Gang.Name + "\n");
                        Message.Append("----------\n");
                    }
                }
            }

            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}