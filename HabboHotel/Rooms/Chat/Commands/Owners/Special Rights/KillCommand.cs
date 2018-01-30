using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class KillCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_kill"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Mata o usuário."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 2)
            {
                Session.SendWhisper("Você deve digitar o nome de usuário que você deseja matar.", 1);
                return;
            }

            var TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Este usuário não foi encontrado! Talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null || TargetClient.GetRoomUser() == null)
            {
                Session.SendWhisper("Este usuário não foi encontrado! Talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetHabbo().VIPRank > 1)
            {
                Session.SendWhisper("Você não pode matar outros funcionários!", 1);
                return;
            }

            string Message = "*Mata imediatamente " + TargetClient.GetHabbo().Username + " com um relâmpago *";
            int Bubble = 23;

            if (Params[0].ToLower() == "explosao")
            {
                Message = "*Aperta os dedos, causando uma explosão em " + TargetClient.GetHabbo().Username + "*";
                Bubble = 24;
				
            }
            foreach (GameClient session in PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                if (session == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(session, "event_livefeedevent", Session.GetHabbo().Username);

            }

            Session.Shout(Message, Bubble);
            TargetClient.GetRoleplay().CurHealth = 0;
            Session.GetRoleplay().ClearWebSocketDialogue(true);
        }
    }
}
