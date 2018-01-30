using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class AdminTaxiCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_taxi"; }
        }

        public string Parameters
        {
            get { return "%quarto_id%"; }
        }

        public string Description
        {
            get { return "Instantaneamente leva você para o ID do quarto desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            UInt32 RoomId = 0;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um ID do quarto!", 1);
                return;
            }

            if (!UInt32.TryParse(Params[1].ToString(), out RoomId))
            {
                Session.SendWhisper("Por favor insira um número válido.", 1);
                return;
            }

            Room TargetRoom = RoleplayManager.GenerateRoom(Convert.ToInt32(RoomId));

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode taxi enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode usar táxi enquanto está preso!!", 1);
                return;
            }

            if (RoomId == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Você já está nesta sala!", 1);
                return;
            }

            if (TargetRoom == null)
            {
                Session.SendWhisper("[RPG TAXI] Desculpe, não conseguimos encontrar esse quarto!", 1);
                return;
            }

            if (Session.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode usar taxi enquanto está dentro de um evento!", 1);
                return;
            }

            if (!Session.GetHabbo().Username.Contains("Ying"))
                Session.Shout("*Pega o Taxi Administrativo e vai imediatamente para o local*", 23);


            RoleplayManager.SendUser(Session, (int)RoomId);
        }
    }
}