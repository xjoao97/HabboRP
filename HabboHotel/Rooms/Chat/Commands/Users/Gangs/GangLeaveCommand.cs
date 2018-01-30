using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangLeaveCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_leave"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite deixar seu grupo atual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Group Group = GroupManager.GetGang(Session.GetRoleplay().GangId);
            #endregion

            #region Conditions
            if (Group == null)
            {
                Session.SendWhisper("Você não tem uma gangue para sair!", 1);
                return;
            }

            if (Group.Id <= 1000)
            {
                Session.SendWhisper("Você não tem uma gangue para sair!", 1);
                return;
            }

            if (Group.CreatorId == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Você não pode simplesmente deixar seu grupo, você deve excluí-lo ou transferi-lo!", 1);
                return;
            }
            #endregion

            #region Execute
            Session.Shout("*Encerra sua gangue '" + Group.Name + "'*", 4);
            Session.GetRoleplay().GangId = 1000;
            Session.GetRoleplay().GangRank = 1;
            Session.GetRoleplay().GangRequest = 0;

            if (Group.RoomId == Room.Id && (Group.AdminOnlyDeco == 0 || Group.IsAdmin(Session.GetHabbo().Id)))
            {
                Session.GetRoomUser().RemoveStatus("flatctrl 1");
                Session.GetRoomUser().UpdateNeeded = true;
                Session.SendMessage(new YouAreControllerComposer(0));
            }

            Group NewGang = GroupManager.GetGang(1000);
            NewGang.AddNewMember(Session.GetHabbo().Id);
            NewGang.SendPackets(Session);

            foreach (int member in Group.Members.Keys)
            {
                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(member);

                if (Client == null)
                    continue;

                Client.SendWhisper("[GANGUE] " + Session.GetHabbo().Username + " acabou de sair do da Gangue!", 34);
            }
            #endregion
        }
    }
}