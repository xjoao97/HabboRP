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
    class RestoreCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_restore"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "O administrador libera o cidadão do hospital se ele estiverer morto."; }
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
            #endregion

            #region Execute

            Session.Shout("*Revive imediatamente " + TargetClient.GetHabbo().Username + "*", 23);
            TargetClient.GetRoleplay().IsDead = false;
            TargetClient.GetRoomUser().ApplyEffect(0);
            TargetClient.GetRoleplay().DeadTimeLeft = 0;
            TargetClient.GetRoleplay().CurHealth = TargetClient.GetRoleplay().MaxHealth;
            TargetClient.SendWhisper("Um administrador reviveu você!", 1);

            #endregion
        }
    }
}