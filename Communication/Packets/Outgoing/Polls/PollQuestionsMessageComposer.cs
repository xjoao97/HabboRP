using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Polls.Enums;

namespace Plus.Communication.Packets.Outgoing.Polls
{
    class PollQuestionsMessageComposer : ServerPacket
    {
        public PollQuestionsMessageComposer(HabboHotel.GameClients.GameClient Session, Poll poll)
            : base(ServerPacketHeader.PollQuestionsMessageComposer)
        {
            if (poll.Id == 500000)
            {
                ATMPoll(Session, poll);
                return;
            }

            base.WriteInteger(poll.Id);
            base.WriteString(poll.PollName);
            base.WriteString(poll.Thanks);
            base.WriteInteger(poll.Questions.Count);

            foreach (PollQuestion current in poll.Questions)
            {
                int QuestionNumber = checked(poll.Questions.IndexOf(current) + 1);

                base.WriteInteger(current.Index);
                base.WriteInteger(QuestionNumber);
                base.WriteInteger((int)current.AType);
                base.WriteString(current.Question);

                if (current.AType == PollAnswerType.Selection || current.AType == PollAnswerType.RadioSelection)
                {
                    base.WriteInteger(0);
                    base.WriteInteger(1);
                    base.WriteInteger(current.Answers.ToList().Count);
                    
                    int index = 0;
                    foreach (string current2 in current.Answers)
                    {
                        index++;
                        base.WriteString(index.ToString());
                        base.WriteString(current2);
                        base.WriteInteger(0);
                    }
                    base.WriteInteger(0);
                }
                else
                {
                    base.WriteInteger(0);
                    base.WriteInteger(3);
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                }
            }
            base.WriteBoolean(false);
        }

        public void ATMPoll(HabboHotel.GameClients.GameClient Session, Poll poll)
        {
            base.WriteInteger(500000);
            base.WriteString(poll.PollName);
            base.WriteString(poll.Thanks);

            if (Session.GetRoleplay().BankAccount <= 0)
                HasNoBankAccount(Session);
            else if (Session.GetRoleplay().BankAccount == 1)
                OnlyHasChequings(Session);
            else
                HasChequingsAndSavings(Session);
        }

        public void HasNoBankAccount(HabboHotel.GameClients.GameClient Session)
        {
            base.WriteInteger(1);

            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteInteger(1); 
            base.WriteString("Desculpe, você não tem uma conta bancária! Visite o banco para obter uma!");

            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteInteger(1);

            base.WriteString("1");
            base.WriteString("Erro, nenhuma conta bancária detectada.");
            base.WriteInteger(0);

            base.WriteInteger(0);

            base.WriteBoolean(false);
        }

        public void OnlyHasChequings(HabboHotel.GameClients.GameClient Session)
        {
            // Question Count
            base.WriteInteger(3);

            #region Question 1
            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteString("Que conta você gostaria de usar?");

            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteInteger(1);

            base.WriteString("1");
            base.WriteString("Cheques");
            base.WriteInteger(0);

            base.WriteInteger(0);
            #endregion

            #region Question 2
            base.WriteInteger(2);
            base.WriteInteger(2);
            base.WriteInteger(1);
            base.WriteString("Seu saldo é R$" + Session.GetRoleplay().BankChequings + ". O que você gostaria de fazer?");

            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteInteger(2);

            base.WriteString("1");
            base.WriteString("Retirar");
            base.WriteInteger(0);

            base.WriteString("2");
            base.WriteString("Depositar");
            base.WriteInteger(0);

            base.WriteInteger(0);
            #endregion

            #region Question 3
            base.WriteInteger(3);
            base.WriteInteger(3);
            base.WriteInteger(3);
            base.WriteString("Digite o valor que deseja depositar ou retirar.");

            base.WriteInteger(0);
            base.WriteInteger(3);
            base.WriteInteger(0);
            base.WriteInteger(0);
            #endregion

            base.WriteBoolean(false);
        }

        public void HasChequingsAndSavings(HabboHotel.GameClients.GameClient Session)
        {
            // Question Count
            base.WriteInteger(3);

            #region Question 1
            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteString("Que conta você gostaria de usar?");

            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteInteger(2);

            base.WriteString("1");
            base.WriteString("Cheques");
            base.WriteInteger(0);

            base.WriteString("2");
            base.WriteString("Poupança");
            base.WriteInteger(0);

            base.WriteInteger(0);
            #endregion

            #region Question 2
            base.WriteInteger(2);
            base.WriteInteger(2);
            base.WriteInteger(1);
            base.WriteString("Seu saldo na conta de Cheques é de R$" + Session.GetRoleplay().BankChequings + " e sua Poupança tem R$" + Session.GetRoleplay().BankSavings + ". O que você gostaria de fazer?");

            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteInteger(2);

            base.WriteString("1");
            base.WriteString("Retirar");
            base.WriteInteger(0);

            base.WriteString("2");
            base.WriteString("Depositar");
            base.WriteInteger(0);

            base.WriteInteger(0);
            #endregion

            #region Question 3
            base.WriteInteger(3);
            base.WriteInteger(3);
            base.WriteInteger(3);
            base.WriteString("Digite o valor que deseja depositar ou retirar.");

            base.WriteInteger(0);
            base.WriteInteger(3);
            base.WriteInteger(0);
            base.WriteInteger(0);
            #endregion

            base.WriteBoolean(false);
        }
    }
}
