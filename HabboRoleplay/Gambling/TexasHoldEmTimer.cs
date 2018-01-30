using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Gambling;
using Plus.Core;
using System.Linq;
using System.Collections.Concurrent;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown for a Texas Hold 'Em Game
    /// </summary>
    public class TexasHoldEmTimer : SystemRoleplayTimer
    {
        public TexasHoldEmTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Ticks timer for Texas Hold 'Em Game
        /// </summary>
        public override void Execute()
        {
            try
            {
                TexasHoldEm Game = (TexasHoldEm)Params[0];

                if (Game == null || !Game.GameStarted || !TexasHoldEmManager.GameList.ContainsKey(Game.GameId))
                {
                    base.EndTimer();
                    return;
                }

                if (Game.PlayerList.Values.Where(x => x != null && x.UserId > 0).ToList().Count <= 0)
                {
                    Game.RemovePotFurni();
                    Game.ResetGame();
                    base.EndTimer();
                    return;
                }

                if (Game.PlayerList.Values.Where(x => x != null && x.UserId > 0).ToList().Count == 1)
                {
                    Game.EndGame(Game.PlayerList.Keys.FirstOrDefault());
                    base.EndTimer();
                    return;
                }

                if (CheckPlayers(Game))
                    return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }

        public bool CheckPlayers(TexasHoldEm Game)
        {
            bool Failed = false;

            try
            {
                if (Game == null || !Game.GameStarted || !TexasHoldEmManager.GameList.ContainsKey(Game.GameId))
                    return true;

                foreach (var Pair in Game.PlayerList)
                {
                    if (Pair.Value == null || Pair.Value.UserId <= 0)
                        continue;

                    GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Pair.Value.UserId);

                    if (Player == null || Player.GetHabbo() == null || Player.GetRoleplay() == null || Player.GetRoleplay().TexasHoldEmPlayer <= 0 || Player.GetRoomUser() == null || Player.GetRoomUser().GetRoom() == null)
                    {
                        if (Player != null && Player.GetRoleplay() != null)
                            Player.GetRoleplay().TexasHoldEmPlayer = 0;

                        if (Game.PlayersTurn == Pair.Key)
                            Game.ChangeTurn();

                        ConcurrentDictionary<int, TexasHoldEmItem> Data;
                        if (Pair.Key == 1)
                            Data = Game.Player1;
                        else if (Pair.Key == 2)
                            Data = Game.Player2;
                        else
                            Data = Game.Player3;

                        if (Data != null)
                        {
                            foreach (var Item in Data.Values)
                            {
                                if (Item.Furni != null)
                                {
                                    Item.Furni.ExtraData = "0";
                                    Item.Furni.UpdateState(false, true);
                                }

                                Item.Rolled = false;
                                Item.Value = 0;
                            }
                        }


                        Game.RemoveBetFurni(Pair.Key);
                        Game.PlayerList.TryUpdate(Pair.Key, new TexasHoldEmPlayer(0, Game.PlayerList[Pair.Key].CurrentBet, Game.PlayerList[Pair.Key].TotalAmount), Pair.Value);
                        Failed = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in CheckPlayers() void: " + e);
            }

            return Failed;
        }
    }
}