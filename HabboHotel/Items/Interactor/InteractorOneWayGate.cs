using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboRoleplay.Games;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorOneWayGate : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;
            
            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (Item == null || User == null)
                return;

            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (Item.GetBaseItem().InteractionType == InteractionType.ONE_WAY_GATE)
            {
                if (Item.GetBaseItem().ItemName == "one_way_door*2")
                {
                    if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.Brawl))
                    {
                        Session.SendWhisper("Não há nenhum evento de briga no momento!", 1);
                        return;
                    }
                }
                else if (Item.GetBaseItem().ItemName == "one_way_door*5")
                {
                    if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.SoloQueue))
                        RoleplayGameManager.CreateGame("soloqueue");
                }
                else if (Item.GetBaseItem().ItemName == "one_way_door*6")
                {
                    if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.SoloQueueGuns))
                        RoleplayGameManager.CreateGame("soloqueueguns");
                }

                if (User.Coordinate != Item.SquareInFront && User.CanWalk)
                {
                    User.MoveTo(Item.SquareInFront);
                    return;
                }
                if (!Item.GetRoom().GetGameMap().ValidTile(Item.SquareBehind.X, Item.SquareBehind.Y) || !Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, false) || !Item.GetRoom().GetGameMap().SquareIsOpen(Item.SquareBehind.X, Item.SquareBehind.Y, false))
                    return;

                if ((User.LastInteraction - PlusEnvironment.GetUnixTimestamp() < 0) && User.InteractingGate && User.GateId == Item.Id)
                {
                    User.InteractingGate = false;
                    User.GateId = 0;
                }

                if (!Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, User.AllowOverride))
                    return;

                if (Item.InteractingUser == 0)
                {
                    #region Brawl
                    if (Item.GetBaseItem().ItemName == "one_way_door*2")
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.Brawl))
                        {
                            if (Session.GetRoleplay().Game != null && Session.GetRoleplay().Game.GetGameMode() == GameMode.Brawl)
                            {
                                Session.SendWhisper("Você já está no evento Briga!", 1);
                                return;
                            }
                            else
                            {
                                if (Session.GetRoleplay().IsWorking)
                                    Session.GetRoleplay().IsWorking = false;

                                if (RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.Brawl), Session, "") != "OK")
                                    return;

                                if (Session.GetRoleplay().EquippedWeapon != null)
                                    Session.GetRoleplay().EquippedWeapon = null;

                                if (Session.GetRoleplay().InsideTaxi)
                                    Session.GetRoleplay().InsideTaxi = false;
                            }
                        }
                        else
                            return;
                    }
                    #endregion

                    #region SoloQueue
                    else if (Item.GetBaseItem().ItemName == "one_way_door*5")
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.SoloQueue))
                        {
                            if (Session.GetRoleplay().Game != null && Session.GetRoleplay().Game.GetGameMode() == GameMode.SoloQueue)
                            {
                                Session.SendWhisper("Você já está no Evento SoloQueue!", 1);
                                return;
                            }
                            else if (Session.GetRoleplay().IsWanted)
                            {
                                Session.SendWhisper("Você não pode completar esta ação enquanto você é procurado!", 1);
                                return;
                            }
                            else
                            {
                                if (Session.GetRoleplay().IsWorking)
                                    Session.GetRoleplay().IsWorking = false;

                                if (RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.SoloQueue), Session, "") != "OK")
                                    return;

                                if (Session.GetRoleplay().EquippedWeapon != null)
                                    Session.GetRoleplay().EquippedWeapon = null;

                                if (Session.GetRoleplay().InsideTaxi)
                                    Session.GetRoleplay().InsideTaxi = false;
                            }
                        }
                        else
                            return;
                    }
                    #endregion

                    #region SoloQueue Guns
                    else if (Item.GetBaseItem().ItemName == "one_way_door*6")
                    {
                        if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.SoloQueueGuns))
                        {
                            if (Session.GetRoleplay().Game != null && Session.GetRoleplay().Game == RoleplayGameManager.GetGame(GameMode.SoloQueueGuns))
                            {
                                Session.SendWhisper("Você já está no evento SoloQueue!", 1);
                                return;
                            }
                            else if (Session.GetRoleplay().IsWanted)
                            {
                                Session.SendWhisper("Você não pode completar esta ação enquanto você é procurado!", 1);
                                return;
                            }
                            else
                            {
                                if (Session.GetRoleplay().IsWorking)
                                    Session.GetRoleplay().IsWorking = false;

                                if (RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.SoloQueueGuns), Session, "") != "OK")
                                    return;

                                if (Session.GetRoleplay().InsideTaxi)
                                    Session.GetRoleplay().InsideTaxi = false;
                            }
                        }
                        else
                            return;
                    }
                    #endregion

                    User.InteractingGate = true;
                    User.GateId = Item.Id;
                    Item.InteractingUser = User.HabboId;

                    User.CanWalk = false;

                    if (User.IsWalking && (User.GoalX != Item.SquareInFront.X || User.GoalY != Item.SquareInFront.Y))
                        User.ClearMovement(true);

                    User.AllowOverride = true;
                    User.MoveTo(Item.Coordinate);

                    Item.RequestUpdate(4, true);
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}