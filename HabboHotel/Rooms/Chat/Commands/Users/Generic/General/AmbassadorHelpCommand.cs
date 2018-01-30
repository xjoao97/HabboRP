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
    class AmbassadorHelpCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ambassador_related_help"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Solicitação de ajuda por um embaixador."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para o embaixador ver!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("ajudaemb"))
                return;

            List<GameClient> AvailableAmbassadors = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && x.GetHabbo().GetPermissions() != null && x.GetHabbo().GetPermissions().HasRight("embaixador")).ToList();
            if (AvailableAmbassadors.Count <= 0)
            {
                Session.SendWhisper("Desculpe, mas não há embaixadores online que vejam o tickets de ajuda agora!", 1);
                return;
            }
            string Message = CommandManager.MergeParams(Params, 1);

            if (Message.Length <= 10)
            {
                Session.SendWhisper("Digite uma mensagem mais descritiva para o embaixador ver!", 1);
                return;
            }

            lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null || client.GetRoleplay() == null)
                        continue;

                    if (!client.GetHabbo().GetPermissions().HasRight("embaixador"))
                        continue;

                    client.SendWhisper("[Alerta EMBAIXADOR] " + Session.GetHabbo().Username + " está solicitando ajuda em '" + Room.Name + " (Quarto ID: " + Room.RoomId + ")' com a mensagem descritiva: '" + Message + "'", 37);
                }
            }

            Session.SendMessage(new RoomNotificationComposer("help_ticket_submit", "message", "Seu ticket de ajuda foi enviado!"));
            Session.GetRoleplay().CooldownManager.CreateCooldown("ajudaemb", 1000, 30);
        }
    }
}