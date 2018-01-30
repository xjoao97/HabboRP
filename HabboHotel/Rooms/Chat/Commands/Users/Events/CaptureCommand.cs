using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Events
{
    class CaptureCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_capture"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Começa a capturar a base da equipe de eventos."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetRoleplay().Game == null)
            {
                Session.SendWhisper("Você não está dentro de um evento para usar esse comando!", 1);
                return;
            }

            var Teams = Session.GetRoleplay().Game.GetTeams();

            if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("capturar"))
            {
                Session.SendWhisper("Você já está capturando uma base de equipes!", 1);
                return;
            }

            if (Room.Id == Session.GetRoleplay().Team.CaptureRoom)
            {
                Session.SendWhisper("Você não pode capturar sua própria base de equipes!", 1);
                return;
            }

            if (Teams == null)
                return;

            var Zones = Teams.Values.Select(x => x.CaptureRoom).ToList();

            if (!Zones.Contains(Room.Id))
            {
                Session.SendWhisper("Você não está dentro de uma base de equipes, ou essa equipe já foi capturada!", 1);
                return;
            }

            var Items = Room.GetGameMap().GetRoomItemForSquare(Session.GetRoomUser().Coordinate.X, Session.GetRoomUser().Coordinate.Y);

            if (Items.Count < 1)
				
            {
                Session.SendWhisper("Você deve estar parado no topo de um Tele Banzai para capturar uma base!", 1);
                return;
            }

            bool HasCaptureTile = Items.ToList().Where(x => x.GetBaseItem().ItemName == "bb_rnd_tele").ToList().Count() > 0;

            if (!HasCaptureTile)
            {
                Session.SendWhisper("Você deve estar parado no topo de um Tele Banzai para capturar uma base!", 1);
                return;
            }

            #endregion

            #region Execute
            var CaptureTeam = Teams.Values.FirstOrDefault(x => x.CaptureRoom == Room.Id);
            Session.Shout("*Começa a Capturar a Base do Time [" + CaptureTeam.Name + "]*", 4);
            Session.GetRoleplay().TimerManager.CreateTimer("capturar", 1000, false);

            if (Session.GetRoomUser().CurrentEffect != 59)
                Session.GetRoomUser().ApplyEffect(59);

            foreach (var Player in CaptureTeam.Members)
            {
                var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Player);

                if (Client == null)
                    continue;

                Client.SendWhisper("[Alerta de Evento] " + Session.GetHabbo().Username + " começou a capturar sua base de equipes!", 34);
            }
            #endregion
        }
    }
}