using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class DeclineGroupMembershipEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();
            Group Group = null;
            GroupRank GroupRank = null;

            if (GroupId < 1000)
            {
                Group = GroupManager.GetJob(GroupId);
                GroupRank = GroupManager.GetJobRank(GroupId, 1);
            }
            else
            {
                Group = GroupManager.GetGang(GroupId);
                GroupRank = GroupManager.GetGangRank(GroupId, 1);
            }

            if (Group == null)
                return;

            bool IsAdmin = false;
            if (Group.IsAdmin(Session.GetHabbo().Id))
                IsAdmin = true;
            if (GroupId < 1000 && Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                IsAdmin = true;

            bool IsOwner = false;
            if (Group.CreatorId == Session.GetHabbo().Id)
                IsOwner = true;
            if (GroupId < 1000 && Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                IsOwner = true;

            if (!IsAdmin && !IsOwner)
                return;

            if (!Group.HasRequest(UserId))
                return;

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);

            if (Habbo == null)
            {
                Session.SendNotification("Opa, ocorreu um erro ao encontrar este usuário.");
                return;
            }

            Group.HandleRequest(UserId, false);

            Habbo = PlusEnvironment.GetHabboById(UserId);

            if (Habbo != null)
            {
                if (Group.Id < 1000)
                    Session.SendWhisper("Sucesso, você rejeitou " + Habbo.Username + " de entrar na sua empresa '" + Group.Name + "'!", 1);
                else
                    Session.SendWhisper("Sucesso, você rejeitou '" + Habbo.Username + "' de entrar na sua gangue '" + Group.Name + "'!", 1);
            }
            else
            {
                using (UserCache Member = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId))
                {

                    if (Group.Id < 1000)
                        Session.SendWhisper("Sucesso, você rejeitou " + Member.Username + " de entrar na sua empresa '" + Group.Name + "'!", 1);
                    else
                        Session.SendWhisper("Sucesso, você rejeitou '" + Member.Username + "' de entrar na sua gangue '" + Group.Name + "'!", 1);
                }
            }

            Session.SendMessage(new GroupInfoComposer(Group, Session));
            Session.SendMessage(new UnknownGroupComposer(Group.Id, UserId));

            if (Group.Id < 1000 && Habbo.GetClient() != null && Habbo.GetClient().GetRoomUser() != null)
            {
                if (Habbo.CurrentRoom != null && Habbo.CurrentRoom.TutorialEnabled)
                    Habbo.SendComposerToCorrectUsers(new UsersComposer(Habbo.GetClient().GetRoomUser()));
            }
        }
    }
}