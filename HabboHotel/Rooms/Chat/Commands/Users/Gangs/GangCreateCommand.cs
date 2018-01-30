using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangCreateCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_create"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Abre a janela de criação de gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            var Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);

            if (Gang != null)
            {
                if (Gang.Id > 1000)
                {
                    if (Gang.CreatorId == Session.GetHabbo().Id)
                    {
                        Session.SendWhisper("Por favor, exclua sua gangue antes de tentar fazer uma nova!", 1);
                        return;
                    }
                }
            }

            List<RoomData> ValidRooms = new List<RoomData>();
            RoomData RoomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(2);

            ValidRooms.Add(RoomData);

            Session.SendMessage(new GroupCreationWindowComposer(ValidRooms));
        }
    }
}