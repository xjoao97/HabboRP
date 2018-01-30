using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment
{
    class KickCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_house_kick"; }
        }

        public string Parameters
        {
            get { return "%usuário% %razão%"; }
        }

        public string Description
        {
            get { return "Expulsa um usuário de uma sala e envia um motivo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            House House;
            if (!Room.TryGetHouse(out House))
            {
                Session.SendWhisper("Você não está dentro de uma casa!", 1);
                return;
            }

            if (House.OwnerId != Session.GetHabbo().Id && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Você não é o dono da casa!", 1);
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome do usuário que deseja expulsar da sala.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetRoomUser() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (TargetClient.GetRoomUser().RoomId != Room.RoomId)
            {
                Session.SendWhisper("Este usuário não está em sua casa!", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode se expulsar da casa!", 1);
                return;
            }

            Room TargetRoom;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(TargetClient.GetHabbo().CurrentRoomId, out TargetRoom))
                return;

            if (Params.Length > 2)
                TargetClient.SendNotification("Você foi expulso da casa pelo seguinte motivo: " + CommandManager.MergeParams(Params, 2));
            else
                TargetClient.SendNotification("Você foi expulso da casa.");

            if (TargetClient.GetRoomUser() != null)
            {
                if (TargetClient.GetRoomUser().RoomId == House.Sign.RoomId)
                   return;
            }

            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(House.Sign.RoomId);
            RoleplayManager.SendUser(TargetClient, roomData.Id);
        }
    }
}
