using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboHotel.Cache;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class GiveAdminRightsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();

            if (GroupId >= 1000)
                return;

            Group Group = GroupManager.GetJob(GroupId);

            if (Group == null)
                return;

            if (!Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager") && (Session.GetHabbo().Id != Group.CreatorId || !Group.IsMember(UserId)))
                return;

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendNotification("Opa, ocorreu um erro ao encontrar este usuário.");
                return;
            }

            Group.MakeAdmin(UserId);
            GroupRank Rank = GroupManager.GetJobRank(Group.Id, 6);

            Session.Shout("*Promove " + Habbo.Username + " todos os cargos da Empresa " + Group.Name + " para o cargo " + Rank.Name + "*", 23);

            #region (Disabled) Room Rights for Admin
            /*
            Room Room = null;
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Group.RoomId, out Room))
            {
                RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(UserId);
                if (User != null)
                {
                    if (!User.Statusses.ContainsKey("flatctrl 3"))
                        User.AddStatus("flatctrl 3", "");

                    User.UpdateNeeded = true;
                    if (User.GetClient() != null)
                        User.GetClient().SendMessage(new YouAreControllerComposer(3));
                }
            }*/
            #endregion

            Session.SendMessage(new GroupMemberUpdatedComposer(GroupId, Habbo, 1));
        }
    }
}