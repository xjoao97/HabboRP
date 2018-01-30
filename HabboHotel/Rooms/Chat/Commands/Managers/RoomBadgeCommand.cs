using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class RoomBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_badge_room"; }
        }

        public string Parameters
        {
            get { return "%badge%"; }
        }

        public string Description
        {
            get { return "Dá um emblema para todos do quarto!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, insira o nome do emblema que gostaria de dar ao quarto.", 1);
                return;
            }

            Badges.BadgeDefinition BadgeDefinition = null;
            if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(Params[1].ToUpper(), out BadgeDefinition))
            {
                Session.SendWhisper("As definições não possuem este emblema!", 1);
                return;
            }

            foreach (RoomUser User in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                    continue;

                if (!User.GetClient().GetHabbo().GetBadgeComponent().HasBadge(Params[1]))
                {
                    User.GetClient().GetHabbo().GetBadgeComponent().GiveBadge(Params[1], true, User.GetClient());
                    User.GetClient().SendNotification("Você acabou de receber um emblema!");
                }
                else
                    User.GetClient().SendWhisper(Session.GetHabbo().Username + " tentou lhe dar um emblema, mas você já o possui!", 1);
            }

            Session.SendWhisper("Você forneceu com sucesso a cada usuário nesta sala o emblema " + Params[1] + "!", 1);
        }
    }
}
