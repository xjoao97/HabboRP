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
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Combat;
using Plus.HabboHotel.Quests;
using System.Collections.Concurrent;
using static Plus.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;
using Plus.HabboRoleplay.Bots.Manager.TimerHandlers;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class FoodServerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        public ConcurrentDictionary<GameClient, ConcurrentDictionary<object, object>> ServingQueue = new ConcurrentDictionary<GameClient, ConcurrentDictionary<object, object>>();

        public FoodServerBot(int VirtualId)
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

        public override void OnTimerTick()
        {
            IBotHandler ServingHandler;
            if (this.GetBotData().TryGetHandler(Handlers.FOODSERVE, out ServingHandler))
            {
                if (ServingHandler.Active)
                    return;

                ServingHandler.ExecuteHandler(this.ServingQueue);
            }
        }

        public void BeginServingFood(Food.Food Food, GameClient Client)
        {

            #region Checks
            if (!OnDuty)
                return;

            if (Client.GetRoleplay() == null)
                return;

            if (Client.GetRoomUser() == null)
                return;

            if (Client.LoggingOut)
                return;

            if (Client.GetRoleplay().Hunger <= 0)
                return;
            #endregion

            string RealName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

            this.GetBotRoleplay().WalkingToItem = true;
            this.GetRoomUser().Chat("Espere aí " + Client.GetHabbo().Username + "! Estou indo te servir um(a) " + RealName + " !", true);

            var UserPoint = new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y);
            var ServePoint = new System.Drawing.Point(Client.GetRoomUser().SquareBehind.X, Client.GetRoomUser().SquareBehind.Y);

            object[] Params = { Client, Food, ServePoint, UserPoint, RealName };

            
            IBotHandler ServingHandler;
            if (!this.GetBotRoleplay().TryGetHandler(Handlers.FOODSERVE, out ServingHandler))
                this.GetBotRoleplay().StartHandler(Handlers.FOODSERVE, out ServingHandler, Params);
            else
                ServingHandler.Active = true;
                

            this.GetRoomUser().MoveTo(ServePoint);

            GetBotRoleplay().TimerManager.CreateTimer("serving", GetBotRoleplay(), 10, true, Params);
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();
            var ServableFoods = FoodManager.GetServableBotItems("food");

            #region Serving Food
            if (Message.ToLower().Contains("serve") && Message.ToLower() != "serve")
            {
                string[] Params = Message.Split(' ');

                if (Params.Length == 1)
                    return;
                else
                {
                    if (Client.GetRoleplay().Hunger <= 0)
                    {
                        string WhisperMessage = "Você não parece com fome " + Client.GetHabbo().Username + "!";
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
                    var Food = FoodManager.GetFood(DesiredFood);

                    if (Food == null)
                    {
                        string WhisperMessage = "Este alimento não existe! Digite 'comidas' para ver o que posso lhe fornecer.";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                    }
                    else
                    {
                        if (FoodManager.CanServe(Client.GetRoomUser()))
                            BeginServingFood(Food, Client);
                        else
                        {
                            string WhisperMessage = "Encontre uma mesa vazia para que eu sirva comida para você!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        }
                    }
                }
            }
            #endregion

            else if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você precisa de algo?", true);
            else
                switch (Message.ToLower())
                {
                    #region Food List
                    case "food":
                    case "hunger":
                    case "serve":
					case "sirva":
                        {
                            if (Client.GetRoleplay().Hunger <= 0)
                            {
                                string WhisperMessage = "Você não parece com fome " + Client.GetHabbo().Username + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            }
                            else
                            {
                                GetRoomUser().Chat("Bem vindo(a) " + Client.GetHabbo().Username + ", você parece com fome! O que você gostaria de comer?", true);

                                StringBuilder FoodList = new StringBuilder().Append("<----- Comidas Disponíveis ----->\n");
                                FoodList.Append("Para solicitar qualquer uma das comidas, digite 'serve <nome da comida>', esteja em uma mesa vazia!\n\n");

                                foreach (var Food in ServableFoods.OrderBy(x => x.Cost))
                                {
                                    if (Food != null)
                                    {
                                        string RealName = Food.Name.Substring(0, 1).ToUpper() + Food.Name.Substring(1);

                                        FoodList.Append("<----- " + RealName + " ----->\n");
                                        FoodList.Append("Preço: R$" + Food.Cost + " <---> Fome: -" + Food.Hunger + "\n");
                                        FoodList.Append("Sangue: +" + Food.Health + " <---> Energia: +" + Food.Energy + "\n\n");
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

            GetRoomUser().Chat("Que bom que você chegou, meu turno está feito, até mais!", true);
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

    }
}