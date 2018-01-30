using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Fleck;
using Plus.Communication.Packets.Outgoing.Notifications;
using Newtonsoft.Json;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class WOnlineCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_wonline"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Usuários online com Websocket onlines"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder SocketAppend = new StringBuilder();
            int UserCount = 0;

            if (PlusEnvironment.GetGame().GetWebEventManager() == null)
            {
                Session.SendWhisper("Websocket é vazio por algum motivo", 1);
                return;
            }

            lock (PlusEnvironment.GetGame().GetWebEventManager()._webSockets)
            {

                Session.SendWhisper("Total de Usuários online: " +
                    PlusEnvironment.GetGame().GetWebEventManager()._webSockets.Count + "/" + PlusEnvironment.GetGame().GetClientManager().GetClients.Where(Client => Client != null && !Client.LoggingOut).ToList().Count);

                lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (Client == null)
                            continue;

                        if (Client.LoggingOut)
                            continue;

                        if (Client.GetHabbo() == null)
                            continue;

                        if (Client.GetRoleplay() == null)
                            continue;

                        if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client))
                            continue;

                        SocketAppend.Append("Usuário: " + Client.GetHabbo().Username + "\n");
                        SocketAppend.Append("Socket Ativo: Sim\n\n");
                        UserCount++;
                    }

                    foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {

                        if (Client == null)
                            continue;

                        if (Client.LoggingOut)
                            continue;

                        if (Client.GetHabbo() == null)
                            continue;

                        if (Client.GetRoleplay() == null)
                            continue;

                        if (Client.GetRoleplay().WebSocketConnection != null)
                            continue;

                        SocketAppend.Append("Usuário: " + Client.GetHabbo().Username + "\n");
                        SocketAppend.Append("Socket Ativo: Não\n\n");
                        UserCount++;
                    }
                }

                string SocketUsers = "===============================\n";
                SocketUsers += "Total de usuários conectados: " + PlusEnvironment.GetGame().GetWebEventManager()._webSockets.Count + "/" + UserCount + "\n";
                SocketUsers += "===============================\n\n";
                SocketUsers += SocketAppend;

                Session.SendMessage(new MOTDNotificationComposer(SocketUsers));
            }

            return;
        }
    }
}
