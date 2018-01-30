using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Notifications;

using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class ChangeUClassCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_change_user_class"; }
        }

        public string Parameters
        {
            get { return "%usuário% %classe%"; }
        }

        public string Description
        {
            get { return "Permite que você altere a classe de um determinado usuário."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            #region Variables
            bool RunQuery = false;
            #endregion

            #region Conditions
            if (Params.Length < 3)
            {
                Session.SendWhisper("Digite ':mudarclasse [usuário] [tipo]' - Tipos: [Atirador] [Civil] [Lutador]!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Desculpe, mas este usuário não pôde ser encontrado!", 1);
                return;
            }

            if (TargetClient.GetRoomUser() == null || TargetClient.GetRoomUser().RoomId != Room.Id)
            {
                Session.SendWhisper("Desculpe, mas este usuário não está na mesma sala que você!", 1);
                return;
            }

            string Class = Params[2].ToString().ToLower();

            if (TargetClient.GetRoleplay().Class.ToLower() == Class)
            {
                Session.SendWhisper("Este usuário já é um " + Class + "!", 1);
                return;
            }
            #endregion

            #region Execute
            switch (Class)
            {
                case "gunner":
				case "atirador":
                    {
                        TargetClient.GetRoleplay().Class = "Atirador - [+Dano]";
                        TargetClient.GetHabbo().Motto = "Atirador - [+Dano]";
                        TargetClient.GetHabbo().Poof(true);
                        RunQuery = true;
                        break;
                    }
                case "fighter":
				case "lutador":
                    {
                        TargetClient.GetRoleplay().Class = "Lutador - [+Força]";
                        TargetClient.GetHabbo().Motto = "Lutador - [+Força]";
                        TargetClient.GetHabbo().Poof(true);
                        RunQuery = true;
                        break;
                    }
                case "civilian":
				case "cidadao":
				case "civil":
                    {
                        TargetClient.GetRoleplay().Class = "Cidadão - [+R$]";
                        TargetClient.GetHabbo().Motto = "Cidadão - [+R$]";
                        TargetClient.GetHabbo().Poof(true);
                        RunQuery = true;
                        break;
                    }
                default:
                    {
                        Session.SendWhisper("Você não digitou uma classe válida! Escolha uma dessas: [Atirador] [Civil] [Lutador]!", 1);
                        break;
                    }
            }

            if (RunQuery)
            {
                #region Update Class in database
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `users` set `motto` = @class WHERE `id` = @userid LIMIT 1");
                    dbClient.AddParameter("class", TargetClient.GetRoleplay().Class);
                    dbClient.AddParameter("userid", TargetClient.GetHabbo().Id);
                    dbClient.RunQuery();

                    dbClient.SetQuery("UPDATE `rp_stats` set `class` = @class WHERE `id` = @userid LIMIT 1");
                    dbClient.AddParameter("class", TargetClient.GetRoleplay().Class);
                    dbClient.AddParameter("userid", TargetClient.GetHabbo().Id);
                    dbClient.RunQuery();
                }
                #endregion
            }
            Session.Shout("*Altera imediatamente a classe de " + TargetClient.GetHabbo().Username + " para " + Class + "*", 23);
            return;
            #endregion
        }
    }
}