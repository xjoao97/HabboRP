using System;
using System.Collections.Generic;
using System.Data;

using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms
{
    public class RoomData
    {
        public int Id;
        public int AllowPets;
        public int AllowPetsEating;
        public int RoomBlockingEnabled;
        public int Category;
        public string Description;
        public string Floor;
        public int FloorThickness;
        public Group Group;
        public int Hidewall;
        public string Landscape;
        public string ModelName;
        public string Name;
        public string OwnerName;
        public int OwnerId;
        public string Password;
        public int Score;
        public RoomAccess Access;
        public List<string> Tags;
        public string Type;
        public int UsersMax;
        public int UsersNow;
        public int WallThickness;
        public string Wallpaper;
        public int WhoCanBan;
        public int WhoCanKick;
        public int WhoCanMute;
        private RoomModel mModel;
        public int chatMode;
        public int chatSpeed;
        public int chatSize;
        public int extraFlood;
        public int chatDistance;

        public int TradeSettings;//Default = 2;

        public RoomPromotion _promotion;

        public bool PushEnabled;
        public bool PullEnabled;
        public bool SPushEnabled;
        public bool SPullEnabled;
        public bool EnablesEnabled;
        public bool RespectNotificationsEnabled;
        public bool PetMorphsAllowed;

        public bool BankEnabled;
        public bool ShootEnabled;
        public bool HitEnabled;
        public bool SafeZoneEnabled;
        public bool SexCommandsEnabled;
        public bool TurfEnabled;
        public bool RobEnabled;
        public bool GymEnabled;
        public bool DeliveryEnabled;
        public bool TutorialEnabled;
        public bool DriveEnabled;
        public bool TaxiToEnabled;
        public bool TaxiFromEnabled;
        public HabboRoleplay.Games.IGame RoleplayEvent;
        public string EnterRoomMessage;

        public void Fill(DataRow Row)
        {
            Id = Convert.ToInt32(Row["id"]);
            Name = Convert.ToString(Row["caption"]);
            Description = Convert.ToString(Row["description"]);
            Type = Convert.ToString(Row["roomtype"]);
            OwnerId = Convert.ToInt32(Row["owner"]);

            OwnerName = "";
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @owner LIMIT 1");
                dbClient.AddParameter("owner", OwnerId);
                string result = dbClient.getString();
                if (!String.IsNullOrEmpty(result))
                    OwnerName = result;
            }

            this.Access = RoomAccessUtility.ToRoomAccess(Row["state"].ToString().ToLower());

            Category = Convert.ToInt32(Row["category"]);
            if (!string.IsNullOrEmpty(Row["users_now"].ToString()))
                UsersNow = Convert.ToInt32(Row["users_now"]);
            else
                UsersNow = 0;
            UsersMax = Convert.ToInt32(Row["users_max"]);
            ModelName = Convert.ToString(Row["model_name"]);
            Score = Convert.ToInt32(Row["score"]);
            Tags = new List<string>();
            AllowPets = Convert.ToInt32(Row["allow_pets"].ToString());
            AllowPetsEating = Convert.ToInt32(Row["allow_pets_eat"].ToString());
            RoomBlockingEnabled = Convert.ToInt32(Row["room_blocking_disabled"].ToString());
            Hidewall = Convert.ToInt32(Row["allow_hidewall"].ToString());
            Password = Convert.ToString(Row["password"]);
            Wallpaper = Convert.ToString(Row["wallpaper"]);
            Floor = Convert.ToString(Row["floor"]);
            Landscape = Convert.ToString(Row["landscape"]);
            FloorThickness = Convert.ToInt32(Row["floorthick"]);
            WallThickness = Convert.ToInt32(Row["wallthick"]);
            WhoCanMute = Convert.ToInt32(Row["mute_settings"]);
            WhoCanKick = Convert.ToInt32(Row["kick_settings"]);
            WhoCanBan = Convert.ToInt32(Row["ban_settings"]);
            chatMode = Convert.ToInt32(Row["chat_mode"]);
            chatSpeed = Convert.ToInt32(Row["chat_speed"]);
            chatSize = Convert.ToInt32(Row["chat_size"]);
            TradeSettings = Convert.ToInt32(Row["trade_settings"]);
            int GroupId = Convert.ToInt32(Row["group_id"]);
            Group G = null;

            if (GroupId < 1000)
                G = GroupManager.GetJob(GroupId);
            else
                G = GroupManager.GetGang(GroupId);

            if (G != null)
                Group = G;
            else
                Group = null;

            foreach (string Tag in Row["tags"].ToString().Split(','))
            {
                Tags.Add(Tag);
            }

            mModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(ModelName);

            this.PushEnabled = PlusEnvironment.EnumToBool(Row["push_enabled"].ToString());
            this.PullEnabled = PlusEnvironment.EnumToBool(Row["pull_enabled"].ToString());
            this.SPushEnabled = PlusEnvironment.EnumToBool(Row["spush_enabled"].ToString());
            this.SPullEnabled = PlusEnvironment.EnumToBool(Row["spull_enabled"].ToString());
            this.EnablesEnabled = PlusEnvironment.EnumToBool(Row["enables_enabled"].ToString());
            this.RespectNotificationsEnabled = PlusEnvironment.EnumToBool(Row["respect_notifications_enabled"].ToString());
            this.PetMorphsAllowed = PlusEnvironment.EnumToBool(Row["pet_morphs_allowed"].ToString());
        }

        public void FillRP(DataRow Row)
        {
            this.RoleplayEvent = null;
            this.BankEnabled = PlusEnvironment.EnumToBool(Row["bank_enabled"].ToString());
            this.ShootEnabled = PlusEnvironment.EnumToBool(Row["shoot_enabled"].ToString());
            this.HitEnabled = PlusEnvironment.EnumToBool(Row["hit_enabled"].ToString());
            this.SafeZoneEnabled = PlusEnvironment.EnumToBool(Row["safezone_enabled"].ToString());
            this.SexCommandsEnabled = PlusEnvironment.EnumToBool(Row["sexcommands_enabled"].ToString());
            this.TurfEnabled = PlusEnvironment.EnumToBool(Row["turf_enabled"].ToString());
            this.RobEnabled = PlusEnvironment.EnumToBool(Row["rob_enabled"].ToString());
            this.GymEnabled = PlusEnvironment.EnumToBool(Row["gym_enabled"].ToString());
            this.DeliveryEnabled = PlusEnvironment.EnumToBool(Row["delivery_enabled"].ToString());
            this.TutorialEnabled = PlusEnvironment.EnumToBool(Row["tutorial_enabled"].ToString());
            this.DriveEnabled = PlusEnvironment.EnumToBool(Row["drive_enabled"].ToString());
            this.TaxiFromEnabled = PlusEnvironment.EnumToBool(Row["taxi_from_enabled"].ToString());
            this.TaxiToEnabled = PlusEnvironment.EnumToBool(Row["taxi_to_enabled"].ToString());
            this.EnterRoomMessage = Row["enter_message"].ToString();
        }

        public RoomPromotion Promotion
        {
            get { return this._promotion; }
            set { this._promotion = value; }
        }

        public bool HasActivePromotion
        {
            get { return this.Promotion != null; }
        }

        public void EndPromotion()
        {
            if (!this.HasActivePromotion)
                return;

            this.Promotion = null;
        }

        public RoomModel Model
        {
            get
            {
                if (mModel == null)
                    mModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(ModelName);
                return mModel;
            }
        }
    }
}