using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.Communication.Packets.Incoming.Users
{
    class ChangeNameEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            string NewName = Packet.PopString();
            string OldName = Session.GetHabbo().Username;

            if (NewName == OldName)
            {
                Session.GetHabbo().ChangeName(OldName);
                Session.SendMessage(new UpdateUsernameComposer(NewName));
                return;
            }

            if (!CanChangeName(Session.GetHabbo()))
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Ops, parece que você atualmente não pode alterar seu nome de usuário!");
                return;
            }

            if (!Session.GetRoleplay().FreeNameChange)
            {
                if (Session.GetHabbo().Diamonds < 1)
                {
                    Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                    Session.SendNotification("Você não tem diamantes suficientes para uma mudança de nome!");
                    return;
                }
            }

            bool InUse = false;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", NewName);
                InUse = dbClient.getInteger() == 1;
            }

            char[] Letters = NewName.ToLower().ToCharArray();
            string AllowedCharacters = "abcdefghijklmnopqrstuvwxyz-";

            foreach (char Chr in Letters)
            {
                if (!AllowedCharacters.Contains(Chr))
                {
                    return;
                }
            }

            List<string> BlacklistedWords = new List<string>();
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `cms_blacklisted_words`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        string Word = Row["word"].ToString();

                        if (Word.Length > 0 && Word.ToLower() != "")
                        {
                            if (!BlacklistedWords.Contains(Word.ToLower()))
                                BlacklistedWords.Add(Word.ToLower());
                        }
                    }
                }
            }

            if (NewName.ToLower().Contains("mod") || NewName.ToLower().Contains("adm") || NewName.ToLower().Contains("admin") || NewName.ToLower().Contains("m0d"))
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Você não pode fazer deste seu nome! Por favor digite ':mudarnick' novamente!");
                return;
            }
            else if (!NewName.Contains('-'))
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Você não pode fazer deste seu nome! Por favor digite ':mudarnick' novamente!");
                return;
            }
            else if (NewName.Split('-')[0].Length < 3 || NewName.Split('-')[1].Length < 1)
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Você não pode fazer deste seu nome! Por favor digite ':mudarnick' novamente!");
                return;
            }
            else if (NewName.Length > 15)
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Você não pode fazer deste seu nome! Por favor digite ':mudarnick' novamente!");
                return;
            }
            else if (NewName.Length < 3)
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Você não pode fazer deste seu nome! Por favor digite ':mudarnick' novamente!");
                return;
            }
            else if (InUse)
            {
                Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                Session.SendNotification("Você não pode fazer deste seu nome! Por favor digite ':mudarnick' novamente!");
                return;
            }
            else
            {
                string FirstName = NewName.Split('-')[0].ToLower();
                string SecondName = NewName.Split('-')[1].ToLower();

                if (BlacklistedWords.Contains(FirstName) || BlacklistedWords.Contains(SecondName))
                {
                    if (BlacklistedWords.Contains(FirstName))
                        Session.SendNotification("Desculpe, mas esse primeiro nome não é permitido");
                    else
                        Session.SendNotification("Desculpe, mas esse segundo nome não é permitido!");
                    Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                    return;
                }

                if (!PlusEnvironment.GetGame().GetClientManager().UpdateClientUsername(Session, OldName, NewName))
                {
                    Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                    Session.SendNotification("Opa! Ocorreu um problema ao atualizar seu nome de usuário. Por favor digite ':mudarnick' novamente!");
                    return;
                }

                Session.GetHabbo().ChangingName = false;
                Room.GetRoomUserManager().RemoveUserFromRoom(Session, true, false);

                Session.GetHabbo().ChangeName(NewName);
                Session.GetHabbo().GetMessenger().OnStatusChanged(true);

                if (!Session.GetRoleplay().FreeNameChange)
                {
                    Session.GetHabbo().Diamonds--;
                    Session.SendMessage(new ActivityPointsComposer(Session.GetHabbo().Duckets, Session.GetHabbo().Diamonds, Session.GetHabbo().EventPoints));
                }

                Session.SendMessage(new UpdateUsernameComposer(NewName));
                Room.SendMessage(new UserNameChangeComposer(Room.Id, User.VirtualId, NewName));

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `logs_client_namechange` (`user_id`,`new_name`,`old_name`,`timestamp`) VALUES ('" + Session.GetHabbo().Id + "', @name, '" + OldName + "', '" + PlusEnvironment.GetUnixTimestamp() + "')");
                    dbClient.AddParameter("name", NewName);
                    dbClient.RunQuery();
                }

                ICollection<RoomData> Rooms = Session.GetHabbo().UsersRooms;
                foreach (RoomData Data in Rooms)
                {
                    if (Data == null)
                        continue;

                    Data.OwnerName = NewName;
                }

                foreach (Room UserRoom in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList())
                {
                    if (UserRoom == null || UserRoom.RoomData.OwnerName != NewName)
                        continue;

                    UserRoom.OwnerName = NewName;
                    UserRoom.RoomData.OwnerName = NewName;

                    UserRoom.SendMessage(new RoomInfoUpdatedComposer(UserRoom.RoomId));
                }

                HabboRoleplay.Misc.RoleplayManager.SendUser(Session, Room.Id, "");
            }
        }

        private static bool CanChangeName(Habbo Habbo)
        {

            if (Habbo.Rank == 1 && Habbo.VIPRank == 0 && Habbo.LastNameChange == 0)
                return true;
            else if (Habbo.Rank == 1 && Habbo.VIPRank == 1 && (Habbo.LastNameChange == 0 || (PlusEnvironment.GetUnixTimestamp() + 604800) > Habbo.LastNameChange))
                return true;
            else if (Habbo.GetPermissions().HasRight("mod_tool"))
                return true;

            return false;
        }
    }
}