using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Navigator;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class FlagOtherCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_flag_other"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Força o usuário especificado a mudar seu nome."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome de usuário da pessoa!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (Params.Length == 2)
            {
                Session.SendWhisper("Você alterou com sucesso o nome deste usuário!", 1);
                TargetClient.GetHabbo().LastNameChange = 0;
                TargetClient.GetHabbo().ChangingName = true;
                TargetClient.GetRoleplay().FreeNameChange = true;
                TargetClient.SendNotification("Tenha em atenção que, se o seu nome de usuário for considerado inapropriado, você será banido.\r\rObserve a Equipe NÃO permitirá que você altere seu nome de usuário novamente se você tiver um problema com o que você escolheu.\r\rFeche esta janela e clique em si mesmo para começar a escolher um novo nome de usuário!");
                TargetClient.SendMessage(new UserObjectComposer(TargetClient.GetHabbo()));
            }
            else if (Params.Length > 2)
            {
                var User = TargetClient.GetRoomUser();

                if (User == null)
                {
                    Session.SendWhisper("Desculpe, mas essa pessoa não está em uma sala!", 1);
                    return;
                }

                if (User.RoomId != Room.Id)
                {
                    Session.SendWhisper("Certifique-se de que o alvo esteja na mesma sala que você para fazer a alteração!", 1);
                    return;
                }

                string NewName = Params[2];
                string OldName = TargetClient.GetHabbo().Username;

                bool InUse = false;
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                    dbClient.AddParameter("name", NewName);
                    InUse = dbClient.getInteger() == 1;
                }

                if (InUse)
                {
                    Session.SendWhisper("Este usuário já possui este nome!", 1);
                    return;
                }

                if (!PlusEnvironment.GetGame().GetClientManager().UpdateClientUsername(TargetClient, OldName, NewName))
                {
                    Session.SendWhisper("Opa! Ocorreu um problema ao atualizar seu nome de usuário. Por favor, tente novamente.", 1);
                    return;
                }

                Room TargetRoom = User.GetRoom();

                TargetClient.GetHabbo().ChangingName = false;
                if (TargetRoom != null)
                    TargetRoom.GetRoomUserManager().RemoveUserFromRoom(TargetClient, true, false);

                TargetClient.GetHabbo().ChangeName(NewName);
                TargetClient.GetHabbo().GetMessenger().OnStatusChanged(true);

                TargetClient.SendMessage(new UpdateUsernameComposer(NewName));
                if (TargetRoom != null)
                    TargetRoom.SendMessage(new UserNameChangeComposer(Room.Id, User.VirtualId, NewName));

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `logs_client_namechange` (`user_id`,`new_name`,`old_name`,`timestamp`) VALUES ('" + TargetClient.GetHabbo().Id + "', @name, '" + OldName + "', '" + PlusEnvironment.GetUnixTimestamp() + "')");
                    dbClient.AddParameter("name", NewName);
                    dbClient.RunQuery();
                }

                ICollection<RoomData> Rooms = TargetClient.GetHabbo().UsersRooms;
                foreach (RoomData Data in Rooms)
                {
                    if (Data == null)
                        continue;

                    Data.OwnerName = NewName;
                }

                foreach (Room UserRoom in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList())
                {
                    if (UserRoom == null || UserRoom.RoomData.OwnerName != NewName)
                        continue;

                    UserRoom.OwnerName = NewName;
                    UserRoom.RoomData.OwnerName = NewName;

                    UserRoom.SendMessage(new RoomInfoUpdatedComposer(UserRoom.RoomId));
                }
                RoleplayManager.SendUser(TargetClient, Room.Id, "");
                Session.Shout("*Altera imediatamente o nome de " + OldName + " para " + NewName + "*", 23);
                return;
            }
        }
    }
}
