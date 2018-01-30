using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class GiveBadgeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_badge"; }
        }

        public string Parameters
        {
            get { return "%usuário% %emblema%"; }
        }

        public string Description
        {
            get { return "Dê um emblema a outro usuário."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Digite um usuário e o código do emblema que você gostaria de dar!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            Badges.BadgeDefinition BadgeDefinition = null;
            if (!PlusEnvironment.GetGame().GetBadgeManager().TryGetBadge(Params[2].ToUpper(), out BadgeDefinition))
            {
                Session.SendWhisper("Emblema não encontrado!", 1);
                return;
            }

            if (TargetClient != null)
            {
                if (!TargetClient.GetHabbo().GetBadgeComponent().HasBadge(Params[2]))
                {
                    TargetClient.GetHabbo().GetBadgeComponent().GiveBadge(Params[2], true, TargetClient);
                    if (TargetClient.GetHabbo().Id != Session.GetHabbo().Id)
                    {
                        TargetClient.SendNotification("Você acabou de receber um emblema!");
                        Session.SendWhisper("Sucesso! " + TargetClient.GetHabbo().Username + " recebeu o emblema " + Params[2].ToUpper() + "!", 1);
                    }
                    else
                        Session.SendWhisper("Você se deu com sucesso o emblema " + Params[2].ToUpper() + "!", 1);
                }
                else
                    Session.SendWhisper("Opa, esse usuário já possui este emblema (" + Params[2].ToUpper() + ") !", 1);
                return;
            }
            else
            {
                Session.SendWhisper("Opa, não conseguimos encontrar esse usuário!", 1);
                return;
            }
        }
    }
}
