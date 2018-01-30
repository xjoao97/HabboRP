using System;
using System.Linq;
using System.Text;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Guides;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class HelpCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_staff_related_help"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Solicitação de ajuda por um membro da equipe."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para o membro da equipe ver!", 1);
                return;
            }

            if (PlusEnvironment.GetGame().GetModerationTool().UsersHasPendingTicket(Session.GetHabbo().Id))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Você já possui um ticket pendente, aguarde uma resposta de um moderador."));
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);

            if (Message.Length <= 10)
            {
                Session.SendWhisper("Digite uma mensagem mais descritiva para o membro da equipe para ver!", 1);
                return;
            }

            PlusEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 25, 0, Message, null);
            Session.SendMessage(new RoomNotificationComposer("help_ticket_submit", "message", "Seu ticket de suporte foi enviado!"));

            PlusEnvironment.GetGame().GetClientManager().ModAlert("Foi recebido um novo ticket de suporte!");
        }
    }
}