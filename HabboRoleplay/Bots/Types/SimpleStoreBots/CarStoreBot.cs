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
    public class CarStoreBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        private bool CancelWorkMovement = false;

        public CarStoreBot(int VirtualId)
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
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", Você precisa de algo?", true);
            else
                switch (Params[0].ToLower())
                {
                    #region Car
                    case "car":
					case "carro":
                        {
                            if (Client.GetRoleplay().CarType > 0)
                            {
                                string WhisperMessage = "Você já tem um carro!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = 800;
                                bool HasOffer = false;
                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "carro")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece um Toyota Corolla para " + Client.GetHabbo().Username + " por R$800,00*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("carro", 0, Cost, this);
                                        Client.SendWhisper("Você recebeu uma oferta de um Toyota Corolla por R$800,00! Digite ':aceitar carro' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Você já recebeu uma oferta de carro! Digite ':aceitar carro' para comprar por R$800,00!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Você não tem R$800,00 para comprar um carro!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                        }
                    #endregion

                    #region Car Upgrade
                    case "upgrade":
                    case "carupgrade":
					case "uparcarro":
					case "attcarro":
					case "upcarro":
					case "atcarro":
                        {
                            if (Client.GetRoleplay().CarType < 1)
                            {
                                string WhisperMessage = "Desculpe, mas você não tem um carro para atualizar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else if (Client.GetRoleplay().CarType > 2)
                            {
                                string WhisperMessage = "Desculpe, mas você já tem o carro mais alto que se pode obter!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = Client.GetRoleplay().CarType == 1 ? 800 : 1500;
                                bool HasOffer = false;
                                string CarName = RoleplayManager.GetCarName(Client, true);

                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "uparcarro")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece uma atualização de carro do seu " + CarName + " para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("uparcarro", 0, Cost, this);
                                        Client.SendWhisper("Você recebeu uma oferta de atualização para o seu " + CarName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar uparcarro' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Desculpe, mas você já recebeu uma oferta de atualização de carro!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Desculpe, mas você não pode pagar uma atualização de carro!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                        }
                    #endregion

                    #region Fuel
                    case "fuel":
					case "gasolina":
					case "combustivel":
                        {
                            int Amount;

                            if (Client.GetRoleplay().CarType < 1)
                            {
                                string WhisperMessage = "Desculpe, mas você não tem um carro para comprar combustível!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else if (Params.Length == 1)
                            {
                                string WhisperMessage = "Insira a quantidade de combustível que deseja comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }
                            else if (!int.TryParse(Params[1], out Amount))
                            {
                                string WhisperMessage = "Por favor insira uma quantidade válida de combustível que você gostaria de comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            if (Amount < 10)
                            {
                                string WhisperMessage = "Você precisa comprar pelo menos 10 galões de combustível de cada vez!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = Convert.ToInt32(Math.Floor((double)(Amount * 2) / 3));
                                bool HasOffer = false;

                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "gasolina")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece " + String.Format("{0:N0}", Amount) + " galões de gasolina " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("gasolina", 0, Amount, this);
                                        Client.SendWhisper("Você recebeu uma oferta de " + String.Format("{0:N0}", Amount) + " galões de gasolina por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar gasolina' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Desculpe, mas você já recebeu uma oferta de combustível!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Desculpe, mas não pode pagar esse combustível!";
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

            GetRoomUser().Chat("Que bom que você chegou, meu turno está feito. Até mais!", true);
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