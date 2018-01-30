using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police
{
    class BailCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_related_bail"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Pagar fiança de um cidadão que está atualmente na prisão."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int BailCost = 0;
            #endregion

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            GameClients.GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
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

            if (!TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode pagar a fiança de alguém que não está na prisão! ", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode pagar a fiança de alguém que está em um evento!", 1);
                return;
            }

            if (TargetClient == RoleplayManager.Defendant)
            {
                Session.SendWhisper("Você não pode pagar a fiança de alguém que solicitou julgamento judicial!", 1);
                return;
            }

            BailCost = TargetClient.GetRoleplay().JailedTimeLeft * 30;

            if (Session.GetHabbo().Credits < BailCost)
            {
                Session.SendWhisper("Você não pode pagar a fiança de " + TargetClient.GetHabbo().Username + " isto custa R$" + String.Format("{0:N0}", BailCost) + "!", 1);
                return;
            }

            if (TargetClient.GetRoomUser().RoomId != Session.GetRoomUser().RoomId)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " não está na mesma sala que você!", 1);
                return;
            }
            #endregion

            #region Execute
            if (Params.Length >= 3)
            {
                if (Params[2].ToString().ToLower() == "sim")
                {
                    Session.Shout("*Paga a fiança de " + TargetClient.GetHabbo().Username + "' por R$" + String.Format("{0:N0}", BailCost) + ", liberta-o da prisão e coloca em liberdade condicional *", 4);

                    Session.GetHabbo().Credits -= BailCost;
                    Session.GetHabbo().UpdateCreditsBalance();

                    TargetClient.GetRoleplay().IsJailed = false;
                    TargetClient.GetRoleplay().JailedTimeLeft = 0;
                    return;
                }
                else
                {
                    Session.SendWhisper("Se você realmente quer pagar R$" + String.Format("{0:N0}", BailCost) + " para a fiança de " + TargetClient.GetHabbo().Username + "! Digite :fianca " + TargetClient.GetHabbo().Username + " sim.", 1);
                    return;
                }
            }
            else
            {
                Session.SendWhisper("Se você realmente quer pagar R$" + String.Format("{0:N0}", BailCost) + " para a fiança " + TargetClient.GetHabbo().Username + "! Digite :fianca " + TargetClient.GetHabbo().Username + " sim.", 1);
                return;
            }
            #endregion
        }
    }
}