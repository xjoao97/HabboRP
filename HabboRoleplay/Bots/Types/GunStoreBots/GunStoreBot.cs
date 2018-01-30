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
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class GunStoreBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;
        private bool CancelWorkMovement = false;

        public GunStoreBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {
            OnDuty = true;
            this.StartActivities();
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

            string MessageType = Message;
            string[] Params = MessageType.Split(' ');

            Weapon purchaseweapon = null;
            foreach (var Weapon in WeaponManager.Weapons.Values)
            {
                if (Message.ToLower() == Weapon.Name.ToLower())
                {
                    Params[0] = "comprar";
                    purchaseweapon = Weapon;
                }
            }

            if (MessageType.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você precisa de algo? Digite Armas!", true);
            else
                switch (Params[0].ToLower())
                {
                    #region Weapons List
                    case "gun":
                    case "weapon":
                    case "guns":
                    case "weapons":
					case "armas":
                        {
                            string WhisperMessage = "Aqui uma lista de todas as armas que vendemos!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));

                            StringBuilder WeaponsList = new StringBuilder().Append("<----- Armas Disponíveis ----->\n\n");
                            foreach (var weapon in WeaponManager.Weapons.Values.OrderBy(x => x.LevelRequirement))
                            {
                                WeaponsList.Append("[" + weapon.Name + "] " + weapon.PublicName + " <--- Preço: R$" + String.Format("{0:N0}", weapon.Cost) + " <--- Level Requerido: " + weapon.LevelRequirement + "\n");
                                WeaponsList.Append("Damage: " + weapon.MinDamage + " - " + weapon.MaxDamage + "\n");
                                WeaponsList.Append("Alcance: " + weapon.Range + " <---> Tiros seguidos: " + weapon.ClipSize + "\n\n");
                            }

                            Client.SendMessage(new MOTDNotificationComposer(WeaponsList.ToString()));
                            break;
                        }
                    #endregion

                    #region Purchasing Weapons
                    case "purchaseweapon":
					case "comprar":
                        {
                            if (purchaseweapon == null)
                                break;

                            if (purchaseweapon.Stock < 1)
                            {
                                if (RoleplayManager.CalledDelivery)
                                {
                                    string WhisperMessage2 = "O entregador já está a caminho!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage2, 0, 2));
                                    break;
                                }

                                if (RoleplayManager.DeliveryWeapon != null)
                                {
                                    if (!RoleplayManager.CalledDelivery)
                                    {
                                        if (GetRoom() != null && GetRoom().GetRoomItemHandler() != null)
                                        {
                                            var Item = GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.DELIVERY_BOX).FirstOrDefault();

                                            if (Item != null)
                                            {
                                                if (!GetBotRoleplay().WalkingToItem && !GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("deliverywait"))
                                                    GetBotRoleplay().TimerManager.CreateTimer("pickupdelivery", GetBotRoleplay(), 10, true);
                                                else
                                                {
                                                    string WhisperMessage2 = "Dê-me um momento para abrir a caixa!";
                                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage2, 0, 2));
                                                }
                                            }
                                            else
                                            {
                                                string WhisperMessage2 = "O entregador já está a caminho!";
                                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage2, 0, 2));
                                            }
                                        }
                                    }
                                    break;
                                }

                                string WhisperMessage = "Não há" + purchaseweapon.PublicName + " sobrando no estoque! Deixe-me chamar rapidamente o entregador para obter alguns!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));

                                new Thread(() =>
                                {
                                    if (GetRoomUser() != null)
                                    {
                                        GetRoomUser().Chat("*Pega seu telefone e chama o entregador, ordenando um novo estoque de " + purchaseweapon.PublicName + "*", true);
                                        GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                                    }

                                    Thread.Sleep(3000);

                                    if (GetRoomUser() != null)
                                        GetRoomUser().ApplyEffect(0);
                                }).Start();

                                RoleplayManager.CalledDelivery = true;
                                RoleplayManager.DeliveryWeapon = purchaseweapon;

                                GetBotRoleplay().TimerManager.CreateTimer("deliverywait", GetBotRoleplay(), 10, true);
                                return;
                            }
                            else if (Client.GetRoleplay().OwnedWeapons.ContainsKey(purchaseweapon.Name) && Client.GetRoleplay().OwnedWeapons[purchaseweapon.Name].CanUse)
                            {
                                string WhisperMessage = "Você já tem uma " + purchaseweapon.PublicName + "!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = (!Client.GetRoleplay().OwnedWeapons.ContainsKey(purchaseweapon.Name) ? purchaseweapon.Cost : purchaseweapon.CostFine);
                                bool HasOffer = false;
                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                                            HasOffer = true;
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece uma " + purchaseweapon.PublicName + " para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer(purchaseweapon.Name.ToLower(), 0, Cost, this);
                                        Client.SendWhisper("Você recebeu uma oferta de " + purchaseweapon.PublicName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar arma' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Você já recebeu uma oferta dessas, não posso oferecer outra!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Você não pode pagar um " + purchaseweapon.PublicName + "!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }
                            }
                        }
                    #endregion

                    #region Bullets
                    case "bullets":
					case "balas":
                        {
                            int Amount;

                            if (Params.Length == 1)
                            {
                                string WhisperMessage = "Digite a quantidade de balas que você gostaria de comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                return;
                            }
                            else if (!int.TryParse(Params[1], out Amount))
                            {
                                string WhisperMessage = "Digite uma quantidade válida de balas que você gostaria de comprar!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }

                            if (Amount < 5)
                            {
                                string WhisperMessage = "Você precisa comprar pelo menos 5 balas por vez!";
                                Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                break;
                            }
                            else
                            {
                                int Cost = Convert.ToInt32(Math.Floor((double)Amount / 1));
                                bool HasOffer = false;

                                if (Client.GetHabbo().Credits >= Cost)
                                {
                                    foreach (var Offer in Client.GetRoleplay().OfferManager.ActiveOffers.Values)
                                    {
                                        if (Offer.Type.ToLower() == "balas")
                                        {
                                            HasOffer = true;
                                        }
                                    }
                                    if (!HasOffer)
                                    {
                                        GetRoomUser().Chat("*Oferece " + String.Format("{0:N0}", Amount) + " balas para " + Client.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", true);
                                        Client.GetRoleplay().OfferManager.CreateOffer("balas", 0, Amount, this);
                                        Client.SendWhisper("Você recebeu uma oferta de " + String.Format("{0:N0}", Amount) + " balas por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar balas' para comprar!", 1);
                                        break;
                                    }
                                    else
                                    {
                                        string WhisperMessage = "Desculpe, mas você já recebeu balas!";
                                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                        break;
                                    }
                                }
                                else
                                {
                                    string WhisperMessage = "Desculpe, mas não pode pagar essas balas!";
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

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("pickupdelivery"))
                GetBotRoleplay().TimerManager.ActiveTimers["pickupdelivery"].EndTimer();

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("deliverywait"))
                GetBotRoleplay().TimerManager.ActiveTimers["deliverywait"].EndTimer();

            GetRoomUser().Chat("Que bom que chegou!", true);
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

            GetRoomUser().Chat("Ok, hora de voltar ao trabalho!", true);
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