using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// ATMWebEvent class.
    /// </summary>
    class ATMWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            if (!Client.GetRoleplay().UsingAtm)
            {
                Client.SendNotification("Boa tentativa, você não pode explorar o sistema, vá para um caixa eletrônico!");
                return;
            }

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {

                #region Open
                case "open":
                    {
                        string SendData = "";
                        SendData += Client.GetRoleplay().BankAccount + ",";
                        SendData += Client.GetRoleplay().BankChequings + ",";
                        SendData += Client.GetRoleplay().BankSavings + ",";
                        Socket.Send("compose_atm:open:" + SendData);
                    }
                    break;
                #endregion

                #region Close
                case "close":
				case "fechar":
                    {
                        Client.GetRoleplay().UsingAtm = false;
                        break;
                    }
                #endregion

                #region Withdraw
                case "withdraw":
				case "retirar":
                    {
                        string[] ReceivedData = Data.Split(',');

                        int Amount;

                        if (!int.TryParse(ReceivedData[1], out Amount))
                        {
                            Socket.Send("compose_atm:error:Digite um número válido!");
                            return;
                        }

                        int WithdrawAmount = Convert.ToInt32(ReceivedData[1]);
                        string AccountType = Convert.ToString(ReceivedData[2]);

                        int ActualAmount = ((AccountType == "Checkings" ? Client.GetRoleplay().BankChequings : Client.GetRoleplay().BankSavings));

                        if (Client.GetRoleplay().TryGetCooldown("withdraw"))
                            return;

                        if (WithdrawAmount <= 0)
                        {
                            Socket.Send("compose_atm:error:Quantidade inválida!");
                            return;
                        }

                        if (WithdrawAmount > ActualAmount || ActualAmount - WithdrawAmount <= -1)
                        {
                            Socket.Send("compose_atm:error:Você não tem esse tipo de dinheiro para retirar!");
                            return;
                        }

                        int TaxAmount = Convert.ToInt32((double)WithdrawAmount * 0.05);

                        if (AccountType == "Checkings")
                        {
                            if (Client.GetRoleplay().BankAccount < 1)
                            {
                                Socket.Send("compose_atm:error:Você não possui uma conta de cheques!");
                                return;
                            }

                            RoleplayManager.Shout(Client, "*Pega R$" + String.Format("{0:N0}", WithdrawAmount) + " da minha conta de Cheques e coloca em meu bolso*", 5);

                            Client.GetRoleplay().BankChequings -= WithdrawAmount;

                            Client.GetHabbo().Credits += WithdrawAmount;
                            Client.GetHabbo().UpdateCreditsBalance();
                            Client.GetRoleplay().CooldownManager.CreateCooldown("withdraw", 1000, 5);

                            Socket.Send("compose_atm:change_balance_1:" + Client.GetRoleplay().BankChequings);
                        }
                        else
                        {
                            if (Client.GetRoleplay().BankAccount < 2)
                            {
                                Socket.Send("compose_atm:error:Você não tem uma conta poupança!");
                                return;
                            }

                            if (Client.GetRoleplay().ATMAmount.IndexOf(WithdrawAmount) != -1)
                            {
                                Socket.Send("compose_atm:error:Você já retirou R$" + WithdrawAmount + "!");
                                return;
                            }

                            RoleplayManager.Shout(Client, "*Retira R$" + String.Format("{0:N0}", WithdrawAmount) + " a minha conta Poupança e coloca em meu bolso*", 5);
                            Client.SendWhisper("Você pagou um imposto de R$" + String.Format("{0:N0}", TaxAmount) + " por retirar R$" + String.Format("{0:N0}", WithdrawAmount) + "!", 1);

                            Client.GetHabbo().Credits += (WithdrawAmount - TaxAmount);
                            Client.GetHabbo().UpdateCreditsBalance();

                            Client.GetRoleplay().BankSavings -= WithdrawAmount;

                            Client.GetRoleplay().ATMAmount.Add(WithdrawAmount);
                            Client.GetRoleplay().CooldownManager.CreateCooldown("withdraw", 1000, 5);

                            Socket.Send("compose_atm:change_balance_2:" + Client.GetRoleplay().BankSavings);
                        }
                    }
                    break;
                #endregion

                #region Deposit
                case "deposit":
				case "depositar":
                    {
                        string[] ReceivedData = Data.Split(',');

                        int Amount;

                        if (!int.TryParse(ReceivedData[1], out Amount))
                        {
                            Socket.Send("compose_atm:error:Digite um número válido!");
                            return;
                        }

                        int DepositAmount = Convert.ToInt32(ReceivedData[1]);
                        string AccountType = Convert.ToString(ReceivedData[2]);

                        int ActualAmount = Client.GetHabbo().Credits;

                        if (Client.GetRoleplay().TryGetCooldown("deposit"))
                            return;

                        if (DepositAmount <= 0)
                        {
                            Socket.Send("compose_atm:error:Quantidade inválida!");
                            return;
                        }

                        if (DepositAmount > ActualAmount || ActualAmount - DepositAmount <= -1)
                        {
                            Socket.Send("compose_atm:error:Você não possui esse tipo de dinheiro para depósitar!");
                            return;
                        }

                        if (AccountType == "Checkings")
                        {
                            if (Client.GetRoleplay().BankAccount < 1)
                            {
                                Socket.Send("compose_atm:error:Você não possui uma conta de cheques!");
                                return;
                            }

                            RoleplayManager.Shout(Client, "*Pega R$" + String.Format("{0:N0}", DepositAmount) + " da carteira e deposita em minha conta de Cheques*", 5);

                            Client.GetHabbo().Credits -= DepositAmount;
                            Client.GetHabbo().UpdateCreditsBalance();

                            Client.GetRoleplay().BankChequings += DepositAmount;

                            Socket.Send("compose_atm:change_balance_1:" + Client.GetRoleplay().BankChequings);
                        }
                        else
                        {

                            if (Client.GetRoleplay().BankAccount < 2)
                            {
                                Socket.Send("compose_atm:error:Você não tem uma conta poupança!");
                                return;
                            }

                            RoleplayManager.Shout(Client, "*Pega R$" + String.Format("{0:N0}", DepositAmount) + " da carteira e deposita em minha conta Poupança*", 5);

                            Client.GetHabbo().Credits -= DepositAmount;
                            Client.GetHabbo().UpdateCreditsBalance();

                            Client.GetRoleplay().BankSavings += DepositAmount;

                            Socket.Send("compose_atm:change_balance_2:" + Client.GetRoleplay().BankSavings);
                        }

                        Client.GetRoleplay().CooldownManager.CreateCooldown("deposit", 1000, 5);
                    }
                    break;
                    #endregion
            }
        }
    }
}
