using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Polls.Enums;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Polls;
using System.Collections.Generic;

namespace Plus.HabboRoleplay.Timers.Types
{
    public class MatchingPollTimer : SystemRoleplayTimer
    {
        public MatchingPollTimer(string Type, int Time, bool Forever, object[] Params)
            : base(Type, Time, Forever, Params)
        {
            // X time seconds converted to milliseconds
            int PollSeconds = (int)Params[0];
            TimeLeft = PollSeconds * 1000;
            TimeCount = 0;
        }

        /// <summary>
        /// Blows up the dynamite
        /// </summary>
        public override void Execute()
        {
            try
            {
                GameClient Session = (GameClient)Params[1];
                Poll Poll = (Poll)Params[2];
                bool RoomOnly = (bool)Params[3];

                Room Room = Session.GetHabbo().CurrentRoom;
                List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();

                if (Poll == null || Room == null || Poll.Type != PollType.Matching)
                {
                    base.EndTimer();
                    return;
                }

                System.Threading.Thread.Sleep(3000);
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                {
                    if (RoomOnly)
                    {
                        lock (Users)
                        {
                            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                Habbo RoomUser = PlusEnvironment.GetHabboById(User.UserId);

                                if (RoomUser == null)
                                    continue;

                                if (RoomUser.AnsweredMatchingPoll)
                                    RoomUser.GetClient().SendMessage(new MatchingPollResultMessageComposer(Poll));
                            }
                        }
                        return;
                    }
                    else
                    {
                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                        {
                            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients)
                            {
                                if (client == null || client.GetHabbo() == null || client.GetRoomUser() == null)
                                    continue;

                                if (client.GetHabbo().AnsweredMatchingPoll)
                                    client.SendMessage(new MatchingPollResultMessageComposer(Poll));
                            }
                        }
                    }
                    return;
                }

                if (RoomOnly)
                {
                    lock (Users)
                    {
                        foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                        {
                            Habbo RoomUser = PlusEnvironment.GetHabboById(User.UserId);

                            if (RoomUser == null)
                                continue;

                            RoomUser.AnsweredMatchingPoll = false;
                        }
                    }
                    return;
                }
                else
                {
                    lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                    {
                        foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients)
                        {
                            if (client == null || client.GetHabbo() == null || client.GetRoomUser() == null)
                                continue;

                            client.GetHabbo().AnsweredMatchingPoll = false;
                        }
                    }
                }

                base.EndTimer();
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}
