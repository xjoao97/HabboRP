using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class CheckPollCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_check_poll"; }
        }

        public string Parameters
        {
            get { return "%username% %poll% or %poll%"; }
        }

        public string Description
        {
            get { return "Verifica as respostas da pesquisa dos usuários."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder();

            if (Params.Length == 1 || Params.Length > 3)
            {
                Session.SendWhisper("Use o comando como: ':checarpergunta (usuário) (pergunta)' ou ':checharpergunta (pergunta)'.", 1);
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Params.Length == 2)
                {
                    int PollId = 0;

                    if (!int.TryParse(Params[1], out PollId))
                    {
                        Session.SendWhisper("Use o comando como: ':checarpergunta (usuário) (id)' ou ':checharpergunta (id)'.", 1);
                        return;
                    }

                    var Poll = PlusEnvironment.GetGame().GetPollManager().TryGetPollById(PollId);

                    if (Poll == null)
                    {
                        Session.SendWhisper("A ID da Enquete que você inseriu não corresponde a nenhuma pesquisa!", 1);
                        return;
                    }

                    Message.Append("<----- [" + Poll.Id + "] " + Poll.PollName + " ----->\n\n");
                    Message.Append("Título da Pergunta: " + Poll.PollInvitation + "\n");
                    Message.Append("Consiste em: " + Poll.Questions.Count + " perguntas totais\n\n");
                    Message.Append("Os seguintes usuários responderam a esta pesquisa:\n");

                    dbClient.SetQuery("SELECT * FROM `user_polls` WHERE `poll_id` = '" + PollId + "'");
                    DataTable Table = dbClient.getTable();

                    if (Table == null)
                    {
                        Session.SendMessage(new MOTDNotificationComposer("Ninguém respondeu esta enquete ainda!"));
                        return;
                    }
                    else
                    {
                        List<string> UserNames = new List<string>();

                        foreach (DataRow Row in Table.Rows)
                        {
                            int UserId = Convert.ToInt32(Row["user_id"]);

                            var User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId);

                            if (!UserNames.Contains(User.Username))
                                UserNames.Add(User.Username);
                        }

                        foreach (string user in UserNames)
                            Message.Append(user + "\n");

                        Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
                    }

                }
                else
                {
                    Habbo Target = PlusEnvironment.GetHabboByUsername(Params[1]);

                    if (Target == null)
                    {
                        Session.SendWhisper("Desculpe, mas esse usuário não existe!", 1);
                        return;
                    }

                    int PollId = 0;

                    if (!int.TryParse(Params[2], out PollId))
                    {
                        Session.SendWhisper("Use o comando como: ':checarpergunta (usuário) (id)' ou ':checharpergunta (id)'.", 1);
                        return;
                    }

                    var Poll = PlusEnvironment.GetGame().GetPollManager().TryGetPollById(PollId);

                    if (Poll == null)
                    {
                        Session.SendWhisper("A ID da Enquete que você inseriu não corresponde a nenhuma pesquisa!", 1);
                        return;
                    }

                    dbClient.SetQuery("SELECT * FROM `user_polls` where `user_id` = '" + Target.Id + "' AND `poll_id` = '" + Poll.Id + "' AND `accepted` = '1'");
                    DataTable Table = dbClient.getTable();

                    Dictionary<int, string> AnswerList = new Dictionary<int, string>();

                    foreach (DataRow row in Table.Rows)
                    {
                        int QuestionId = Convert.ToInt32(row["question_id"]);
                        string Answer = row["answer"].ToString();

                        if (!AnswerList.ContainsKey(QuestionId))
                            AnswerList.Add(QuestionId, Answer);
                    }

                    if (Table == null)
                    {
                        Session.SendWhisper("Este usuário ainda não respondeu esta enquete!", 1);
                        return;
                    }

                    Message.Append(">----- [" + Poll.Id + "] " + Poll.PollName + " ----->\n\n");
                    Message.Append("Título da Pergunta: " + Poll.PollInvitation + "\n");
                    Message.Append("Consiste em: " + Poll.Questions.Count + " perguntas totais\n\n");

                    foreach (var question in Poll.Questions)
                    {
                        Message.Append("Número da pergunta: " + question.Index + "\n");
                        Message.Append("Pergunta: " + question.Question + "\n");

                        if (question.AType == Polls.Enums.PollAnswerType.Text)
                        {
                            Message.Append("Answer Type: Text\n");
                            if (AnswerList.ContainsKey(question.Index))
                                Message.Append("User Answered with: " + AnswerList[question.Index] + "\n\n");
                            else
                                Message.Append("User did NOT answer this question!\n\n");
                        }
                        else
                        {
                            Message.Append("Tipo de resposta: seleção\n");
                            Message.Append("Possível resposta: " + string.Join(",", question.Answers.ToList()) + "\n");
                            Message.Append("Resposta correta(s): " + string.Join(",", question.CorrectAnswers.ToList()) + "\n");
                            if (AnswerList.ContainsKey(question.Index))
                                Message.Append("Usuário respondeu: " + AnswerList[question.Index] + "\n\n");
                            else
                                Message.Append("O usuário NÃO respondeu esta pergunta!\n\n");
                        }
                    }
                    Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
                }
            }
        }
    }
}
