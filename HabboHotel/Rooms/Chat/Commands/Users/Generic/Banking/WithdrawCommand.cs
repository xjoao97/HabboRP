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
    class WithdrawCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_banking_deposit"; }
        }

        public string Parameters
        {
            get { return "%amount% %account%"; }
        }

        public string Description
        {
            get { return "Retirar dinheiro da sua conta desejada."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int Amount;
            int TaxAmount;
            #endregion

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor insira o valor que deseja retirar.", 1);
                return;
            }

            if (Room.BankEnabled == false)
            {
                Session.SendWhisper("Você deve estar no banco para retirar dinheiro em suas contas!", 1);
                return;
            }

            if (Session.GetRoleplay().BankAccount <= 0)
            {
                Session.SendWhisper("Você não possui contas bancárias! Peça a um trabalhador bancário para abrir uma conta para você.", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("retirar"))
                return;
            #endregion

            #region Execute
            if (Params.Length == 2)
            {
                if (Session.GetRoleplay().BankAccount < 1)
                {
                    Session.SendWhisper("Você não tem uma conta Cheques! Peça um trabalhador bancário para abrir um para você!", 1);
                    return;
                }

                if (int.TryParse(Params[1], out Amount))
                {
                    if (Amount <= 0)
                    {
                        Session.SendWhisper("Por favor, insira um valor válido para retirar!", 1);
                        return;
                    }

                    if (Session.GetRoleplay().BankChequings < Amount)
                    {
                        Session.SendWhisper("Você não tem R$" + String.Format("{0:N0}", Amount) + " para retirar!", 1);
                        return;
                    }
                }
                else
                {
                    Session.SendWhisper("Por favor insira um número para retirar!", 1);
                    return;
                }

                Session.Shout("*Retira R$" + String.Format("{0:N0}", Amount) + " da sua conta de Cheques e guarda no bolso*", 5);

                Session.GetHabbo().Credits += Amount;
                Session.GetHabbo().UpdateCreditsBalance();

                Session.GetRoleplay().BankChequings -= Amount;
                Session.GetRoleplay().CooldownManager.CreateCooldown("retirar", 1000, 5);
            }
            else
            {
                switch (Params[2].ToLower())
                {
                    case "1":
                    case "chequings":
					case "cheques":
					case "check":
                        {
                            if (Session.GetRoleplay().BankAccount < 1)
                            {
                                Session.SendWhisper("Você não tem uma conta de Cheques! Peça um trabalhador bancário para abrir um para você!", 1);
                                return;
                            }

                            if (int.TryParse(Params[1], out Amount))
                            {
                                if (Session.GetRoleplay().BankChequings < Amount)
                                {
                                    Session.SendWhisper("Você não tem R$" + String.Format("{0:N0}", Amount) + " para retirar!", 1);
                                    return;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Por favor insira um número para retirar!", 1);
                                return;
                            }

                            Session.Shout("*Retira  R$" + String.Format("{0:N0}", Amount) + " da sua conta de Cheques e guarda no bolso bolso*", 5);

                            Session.GetHabbo().Credits += Amount;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankChequings -= Amount;
                            Session.GetRoleplay().CooldownManager.CreateCooldown("retirar", 1000, 5);
                            break;
                        }
                    case "2":
                    case "savings":
					case "poupanca":
                        {
                            if (Session.GetRoleplay().BankAccount < 2)
                            {
                                Session.SendWhisper("Você não tem uma conta Poupança! Peça um trabalhador bancário para abrir um para você!", 1);
                                return;
                            }

                            if (int.TryParse(Params[1], out Amount))
                            {
                                TaxAmount = Convert.ToInt32((double)Amount * 0.05);

                                if (Amount < 5)
                                {
                                    Session.SendWhisper("O valor mínimo que você pode retirar da sua conta de poupança é R$5", 1);
                                    return;
                                }

                                if (Session.GetRoleplay().BankSavings < Amount)
                                {
                                    Session.SendWhisper("Você não tem R$" + String.Format("{0:N0}", Amount) + " para retirar!", 1);
                                    return;
                                }

                                if (Session.GetRoleplay().ATMAmount.IndexOf(Amount) != -1)
                                {
                                    Session.SendWhisper("Você já retirou R$" + Amount + "!", 1);
                                    return;
                                }

                                if (Params.Length < 4)
                                {
                                    Session.SendWhisper("Vai custa R$" + String.Format("{0:N0}", TaxAmount) + " para retirar R$" + Amount + " da sua conta Poupança! Digite ':retirar " + String.Format("{0:N0}", Amount) + " poupanca sim' se você aceitar esse valor de imposto.", 1);
                                    return;
                                }

                                if (Params[3].ToLower() != "sim")
                                {
                                    Session.SendWhisper("Vai custa R$" + String.Format("{0:N0}", TaxAmount) + " para retirar R$" + String.Format("{0:N0}", Amount) + "  da sua conta Poupança! Digite ':retirar " + String.Format("{0:N0}", Amount) + " poupanca sim' se você aceitar esse valor de imposto.", 1);
                                    return;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Por favor insira um número para retirar!", 1);
                                return;
                            }

                            Session.Shout("*Retira $" + String.Format("{0:N0}", Amount) + " da sua conta Poupança e coloca no bolso*", 5);
                            Session.SendWhisper("Você pagou um juros de R$" + String.Format("{0:N0}", TaxAmount) + " para retirar R$" + String.Format("{0:N0}", Amount) + "!", 1);

                            Session.GetHabbo().Credits += (Amount - TaxAmount);
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankSavings -= Amount;

                            Session.GetRoleplay().ATMAmount.Add(Amount);
                            Session.GetRoleplay().CooldownManager.CreateCooldown("retirar", 1000, 5);
                            break;
                        }
                    default:
                        {
                            Session.SendWhisper("Por favor, use 'cheques' ou 'poupanca' para retirar do tipo de conta!", 1);
                            break;
                        }
                }
            }
            #endregion
        }
    }
}