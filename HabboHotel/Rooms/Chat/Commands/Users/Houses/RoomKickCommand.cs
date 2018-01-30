using System;
using System.Linq;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment
{
    class RoomKickCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_house_kick_room"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Expulsa e forneça uma mensagem aos usuários."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            House House;
            if (!Room.TryGetHouse(out House))
            {
                Session.SendWhisper("Você não está nem dentro de uma casa!", 1);
                return;
            }

            if (House.OwnerId != Session.GetHabbo().Id && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Você não é o dono da casa!", 1);
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, forneça um motivo expulsar os usuários da sala.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (RoomUser == null || RoomUser.IsBot || RoomUser.GetClient() == null || RoomUser.GetClient().GetHabbo() == null || RoomUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || RoomUser.GetClient().GetHabbo().Id == Session.GetHabbo().Id)
                    continue;

                RoomUser.GetClient().SendNotification("Você foi expulso da casa por: " + Message);

                RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(House.Sign.RoomId);
                RoleplayManager.SendUser(Session, roomData.Id);
            }

            Session.SendWhisper("Expulsou com sucesso a todos os usuários da sua casa.", 1);
        }
    }
}
