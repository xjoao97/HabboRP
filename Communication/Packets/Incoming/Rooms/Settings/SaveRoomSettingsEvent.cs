using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Navigator;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;
using Plus.Database.Interfaces;


namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    class SaveRoomSettingsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            int RoomId = Packet.PopInt();

            Room Room = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId);
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            string Name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            string Description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            RoomAccess Access = RoomAccessUtility.ToRoomAccess(Packet.PopInt());
            string Password = Packet.PopString();
            int MaxUsers = Packet.PopInt();
            int CategoryId = Packet.PopInt();
            int TagCount = Packet.PopInt();

            List<string> Tags = new List<string>();
            StringBuilder formattedTags = new StringBuilder();

            for (int i = 0; i < TagCount; i++)
            {
                if (i > 0)
                {
                    formattedTags.Append(",");
                }

                string tag = Packet.PopString().ToLower();

                Tags.Add(tag);
                formattedTags.Append(tag);
            }

            int TradeSettings = Packet.PopInt();//2 = All can trade, 1 = owner only, 0 = no trading.
            int AllowPets = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            int AllowPetsEat = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            int RoomBlockingEnabled = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            int Hidewall = Convert.ToInt32(PlusEnvironment.BoolToEnum(Packet.PopBoolean()));
            int WallThickness = Packet.PopInt();
            int FloorThickness = Packet.PopInt();
            int WhoMute = Packet.PopInt(); // mute
            int WhoKick = Packet.PopInt(); // kick
            int WhoBan = Packet.PopInt(); // ban

            int chatMode = Packet.PopInt();
            int chatSize = Packet.PopInt();
            int chatSpeed = Packet.PopInt();
            int chatDistance = Packet.PopInt();
            int extraFlood = Packet.PopInt();

            if (chatMode < 0 || chatMode > 1)
                chatMode = 0;

            if (chatSize < 0 || chatSize > 2)
                chatSize = 0;

            if (chatSpeed < 0 || chatSpeed > 2)
                chatSpeed = 0;

            if (chatDistance < 0)
                chatDistance = 1;

            if (chatDistance > 99)
                chatDistance = 100;

            if (extraFlood < 0 || extraFlood > 2)
                extraFlood = 0;

            if (TradeSettings < 0 || TradeSettings > 2)
                TradeSettings = 0;

            if (WhoMute < 0 || WhoMute > 1)
                WhoMute = 0;

            if (WhoKick < 0 || WhoKick > 1)
                WhoKick = 0;

            if (WhoBan < 0 || WhoBan > 1)
                WhoBan = 0;

            if (WallThickness < -2 || WallThickness > 1)
                WallThickness = 0;

            if (FloorThickness < -2 || FloorThickness > 1)
                FloorThickness = 0;

            if (Name.Length < 1)
                return;

            if (Name.Length > 60)
                Name = Name.Substring(0, 60);

            if (Access == RoomAccess.PASSWORD && Password.Length == 0)
                Access = RoomAccess.OPEN;

            if (MaxUsers < 0)
                MaxUsers = 10;

            if (MaxUsers > 75)
                MaxUsers = 75;

            SearchResultList SearchResultList = null;
            if (!PlusEnvironment.GetGame().GetNavigator().TryGetSearchResultList(CategoryId, out SearchResultList))
                CategoryId = 36;

            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY || SearchResultList.RequiredRank > Session.GetHabbo().Rank || (Session.GetHabbo().Id != Room.OwnerId && Session.GetHabbo().Rank >= SearchResultList.RequiredRank))
                CategoryId = 36;

            if (TagCount > 2)
                return;

            Room.AllowPets = AllowPets;
            Room.AllowPetsEating = AllowPetsEat;
            Room.RoomBlockingEnabled = RoomBlockingEnabled;
            Room.Hidewall = Hidewall;

            Room.RoomData.AllowPets = AllowPets;
            Room.RoomData.AllowPetsEating = AllowPetsEat;
            Room.RoomData.RoomBlockingEnabled = RoomBlockingEnabled;
            Room.RoomData.Hidewall = Hidewall;

            Room.Name = Name;
            Room.Access = Access;
            Room.Description = Description;
            Room.Category = CategoryId;
            Room.Password = Password;

            Room.RoomData.Name = Name;
            Room.RoomData.Access = Access;
            Room.RoomData.Description = Description;
            Room.RoomData.Category = CategoryId;
            Room.RoomData.Password = Password;

            Room.WhoCanBan = WhoBan;
            Room.WhoCanKick = WhoKick;
            Room.WhoCanMute = WhoMute;
            Room.RoomData.WhoCanBan = WhoBan;
            Room.RoomData.WhoCanKick = WhoKick;
            Room.RoomData.WhoCanMute = WhoMute;

            Room.ClearTags();
            Room.AddTagRange(Tags);
            Room.UsersMax = MaxUsers;

            Room.RoomData.Tags.Clear();
            Room.RoomData.Tags.AddRange(Tags);
            Room.RoomData.UsersMax = MaxUsers;

            Room.WallThickness = WallThickness;
            Room.FloorThickness = FloorThickness;
            Room.RoomData.WallThickness = WallThickness;
            Room.RoomData.FloorThickness = FloorThickness;

            Room.chatMode = chatMode;
            Room.chatSize = chatSize;
            Room.chatSpeed = chatSpeed;
            Room.chatDistance = chatDistance;
            Room.extraFlood = extraFlood;

            Room.TradeSettings = TradeSettings;

            Room.RoomData.chatMode = chatMode;
            Room.RoomData.chatSize = chatSize;
            Room.RoomData.chatSpeed = chatSpeed;
            Room.RoomData.chatDistance = chatDistance;
            Room.RoomData.extraFlood = extraFlood;

            Room.RoomData.TradeSettings = TradeSettings;

            string AccessStr = Password.Length > 0 ? "password" : "open";
            switch (Access)
            {
                default:
                case RoomAccess.OPEN:
                    AccessStr = "open";
                    break;

                case RoomAccess.PASSWORD:
                    AccessStr = "password";
                    break;

                case RoomAccess.DOORBELL:
                    AccessStr = "locked";
                    break;

                case RoomAccess.INVISIBLE:
                    AccessStr = "invisible";
                    break;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE rooms SET caption = @caption, description = @description, password = @password, category = " +
                    CategoryId + ", state = '" + AccessStr + "', tags = @tags, users_max = " + MaxUsers +
                    ", allow_pets = '" + AllowPets + "', allow_pets_eat = '" + AllowPetsEat + "', room_blocking_disabled = '" +
                    RoomBlockingEnabled + "', allow_hidewall = '" + Room.Hidewall + "', floorthick = " +
                    Room.FloorThickness + ", wallthick = " + Room.WallThickness + ", mute_settings='" + Room.WhoCanMute +
                    "', kick_settings='" + Room.WhoCanKick + "',ban_settings='" + Room.WhoCanBan + "', `chat_mode` = '" + Room.chatMode + "', `chat_size` = '" + Room.chatSize + "', `chat_speed` = '" + Room.chatSpeed + "', `chat_extra_flood` = '" + Room.extraFlood + "', `chat_hearing_distance` = '" + Room.chatDistance + "', `trade_settings` = '" + Room.TradeSettings + "' WHERE `id` = '" + Room.RoomId + "' LIMIT 1");
                dbClient.AddParameter("caption", Room.Name);
                dbClient.AddParameter("description", Room.Description);
                dbClient.AddParameter("password", Room.Password);
                dbClient.AddParameter("tags", formattedTags.ToString());
                dbClient.RunQuery();
            }

            Room.GetGameMap().GenerateMaps();

            if (Session.GetHabbo().CurrentRoom == null)
            {
                Session.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
                Session.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));
                Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
            }
            else
            {
                Room.SendMessage(new RoomSettingsSavedComposer(Room.RoomId));
                Room.SendMessage(new RoomInfoUpdatedComposer(Room.RoomId));
                Room.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));
            }
            
            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModDoorModeSeen", 1);
            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModWalkthroughSeen", 1);
            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatScrollSpeedSeen", 1);
            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatFloodFilterSeen", 1);
            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_SelfModChatHearRangeSeen", 1);
        }
    }
}
