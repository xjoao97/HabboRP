using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    class UsersComposer : ServerPacket
    {
        public UsersComposer(ICollection<RoomUser> Users)
            : base(ServerPacketHeader.UsersMessageComposer)
        {
            base.WriteInteger(Users.Count);
            foreach (RoomUser User in Users.ToList())
            {
                WriteUser(User);
            }
        }

        public UsersComposer(RoomUser User)
            : base(ServerPacketHeader.UsersMessageComposer)
        {
            base.WriteInteger(1);//1 avatar
            WriteUser(User);
        }

        private void WriteUser(RoomUser User)
        {
            if (!User.IsPet && !User.IsBot)
            {
                Habbo Habbo = User.GetClient().GetHabbo();
                Group Group = GroupManager.GetJob(User.GetClient().GetRoleplay().JobId);

                if (Habbo.PetId == 0)
                {
                    base.WriteInteger(Habbo.Id);
                    base.WriteString(Habbo.Username);
                    base.WriteString(Habbo.Motto);
                    base.WriteString(Habbo.Look);
                    base.WriteInteger(User.VirtualId);
                    base.WriteInteger(User.X);
                    base.WriteInteger(User.Y);
                    base.WriteDouble(User.Z);

                    base.WriteInteger(0);//2 for user, 4 for bot.
                    base.WriteInteger(1);//1 for user, 2 for pet, 3 for bot.
                    base.WriteString(Habbo.Gender.ToLower());

                    if (Group != null)
                    {
                        base.WriteInteger(Group.Id);
                        base.WriteInteger(0);
                        base.WriteString(Group.Name);
                    }
                    else
                    {
                        base.WriteInteger(0);
                        base.WriteInteger(0);
                        base.WriteString("");
                    }

                    base.WriteString("");//Whats this?
                    base.WriteInteger(Habbo.GetStats().AchievementPoints);//Achievement score
                    base.WriteBoolean(false);//Builders club?
                }
                else if (Habbo.PetId > 0 && Habbo.PetId != 100)
                {
                    base.WriteInteger(Habbo.Id);
                    base.WriteString(Habbo.Username);
                    base.WriteString(Habbo.Motto);
                    base.WriteString((Habbo.PetFigure == null) ? PetFigureForType(Habbo) : Habbo.PetFigure);

                    base.WriteInteger(User.VirtualId);
                    base.WriteInteger(User.X);
                    base.WriteInteger(User.Y);
                    base.WriteDouble(User.Z);
                    base.WriteInteger(0);
                    base.WriteInteger(2);//Pet.

                    base.WriteInteger(Habbo.PetId);//pet type.
                    base.WriteInteger(Habbo.Id);//UserId of the owner.
                    base.WriteString(Habbo.Username);//Username of the owner.
                    base.WriteInteger(1);
                    base.WriteBoolean(false);//Has saddle.
                    base.WriteBoolean(false);//Is someone riding this horse?
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                    base.WriteString("");
                }
                else if (Habbo.PetId > 0 && Habbo.PetId == 100)
                {
                    base.WriteInteger(Habbo.Id);
                    base.WriteString(Habbo.Username);
                    base.WriteString(Habbo.Motto);
                    base.WriteString(Habbo.Look.ToLower());
                    base.WriteInteger(User.VirtualId);
                    base.WriteInteger(User.X);
                    base.WriteInteger(User.Y);
                    base.WriteDouble(User.Z);
                    base.WriteInteger(0);
                    base.WriteInteger(4);

                    base.WriteString(Habbo.Gender.ToLower()); // ?
                    base.WriteInteger(Habbo.Id); //Owner Id
                    base.WriteString(Habbo.Username); // Owner name
                    base.WriteInteger(0);//Action Count
                }
            }
            else if (User.IsPet)
            {
                base.WriteInteger(User.BotAI.BaseId);
                base.WriteString(User.BotData.Name);
                base.WriteString(User.BotData.Motto);

                //base.WriteString("26 30 ffffff 5 3 302 4 2 201 11 1 102 12 0 -1 28 4 401 24");
                base.WriteString(User.BotData.Look.ToLower() + ((User.PetData.Saddle > 0) ? " 3 2 " + User.PetData.PetHair + " " + User.PetData.HairDye + " 3 " + User.PetData.PetHair + " " + User.PetData.HairDye + " 4 " + User.PetData.Saddle + " 0" : " 2 2 " + User.PetData.PetHair + " " + User.PetData.HairDye + " 3 " + User.PetData.PetHair + " " + User.PetData.HairDye + ""));

                base.WriteInteger(User.VirtualId);
                base.WriteInteger(User.X);
                base.WriteInteger(User.Y);
                base.WriteDouble(User.Z);
                base.WriteInteger(0);
                base.WriteInteger((User.BotData.AiType == BotAIType.PET) ? 2 : 4);
                base.WriteInteger(User.PetData.Type);
                base.WriteInteger(User.PetData.OwnerId); // userid
                base.WriteString(User.PetData.OwnerName); // username
                base.WriteInteger(1);
                base.WriteBoolean(User.PetData.Saddle > 0);
                base.WriteBoolean(User.RidingHorse);
                base.WriteInteger(0);
                base.WriteInteger(0);
                base.WriteString("");
            }
            else if (User.IsBot)
            {
                if (User.IsRoleplayBot)
                {
                    string[] Outfit = User.GetBotRoleplay().GetOutFit();

                    #region BOT Profile Type 1
                    base.WriteInteger(User.BotAI.BaseId + 1000000);
                    base.WriteString(User.GetBotRoleplay().Name);
                    base.WriteString(Outfit[1]);
                    base.WriteString(Outfit[0]);
                    base.WriteInteger(User.VirtualId);
                    base.WriteInteger(User.X);
                    base.WriteInteger(User.Y);
                    base.WriteDouble(User.Z);

                    base.WriteInteger(0);//2 for user, 4 for bot.
                    base.WriteInteger(1);//1 for user, 2 for pet, 3 for bot.
                    base.WriteString(User.GetBotRoleplay().Gender.ToLower());

                    Group BotGroup = GroupManager.GetJob(User.GetBotRoleplay().Corporation);
                    if (BotGroup != null)
                    {
                        base.WriteInteger(BotGroup.Id);
                        base.WriteInteger(0);
                        base.WriteString(BotGroup.Name);
                    }
                    else
                    {
                        base.WriteInteger(0);
                        base.WriteInteger(0);
                        base.WriteString("");
                    }

                    base.WriteString("");//Whats this?
                    base.WriteInteger(0);//Achievement score
                    base.WriteBoolean(false);//Builders club?
                    #endregion
                }
                else
                {
                    #region BOT Profile Type 2
                    base.WriteInteger(User.BotAI.BaseId);
                    base.WriteString(User.BotData.Name);
                    base.WriteString(User.BotData.Motto);
                    base.WriteString(User.BotData.Look.ToLower());
                    base.WriteInteger(User.VirtualId);
                    base.WriteInteger(User.X);
                    base.WriteInteger(User.Y);
                    base.WriteDouble(User.Z);
                    base.WriteInteger(0);
                    base.WriteInteger((User.BotData.AiType == BotAIType.PET) ? 2 : 4);

                    base.WriteString(User.BotData.Gender.ToLower()); // ?
                    base.WriteInteger(User.BotData.ownerID); //Owner Id
                    base.WriteString(PlusEnvironment.GetUsernameById(User.BotData.ownerID)); // Owner name
                    base.WriteInteger(5);//Action Count
                    base.WriteShort(1);//Copy looks
                    base.WriteShort(2);//Setup speech
                    base.WriteShort(3);//Relax
                    base.WriteShort(4);//Dance
                    base.WriteShort(5);//Change name
                    #endregion
                }
            }
        }

        public string PetFigureForType(Habbo Habbo)
        {
            Random _random = new Random();
            int Type = Habbo.PetId;
            string ChosenFigure = "";

            switch (Type)
            {
                #region Dog Figures
                default:
                case 60:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "0 0 f08b90 2 2 -1 1 3 -1 1"; break;
                            case 2:
                                ChosenFigure = "0 15 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "0 20 d98961 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "0 21 da9dbd 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Cat Figures
                case 1:
                    {
                        int RandomNumber = _random.Next(1, 6);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "1 18 d5b35f 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "1 0 ff7b3a 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "1 18 d98961 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "1 0 ff7b3a 2 2 -1 0 3 -1 1"; break;
                            case 5:
                                ChosenFigure = "1 24 d5b35f 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Terrier Figures
                case 2:
                    {
                        int RandomNumber = _random.Next(1, 7);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "3 3 eeeeee 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "3 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "3 5 eeeeee 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "3 6 eeeeee 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "3 4 dddddd 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "3 5 dddddd 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Croco Figures
                case 3:
                    {
                        int RandomNumber = _random.Next(1, 6);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "2 10 84ce84 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "2 8 838851 2 2 0 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "2 11 b99105 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "2 3 e8ce25 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "2 2 fcfad3 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Bear Figures
                case 4:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "4 2 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "4 3 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "4 1 eaeddf 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "4 0 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Pig Figures
                case 5:
                    {
                        int RandomNumber = _random.Next(1, 8);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "5 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "5 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "5 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "5 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "5 7 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "5 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "5 8 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Lion Figures
                case 6:
                    {
                        int RandomNumber = _random.Next(1, 12);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "6 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "6 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "6 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "6 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "6 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "6 0 ffd8c9 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "6 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 8:
                                ChosenFigure = "6 11 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 9:
                                ChosenFigure = "6 2 ffe49d 2 2 -1 0 3 -1 0"; break;
                            case 10:
                                ChosenFigure = "6 11 ff9ae 2 2 -1 0 3 -1 0"; break;
                            case 11:
                                ChosenFigure = "6 2 ff9ae 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Rhino Figures
                case 7:
                    {
                        int RandomNumber = _random.Next(1, 8);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "7 5 aeaeae 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "7 7 ffc99a 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "7 5 cccccc 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "7 5 9adcff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "7 5 ff7d6a 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "7 6 cccccc 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "7 0 cccccc 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Spider Figures
                case 8:
                    {
                        int RandomNumber = _random.Next(1, 14);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "8 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "8 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "8 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "8 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "8 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "8 14 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "8 11 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 8:
                                ChosenFigure = "8 8 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 9:
                                ChosenFigure = "8 6 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 10:
                                ChosenFigure = "8 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 11:
                                ChosenFigure = "8 9 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 12:
                                ChosenFigure = "8 10 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 13:
                                ChosenFigure = "8 7 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Turtle Figures
                case 9:
                    {
                        int RandomNumber = _random.Next(1, 10);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "9 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "9 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "9 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "9 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "9 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "9 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "9 6 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 8:
                                ChosenFigure = "9 7 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 9:
                                ChosenFigure = "9 8 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Chick Figures
                case 10:
                    {
                        ChosenFigure = "10 0 ffffff 2 2 -1 0 3 -1 0";
                        break;
                    }
                #endregion

                #region Frog Figures
                case 11:
                    {
                        int RandomNumber = _random.Next(1, 14);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "11 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "11 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "11 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "11 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "11 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "11 9 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "11 10 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 8:
                                ChosenFigure = "11 6 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 9:
                                ChosenFigure = "11 12 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 10:
                                ChosenFigure = "11 11 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 11:
                                ChosenFigure = "11 15 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 12:
                                ChosenFigure = "11 13 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 13:
                                ChosenFigure = "11 18 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Dragon Figures
                case 12:
                    {
                        int RandomNumber = _random.Next(1, 7);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "12 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "12 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "12 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "12 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "12 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "12 5 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Monster Figures
                // Case 13 is disabled as habbo did not add an avatar for it
                #endregion

                #region Monkey Figures
                case 14:
                    {
                        int RandomNumber = _random.Next(1, 15);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "14 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "14 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "14 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "14 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "14 6 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "14 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "14 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 8:
                                ChosenFigure = "14 7 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 9:
                                ChosenFigure = "14 8 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 10:
                                ChosenFigure = "14 9 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 11:
                                ChosenFigure = "14 10 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 12:
                                ChosenFigure = "14 11 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 13:
                                ChosenFigure = "14 12 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 14:
                                ChosenFigure = "14 13 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Horse Figures
                case 15:
                    {
                        int RandomNumber = _random.Next(1, 21);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "15 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "15 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "15 4 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "15 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "15 6 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "15 7 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "15 8 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 8:
                                ChosenFigure = "15 9 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 9:
                                ChosenFigure = "15 10 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 10:
                                ChosenFigure = "15 11 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 11:
                                ChosenFigure = "15 12 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 12:
                                ChosenFigure = "15 13 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 13:
                                ChosenFigure = "15 14 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 14:
                                ChosenFigure = "15 15 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 15:
                                ChosenFigure = "15 16 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 16:
                                ChosenFigure = "15 17 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 17:
                                ChosenFigure = "15 78 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 18:
                                ChosenFigure = "15 77 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 19:
                                ChosenFigure = "15 79 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 20:
                                ChosenFigure = "15 80 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Monster Plant Figures
                case 16:
                    {
                        int RandomNumber = _random.Next(1, 11);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "18 1 ffffff"; break;
                            case 2:
                                ChosenFigure = "18 2 ffffff"; break;
                            case 3:
                                ChosenFigure = "18 3 ffffff"; break;
                            case 4:
                                ChosenFigure = "18 4 ffffff"; break;
                            case 5:
                                ChosenFigure = "18 5 ffffff"; break;
                            case 6:
                                ChosenFigure = "18 6 ffffff"; break;
                            case 7:
                                ChosenFigure = "18 7 ffffff"; break;
                            case 8:
                                ChosenFigure = "18 8 ffffff"; break;
                            case 9:
                                ChosenFigure = "18 9 ffffff"; break;
                            case 10:
                                ChosenFigure = "18 10 ffffff"; break;
                        }
                        break;
                    }
                #endregion

                #region Bunny Figures
                case 17:
                    {
                        int RandomNumber = _random.Next(1, 6);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "17 1 ffffff"; break;
                            case 2:
                                ChosenFigure = "17 2 ffffff"; break;
                            case 3:
                                ChosenFigure = "17 3 ffffff"; break;
                            case 4:
                                ChosenFigure = "17 4 ffffff"; break;
                            case 5:
                                ChosenFigure = "17 5 ffffff"; break;
                        }
                        break;
                    }

                case 18:
                    {
                        ChosenFigure = "18 0 ffffff";
                        break;
                    }

                case 19:
                    {
                        ChosenFigure = "19 0 ffffff";
                        break;
                    }

                case 20:
                    {
                        ChosenFigure = "20 0 ffffff";
                        break;
                    }
                #endregion

                #region Pigeon Figures (White & Black)
                case 21:
                    {
                        ChosenFigure = "21 0 ffffff";
                        break;
                    }
                case 22:
                    {
                        ChosenFigure = "22 0 ffffff";
                        break;
                    }
                #endregion

                #region Demon Monkey Figures
                case 23:
                    {
                        int RandomNumber = _random.Next(1, 4);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "23 0 ffffff"; break;
                            case 2:
                                ChosenFigure = "23 1 ffffff"; break;
                            case 3:
                                ChosenFigure = "23 3 ffffff"; break;
                        }
                    }
                    break;
                #endregion

                #region Baby Bear Figures
                case 24:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "4 2 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "4 3 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "4 1 eaeddf 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "4 0 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Baby Terrier Figures
                case 25:
                    {
                        int RandomNumber = _random.Next(1, 7);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "3 3 eeeeee 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "3 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "3 5 eeeeee 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "3 6 eeeeee 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "3 4 dddddd 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "3 5 dddddd 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Gnome Figures
                case 26:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "26 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3"; break;
                            case 2:
                                ChosenFigure = "26 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3"; break;
                            case 3:
                                ChosenFigure = "26 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6"; break;
                            case 4:
                                ChosenFigure = "26 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3"; break;
                        }
                        break;
                    }

                case 27:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "27 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3"; break;
                            case 2:
                                ChosenFigure = "27 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3"; break;
                            case 3:
                                ChosenFigure = "27 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6"; break;
                            case 4:
                                ChosenFigure = "27 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3"; break;
                        }
                        break;
                    }
                #endregion

                #region Baby Cat Figures
                case 28:
                    {
                        int RandomNumber = _random.Next(1, 6);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "28 18 d5b35f 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "28 0 ff7b3a 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "28 18 d98961 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "28 0 ff7b3a 2 2 -1 0 3 -1 1"; break;
                            case 5:
                                ChosenFigure = "28 24 d5b35f 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Baby Dog Figures
                case 29:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "29 0 f08b90 2 2 -1 1 3 -1 1"; break;
                            case 2:
                                ChosenFigure = "29 15 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "29 20 d98961 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "29 21 da9dbd 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Baby Pig Figures
                case 30:
                    {
                        int RandomNumber = _random.Next(1, 8);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "30 2 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "30 0 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "30 3 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "30 5 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 5:
                                ChosenFigure = "30 7 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 6:
                                ChosenFigure = "30 1 ffffff 2 2 -1 0 3 -1 0"; break;
                            case 7:
                                ChosenFigure = "30 8 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region Oompa Loompa Figures
                case 31:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "31 1 ffffff 5 0 -1 0 4 402 5 3 301 4 1 101 2 2 201 3"; break;
                            case 2:
                                ChosenFigure = "31 1 ffffff 5 0 -1 0 1 102 13 3 301 4 4 401 5 2 201 3"; break;
                            case 3:
                                ChosenFigure = "31 6 ffffff 5 1 102 8 2 201 16 4 401 9 3 303 4 0 -1 6"; break;
                            case 4:
                                ChosenFigure = "31 30 ffffff 5 0 -1 0 3 303 4 4 401 5 1 101 2 2 201 3"; break;
                        }
                        break;
                    }
                #endregion

                #region Pet Rock Figures
                case 32:
                    {
                        ChosenFigure = "32 0 ffffff";
                        break;
                    }
                #endregion

                #region Pteradactyl Figures
                case 33:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "33 2 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "33 3 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "33 1 eaeddf 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "33 0 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion

                #region T-Rex Figures
                case 34:
                    {
                        int RandomNumber = _random.Next(1, 5);
                        switch (RandomNumber)
                        {
                            case 1:
                                ChosenFigure = "34 2 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 2:
                                ChosenFigure = "34 3 e4feff 2 2 -1 0 3 -1 0"; break;
                            case 3:
                                ChosenFigure = "34 1 eaeddf 2 2 -1 0 3 -1 0"; break;
                            case 4:
                                ChosenFigure = "34 0 ffffff 2 2 -1 0 3 -1 0"; break;
                        }
                        break;
                    }
                #endregion
            }

            Habbo.PetFigure = ChosenFigure;
            return ChosenFigure;
        }
    }
}