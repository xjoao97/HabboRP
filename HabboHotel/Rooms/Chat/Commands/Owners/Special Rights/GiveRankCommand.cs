using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class GiveRankCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_rank"; }
        }

        public string Parameters
        {
            get { return "%usuário% %quantidade%"; }
        }

        public string Description
        {
            get { return "Dá ao usuário um rank desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Você deve digitar o nome de usuário e classificação que você deseja dar a eles.", 1);
                return;
            }

            var TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Este usuário não foi encontrado! Talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null || TargetClient.GetRoomUser() == null)
            {
                Session.SendWhisper("Este usuário não foi encontrado! Talvez ele esteja offline.", 1);
                return;
            }

            int Rank;
            if (int.TryParse(Params[2], out Rank))
            {
                int OldRank = TargetClient.GetHabbo().Rank;

                if (Rank <= 0 || Rank > 9)
                {
                    Session.SendWhisper("Desculpe esta posição não existe!", 1);
                    return;
                }

                if (OldRank == Rank)
                {
                    Session.SendWhisper("Este usuário já tem [Cargo ID: " + Rank + "]", 1);
                    return;
                }

                TargetClient.GetHabbo().Rank = Rank;
                TargetClient.GetHabbo().InitPermissions();

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    dbClient.RunQuery("UPDATE `users` SET `rank` = '" + Rank + "' WHERE `id` = '" + TargetClient.GetHabbo().Id + "'");

                if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tickets"))
                {
                    TargetClient.SendMessage(new ModeratorInitComposer(
                        PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                        PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                        PlusEnvironment.GetGame().GetModerationManager().UserActionPresets,
                        PlusEnvironment.GetGame().GetModerationTool().GetTickets));
                }

                if (OldRank > 1 && Rank == 1)
                    TargetClient.SendNotification("Você foi rebaixado, você não tem mais acesso às Ferramentas de Modificação.\n\nPor favor recarregue para se livrar da caixa irritante.");
                else if (OldRank > Rank)
                    TargetClient.SendNotification("Desculpe...\n\nVocê foi rebaixado para [Rank ID: " + Rank + "] por " + Session.GetHabbo().Username + ".\n\nNão há necessidade de recarregar como suas permissões foram alteradas automaticamente.");
                else
                    TargetClient.SendNotification("Parabéns e bem-vindo à Equipe do HabboRPG!\n\nVocê foi promovido a [Cargo ID: " + Rank + "] por " + Session.GetHabbo().Username + "\n\nNão há necessidade de recarregar como todas as suas permissões foram ativadas!");

                Session.Shout("*Altera imediatamente o cargo " + TargetClient.GetHabbo().Username + " para [Cargo ID: " + Rank + "]*", 23);
            }
            else
                Session.SendWhisper("Por favor insira um número válido!", 1);
        }
    }
}
