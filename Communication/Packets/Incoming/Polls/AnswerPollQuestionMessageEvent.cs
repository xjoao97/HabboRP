using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Polls.Enums;
using Plus.Communication.Packets.Outgoing.Polls;
using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Incoming.Polls
{
    class AnswerPollQuestionMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int pollId = Packet.PopInt();
            int questionId = Packet.PopInt();
            int num3 = Packet.PopInt();
            string liststring = "";
            if (Packet.RemainingLength > 0)
                liststring = Packet.PopString();

            if (pollId == 500000)
            {
                HandleATMQuestion(Session, questionId, liststring);
                return;
            }

            List<string> list = new List<string>();

            for (int i = 0; i < num3; i++)
                list.Add(liststring);

            string text = string.Join("\r\n", list);

            if (text == "")
                text = "Não respondeu esta pergunta.";

            Poll poll = PlusEnvironment.GetGame().GetPollManager().TryGetPollById(pollId);

            if (poll == null)
            {
                Session.SendWhisper("Ups! Algo deu errado. Esta Enquete não pôde ser encontrada!", 1);
                return;
            }

            if (poll.Type == PollType.Matching)
            {
                if (text == "1")
                    poll.AnswersPositive++;
                else
                    poll.AnswersNegative++;

                Session.GetHabbo().AnsweredMatchingPoll = true;
                Session.SendMessage(new MatchingPollAnsweredMessageComposer(Session, text));
                return;
            }

            PollQuestion Question = PlusEnvironment.GetGame().GetPollManager().getPollQuestion(poll, questionId);

            if (Question != null)
            {
                if (text != "Não respondeu esta pergunta. " && (Question.AType == PollAnswerType.RadioSelection || Question.AType == PollAnswerType.RadioSelection))
                {
                    List<string> SelectionAnswers = new List<string>();

                    foreach (string answerid in list)
                    {
                        int index = 0;
                        
                        foreach (string answer in Question.Answers)
                        {
                            index++;

                            if (index != Convert.ToInt32(answerid))
                                continue;

                            SelectionAnswers.Add(answer);
                        }
                    }
                    text = string.Join(",", SelectionAnswers);
                }
            }

            if (questionId == poll.Questions.Count)
            {
                if (!Session.GetHabbo().AnsweredPolls.Contains(poll.Id))
                    Session.GetHabbo().AnsweredPolls.Add(poll.Id);
            }

            var NewQuestion = new PollQuestion(questionId, Question.Question, 1, text, "");
            var AnsweredQuestions = Session.GetRoleplay().AnsweredPollQuestions;
            if (AnsweredQuestions.ContainsKey(poll.Id))
            {
                if (!AnsweredQuestions[poll.Id].Contains(NewQuestion))
                    AnsweredQuestions[poll.Id].Add(NewQuestion);
            }
            else
            {
                List<PollQuestion> List = new List<PollQuestion>();
                List.Add(NewQuestion);
                AnsweredQuestions.TryAdd(poll.Id, List);
            }

            if (AnsweredQuestions[poll.Id].Count == poll.Questions.Count)
            {
                string Answers = "";

                var OrderedAnswers = AnsweredQuestions[poll.Id].OrderBy(x => x.Index).ToList();

                int count = 0;
                foreach (string Answer in OrderedAnswers.Select(x => x.Answers[0]))
                {
                    count++;
                    if (count < OrderedAnswers.Count)
                        Answers += Answer + "Ø";
                    else
                        Answers += Answer;
                }

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `user_polls` VALUES (@userid ,@pollid ,@answers)");
                    dbClient.AddParameter("userid", Session.GetHabbo().Id);
                    dbClient.AddParameter("pollid", poll.Id);
                    dbClient.AddParameter("answers", Answers);
                    dbClient.RunQuery();
                }
            }
        }

        public void HandleATMQuestion(HabboHotel.GameClients.GameClient Session, int questionId, string amount)
        {
            if (Session.GetRoleplay().BankAccount <= 0)
            {
                if (Session.GetRoomUser() != null)
                {
                    if (!Session.GetRoomUser().CanWalk)
                        Session.GetRoomUser().CanWalk = true;
                }
            }

            if (questionId == 1)
            {
                if (!Session.GetRoleplay().ATMFailed)
                {
                    if (amount == "1")
                        Session.GetRoleplay().ATMAccount = "Cheques";
                    else if (amount == "2")
                        Session.GetRoleplay().ATMAccount = "Poupanca";
                    else
                        Session.GetRoleplay().ATMFailed = true;
                }
            }
            else if (questionId == 2)
            {
                if (!Session.GetRoleplay().ATMFailed)
                {
                    if (amount == "1")
                        Session.GetRoleplay().ATMAction = "Retirar";
                    else if (amount == "2")
                        Session.GetRoleplay().ATMAction = "Depositar";
                    else
                        Session.GetRoleplay().ATMFailed = true;
                }
            }
            else
            {
                int Amount;
                if (int.TryParse(amount, out Amount))
                {
                    if (Amount <= 0)
                        Session.GetRoleplay().ATMFailed = true;

                    if (!Session.GetRoleplay().ATMFailed)
                    {
                        if (Session.GetRoleplay().ATMAccount.ToLower() == "cheques")
                        {
                            if (Session.GetRoleplay().ATMAction.ToLower() == "retirar")
                            {
                                if (Session.GetRoleplay().BankChequings < Amount)
                                    Session.GetRoleplay().ATMFailed = true;
                                else
                                {
                                    Session.GetRoleplay().BankChequings -= Amount;
                                    Session.GetHabbo().Credits += Amount;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    Session.Shout("*Usa o ATM para retirar R$" + Amount + " na minha conta de Cheques*", 5);
                                }
                            }
                            else if (Session.GetRoleplay().ATMAction.ToLower() == "depositar")
                            {
                                if (Amount > Session.GetHabbo().Credits)
                                    Session.GetRoleplay().ATMFailed = true;
                                else
                                {
                                    Session.GetRoleplay().BankChequings += Amount;
                                    Session.GetHabbo().Credits -= Amount;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    Session.Shout("*Usa o ATM para depositar R$" + Amount + " na mina conta de Cheques*", 5);
                                }
                            }
                            else
                                Session.GetRoleplay().ATMFailed = true;
                        }
                        else
                        {
                            if (Session.GetRoleplay().ATMAction.ToLower() == "retirar")
                            {
                                if (Amount > 20)
                                {
                                    if (Session.GetRoleplay().BankSavings < Amount)
                                        Session.GetRoleplay().ATMFailed = true;
                                    else
                                    {
                                        int TaxAmount = Convert.ToInt32((double)Amount * 0.05);

                                        Session.GetHabbo().Credits += (Amount - TaxAmount);
                                        Session.GetHabbo().UpdateCreditsBalance();

                                        Session.GetRoleplay().BankSavings -= Amount;
                                        Session.Shout("*Retira R$" + Amount + " da minha conta de Cheques e coloca no bolso*", 5);
                                        Session.SendWhisper("Você pagou uma taxa de R$" + TaxAmount + " por retirar " + Amount, 1);
                                    }
                                }
                                else
                                    Session.GetRoleplay().ATMFailed = true;
                            }
                            else if (Session.GetRoleplay().ATMAction.ToLower() == "depositar")
                            {
                                if (Amount > Session.GetHabbo().Credits)
                                    Session.GetRoleplay().ATMFailed = true;
                                else
                                {
                                    Session.GetRoleplay().BankSavings += Amount;
                                    Session.GetHabbo().Credits -= Amount;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    Session.Shout("*Usa o ATM para depositar R$" + Amount + " na minha conta Poupança*", 5);
                                }
                            }
                            else
                                Session.GetRoleplay().ATMFailed = true;
                        }
                    }
                    else
                        Session.GetRoleplay().ATMFailed = true;

                    if (Session.GetRoleplay().ATMFailed == true)
                        Session.SendWhisper("Error... ATM Falhou!", 1);
                }
                else
                    Session.SendWhisper("Error... ATM Falhou!", 1);

                Session.GetRoleplay().ATMAccount = "";
                Session.GetRoleplay().ATMAction = "";
                Session.GetRoleplay().ATMFailed = false;
            }
        }
    }
}
