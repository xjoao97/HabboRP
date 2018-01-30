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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ReleaseCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_release"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Libera um condenado da prisão."; }
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

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "release"))
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode soltar alguém que não está na prisão!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode liberar alguém que está ausente!", 1);
                return;
            }

            if (TargetClient.GetRoomUser().RoomId != Session.GetRoomUser().RoomId)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " não está na mesma sala que você!", 1);
                return;
            }
            #endregion

            #region Execute
            Session.Shout("*Liberta " + TargetClient.GetHabbo().Username + " da prisão e coloca em liberdade condicional*", 37);
            TargetClient.GetRoleplay().IsJailed = false;
            TargetClient.GetRoleplay().JailedTimeLeft = 0;
            #endregion
        }
    }
}