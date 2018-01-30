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
using Plus.HabboHotel.Quests;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class EatCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_eat"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Coma o prato de comida em frente de você."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            bool Stolen = false;
            int FoodId = 0;
            Item Item = null;
            RoomUser User = Session.GetRoomUser();
            #endregion

            #region Conditions
            if (User == null)
                return;

            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetX == User.SquareInFront.X && item.GetY == User.SquareInFront.Y)
                {
                    if (FoodManager.GetFood(item.BaseItem) != null)
                    {
                        Item = item;
                        FoodId = item.BaseItem;
                    }
                }
            }

            Food Food = FoodManager.GetFood(FoodId);

            if (Food == null || Item == null)
            {
                Session.SendWhisper("Não há comida na sua frente!", 1);
                return;
            }

            if (Food.Type != "food")
            {
                if (Food.Type == "drink")
                {
                    Session.SendWhisper("Use o comando :beber para beber isso!", 1);
                    return;
                }
                else
                {
                    Session.SendWhisper("Você não pode comer isso!", 1);
                    return;
                }
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode comer enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode comer enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().Hunger <= 0)
            {
                Session.SendWhisper("Seu estômago está cheio demais para ter mais comida!", 1);
                return;
            }

            if (Food.Cost > 0)
            {
                if (Session.GetHabbo().Credits < Food.Cost)
                {
                    if (Session.GetRoleplay().RobItem != Item)
                    {
                        Session.GetRoleplay().RobItem = Item;
                        Session.SendWhisper("Você não tem dinheiro suficiente para comprar essa comida! Tente novamente...", 1);
                        return;
                    }
                    else
                    {
                        Session.GetRoleplay().RobItem = null;
                        Session.SendWhisper("Você comeu sem pagar por! Fique atento se alguém o viu!", 1);
                        Stolen = true;
                    }
                }
            }
            #endregion

            #region Execute
            string EatText = Food.EatText;

            if (Food.Health == 0)
                EatText = EatText.Replace("[HEALTH]", "");
            else
                EatText = EatText.Replace("[HEALTH]", "[+" + Food.Health + " HP]");

            if (Food.Energy == 0)
                EatText = EatText.Replace("[ENERGY]", "");
            else
                EatText = EatText.Replace("[ENERGY]", "[+" + Food.Energy + " Energia]");

            if (Food.Cost == 0)
                EatText = EatText.Replace("[COST]", "");
            else
                EatText = EatText.Replace("[COST]", "[-$" + Food.Cost + "]");

            if (Food.Hunger == 0)
                EatText = EatText.Replace("[HUNGER]", "");
            else
                EatText = EatText.Replace("[HUNGER]", "[-" + Food.Hunger + " Fome]");

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Eating", 1);
            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.EAT_FOOD);

            if (Food.Hunger > 0)
            {
                int HungerChange = Session.GetRoleplay().Hunger - Food.Hunger;

                if (HungerChange <= 0)
                    Session.GetRoleplay().Hunger = 0;
                else
                    Session.GetRoleplay().Hunger = HungerChange;
            }

            if (Food.Energy > 0)
            {
                int EnergyChange = Session.GetRoleplay().CurEnergy + Food.Energy;

                if (EnergyChange >= Session.GetRoleplay().MaxEnergy)
                    Session.GetRoleplay().CurEnergy = Session.GetRoleplay().MaxEnergy;
                else
                    Session.GetRoleplay().CurEnergy = EnergyChange;
            }

            if (Food.Health > 0)
            {
                int HealthChange = Session.GetRoleplay().CurHealth + Food.Health;

                if (HealthChange >= Session.GetRoleplay().MaxHealth)
                    Session.GetRoleplay().CurHealth = Session.GetRoleplay().MaxHealth;
                else
                    Session.GetRoleplay().CurHealth = HealthChange;
            }

            if (Food.Cost > 0 && !Stolen)
            {
                Session.GetHabbo().Credits -= Food.Cost;
                Session.GetHabbo().UpdateCreditsBalance();
            }

            string FoodName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

            if (Stolen)
            {
                Session.Shout("*Rapidamente come " + FoodName + " sem pagar*", 4);

                if (!Session.GetRoleplay().WantedFor.Contains("roubando comida"))
                    Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "roubando comida, ";
            }
            else
                Session.Shout(EatText, 4);

            if (Item.InteractingUser > 0 && !Stolen)
            {
                var Server = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Item.InteractingUser);

                if (Server != null)
                {
                    if (Session != Server)
                    {
                        Server.GetHabbo().Credits += 1;
                        Server.GetHabbo().UpdateCreditsBalance();
                        Server.SendWhisper("Você ganhou R$1 por servir comida para " + Session.GetHabbo().Username + "!", 1);
                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Server, "ACH_ServingFood", 1);
                    }
                }

                Item.InteractingUser = 0;
            }

            Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);
            #endregion
        }
    }
}