using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Pathfinding;
using Plus.Database.Interfaces;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Turf capture timer
    /// </summary>
    public class TurfCaptureTimer : RoleplayTimer
    {
        public TurfCaptureTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 5 minutes convert to milliseconds
            TimeLeft = 5 * 60000;
        }

        /// <summary>
        /// Turf capture timer
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetRoleplay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoleplay().GangId == 1000)
                {
                    base.EndTimer();
                    return;
                }

                Group Gang = GroupManager.GetGang(base.Client.GetRoleplay().GangId);
                Group CurrentGang = GroupManager.GetGang(base.Client.GetRoleplay().CapturingTurf.GangId);
                HabboHotel.Rooms.Room Room = base.Client.GetHabbo().CurrentRoom;

                if (Gang == null)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                bool InsideTurf = false;

                if (base.Client.GetRoleplay().CapturingTurf == null)
                {
                    base.EndTimer();
                    return;
                }
                else
                {
                    foreach (ThreeDCoord Coord in base.Client.GetRoleplay().CapturingTurf.CaptureSquares)
                    {
                        if (Client.GetRoomUser() == null)
                            break;

                        if (Coord.X == base.Client.GetRoomUser().X && Coord.Y == base.Client.GetRoomUser().Y)
                            InsideTurf = true;
                    }
                }

                if (!InsideTurf)
                {
                    base.Client.GetRoleplay().CapturingTurf = null;
                    RoleplayManager.Shout(base.Client, "*Para a captura do território de gangue *", 4);
                    base.EndTimer();
                    return;
                }

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        RoleplayManager.Shout(base.Client, "*Chega mais perto de capturar o território [" + (TimeLeft / 60000) + " minutos restantes]*", 4);
                        TimeCount = 0;

                        if (CurrentGang.Id > 1000)
                        {
                            lock (CurrentGang.Members.Values)
                            {
                                foreach (GroupMember Member in CurrentGang.Members.Values)
                                {
                                    GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member.UserId);

                                    if (Client == null)
                                        continue;

                                    Client.SendWhisper("[GANGUE] Seu território de gangue no quarto " + Room.Name + " [Quarto ID: " + Room.Id + "] está sendo capturado!", 34);
                                }
                            }
                        }
                    }
                    return;
                }

                Gang.GangScore += new Utilities.CryptoRandom().Next(5, 25);

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `rp_gangs` SET `gang_kills` = @gangkills, `gang_score` = @gangscore WHERE `id` = @id");
                    dbClient.AddParameter("gangkills", Gang.GangKills);
                    dbClient.AddParameter("gangscore", Gang.GangScore);
                    dbClient.AddParameter("id", Gang.Id);
                    dbClient.RunQuery();
                }

                RoleplayManager.Shout(base.Client, "*Captura com sucesso o território em nome da minha gangue " + Gang.Name + "*", 4);

                if (CurrentGang.Id > 1000)
                {
                    lock (CurrentGang.Members.Values)
                    {
                        foreach (GroupMember Member in CurrentGang.Members.Values)
                        {
                            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member.UserId);

                            if (Client == null)
                                continue;

                            Client.SendWhisper("[GANGUE] Sua gangue dominou o " + Room.Name + " [ID: " + Room.Id + "]!", 34);
                        }
                    }
                }

                base.Client.GetRoleplay().CapturingTurf.UpdateTurf(Gang.Id);
                base.Client.GetRoleplay().CapturingTurf = null;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}