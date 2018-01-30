using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Pathfinding;
using Plus.Database.Interfaces;
using System.Linq;
using Plus.Core;
using Plus.HabboRoleplay.Games;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Event capture timer
    /// </summary>
    public class EventCaptureTimer : RoleplayTimer
    {
        public EventCaptureTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
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
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetRoleplay() == null || base.Client.GetRoomUser() == null)
                {
                    base.EndTimer();
                    return;
                }

                IGame Game = base.Client.GetRoleplay().Game;
                RoleplayTeam Team = base.Client.GetRoleplay().Team;

                if (Game == null || Team == null)
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                        base.Client.GetRoomUser().ApplyEffect(0);

                    base.EndTimer();
                    return;
                }

                Room room = RoleplayManager.GenerateRoom(base.Client.GetRoomUser().RoomId);

                if (room == null)
                    return;

                var Items = room.GetGameMap().GetRoomItemForSquare(base.Client.GetRoomUser().Coordinate.X, base.Client.GetRoomUser().Coordinate.Y);

                if (Items.Count < 1)
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    RoleplayManager.Shout(base.Client, "*Para de capturar a base*", 1);
                    base.EndTimer();
                    return;
                }

                bool HasCaptureTile = Items.ToList().Where(x => x.GetBaseItem().ItemName == "bb_rnd_tele").ToList().Count() > 0;

                if (!HasCaptureTile)
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    RoleplayManager.Shout(base.Client, "*Para de capturar a base*", 1);
                    base.EndTimer();
                    return;
                }

                var Teams = base.Client.GetRoleplay().Game.GetTeams();

                if (Teams == null)
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    base.EndTimer();
                    return;
                }

                var Zones = Teams.Values.Select(x => x.CaptureRoom).ToList();

                if (!Zones.Contains(room.Id))
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    RoleplayManager.Shout(base.Client, "*Para de capturar a base*", 1);
                    base.EndTimer();
                    return;
                }

                RoleplayTeam CaptureTeam = Teams.Values.FirstOrDefault(x => x.CaptureRoom == room.Id);

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        RoleplayManager.Shout(base.Client, "*Você está quase capturando a base! [" + (TimeLeft / 60000) + " minutos restantes]*", 4);
                        TimeCount = 0;

                        lock (CaptureTeam.Members)
                        {
                            foreach (int Player in CaptureTeam.Members)
                            {
                                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Player);

                                if (Client == null)
                                    continue;

                                Client.SendWhisper("[Alerta de EVENTO] " + base.Client.GetHabbo().Username + " ainda está capturando a base de sua equipe!", 34);
                            }
                        }
                    }
                    return;
                }

                RoleplayManager.Shout(base.Client, "*Capturou com sucesso a base da [Equipe " + CaptureTeam.Name + "]*", 4);
                base.Client.GetRoleplay().Game.NotifyPlayers("A [Equipe " + CaptureTeam.Name + "] acaba de ser removida do jogo!");
                base.Client.GetRoleplay().Game.RemoveTeamMembers(CaptureTeam);

                if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                lock (Team.Members)
                {
                    foreach (var Member in Team.Members)
                    {
                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);
                        if (Client != null && Client.GetHabbo() != null)
                        {
                            Client.GetHabbo().EventPoints += 3;
                            Client.GetHabbo().UpdateEventPointsBalance();
                            Client.SendWhisper("Você recebeu 3 Pontos de Evento por seu time capturar uma base!", 1);
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