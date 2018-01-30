using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police
{
    class SurrenderCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_related_surrender"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Entrega você às autoridades se você estiver na lista de procurados."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null || Session.GetRoomUser() == null)
                return;

            if (!Session.GetRoleplay().IsWanted || !RoleplayManager.WantedList.ContainsKey(Session.GetHabbo().Id))
            {
                Session.SendWhisper("Você não está sendo procurado!", 1);
                return;
            }

            if (Params.Length < 2)
            {
                Session.SendWhisper("Para confirmar que deseja se entregar, Digite :serender sim");
                return;
            }

            if (Params[1].ToString().ToLower() == "sim")
            {
                int JailRID = Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid"));

                Session.Shout("*Se entrega às autoridades da lei e é escoltado para a prisão*", 4);

                if (Session.GetHabbo().CurrentRoomId != JailRID)
                    RoleplayManager.SendUser(Session, JailRID, "Você se entregou às autoridades da lei e foi preso por " + Session.GetRoleplay().WantedLevel * 5 + " minutos!");

                Wanted Junk;
                RoleplayManager.WantedList.TryRemove(Session.GetHabbo().Id, out Junk);

                PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] " + Session.GetHabbo().Username + " se rendeu às autoridades da lei!");

                if (Session.GetRoleplay().EquippedWeapon != null)
                    Session.GetRoleplay().EquippedWeapon = null;

                Session.GetRoleplay().IsJailed = true;
                Session.GetRoleplay().JailedTimeLeft = Session.GetRoleplay().WantedLevel * 5;

                Session.GetRoleplay().TimerManager.CreateTimer("preso", 1000, false);
                return;
            }
            else
            {
                Session.SendWhisper("Tem certeza de que deseja se render? Você será preso por " + Session.GetRoleplay().WantedLevel * 5 + " minutos!", 1);
                Session.SendWhisper("Digite :serender sim se realmente deseja se entregar.", 1);
                return;
            }
        }
    }
}