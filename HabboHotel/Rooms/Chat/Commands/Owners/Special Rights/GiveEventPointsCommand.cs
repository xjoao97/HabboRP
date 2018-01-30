using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class GiveEventPointsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_event_points"; }
        }

        public string Parameters
        {
            get { return "%usuário% %quantidade%"; }
        }

        public string Description
        {
            get { return "Fornece ao usuário a quantidade escolhida de pontos de evento."; }
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
                TargetClient.GetHabbo().EventPoints += Amount;
                TargetClient.GetHabbo().UpdateEventPointsBalance();

                if (TargetClient != Session)
                {
                    TargetClient.SendWhisper("Você acabou de ser premiado com " + String.Format("{0:N0}", Amount) + " Pontos de Eventos, " + Session.GetHabbo().Username + ".", 1);
                    Session.SendWhisper("Você acabou de dar " + TargetClient.GetHabbo().Username + " " + String.Format("{0:N0}", Amount) + " Pontos de Evento colocando seu total em " + String.Format("{0:N0}", TargetClient.GetHabbo().EventPoints) + ".", 1);
                }
                else
                    Session.SendWhisper("Você acabou de dar " + String.Format("{0:N0}", Amount) + " Pontos de Evento colocando seu total em " + String.Format("{0:N0}", TargetClient.GetHabbo().EventPoints) + ".", 1);
            }
            else if (Params[2] == "limpar" || Params[2] == "remover" || Params[2] == "retirar")
            {
                TargetClient.GetHabbo().EventPoints = 0;
                TargetClient.GetHabbo().UpdateEventPointsBalance();

                TargetClient.SendWhisper("Acabou de remover os Pontos de Evento por " + Session.GetHabbo().Username + ".", 1);
                Session.SendWhisper("Você acabou de remover todos os pontos de evento de " + TargetClient.GetHabbo().Username + ".", 1);
            }
            else
                Session.SendWhisper("Por favor insira um número válido, ou use ':pontosev (usuário) remover' Para tirar todos os seus pontos de evento.", 1);
        }
    }
}
