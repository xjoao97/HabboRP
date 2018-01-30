using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangBackupCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_backup"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Envia uma mensagem aos seus membros do grupo solicitando backup."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            GroupRank GangRank = GroupManager.GetGangRank(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank);
            #endregion

            #region Conditions
            if (Gang == null)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (!GroupManager.HasGangCommand(Session, "gajuda"))
            {
                Session.SendWhisper("Você não tem permissão para usar este comando!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("gajuda"))
                return;
            #endregion

            #region Execute
            foreach (int Member in Gang.Members.Keys)
            {
                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                if (Client == null)
                    continue;

                Client.SendWhisper("[GANGUE] " + Session.GetHabbo().Username + " está pedindo ajuda em " + Room.Name + " [Quarto ID: " + Room.RoomId + "]", 34);
            }

            Session.GetRoleplay().CooldownManager.CreateCooldown("gajuda", 1000, 5);
            #endregion
        }
    }
}