using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Banking
{
    class BalanceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_banking_balance"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Diz-lhe o saldo do seu banco."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            bool BalanceHide = false;
            if (Params.Length > 1)
            {
                if (Params[1].ToLower() == "seguro")
                    BalanceHide = true;
            }
           
            if (Session.GetRoleplay().BankAccount <= 0)
            {
                Session.SendWhisper("Você não possui contas bancárias! Peça a um trabalhador bancário para abrir uma conta para você.", 1);
                return;
            }

            if (Room.BankEnabled == false && Session.GetRoleplay().PhoneType == 0)
            {
                Session.SendWhisper("Você não tem um celular para verificar sua conta bancária remotamente! Por favor, vá ao banco se quiser verificar o seu saldo.", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("saldo"))
                return;
            #endregion

            #region Execute
            if (!BalanceHide)
            {
                Session.Shout("*Verifica o saldo bancário e vejo que tenho R$" + String.Format("{0:N0}", Session.GetRoleplay().BankChequings) + " na minha conta de Cheques e R$" + String.Format("{0:N0}", Session.GetRoleplay().BankSavings) + " na Poupança.", 5);
                Session.GetRoleplay().CooldownManager.CreateCooldown("saldo", 1000, 5);
            }
            else
            {
                Session.Shout("*Verifica o saldo bancário*", 5);
                Session.SendWhisper("Você tem R$" + String.Format("{0:N0}", Session.GetRoleplay().BankChequings) + " na sua conta de Cheques.", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("saldo", 1000, 5);
            }

            if (Session.GetRoleplay().BankAccount > 1)
                Session.SendWhisper("Vá até o banco para retirar saldo da conta que deseja.", 1);
            #endregion
        }
    }
}