using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangCaptureCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_capture"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Começa a capturar o território da gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            Turf Turf = TurfManager.GetTurf(Room.RoomId);
            bool InsideTurf = false;
            #endregion

            #region Conditions
            if (Turf == null)
            {
                Session.SendWhisper("A sala em que você se encontra não é um território de gangue!", 1);
                return;
            }

            if (Turf.GangId == Session.GetRoleplay().GangId)
            {
                Session.SendWhisper("Sua gangue já possui esse território!", 1);
                return;
            }

            if (Gang == null)
            {
                Session.SendWhisper("Você não faz parte de nenhuma gangue para capturar este território!", 1);
                return;
            }

            if (Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não faz parte de nenhuma gangue para capturar este território!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode capturar um território de gangue enquanto está morto", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode capturar um território de gangue enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoomUser().Frozen)
            {
                Session.SendWhisper("Você não pode capturar um território de gangue enquanto você está atordoado!", 1);
                return;
            }

            if (Session.GetRoleplay().GangId == Turf.GangId)
            {
                Session.SendWhisper("Sua gangue já possui esse território de gangue!", 1);
                return;
            }

            if (Turf.CaptureSquares.Where(x => x.X == Session.GetRoomUser().Coordinate.X && x.Y == Session.GetRoomUser().Coordinate.Y).ToList().Count > 0)
                InsideTurf = true;

            if (!InsideTurf)
            {
                Session.SendWhisper("Você não está dentro da zona de território de gangue!", 1);
                return;
            }

            if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("gcapturar") || Session.GetRoleplay().CapturingTurf != null)
            {
                Session.SendWhisper("Você já está capturando um território de gangue!", 1);
                return;
            }

            if (Session.GetRoleplay().DrivingCar)
            {
                Session.SendWhisper("Você não pode capturar um território enquanto dirige um veículo ", 1);
                return;
            }
            #endregion

            #region Execute
            Session.Shout("*Começa a capturar o território para a minha Gangue [" + Gang.Name + "]*", 4);
            Session.SendWhisper("Você tem 5 minutos restantes até capturar este território!", 1);

            Session.GetRoleplay().TimerManager.CreateTimer("gcapturar", 1000, false);
            Session.GetRoleplay().CapturingTurf = Turf;

            Group CurrentGang = GroupManager.GetGang(Turf.GangId);

            if (CurrentGang.Id > 1000)
            {
                lock (CurrentGang.Members.Values)
                {
                    foreach (GroupMember Member in CurrentGang.Members.Values)
                    {
                        GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member.UserId);

                        if (Client == null)
                            continue;

                        Client.SendWhisper("[GANGUE] Seu território de gangue no quarto " + Room.Name + " [ID: " + Room.Id + "] está sendo capturado!", 34);
                    }
                }
            }
            #endregion
        }
    }
}