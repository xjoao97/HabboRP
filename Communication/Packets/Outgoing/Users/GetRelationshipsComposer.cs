using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Outgoing.Users
{
    class GetRelationshipsComposer : ServerPacket
    {
        public GetRelationshipsComposer(Habbo Habbo, RoleplayBot Bot = null)
            : base(ServerPacketHeader.GetRelationshipsMessageComposer)
        {
            if (Bot != null)
            {
                WriteBotData(Bot);
                return;
            }

            if (Habbo == null)
            {
                WriteNullData();
                return;
            }
            else
            {
                WriteHabboData(Habbo);
                return;
            }
        }

        public void WriteBotData(RoleplayBot Bot)
        {
            int FakeBotId = Bot.Id + 1000000;

            base.WriteInteger(FakeBotId);
            base.WriteInteger(3);

            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteInteger(FakeBotId);
            base.WriteString("Casado(a) com: Ninguém");
            base.WriteString(Bot.Figure);

            base.WriteInteger(2);
            base.WriteInteger(1);
            base.WriteInteger(FakeBotId);
            base.WriteString("Level: " + Bot.Level + "/" + RoleplayManager.LevelCap);
            base.WriteString(Bot.Figure);

            base.WriteInteger(3);
            base.WriteInteger(1);
            base.WriteInteger(FakeBotId);
            base.WriteString("Força: " + Bot.Strength + "/" + (Bot.Strength > 20 ? Bot.Strength : 20));
            base.WriteString(Bot.Figure);
        }

        public void WriteNullData()
        {
            base.WriteInteger(0);
            base.WriteInteger(0);
        }

        public void WriteHabboData(Habbo Habbo)
        {
            #region Default
            base.WriteInteger(Habbo.Id);
            base.WriteInteger(3);
            #endregion

            #region Marriage
            base.WriteInteger(1);
            base.WriteInteger(1);

            if (Habbo.GetClient() != null && Habbo.GetClient().GetRoleplay() != null)
            {
                if (Habbo.GetClient().GetRoleplay().MarriedTo > 0)
                {
                    Habbo Married = PlusEnvironment.GetHabboById(Habbo.GetClient().GetRoleplay().MarriedTo);

                    if (Married == null)
                    {
                        Habbo.GetClient().GetRoleplay().MarriedTo = 0;
                        base.WriteInteger(Habbo.Id);
                        base.WriteString("Casado(a) com: Ninguém");
                        base.WriteString(Habbo.Look);
                    }
                    else
                    {
                        base.WriteInteger(Married.Id);
                        base.WriteString("Casado(a) com: " + Married.Username);
                        base.WriteString(Married.Look);
                    }
                }
                else
                {
                    base.WriteInteger(Habbo.Id);
                    base.WriteString("Casado(a) com: Ninguém");
                    base.WriteString(Habbo.Look);
                }
            }
            else
            {
                using (UserCache Cache = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Habbo.Id))
                {
                    if (Cache.MarriedId > 0)
                    {
                        using (UserCache Married = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Cache.MarriedId))
                        {
                            if (Married == null)
                            {
                                Cache.MarriedId = 0;
                                base.WriteInteger(Habbo.Id);
                                base.WriteString("Casado(a) com: Ninguém");
                                base.WriteString(Habbo.Look);
                            }
                            else
                            {
                                base.WriteInteger(Married.Id);
                                base.WriteString("Casado(a) com: " + Married.Username);
                                base.WriteString(Married.Look);
                            }
                        }
                    }
                    else
                    {
                        base.WriteInteger(Habbo.Id);
                        base.WriteString("Casado(a) com: Ninguém");
                        base.WriteString(Habbo.Look);
                    }
                }
            }
            #endregion

            #region Level
            base.WriteInteger(2);
            base.WriteInteger(1);

            if (Habbo.GetClient() != null && Habbo.GetClient().GetRoleplay() != null)
            {
                base.WriteInteger(Habbo.Id);
                base.WriteString("Level: " + Habbo.GetClient().GetRoleplay().Level + "/" + RoleplayManager.LevelCap);
                base.WriteString(Habbo.Look);
            }
            else
            {
                using (UserCache Cache = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Habbo.Id))
                {
                    base.WriteInteger(Habbo.Id);
                    base.WriteString("Level: " + Cache.Level + "/" + RoleplayManager.LevelCap);
                    base.WriteString(Habbo.Look);
                }
            }
            #endregion

            #region Gang
            base.WriteInteger(3);
            base.WriteInteger(1);

            if (Habbo.Id == 0 || (Habbo.GetClient() != null && Habbo.GetClient().GetRoleplay() != null))
            {
                if (Habbo.Id == 0)
                {
                    base.WriteInteger(Habbo.Id);
                        base.WriteString("Força: " + Habbo.GetClient().GetRoleplay().Strength + "/" + RoleplayManager.StrengthCap);
                        base.WriteString(Habbo.Look);
                }
                else
                {
                    Group Gang = GroupManager.GetGang(Habbo.GetClient().GetRoleplay().GangId);
                    if (Gang != null)
                    {
                        //Habbo GangOwner = PlusEnvironment.GetHabboById(Gang.CreatorId);
                        base.WriteInteger(Habbo.Id);
                        base.WriteString("Força: " + Habbo.GetClient().GetRoleplay().Strength + "/" + RoleplayManager.StrengthCap);
                        base.WriteString(Habbo.Look);
                    }
                    else
                    {
                        base.WriteInteger(Habbo.Id);
                        base.WriteString("Força: " + Habbo.GetClient().GetRoleplay().Strength + "/" + RoleplayManager.StrengthCap);
                        base.WriteString(Habbo.Look);
                    }
                }
            }
            else
            {
                if (GroupManager.Gangs.Values.Where(x => x.Members.ContainsKey(Habbo.Id)).ToList().Count > 0)
                {
                    Group Gang = GroupManager.Gangs.Values.FirstOrDefault(x => x.Members.ContainsKey(Habbo.Id));
                    Habbo GangOwner = PlusEnvironment.GetHabboById(Gang.CreatorId);
                    base.WriteInteger(Habbo.Id);
                        base.WriteString("Força: " + Habbo.GetClient().GetRoleplay().Strength + "/" + RoleplayManager.StrengthCap);
                        base.WriteString(Habbo.Look);
                }
                else
                {
                        base.WriteInteger(Habbo.Id);
                        base.WriteString("Força: " + Habbo.GetClient().GetRoleplay().Strength + "/" + RoleplayManager.StrengthCap);
                        base.WriteString(Habbo.Look);
                }
            }
            #endregion
        }
    }
}