using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class RoomAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert_room"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envie uma mensagem para os usuários nesta sala."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor insira uma mensagem que deseja enviar para a sala.", 1);
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_alert") && Room.OwnerId != Session.GetHabbo().Id)
            {
                Session.SendWhisper("Você só pode Alerta de sala em seu próprio apartamento!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUser == null || RoomUser.GetClient() == null || Session.GetHabbo().Id == RoomUser.UserId)
                    continue;

                RoomUser.GetClient().SendNotification(Session.GetHabbo().Username + " alertou a sala com a seguinte mensagem:\n\n" + Message);
            }
            Session.SendWhisper("Mensagem enviada com sucesso para o quarto.", 1);
        }
    }
}
