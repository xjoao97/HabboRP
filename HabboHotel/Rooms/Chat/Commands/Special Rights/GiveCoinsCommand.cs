using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class GiveCoinsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_coins"; }
        }

        public string Parameters
        {
            get { return "%usuário% %quantidade%"; }
        }

        public string Description
        {
            get { return "Dá ao usuário a quantidade escolhida de grana."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Você deve digitar o nome de usuário e o valor que você deseja dar a eles.", 1);
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

            int Amount;
            if (int.TryParse(Params[2], out Amount))
            {
                TargetClient.GetHabbo().Credits += Amount;
                TargetClient.GetHabbo().UpdateCreditsBalance();

                if (TargetClient != Session)
                {
                    TargetClient.SendWhisper("Você acabou de ser premiado R$" + String.Format("{0:N0}", Amount) + " por " + Session.GetHabbo().Username + ".", 1);
                    Session.SendWhisper("Você acabou de dar " + TargetClient.GetHabbo().Username + " R$" + String.Format("{0:N0}", Amount) + " colocando seu total em R$" + String.Format("{0:N0}", TargetClient.GetHabbo().Credits) + ".", 1);
                }
                else
                    Session.SendWhisper("Você acabou de se dar R$" + String.Format("{0:N0}", Amount) + " colocando seu total para R$" + String.Format("{0:N0}", TargetClient.GetHabbo().Credits) + ".", 1);
            }
            else if (Params[2] == "limpar" || Params[2] == "remover" || Params[2] == "retirar")
            {
                TargetClient.GetHabbo().Credits = 0;
                TargetClient.GetHabbo().UpdateCreditsBalance();

                TargetClient.SendWhisper("Todo seu dinheiro foi retirado por " + Session.GetHabbo().Username + ".", 1);
                Session.SendWhisper("Você acabou de remover todo o dinheir de " + TargetClient.GetHabbo().Username + ".", 1);
            }
            else
                Session.SendWhisper("Por favor insira um número válido, ou use ':grana (usuário) remover' para tirar todos os seus créditos.", 1);
        }
    }
}
