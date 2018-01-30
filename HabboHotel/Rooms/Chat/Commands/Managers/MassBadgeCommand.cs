using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class MassBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_badge_mass"; }
        }

        public string Parameters
        {
            get { return "%emblema%"; }
        }

        public string Description
        {
            get { return "Dê um emblema a todo o hotel."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor insira o código do emblema que deseja dar ao hotel inteiro.", 1);
                return;
            }

            Badges.BadgeDefinition BadgeDefinition = null;
            if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(Params[1].ToUpper(), out BadgeDefinition))
            {
                Session.SendWhisper("As definições de emblema não contêm este emblema!", 1);
                return;
            }

            foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().Username == Session.GetHabbo().Username)
                    continue;

                if (!Client.GetHabbo().GetBadgeComponent().HasBadge(Params[1]))
                {
                    Client.GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, Client);
                    Client.SendNotification("Você acabou de receber um emblema!");
                }
                else
                    Client.SendWhisper(Session.GetHabbo().Username + " tentou lhe dar um emblema, mas você já o possui!", 1);
            }

            Session.SendWhisper("Você deu com sucesso a cada usuário neste hotel o emblema " + Params[1] + "!", 1);
        }
    }
}
