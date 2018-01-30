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
    class UnStunCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_stun_undo"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Desatordoa/Despulveriza um usuário."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            string Type = "";

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            if (Params[0].ToLower() == "despulverizar")
                Type = "despulverizar";

            if (Params[0].ToLower() == "desatordoar")
                Type = "desatordoar";

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

            if (!GroupManager.HasJobCommand(Session, "unstun") && !Session.GetRoleplay().PoliceTrial)
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetRoleplay().PoliceTrial)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar este comando!", 1);
                return;
            }

            if (!TargetClient.GetRoomUser().Frozen)
            {
                if (Type == "desatordoar")
                    Session.SendWhisper("Você não pode desatordoar alguém que não está atordoado!", 1);

                if (Type == "despulverizar")
                    Session.SendWhisper("Você não pode despulverizar alguém que não está pulverizado", 1);

                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode desatordoar alguém que está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 2)
            {
                        if (Type == "desatordoar")
                        {
                            Session.Shout("*Retira o efeito de atordoamento de " + TargetClient.GetHabbo().Username + "*", 37);
                            TargetClient.GetRoleplay().TimerManager.CreateTimer("desatordoar", 1000, false);
                            TargetClient.SendMessage(new FloodControlComposer(1));
                        }
                        else if (Type == "despulverizar")
                        {
                            Session.Shout("*Despulveriza " + TargetClient.GetHabbo().Username + "*", 37);
                            TargetClient.GetRoleplay().TimerManager.CreateTimer("despulverizar", 1000, false);
                        }

                        if (TargetClient.GetRoleplay().InsideTaxi)
                            TargetClient.GetRoleplay().InsideTaxi = true;

                        TargetClient.GetRoomUser().Frozen = false;
                        TargetClient.GetRoomUser().CanWalk = true;
                        TargetClient.GetRoomUser().ClearMovement(false);
                        Session.GetRoleplay().CooldownManager.CreateCooldown("desatordoar", 1000, 3);
                        return;
            }
            else
            {
                if (Type == "desatordoar")
                    Session.SendWhisper("Chegue mais perto para desatordoar este cidadão!", 1);

                if (Type == "despulverizar")
                    Session.SendWhisper("Chegue mais perto para despulverizar este cidadão!", 1);

                return;
            }
            #endregion
        }
    }
}