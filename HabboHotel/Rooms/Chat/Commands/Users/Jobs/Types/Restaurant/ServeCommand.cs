using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.Food;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Restaurant
{
    class ServeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_restaurant_serve"; }
        }

        public string Parameters
        {
            get { return "%name%"; }
        }

        public string Description
        {
            get { return "Serve a comida ou bebida desejada diante de você."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            RoomUser User = Session.GetRoomUser();
            Group Group = GroupManager.GetJob(Session.GetRoleplay().JobId);
            GroupRank GroupRank = GroupManager.GetJobRank(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank);
            #endregion

            #region Conditions
            if (User == null)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite [:servir (item)]! Você só pode servir os seguintes itens: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            string FoodName = Params[1].ToString();
            Food Food = FoodManager.GetFoodAndDrink(FoodName);

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para fazer isso!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode servir alimentos ou bebidas enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode servir alimentos ou bebidas enquanto está preso!", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "serve"))
            {
                Session.SendWhisper("Você não tem permissão para usar este comando!", 1);
                return;
            }

            if (!GroupRank.CanWorkHere(Session.GetHabbo().CurrentRoomId))
            {
                Session.SendWhisper("Você não trabalha aqui! Seu(s) quarto de trabalho é [Quarto ID(s): " + String.Join(",", GroupRank.WorkRooms) + "]", 1);
                return;
            }

            if (Food == null)
            {
                Session.SendWhisper("Este não é um tipo de alimento ou bebida válido! Você só pode servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            if (!FoodManager.CanServe(User))
            {
                Session.SendWhisper("Encontre uma mesa agradável para servir!", 1);
                return;
            }

            if (Food.Type == "food" && !GroupManager.HasJobCommand(Session, "food"))
            {
                Session.SendWhisper("Desculpe! Você só pode servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            if (Food.Type == "drink" && !GroupManager.HasJobCommand(Session, "drinks"))
            {
                Session.SendWhisper("Desculpe! Você só pode servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            if (!Food.Servable)
            {
                if (Food.Type == "drink")
                    Session.SendWhisper("Desculpe! Você só pode servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                else
                    Session.SendWhisper("Desculpe! Você só pode servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }
            #endregion

            #region Execute
            double MaxHeight = 0.0;
            Item ItemInFront;
            if (Room.GetGameMap().GetHighestItemForSquare(User.SquareInFront, out ItemInFront))
            {
                if (ItemInFront != null)
                    MaxHeight = ItemInFront.TotalHeight;
            }

            Session.Shout(Food.ServeText, 4);
            RoleplayManager.PlaceItemToRoom(Session, Food.ItemId, 0, User.SquareInFront.X, User.SquareInFront.Y, MaxHeight, User.RotBody, false, Room.Id, false, Food.ExtraData, true);
            #endregion
        }
    }
}