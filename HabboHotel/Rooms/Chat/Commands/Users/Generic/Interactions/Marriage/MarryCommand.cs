using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Marriage
{
    class MarryCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_marry"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Faz um pedido de casamento para um cidadão."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Opa, usuário não encontrado!", 1);
                return;
            }

            if (Session == TargetClient)
            {
                Session.SendWhisper("Você não pode se casar com você mesmo!", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (Session.GetRoleplay().MarriedTo > 0)
            {
                Session.SendWhisper("Você já se casou com alguém! Divorcie primeiro!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().MarriedTo > 0)
            {
                Session.SendWhisper("Desculpe, mas esse usuário já está casado(a) com outra pessoa!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("casamento"))
            {
                Session.SendWhisper("Alguém deve ter oferecido recentemente para casar com ele(a) antes de você! Mais sorte da próxima vez!", 1);
                return;
            }
            #endregion

            #region Execute
            TargetClient.GetRoleplay().OfferManager.CreateOffer("casamento", Session.GetHabbo().Id, 0);
            TargetClient.SendWhisper(Session.GetHabbo().Username + " acabou de se casar com você! Digite ':aceitar casamento' para casar com ele!", 1);
            Session.Shout("*Fica de joelhos e pergunta " + TargetClient.GetHabbo().Username + ", quer casar comigo?*", 16);
            #endregion
        }
    }
}