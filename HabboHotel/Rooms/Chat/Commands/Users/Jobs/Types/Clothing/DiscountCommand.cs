using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Clothing
{
    class DiscountCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_discount"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Fornece um desconto para um comprador na loja de roupas, ao comprar roupas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome do comprador!");
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("Opa, não encontrou esse usuário!");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "discount") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
            {
                Session.SendWhisper("Desculpe, você não trabalha na corporação da Loja de Roupas!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
            {
                Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um desconto de roupas!", 1);
                return;
            }

            if (Target.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("roupas"))
            {
                Session.SendWhisper("Este usuário já recebeu um desconto na loja de roupas!", 1);
                return;
            }
            #endregion

            #region Execute
            Session.Shout("*Oferece um desconto para " + Target.GetHabbo().Username + " de 5% ao comprar um item de vestuário na loja*", 4);
            Target.GetRoleplay().OfferManager.CreateOffer("roupas", Session.GetHabbo().Id, 0);
            return;
            #endregion
        }
    }
}