using System;
using System.Linq;
using System.Threading;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class SendUserCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_send_user"; }
        }

        public string Parameters
        {
            get { return "%usuário% %quarto_id%"; }
        }

        public string Description
        {
            get { return "Envia um usuário para o ID de sua escolha."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Por favor, use o comando ':uenviar (usuário) (id do quarto)'.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (TargetClient.GetHabbo().CurrentRoom != null)
            {
                if (TargetClient.GetHabbo().CurrentRoom.TutorialEnabled)
                {
                    Session.SendWhisper("Você não pode enviar alguém que está em uma sala de tutorial!", 1);
                    return;
                }
            }

            int RoomId;
            if (!int.TryParse(Params[2], out RoomId))
            {
                Session.SendWhisper("Digite um ID válido!", 1);
                return;
            }

            Room TargetRoom = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId);

            if (TargetRoom == null)
            {
                Session.SendWhisper("Desculpe, não conseguimos encontrar o ID!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                TargetClient.GetRoleplay().IsDead = false;
                TargetClient.GetRoleplay().ReplenishStats(true);
                TargetClient.GetHabbo().Poof();
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                TargetClient.GetRoleplay().IsJailed = false;
                TargetClient.GetRoleplay().JailedTimeLeft = 0;
            }

            Session.Shout("*Envia imediatamente " + TargetClient.GetHabbo().Username + " para " + TargetRoom.Name + " [ID: " + RoomId + "]*", 23);
            RoleplayManager.SendUser(TargetClient, RoomId, "Você foi enviado para " + TargetRoom.Name + " [ID: " + RoomId + "] por " + Session.GetHabbo().Username + "!");
        }
    }
}