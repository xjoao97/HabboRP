using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class GiveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_give"; }
        }

        public string Parameters
        {
            get { return "%user% %amount%"; }
        }

        public string Description
        {
            get { return "Permite que você dê uma quantia desejada de dinheiro ao usuário desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            int Amount;

            if (Params.Length != 3)
            {
                Session.SendWhisper("Digite um usuário e o valor que deseja dar!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (Session.GetRoleplay().Level < 3)
            {
                Session.SendWhisper("Seu nível é muito baixo para dar dinheiro! Você deve ser pelo menos nível 3!", 1);
                return;
            }

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode dar dinheiro para você mesmo!", 1);
                return;
            }

            if (TargetClient.MachineId == Session.MachineId)
            {
                Session.SendWhisper("Você não pode dar dinheiro a outra de suas contas seu ixpertinho!", 1);
                return;
            }

            if (int.TryParse((Params[2]), out Amount))
            {
                if (Amount <= 0)
                {
                    Session.SendWhisper("Por favor insira uma quantia válida de dinheiro!", 1);
                    return;
                }

                if (Session.GetHabbo().Credits < Amount)
                {
                    Session.SendWhisper("Você não tem R$" + String.Format("{0:N0}", Amount) + " para dar!", 1);
                    return;
                }

                if (Session.GetRoleplay().TryGetCooldown("dar"))
                    return;

                Session.GetHabbo().Credits -= Amount;
                TargetClient.GetHabbo().Credits += Amount;

                Session.GetHabbo().UpdateCreditsBalance();
                TargetClient.GetHabbo().UpdateCreditsBalance();

                Session.Shout("*Dá para " + TargetClient.GetHabbo().Username + " R$" + String.Format("{0:N0}", Amount) + "*", 4);
                TargetClient.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", Amount) + " de " + Session.GetHabbo().Username + "!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("dar", 1000, 3);
            }
            else
                Session.SendWhisper("Por favor insira um número válido!", 1);
        }
    }
}