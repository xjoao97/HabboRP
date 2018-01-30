using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.Utilities;

namespace Plus.HabboRoleplay.RoleplayUsers
{
    public static class LevelManager
    {
        #region Levels Dictionary
        public static readonly Dictionary<int, int> Levels = new Dictionary<int, int>
        {
            {1,0},
            {2,2000},
            {3,4500},
            {4,10000},
            {5,17000},
            {6,30000},
            {7,53000},
            {8,91000},
            {9,154000},
            {10,256000},
            {11,423000},
            {12,692000},
            {13,1124000},
            {14,1816000},
            {15,2913000},
            {16,4671000},
            {17,7444000},
            {18,11823000},
            {19,18720000},
            {20,30000000},
            {21,40000000}
        };
        #endregion

        #region Intelligence Levels Dictionary
        public static readonly Dictionary<int, int> IntelligenceLevels = new Dictionary<int, int>
        {
            {1,200},
            {2,500},
            {3,900},
            {4,1400},
            {5,2000},
            {6,2700},
            {7,3500},
            {8,4400},
            {9,5400},
            {10,6500},
            {11,7700},
            {12,9000},
            {13,10400}
        };
        #endregion

        #region Strength Levels Dictionary
        public static readonly Dictionary<int, int> StrengthLevels = new Dictionary<int, int>
        {
            {1,200},
            {2,500},
            {3,900},
            {4,1400},
            {5,2000},
            {6,2700},
            {7,3500},
            {8,4400},
            {9,5400},
            {10,6500},
            {11,7700},
            {12,9000},
            {13,10400}
        };
        #endregion

        #region Stamina Levels Dictionary
        public static readonly Dictionary<int, int> StaminaLevels = new Dictionary<int, int>
        {
            {1,200},
            {2,500},
            {3,900},
            {4,1400},
            {5,2000},
            {6,2700},
            {7,3500},
            {8,4400},
            {9,5400},
            {10,6500},
            {11,7700},
            {12,9000},
            {13,10400},
            {14,11900},
            {15,13500},
            {16,15200},
            {17,17000},
            {18,18900},
            {19,20900},
            {20,23000},
            {21,25200}
        };
        #endregion

        public static void AddLevelEXP(GameClient Session, int amount)
        {
            try
            {
                Session.GetRoleplay().RefreshStatDialogue();
                amount = Convert.ToInt32(RoleplayData.GetData("level", "modifier")) * amount;

                if (Session != null && Session.GetRoleplay() != null)
                {
                    Session.GetRoleplay().LevelEXP += amount;

                    if (LevelUp(Session, "level"))
                    {
                        Session.GetRoleplay().Level += 1;
                        Session.GetRoleplay().RefreshStatDialogue();

                        if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(Session.GetHabbo().Id))
                            PlusEnvironment.GetGame().GetCacheManager().TryUpdateUser(Session);

                        Session.SendWhisper("Você upou seu nível, agora você é Nível: " + Session.GetRoleplay().Level + ".");
                    }
                    else
                        Session.SendWhisper("Você recebeu " + String.Format("{0:N0}", amount) + "XP! você precisa de " + String.Format("{0:N0}", (Levels[Session.GetRoleplay().Level + 1] - Session.GetRoleplay().LevelEXP)) + " para ir para o nível " + (Session.GetRoleplay().Level + 1), 1);
                }
            }
            catch
            {

            }
        }

        public static void AddIntelligenceEXP(GameClient Session, int amount)
        {
            try
            {
                amount = Convert.ToInt32(RoleplayData.GetData("intelligence", "modifier")) * amount;

                if (Session != null && Session.GetRoleplay() != null)
                {
                    Session.GetRoleplay().IntelligenceEXP += amount;

                    if (LevelUp(Session, "intelligence"))
                    {
                        Session.GetRoleplay().Intelligence += 1;
                        Session.Shout("*Se sente um pouco mais inteligente [+1 Inteligência]*", 4);
                    }
                    else
                        Session.SendWhisper("Lendo: " + String.Format("{0:N0}", Session.GetRoleplay().IntelligenceEXP) + "/" + String.Format("{0:N0}", (IntelligenceLevels[Session.GetRoleplay().Intelligence + 1])), 1);
                }
            }
            catch
            {

            }
        }

        public static void AddStrengthEXP(GameClient Session, int amount)
        {
            try
            {
                amount = Convert.ToInt32(RoleplayData.GetData("strength", "modifier")) * amount;

                if (Session != null && Session.GetRoleplay() != null)
                {
                    Session.GetRoleplay().StrengthEXP += amount;

                    if (LevelUp(Session, "strength"))
                    {
                        Session.GetRoleplay().Strength += 1;
                        Session.Shout("*Se sente um pouco mais forte [+1 Força]*", 4);
                    }
                    else
                        Session.SendWhisper("Malhando: " + String.Format("{0:N0}", Session.GetRoleplay().StrengthEXP) + "/" + String.Format("{0:N0}", (StrengthLevels[Session.GetRoleplay().Strength + 1])), 1);
                }
            }
            catch
            {

            }
        }

        public static void AddStaminaEXP(GameClient Session, int amount)
        {
            try
            {
                amount = Convert.ToInt32(RoleplayData.GetData("stamina", "modifier")) * amount;

                if (Session != null && Session.GetRoleplay() != null)
                {
                    Session.GetRoleplay().StaminaEXP += amount;

                    if (LevelUp(Session, "stamina"))
                    {
                        Session.GetRoleplay().Stamina += 1;
                        Session.GetRoleplay().MaxEnergy = (100 + Session.GetRoleplay().Stamina * 5);
                        Session.Shout("*Se senta com mais vida [+1 Vigor]*", 4);
                    }
                    else
                        Session.SendWhisper("Malhando: " + String.Format("{0:N0}", Session.GetRoleplay().StaminaEXP) + "/" + String.Format("{0:N0}", (StaminaLevels[Session.GetRoleplay().Stamina + 1])), 1);
                }
            }
            catch
            {

            }
        }

        public static bool LevelUp(GameClient Session, string Type)
        {
            try
            {
                if (Session != null && Session.GetRoleplay() != null)
                {
                    int Level = 1;
                    int EXP = 0;
                    Dictionary<int, int> Dictionary = null;

                    switch (Type.ToLower())
                    {
                        case "level":
                            {
                                Level = Session.GetRoleplay().Level;
                                EXP = Session.GetRoleplay().LevelEXP;
                                Dictionary = Levels;
                                break;
                            }
                        case "intelligence":
                            {
                                Level = Session.GetRoleplay().Intelligence;
                                EXP = Session.GetRoleplay().IntelligenceEXP;
                                Dictionary = IntelligenceLevels;
                                break;
                            }
                        case "strength":
                            {
                                Level = Session.GetRoleplay().Strength;
                                EXP = Session.GetRoleplay().StrengthEXP;
                                Dictionary = StrengthLevels;
                                break;
                            }
                        case "stamina":
                            {
                                Level = Session.GetRoleplay().Stamina;
                                EXP = Session.GetRoleplay().StaminaEXP;
                                Dictionary = StaminaLevels;
                                break;
                            }
                    }

                    if (Dictionary == null)
                        return false;

                    if (Dictionary.ContainsKey(Level + 1))
                    {
                        if (EXP >= Dictionary[Level + 1])
                            return true;
                    }
                    return false;
                }
                return false;
            }
            catch
            {
                return false;
            }   
        }

        public static int IntelligenceChance(GameClient Session)
        {
            if (Session == null || Session.GetRoleplay() == null || Session.GetHabbo() == null)
                return 0;

            CryptoRandom Random = new CryptoRandom();
            int Intelligence = Session.GetRoleplay().Intelligence;
            int Multiplier = Random.Next(2, 5);

            if (Intelligence < 0)
                Intelligence = 0;

            if (Intelligence > RoleplayManager.IntelligenceCap)
                Intelligence = RoleplayManager.IntelligenceCap;

            return Intelligence * Multiplier;
        }
    }
}
