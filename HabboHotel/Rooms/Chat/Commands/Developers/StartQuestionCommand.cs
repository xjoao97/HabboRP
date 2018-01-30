using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Polls.Enums;
using Plus.Communication.Packets.Outgoing.Polls;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class StartQuestionCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_start_question"; }
        }

        public string Parameters
        {
            get { return "%id%"; }
        }

        public string Description
        {
            get { return "Inicia uma pergunta com base no id."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 4)
            {
                Session.SendWhisper("Use o comando: ':ipergunta (pergunta) (tempo) (true = quarto/false =hotel)'", 1);
                return;
            }

            int id = 0;
            int TimeLeft = 0;
            bool RoomOnly = true;

            if (!int.TryParse(Params[1], out id))
            {
                Session.SendWhisper("Use o comando: ':ipergunta (pergunta) (tempo) (true = quarto/false =hotel)'", 1);
                return;
            }

            Poll poll = PlusEnvironment.GetGame().GetPollManager().TryGetPollById(id);

            if (poll == null || poll.Type != PollType.Matching)
            {
                Session.SendWhisper("A enquete não existe ou não é uma pesquisa correspondente.");
                return;
            }

            if (!int.TryParse(Params[2], out TimeLeft))
            {
                Session.SendWhisper("Use o comando: ':ipergunta (pergunta) (tempo) (true = quarto/false = hotel)'", 1);
                return;
            }
            else
            {
                if (TimeLeft < 10)
                {
                    Session.SendWhisper("Use um tempo de pelo menos 10 segundos!", 1);
                    return;
                }
            }

            if (!bool.TryParse(Params[3], out RoomOnly))
            {
                Session.SendWhisper("Use o comando: ':ipergunta (pergunta) (tempo) (true = quarto/false = hotel)'", 1);
                return;
            }

            poll.AnswersPositive = 0;
            poll.AnswersNegative = 0;

            MatchingPollAnswer(Session, poll, RoomOnly);

            object[] Objects = { TimeLeft, Session, poll, RoomOnly };
            RoleplayManager.TimerManager.CreateTimer("matchingpoll", 1000, true, Objects);
            return;
        }

        private static void MatchingPollAnswer(GameClients.GameClient Session, Poll poll, bool RoomOnly = true)
        {
            if (poll == null || poll.Type != PollType.Matching)
                return;

            if (RoomOnly)
                Session.GetHabbo().CurrentRoom.SendMessage(new MatchingPollMessageComposer(poll));
            else
            {
                lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    foreach (GameClients.GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null || client.GetRoomUser() == null)
                            continue;

                        client.SendMessage(new MatchingPollMessageComposer(poll));
                    }
                }
            }
        }
    }
}