using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Notifications;

using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class AboutCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_about"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Exibe informações do servidor."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            TimeSpan Uptime = DateTime.Now - PlusEnvironment.ServerStarted;
            TimeSpan SUptime = TimeSpan.FromMilliseconds(Environment.TickCount);
            int OnlineUsers = PlusEnvironment.GetGame().GetClientManager().Count;
            int PlayersOnline = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null).ToList().Count;
            int RoomCount = PlusEnvironment.GetGame().GetRoomManager().Count;

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(1053); // for server time

            Session.SendMessage(new RoomNotificationComposer("HabboRP Emulator",
                 "<b>Créditos</b>\n" +
                 "Carlos[Byxhp] (Roleplay)\n" +
                 "Adenir[Macker]\n" +
                 "Plus Emulador - Butterfly Edição Roleplay\n\n" +
                 "<b>Informações Atuais</b>:\n" +
                 "Horário: " + DateTime.Now + "\n" +
                 "Usuários Onlines: " + String.Format("{0:N0}", OnlineUsers) + (HabboRoleplay.Misc.RoleplayManager.AccurateUserCount ? " (Usuários reais onlines: " + PlayersOnline.ToString() + ")" : "") + "\n" +
                 "Empregos Carregados: " + String.Format("{0:N0}", RoomCount) + "\n" +
                 "Atividade do Emulador: " + Uptime.Days + " Dia(s), " + Uptime.Hours + " horas e " + Uptime.Minutes + " minutos.\n" +
                 "Atividade do Sistema: " + SUptime.Days + " Dia(s), " + SUptime.Hours + " horas e " + SUptime.Minutes + " minutos.\n\n" +
                 "<b>Revisão da SWF</b>:\n" + PlusEnvironment.SWFRevision, "plus", ""));

        }
    }
}