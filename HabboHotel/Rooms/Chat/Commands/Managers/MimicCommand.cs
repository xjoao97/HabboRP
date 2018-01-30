using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class MimicCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mimic"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Muda sua roupa e procura combinar os usuários-alvo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome do usuário que deseja imitar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (!TargetClient.GetHabbo().AllowMimic)
            {
                Session.SendWhisper("Opa, você não pode imitar esse usuário - desculpe!", 1);
                return;
            }

            RoomUser TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            Session.GetHabbo().Gender = TargetUser.GetClient().GetHabbo().Gender;
            Session.GetHabbo().Look = TargetUser.GetClient().GetHabbo().Look;
            Session.GetRoleplay().OriginalOutfit = Session.GetHabbo().Look;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `gender` = @gender, `look` = @look WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("gender", Session.GetHabbo().Gender);
                dbClient.AddParameter("look", Session.GetHabbo().Look);
                dbClient.AddParameter("id", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            RoomUser User = Session.GetRoomUser();
            if (User != null)
            {
                Session.SendMessage(new UserChangeComposer(User, true));
                Room.SendMessage(new UserChangeComposer(User, false));
            }
            Session.Shout("*Copia imediatamente o visual de " + TargetClient.GetHabbo().Username + "*", 23);
            Session.SendWhisper("Você imitou com sucesso " + TargetClient.GetHabbo().Username, 1);
        }
    }
}