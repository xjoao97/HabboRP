using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class CheckBalanceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_check_balance"; }
        }

        public string Parameters
        {
            get { return "%user% %account%"; }
        }

        public string Description
        {
            get { return "Verifique o saldo do tipo de conta do usuário alvo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira um alvo e tipo de banco! (cheques, poupanca)");
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("Opa, usuário não encontrado!");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "checkbalance"))
            {
                Session.SendWhisper("Desculpe, você não trabalha na corporação do banco!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("checarsaldo"))
                return;

            #endregion

            #region Execute

            if (Params.Length < 3)
            {
                if (Target.GetRoleplay().BankAccount <= 0)
                {
                    Session.SendWhisper("Este usuário não possui uma Conta de Cheques!", 1);
                    return;
                }

                Session.Shout("*Verifica o saldo bancário de " + Target.GetHabbo().Username + " na sua conta de Cheques*", 4);
                Session.SendWhisper(Target.GetHabbo().Username + " tem R$" + String.Format("{0:N0}", Target.GetRoleplay().BankChequings) + " na conta de Cheques!", 1);
                Session.GetRoleplay().CooldownManager.CreateCooldown("checarsaldo", 1000, 3);
                return;
            }
            else
            {
                switch (Params[2].ToLower())
                {
                    case "chequings":
                    case "checkings":
					case "cheques":
                        {
                            if (Target.GetRoleplay().BankAccount <= 0)
                            {
                                Session.SendWhisper("Este usuário não possui uma Conta de Cheques!", 1);
                                break;
                            }

                            Session.Shout("*Verifica o saldo bancário de " + Target.GetHabbo().Username + " na sua conta de Cheques*", 4);
                            Session.SendWhisper(Target.GetHabbo().Username + " tem R$" + Target.GetRoleplay().BankChequings + "  na conta de Cheques!", 1);
                            Session.GetRoleplay().CooldownManager.CreateCooldown("checarsaldo", 1000, 3);
                            break;
                        }
                    case "savings":
					case "poupanca":
                        {
                            if (Target.GetRoleplay().BankAccount <= 1)
                            {
                                Session.SendWhisper("Este usuário não possui uma conta de Poupança!", 1);
                                break;
                            }

                            Session.Shout("*Verifica o saldo bancário de " + Target.GetHabbo().Username + " na sua conta Poupança*", 4);
                            Session.SendWhisper(Target.GetHabbo().Username + " tem R$" + Target.GetRoleplay().BankSavings + " na conta Poupança!", 1);
                            Session.GetRoleplay().CooldownManager.CreateCooldown("checarsaldo", 1000, 3);
                            break;
                        }
                    default:
                        {
                            Session.SendWhisper("Insira um tipo de conta bancária válida! (poupanca)");
                            break;
                        }
                }
            }

            #endregion
        }
    }
}
