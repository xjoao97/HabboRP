using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class DeleteGroupEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();

            if (GroupId <= 1000)
            {
                Session.SendNotification("Você não pode excluir uma Empresa!");
                return;
            }

            Group Group = GroupManager.GetGang(GroupId);
            if (Group == null)
            {
                Session.SendNotification("Opa, não conseguimos encontrar esta gangue!");
                return;
            }

            if (Group.CreatorId != Session.GetHabbo().Id && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))//Maybe a FUSE check for staff override?
            {
                Session.SendNotification("Opa, apenas o proprietário pode excluir a empresa!");
                return;
            }

            if (Group.Members.Count >= PlusStaticGameSettings.GroupMemberDeletionLimit)
            {
                Session.SendNotification("Opa, sua empresa excedeu o valor máximo de empregados (" + PlusStaticGameSettings.GroupMemberDeletionLimit + "). Procure assistência de um membro da equipe.");
                return;
            }

            Room Room = RoleplayManager.GenerateRoom(Group.RoomId);

            if (Room != null)
            {
                Room.Group = null;
                Room.RoomData.Group = null;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rp_stats` SET `gang_id` = '1000', `gang_rank` = '1', `gang_request` = '0' WHERE `gang_id` = '" + Group.Id + "'");
                dbClient.RunQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + Group.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `rp_gangs` WHERE `id` = '" + Group.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + Group.Id + "'");
            }

            foreach (int Member in Group.Members.Keys)
            {
                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);
                if (Client != null)
                {
                    Client.GetRoleplay().GangId = 1000;
                    Client.GetRoleplay().GangRank = 1;
                    Client.GetRoleplay().GangRequest = 0;

                    Group NoGang = GroupManager.GetGang(1000);
                    NoGang.AddNewMember(Client.GetHabbo().Id);

                    UserCache Junk = null;
                    PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Member, out Junk);
                    PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Member);
                    NoGang.SendPackets(Client);
                }
            }

            foreach (var turf in TurfManager.TurfList.Values)
            {
                if (turf.GangId != Group.Id)
                    continue;

                turf.UpdateTurf(1000);
            }

            PlusEnvironment.GetGame().GetGroupManager().DeleteGroup(Group.Id);
            Session.SendNotification("Você deletou com sucesso o seu grupo!");
            return;
        }
    }
}
