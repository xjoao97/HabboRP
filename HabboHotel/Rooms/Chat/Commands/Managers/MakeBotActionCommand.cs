using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Fleck;
using Plus.Communication.Packets.Outgoing.Notifications;
using Newtonsoft.Json;
using static Plus.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Bots.Manager.TimerHandlers;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class MakeBotActionCommand : IChatCommand
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
            get { return "Usuários online com Websocket trabalhando"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido! Use :botacao <bot> <ação>");
                return;
            }

            string BotName = Convert.ToString(Params[1]);
            string BotAction = Convert.ToString(Params[2]);

            RoomUser Bot = Room.GetRoomUserManager().GetBotByName(BotName);

            if (Bot == null)
            {
                Session.SendWhisper("Este bot é nulo", 1);
                return;
            }

            IBotHandler Handler = null;

            switch (BotAction.ToLower())
            {
                case "teleport":
                case "teleportar":
                    object[] Parameters = { Bot.GetBotRoleplay().GetRandomTeleport() };
                    Bot.GetBotRoleplay().StartHandler(Handlers.TELEPORT, out Handler, Parameters);              
                    break;
                default:
                    Session.SendWhisper("Ação inválida!", 1);
                    break;
            }

            return;
        }
    }
}
