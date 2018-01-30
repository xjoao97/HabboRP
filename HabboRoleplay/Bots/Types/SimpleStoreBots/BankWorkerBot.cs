using System;
using System.Linq;
using System.Text;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Combat;
using Plus.HabboHotel.Quests;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class BankWorkerBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        private bool CancelWorkMovement = false;

        public BankWorkerBot(int VirtualId)
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
            int Amount = CombatManager.GetCombatType("fist").GetCoins(null, GetBotRoleplay());

            Client.GetHabbo().Credits += Amount;
            Client.GetHabbo().UpdateCreditsBalance();

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Client, QuestType.KILL_USER);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Kills", 1);

            Client.GetRoleplay().Kills++;
            Client.GetRoleplay().HitKills++;

            CryptoRandom Random = new CryptoRandom();
            int Multiplier = 1;

            int Chance = Random.Next(1, 101);

            if (Chance <= 16)
            {
                if (Chance <= 8)
                    Multiplier = 3;
                else
                    Multiplier = 2;
            }

            LevelManager.AddLevelEXP(Client, CombatManager.GetCombatType("fist").GetEXP(Client, null, GetBotRoleplay()) * Multiplier);

            if (Amount > 0)
                RoleplayManager.Shout(Client, "*Dá um soco em " + GetBotRoleplay().Name + ", matando-o e roubando R$" + Amount + " da sua carteira*", 6);
            else
                RoleplayManager.Shout(Client, "*Dá um soco em " + GetBotRoleplay().Name + ", matando-o, mas não consegue roubar pois a carteira está vazia*", 6);


            GetBotRoleplay().InitiateDeath();
        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

            GetBotRoleplay().UserAttacking = Client;

            if (!GetBotRoleplay().ActiveTimers.ContainsKey("attack"))
            {

                GetBotRoleplay().ActiveTimers.TryAdd("attack", GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id));

                if (GetBotRoleplay().UserAttacking == null)
                    GetRoomUser().Chat("Seu filho da puta, eu vou pegar você " + Client.GetHabbo().Username + "!", true, 4);
            }
            else
            {
                if (GetBotRoleplay().ActiveTimers["attack"] == null)
                    GetBotRoleplay().ActiveTimers["attack"] = GetBotRoleplay().TimerManager.CreateTimer("attack", GetBotRoleplay(), 10, true, Client.GetHabbo().Id);
            }

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

            if (GetBotRoleplay().WalkingToItem)
                return;

            string Name = GetBotRoleplay().Name.ToLower();

            if (RespondToSpeech(Client, Message))
                return;

            if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você precisa de algo? [Poupanca, Cheques]", true);
            else
                switch (Message.ToLower())
                {
                    #region Chequings Account
                    case "chequings":
                    case "checkings":
					case "cheques":
                        {
                            if (Client.GetRoleplay().BankAccount > 0)
                            {
                                string WhisperMessage = "Você já tem uma Conta de Cheques!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            Message = "cheques";
                            bool HasOffer = false;
                            foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == Message.ToLower())
                                    HasOffer = true;
                            }
                            if (!HasOffer)
                            {
                                GetRoomUser().Chat("Oferece para abrir uma Conta de Cheques para " + Client.GetHabbo().Username + " de graça*", true);
                                Client.GetRoleplay().OfferManager.CreateOffer("cheques", 0, 0, this);
                                Client.SendWhisper("Você recebeu uma oferta para abrir uma Conta de Cheques de graça! Digite ':aceitar cheques' para confirmar!", 1);
                                break;
                            }
                            else
                            {
                                string WhisperMessage = "Você já recebeu uma oferta de Conta de Cheques!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                        }
                    #endregion

                    #region Savings Account
                    case "savings":
					case "poupanca":
                        {
                            int Cost = 100;
                            if (Client.GetRoleplay().BankAccount > 1)
                            {
                                string WhisperMessage = "Você já tem uma Conta Poupança!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            bool HasOffer = false;
                            if (Client.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Message.ToLower())
                                        HasOffer = true;
                                }
                                if (!HasOffer)
                                {
                                    GetRoomUser().Chat("*Oferece uma abertura de Conta Poupança para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                    Client.GetRoleplay().OfferManager.CreateOffer("poupanca", 0, Cost, this);
                                    Client.SendWhisper("Você recebeu uma oferta para abrir uma Conta Poupança por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar poupanca' para confirmar!", 1);
                                    break;
                                }
                                else
                                {
                                    string WhisperMessage = "Você não tem uma oferta de Conta Poupança!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                            else
                            {
                                string WhisperMessage = "Você não pode pagar uma Conta Poupança, isso custa R$" + String.Format("{0:N0}", Cost) + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
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

            GetRoomUser().Chat("Que bom que chegou. Meu turno está feito. Até mais!", true);
            OnDuty = false;

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