using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupInfoComposer : ServerPacket
    {
        public GroupInfoComposer(Group Group, GameClient Session, bool NewWindow = false)
            : base(ServerPacketHeader.GroupInfoMessageComposer)
        {
            DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Group.CreateTime);

            bool IsAdmin = false;
            if (Group.IsAdmin(Session.GetHabbo().Id))
                IsAdmin = true;
            if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                IsAdmin = true;

            bool IsOwner = false;
            if (Group.CreatorId == Session.GetHabbo().Id)
                IsOwner = true;
            if (Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                IsOwner = true;

            base.WriteInteger(Group.Id);
            base.WriteBoolean(true);
            base.WriteInteger(Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2);
            base.WriteString(Group.Name);
            base.WriteString(Group.Description);
            base.WriteString(Group.Badge);
            base.WriteInteger(0); //Group.RoomId
            base.WriteString((PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId) == null) ? "Nenhum quarto encontrado.." : PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId).Name);    // room name
            base.WriteInteger(Group.CreatorId == Session.GetHabbo().Id ? 3 : Group.HasRequest(Session.GetHabbo().Id) ? 2 : Group.IsMember(Session.GetHabbo().Id) ? 1 : 0);
            base.WriteInteger(Group.Id < 1000 ? (Group.Members.Count + 1) : Group.Members.Count); // Members
            base.WriteBoolean(false);//?? CHANGED
            base.WriteString(Origin.Day + "-" + Origin.Month + "-" + Origin.Year);
            base.WriteBoolean(IsOwner); // Owner Check
            base.WriteBoolean(IsAdmin); // Admin Check
            base.WriteString(PlusEnvironment.GetUsernameById(Group.CreatorId));
            base.WriteBoolean(NewWindow); // Show group info
            base.WriteBoolean(Group.AdminOnlyDeco == 0); // Any user can place furni in home room
            base.WriteInteger((IsOwner || IsAdmin) ? Group.Requests.Count : 0); // Pending users
            //base.WriteInteger(0);//what the fuck
            base.WriteBoolean(Group != null ? Group.ForumEnabled : true);//HabboTalk.
        }
    }
}