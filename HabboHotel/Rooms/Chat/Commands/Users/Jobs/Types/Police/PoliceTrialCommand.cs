using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class PoliceTrialCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_trial"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Permite que um usuário seja julgado."; }
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

            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "trial") && !Session.GetHabbo().GetPermissions().HasRight("give_police_trial"))
            {
                Session.SendWhisper("Somente um chefe da polícia pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("give_police_trial"))
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("You can't arrest someone who isn't playing the game right now!", 1);
                return;
            }

            int PoliceTrialRoom = Convert.ToInt32(RoleplayData.GetData("police", "trialroomid"));
            int PoliceTrialRoom2 = Convert.ToInt32(RoleplayData.GetData("police", "trialroomid2"));

            if (Room.Id != PoliceTrialRoom && Room.Id != PoliceTrialRoom2)
            {
                Session.SendWhisper("Você não pode prender alguém que está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            if (TargetClient.GetRoleplay().PoliceTrial)
            {
                TargetClient.GetRoleplay().PoliceTrial = false;
                Session.Shout("*Remove " + TargetClient.GetHabbo().Username + " do julgamento policial*", 37);
            }
            else
            {
                TargetClient.GetRoleplay().PoliceTrial = true;
                Session.Shout("*Coloca " + TargetClient.GetHabbo().Username + " em um julgamento policial*", 37);
            }
            #endregion
        }
    }
}