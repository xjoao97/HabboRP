using System;
using System.Linq;
using System.Text;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Food;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Quests;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class DrinkServerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;

        public DrinkServerBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {

        }

        public override void OnDeath(GameClient Client)
        {

        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
            if (!OnDuty)
                return;
        }

        public override void OnUserEnterRoom(GameClient Client)
        {
            if (!OnDuty)
                return;

            if (!GetRoomUser().IsWalking)
            {
                // Look at the user 
            }
        }

        public override void OnUserUseTeleport(GameClient Client, object[] Params)
        {
            if (!OnDuty)
                return;

            if (Client == null) return;
            if (Client.GetRoomUser() == null) return;

            if (Client == GetBotRoleplay().UserFollowing || Client == GetBotRoleplay().UserAttacking)
                GetBotRoleplay().StartTeleporting(GetRoomUser(), GetRoom(), Params);
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            GameClient Client = User.GetClient();

            if (Client == null)
                return;
            HandleRequest(Client, Message);
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            if (User.GetClient() == null)
                return;
            HandleRequest(User.GetClient(), Message);
        }

        public override void OnMessaged(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();
            var ServableFoods = FoodManager.GetServableBotItems("drink");

            #region Serving Drinks
            if (Message.ToLower().Contains("serve") && Message.ToLower() != "serve")
            {
                string[] Params = Message.Split(' ');

                if (Params.Length == 1)
                    return;
                else
                {
                    if (Client.GetRoleplay().CurEnergy >= Client.GetRoleplay().MaxEnergy)
                    {
                        string WhisperMessage = "Sua energia já está cheia " + Client.GetHabbo().Username + "!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    if (GetBotRoleplay().WalkingToItem)
                    {
                        string WhisperMessage = "Eu já estou servindo alguém! Espera aí.";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    string DesiredFood = Params[1].ToLower();
                    var Food = FoodManager.GetDrink(DesiredFood);

                    if (Food == null)
                    {
                        string WhisperMessage = "Esta bebida não existe! Por favor digite 'bebidas' para ver o que posso lhe fornecer.";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    }
                    else
                    {
                        if (FoodManager.CanServe(Client.GetRoomUser()))
                            BeginServingFood(Food, Client);
                        else
                        {
                            string WhisperMessage = "Encontre uma mesa vazia para que eu sirva uma bebida para você!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        }
                    }
                }
            }
            #endregion

            else if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você precisava de algo?", true);
            else
                switch (Message.ToLower())
                {
                    #region Drinks List
                    case "drinks":
                    case "drink":
                    case "energy":
                    case "serve":
                        {
                            if (Client.GetRoleplay().CurEnergy >= Client.GetRoleplay().MaxEnergy)
                            {
                                string WhisperMessage = "Você não parece com sede " + Client.GetHabbo().Username + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            }
                            else
                            {
                                GetRoomUser().Chat("Bem vindo(a) " + Client.GetHabbo().Username + ", você está com muita sede! O que você gostaria de beber?", true);

                                StringBuilder FoodList = new StringBuilder().Append("<----- Bebidas Disponíveis ----->\n");
                                FoodList.Append("Para solicitar qualquer um dos seguintes, digite 'serve <nome da bebidas>', esteja em uma mesa vazia!\n\n");

                                foreach (var Food in ServableFoods.OrderBy(x => x.Cost))
                                {
                                    if (Food != null)
                                    {
                                        string RealName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

                                        FoodList.Append("<----- " + RealName + " ----->\n");
                                        FoodList.Append("Preço: $" + Food.Cost + " ---> Fome: -" + Food.Hunger + "\n");
                                        FoodList.Append("Sangue: +" + Food.Health + " ---> Energia: +" + Food.Energy + "\n\n");
                                    }
                                }

                                Client.SendMessage(new MOTDNotificationComposer(FoodList.ToString()));
                            }
                            break;
                        }
                        #endregion
                }
        }

        public override void StopActivities()
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("startwork"))
                GetBotRoleplay().TimerManager.ActiveTimers["startwork"].EndTimer();

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("serving"))
                GetBotRoleplay().TimerManager.ActiveTimers["serving"].EndTimer();

            GetRoomUser().Chat("Que bom que chegou, meu turno já foi feito. Até mais!", true);
            OnDuty = false;
            GetBotRoleplay().WalkingToItem = false;

            if (GetBotRoleplay().WorkUniform != "none")
                GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            Item Item;
            if (GetBotRoleplay().GetStopWorkItem(this.GetRoom(), out Item))
            {
                var Point = new System.Drawing.Point(Item.GetX, Item.GetY);
                GetRoomUser().MoveTo(Point);
                GetBotRoleplay().TimerManager.CreateTimer("stopwork", GetBotRoleplay(), 10, true, null);
            }
        }

        public override void StartActivities()
        {
            if (OnDuty)
                return;

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("stopwork"))
                GetBotRoleplay().TimerManager.ActiveTimers["stopwork"].EndTimer();

            GetBotRoleplay().Invisible = false;
            GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            GetRoomUser().Chat("Merda! Hora de voltar ao trabalho!", true);
            OnDuty = true;

            if (GetBotRoleplay().WorkUniform != "none")
                GetRoom().SendMessage(new UsersComposer(GetRoomUser()));

            Item Item;
            if (GetBotRoleplay().GetStopWorkItem(this.GetRoom(), out Item))
            {
                var ItemPoint = new System.Drawing.Point(Item.GetX, Item.GetY);
                if (GetRoomUser().Coordinate == ItemPoint)
                {
                    Item.ExtraData = "2";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(2, true);
                }
            }

            var Point = new System.Drawing.Point(GetBotRoleplay().oX, GetBotRoleplay().oY);
            GetRoomUser().MoveTo(Point);
            GetBotRoleplay().TimerManager.CreateTimer("startwork", GetBotRoleplay(), 10, true, null);
        }

        public void BeginServingFood(Food.Food Food, GameClient Client)
        {
            if (!OnDuty)
                return;

            if (Client.GetRoleplay() == null)
                return;

            if (Client.GetRoomUser() == null)
                return;

            if (Client.LoggingOut)
                return;

            if (Client.GetRoleplay().CurEnergy >= Client.GetRoleplay().MaxEnergy)
                return;

            string RealName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

            GetBotRoleplay().WalkingToItem = true;
            GetRoomUser().Chat("Servindo " + Client.GetHabbo().Username + "! Um copo de " + RealName + " estou indo aí!", true);

            var UserPoint = new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y);
            var ServePoint = new System.Drawing.Point(Client.GetRoomUser().SquareBehind.X, Client.GetRoomUser().SquareBehind.Y);

            object[] Params = { Client, Food, ServePoint, UserPoint, RealName };

            GetRoomUser().MoveTo(ServePoint);
            GetBotRoleplay().TimerManager.CreateTimer("serving", GetBotRoleplay(), 10, true, Params);
        }
    }
}