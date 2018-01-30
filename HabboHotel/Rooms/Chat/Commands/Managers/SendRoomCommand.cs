using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class SendRoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_send_room"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envia todos os usuários na mesma sala que você para o ID do quarto desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira um quarto para enviar os usuários!", 1);
                return;
            }

            if (Room.GetRoomUserManager().GetRoomUsers().Count == 1)
            {
                Session.SendWhisper("Você é a única pessoa na sala!", 1);
                return;
            }

            int RoomId;
            if (!int.TryParse(Params[1], out RoomId))
            {
                Session.SendWhisper("Digite um id válido para o quarto!", 1);
                return;
            }

            Room TargetRoom = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId);

            if (TargetRoom == null)
            {
                Session.SendWhisper("Desculpe, mas este quarto não pôde ser encontrado!", 1);
                return;
            }

            if (TargetRoom == Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Você e todos os outros já estão nteste quarto!", 1);
                return;
            }

            List<string> CantSend = new List<string>();

            int count = 0;
            foreach (var user in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                    continue;

                if (user.GetClient() == Session)
                    continue;

                if (user.GetClient().GetRoleplay().Game != null)
                {
                    CantSend.Add(user.GetClient().GetHabbo().Username);
                    continue;
                }

                count++;

                if (user.GetClient().GetRoleplay().IsDead)
                {
                    user.GetClient().GetRoleplay().IsDead = false;
                    user.GetClient().GetRoleplay().ReplenishStats(true);
                    user.GetClient().GetHabbo().Poof();
                }

                if (user.GetClient().GetRoleplay().IsJailed)
                {
                    user.GetClient().GetRoleplay().IsJailed = false;
                    user.GetClient().GetRoleplay().JailedTimeLeft = 0;
                }

                RoleplayManager.SendUser(user.GetClient(), RoomId, "Você foi enviado para o quarto [" + TargetRoom.Name + " -´Quarto ID:  " + RoomId + "] por " + Session.GetHabbo().Username + "!");
            }

            if (count > 0)
                Session.Shout("*Envia imediatamente todas as pessoas do quarto para [" + TargetRoom.Name + " Quarto ID: " + RoomId + "]*", 23);

            if (CantSend.Count > 0)
            {
                string Users = "";

                foreach (string user in CantSend)
                {
                    Users += user + ",";
                }

                Session.SendMessage(new MOTDNotificationComposer("Desculpe, não foi possível enviar os seguintes usuários, pois estão dentro de um Evento!\n\n " + Users));
            }
        }
    }
}
