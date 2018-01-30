using System;
using System.Linq;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Quests;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class HospitalBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;

        public HospitalBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {
            OnDuty = false;
            this.StartActivities();
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
                RoleplayManager.Shout(Client, "*Soco em " + GetBotRoleplay().Name + ", nocauteando e roubando R$" + Amount + " de sua carteira*", 6);
            else
                RoleplayManager.Shout(Client, "*Soco em " + GetBotRoleplay().Name + ", nocauteando mas não rouba nada na carteira vazia*", 6);


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
                    GetRoomUser().Chat("Seu idiota! Eu vou pegar você " + Client.GetHabbo().Username + "!", true, 4);
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

            if (RespondToSpeech(Client, Message))
                return;

            string Name = GetBotRoleplay().Name.ToLower();

            string[] keys = new string[] { "cura", "ajuda", "me cura", "sangue", "me ajuda" };
            string sKeyResult = keys.FirstOrDefault<string>(s => Message.Contains(s));

            if (sKeyResult == null)
                return;

            if (Message.ToLower() == Name)
                GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", me diga do que precisa?", true);
            else
                switch (sKeyResult.ToLower())
                {
                    #region Healing
                    case "cura":
                    case "ajuda":
                    case "me cura":
                    case "sangue":
                    case "me ajuda":
                        {
                            if (Client.GetRoleplay().IsDead)
                            {
                                if (Client.GetRoleplay().DeadTimeLeft <= 1 || Client.GetRoleplay().BeingHealed)
                                {
                                    string WhisperMessage = "Você já irá reviver, não precisa da minha ajuda.";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }

                                if (!GetBotRoleplay().WalkingToItem)
                                    InitiateDischarge(Client);
                                else
                                {
                                    string WhisperMessage = "Eu já estou a caminho de ajudar alguém! Aguarde até eu estar livre.";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                }
                            }
                            else
                            {
                                if (Client.GetRoleplay().CurHealth >= Client.GetRoleplay().MaxHealth)
                                {
                                    string WhisperMessage = "Seu sangue já está cheio!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }

                                if (Client.GetRoleplay().BeingHealed)
                                {
                                    string WhisperMessage = "Seu sangue já está cheio!";
                                    Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                                    break;
                                }

                                GetRoomUser().Chat("Espero que você se sinta bem logo, " + Client.GetHabbo().Username + "!", true);
                                Client.GetRoleplay().BeingHealed = true;

                                Client.GetRoleplay().TimerManager.CreateTimer("heal", 1000, false);
                                break;
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

            if (GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("discharge"))
                GetBotRoleplay().TimerManager.ActiveTimers["discharge"].EndTimer();

            GetRoomUser().Chat("Que bom que chegou, até mais!", true);
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

            GetRoomUser().Chat("Merda, hora de voltar ao trabalho!", true);
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

        public void InitiateDischarge(GameClient Client)
        {
            if (!OnDuty)
                return;

            if (Client.GetRoleplay() == null)
                return;

            if (Client.GetRoomUser() == null)
                return;

            if (Client.LoggingOut)
                return;

            if (!Client.GetRoleplay().IsDead)
                return;

            GetBotRoleplay().WalkingToItem = true;
            GetRoomUser().Chat("Estou a caminho " + Client.GetHabbo().Username + "!", true);

            var UserPoint = new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y);

            var Items = GetRoom().GetGameMap().GetAllRoomItemForSquare(UserPoint.X, UserPoint.Y);
            bool HasBed = Items.ToList().Where(x => x.GetBaseItem().ItemName == "hosptl_bed").ToList().Count() > 0;

            Item Item = null;
            if (HasBed)
                Item = Items.ToList().FirstOrDefault(x => x.GetBaseItem().ItemName == "hosptl_bed");

            if (Item != null)
            {
                var GoToPoint = new System.Drawing.Point(Item.SquareLeft.X, Item.SquareLeft.Y);
                GetRoomUser().MoveTo(GoToPoint);
                GetBotRoleplay().TimerManager.CreateTimer("discharge", GetBotRoleplay(), 10, true, Client);
            }
        }
    }
}