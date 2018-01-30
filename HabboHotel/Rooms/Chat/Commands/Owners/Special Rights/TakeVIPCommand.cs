using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class TakeVIPCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_vip_undo"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Retira o VIP de um cidadão."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 2)
            {
                Session.SendWhisper("Você deve digitar o nome de usuário da pessoa para a qual deseja fazer o VIP.", 1);
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

            if (TargetClient.GetHabbo().VIPRank != 1)
            {
                Session.SendWhisper("Desculpe, este usuário não tem VIP para retirar!", 1);
                return;
            }

            TargetClient.GetHabbo().VIPRank = 0;
            TargetClient.GetHabbo().Colour = "";
            TargetClient.SendNotification("Seu VIP foi removido por " + Session.GetHabbo().Username);
            TargetClient.GetHabbo().GetPermissions().Init(TargetClient.GetHabbo());

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                dbClient.RunQuery("UPDATE `users` SET `rank_vip` = '0', `colour` = '' WHERE `id` = '" + TargetClient.GetHabbo().Id + "'");

            Session.SendWhisper("Você retirou o VIP de " + TargetClient.GetHabbo().Username + "!", 1);
        }
    }
}
