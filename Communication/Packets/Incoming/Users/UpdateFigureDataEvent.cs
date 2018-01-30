using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Global;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Quests;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Moderation;

using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Incoming.Users
{
    class UpdateFigureDataEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            string Gender = Packet.PopString().ToUpper();
            string Look = PlusEnvironment.GetGame().GetAntiMutant().RunLook(Packet.PopString());
            int ClothingRoom = Convert.ToInt32(RoleplayData.GetData("clothing", "roomid"));

            if (Session.GetRoomUser() == null || !Session.GetHabbo().InRoom)
                return;

            if (Session.GetRoomUser().RoomId != ClothingRoom)
            {
                Session.SendNotification("Você deve estar dentro da Loja de roupas para mudar suas roupas! [Quarto ID: " + ClothingRoom + "]");
                return;
            }

            if (Session.GetRoleplay().IsWorking)
            {
                Session.SendNotification("Você não pode mudar sua roupa enquanto está trabalhando!");
                return;
            }

            if (Look == Session.GetHabbo().Look)
            {
                Session.SendWhisper("Você já está vestido assim!", 1);
                return;
            }

            if ((DateTime.Now - Session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().ClothingUpdateWarnings += 1;
                if (Session.GetHabbo().ClothingUpdateWarnings >= 25)
                    Session.GetHabbo().SessionClothingBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionClothingBlocked)
                return;

            if (Session.GetRoleplay().PurchasingClothing)
            {
                Session.GetRoleplay().PurchasingClothing = false;
                return;
            }

            Session.GetHabbo().LastClothingUpdateTime = DateTime.Now;

            string[] AllowedGenders = { "M", "F" };
            if (!AllowedGenders.Contains(Gender))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Desculpe, você escolheu um gênero inválido."));
                return;
            }

            //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);

            Session.GetHabbo().Look = PlusEnvironment.FilterFigure(Look);
            Session.GetHabbo().Gender = Gender.ToLower();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("look", Look);
                dbClient.AddParameter("gender", Gender);
                dbClient.RunQuery();
            }

            Session.SendMessage(new AvatarAspectUpdateMessageComposer(Look, Gender));

            //PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_AvatarLooks", 1);
            //if (Session.GetHabbo().Look.Contains("ha-1006"))
            //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.WEAR_HAT);

            if (Session.GetHabbo().InRoom)
            {
                RoomUser RoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (RoomUser != null)
                {
                    Session.SendMessage(new UserChangeComposer(RoomUser, true));
                    Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
                }
            }

            if (Session.GetHabbo().GetMessenger() != null)
                Session.GetHabbo().GetMessenger().OnStatusChanged(true);

            Session.GetRoleplay().OriginalOutfit = Session.GetHabbo().Look;
        }
    }
}
