using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class GiveDucketsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_duckets"; }
        }

        public string Parameters
        {
            get { return "%usuário% %quantidade%"; }
        }

        public string Description
        {
            get { return "Dá ao usuário a quantidade escolhida de créditos de celular."; }
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
                TargetClient.GetHabbo().Duckets += Amount;
                TargetClient.GetHabbo().UpdateDucketsBalance();

                if (TargetClient != Session)
                {
                    TargetClient.SendWhisper("Você acabou de ser premiado com " + String.Format("{0:N0}", Amount) + " créditos por telefone de " + Session.GetHabbo().Username + ".", 1);
                    Session.SendWhisper("Você acabou de dar " + TargetClient.GetHabbo().Username + " " + String.Format("{0:N0}", Amount) + " créditos de telefone colocando seu total em " + String.Format("{0:N0}", TargetClient.GetHabbo().Duckets) + ".", 1);
                }
                else
                    Session.SendWhisper("Você acabou de dar " + String.Format("{0:N0}", Amount) + " crédito por telefone colocando seu total em " + String.Format("{0:N0}", TargetClient.GetHabbo().Duckets) + ".", 1);
            }
            else if (Params[2] == "limpar" || Params[2] == "remover" || Params[2] == "retirar")
            {
                TargetClient.GetHabbo().Duckets = 0;
                TargetClient.GetHabbo().UpdateDucketsBalance();

                TargetClient.SendWhisper("Todos seus créditos de celular foi retirado por " + Session.GetHabbo().Username + ".", 1);
                Session.SendWhisper("Você acabou de remover todos os créditos de telefone de " + TargetClient.GetHabbo().Username + ".", 1);
            }
            else
                Session.SendWhisper("Por favor insira um número válido, ou use ':creditoscel (usuário) remover' para tirar todo o seu crédito de telefone.", 1);
        }
    }
}
