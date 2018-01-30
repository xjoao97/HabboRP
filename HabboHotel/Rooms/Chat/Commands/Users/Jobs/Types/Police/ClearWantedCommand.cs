using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ClearWantedCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_clear_wanted"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Limpa toda a lista de desejos."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            int Bubble = 37;
            var RoomUser = Session.GetRoomUser();

            if (RoomUser == null)
                return;

            if (!GroupManager.HasJobCommand(Session, "clearwanted") && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
            {
                Session.SendWhisper("Somente um chefe da polícia pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (RoleplayManager.WantedList.Count <= 0)
            {
                Session.SendWhisper("A lista de procurados já está vazia!", 1);
                return;
            }

            if (Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager") && !Session.GetRoleplay().IsWorking)
                Bubble = 24;
            #endregion

            #region Execute
            Session.Shout("*Limpa toda a Lista de Procurados, removendo todos que estava nela*", Bubble);
            RoleplayManager.WantedList.Clear();
            #endregion
        }
    }
}