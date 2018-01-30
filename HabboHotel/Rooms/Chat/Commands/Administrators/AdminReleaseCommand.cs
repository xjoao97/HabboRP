using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class AdminReleaseCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_release"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "O administrador libera o cidadão da prisão se estiver preso."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            var RoomUser = Session.GetRoomUser();
            var TargetRoomUser = TargetClient.GetRoomUser();

            if (RoomUser == null || TargetRoomUser == null)
                return;

            if (!TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode libertar alguém que não está preso!", 1);
                return;
            }
            #endregion

            #region Execute

            Session.Shout("Retira imediatamente " + TargetClient.GetHabbo().Username + " da prisão*", 23);
            TargetClient.GetRoleplay().IsJailed = false;
            TargetClient.GetRoleplay().JailedTimeLeft = -5;

            #endregion
        }
    }
}