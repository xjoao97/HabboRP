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

namespace Plus.HabboRoleplay.Bots.Types
{
    public class PhoneStoreBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        private bool CancelWorkMovement = false;

        public PhoneStoreBot(int VirtualId)
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

            if (GetBotRoleplay().WalkingToItem)
                return;

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();

            string[] Params = Message.Split(' ');

            if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você precisa de algo?", true);
            else
                switch (Params[0].ToLower())
                {
                    #region Phone
                    case "phone":
					case "celular":
					case "telefone":
                        {
                            if (Client.GetRoleplay().PhoneType > 0)
                            {
                                string WhisperMessage = "Você já possui um telefone!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = 50;
                                bool HasOffer = false;
                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "celular")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece um Nokia Tijolão para " + Client.GetHabbo().Username + " por R$50*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("celular", 0, Cost, this);
                                        Client.SendWhisper("Você recebeu uma oferta de um Nokia Tijolão por R$50! Digite ':aceitar celular' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Você já recebeu uma oferta de Celular! Digite ':aceitar celular' para comprar por R$50!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Você não tem R$50 para comprar um Celular!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                        }
                    #endregion

                    #region Phone Upgrade
                    case "upgrade":
                    case "phoneupgrade":
					case "uparcel":
					case "upartelefone":
					case "aceitarcelular":
					case "uparcelular":
					case "uparcelu":
					case "atualizarcel":
                        {
                            if (Client.GetRoleplay().PhoneType < 1)
                            {
                                string WhisperMessage = "Desculpe, mas você não tem um telefone para atualizar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else if (Client.GetRoleplay().PhoneType > 2)
                            {
                                string WhisperMessage = "Desculpe, mas você já tem o telefone mais alto que se pode comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = Client.GetRoleplay().PhoneType == 1 ? 500 : 1000;
                                bool HasOffer = false;
                                string PhoneName = RoleplayManager.GetPhoneName(Client, true);

                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "uparcel")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece uma atualização de celular para o " + PhoneName + " de " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("uparcel", 0, Cost, this);
                                        Client.SendWhisper("Você recebeu uma oferta de atualização de celular para o seu " + PhoneName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar uparcel' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Desculpe, mas você já recebeu uma atualização para o celular!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Desculpe, mas você não pode pagar uma atualização para o seu celular!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                        }
                    #endregion

                    #region Phone Credit
                    case "phonecredit":
                    case "phonecredits":
                    case "credit":
                    case "text":
                    case "texts":
                    case "credits":
					case "creditos":
					case "celcreditos":
                        {
                            int Amount;

                            if (Client.GetRoleplay().PhoneType < 1)
                            {
                                string WhisperMessage = "Desculpe, mas você não tem um telefone para comprar créditos!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else if (Params.Length == 1)
                            {
                                string WhisperMessage = "Digite a quantidade de crédito!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }
                            else if (!int.TryParse(Params[1], out Amount))
                            {
                                string WhisperMessage = "Insira uma quantidade válida de crédito do telefone que você gostaria de comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            if (Amount < 10)
                            {
                                string WhisperMessage = "Você precisa comprar pelo menos 10 créditos de telefone ao mesmo tempo!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = Convert.ToInt32(Math.Floor((double)Amount / 2));
                                bool HasOffer = false;

                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "creditos")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece " + String.Format("{0:N0}", Amount) + " créditos de celular para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("creditos", 0, Amount, this);
                                        Client.SendWhisper("Você recebeu uma oferta de " + String.Format("{0:N0}", Amount) + " phone créditos de celular por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar creditos' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Desculpe, mas você já recebeu uma oferta de créditos de celular!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Desculpe, mas você não pode pagar esses créditos de telefone!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
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

            GetRoomUser().Chat("Que bom que chegou! meu turno está feito. Até mais.", true);
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