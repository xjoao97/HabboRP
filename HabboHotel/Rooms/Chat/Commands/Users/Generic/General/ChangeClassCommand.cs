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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class ChangeClassCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_change_class"; }
        }

        public string Parameters
        {
            get { return "%classe%"; }
        }

        public string Description
        {
            get { return "Permite mudar sua classe se você ainda estiver no nível 1."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            #region Variables
            bool RunQuery = false;
            #endregion

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, digite ':mudarclasse [tipo]' Os tipos são [Atirador] [Civil] [Lutador] ou [Lista] para ver informações de cada Classe!", 1);
                return;
            }

            if (Session.GetRoleplay().Level > 1)
            {
                Session.SendWhisper("Muito tarde, você não é mais nível 1!", 1);
                return;
            }

            if (Session.GetRoleplay().PermanentClass)
            {
                Session.SendWhisper("Você já escolheu sua classe permanente, desculpe!", 1);
                return;
            }

            if (Session.GetRoleplay().Class.ToLower() == Params[1].ToLower())
            {
                Session.SendWhisper("Você já é um " + Session.GetRoleplay().Class + "!", 1);
                return;
            }
            #endregion

            #region Execute
            switch (Params[1].ToLower())
            {
				case "lista":
                case "list":
                    {
                        StringBuilder Message = new StringBuilder().Append("<----- HabboRPG Classes ----->\n\n");
                        Message.Append("Os civis recebem mais dinheiro quando concluem o tempo de trabalho!\n\n");
                        Message.Append("Os lutadores causam um pouco mais de dano com os punhos, :soco x!\n\n");
                        Message.Append("Os artilheiros causam um pouco mais de dano com armas, :atirar x!\n\n");
                        Message.Append("Escolha com sabedoria, pois você não pode mudar sua classe novamente!\n\n");
                        Session.SendNotification(Message.ToString());
                        break;
                    }
				case "atirador":
                case "gunner":
                    {
                        if (Params.Length > 2)
                        {
                            if (Params[2].ToLower() == "sim")
                            {
                                Session.GetRoleplay().PermanentClass = true;
                                Session.GetRoleplay().Class = "Atirador - [+Dano]";
                                Session.GetHabbo().Motto = "Classe: Atirador - [+Dano]";
                                Session.GetHabbo().Poof(true);
                                RunQuery = true;
                            }
                            else
                                Session.SendWhisper("Tem certeza de que quer ser um Atirador? Se sim, digite ':mudarclasse Atirador sim'", 1);
                        }
                        else
                            Session.SendWhisper("Tem certeza de que quer ser um Atirador? Se sim, digite ':mudarclasse Atirador sim'", 1);

                        break;
                    }
                case "lutador":
				case "fighter":
                    {
                        if (Params.Length > 2)
                        {
                            if (Params[2].ToLower() == "sim")
                            {
                                Session.GetRoleplay().PermanentClass = true;
                                Session.GetRoleplay().Class = "Lutador - [+Força]";
                                Session.GetHabbo().Motto = "Classe: Lutador - [+Força]";
                                Session.GetHabbo().Poof(true);
                                RunQuery = true;
                            }
                            else
                                Session.SendWhisper("Você tem certeza de que quer ser um Lutador? Se sim, digite ':mudarclasse Lutador sim'", 1);
                        }
                        else
                            Session.SendWhisper("Você tem certeza de que quer ser um Lutador? Se sim, digite ':mudarclasse Lutador sim'", 1);

                        break;
                    }
                case "civil":
				case "cidadao":
				case "civilian":
                    {
                        if (Params.Length > 2)
                        {
                            if (Params[2].ToLower() == "sim")
                            {
                                Session.GetRoleplay().PermanentClass = true;
                                Session.GetRoleplay().Class = "Civil - [+R$]";
                                Session.GetHabbo().Motto = "Classe: Civil - [+R$]";
                                Session.GetHabbo().Poof(true);
                                RunQuery = true;
                            }
                            else
                                Session.SendWhisper("Você tem certeza de que quer ser um Civil? Se sim, digite ':mudarclasse Civil sim'", 1);
                        }
                        else
                            Session.SendWhisper("Você tem certeza de que quer ser um Civil? Se sim, digite ':mudarclasse Civil sim'", 1);
                        break;
                    }
                default:
                    {
                        Session.SendWhisper("Você não digitou uma classe válida! Por favor escolha [Atirador] [Lutador] [Civil] ou [Lista] para saber sobre as Classes!", 1);
                        break;
                    }
            }

            if (RunQuery)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `users` set `motto` = @class WHERE `id` = @userid LIMIT 1");
                    dbClient.AddParameter("class", Session.GetRoleplay().Class);
                    dbClient.AddParameter("userid", Session.GetHabbo().Id);
                    dbClient.RunQuery();

                    dbClient.SetQuery("UPDATE `rp_stats` set `class` = @class, `permanent_class` = @permanent_class WHERE `id` = @userid LIMIT 1");
                    dbClient.AddParameter("class", Session.GetRoleplay().Class);
                    dbClient.AddParameter("permanent_class", PlusEnvironment.BoolToEnum(Session.GetRoleplay().PermanentClass));
                    dbClient.AddParameter("userid", Session.GetHabbo().Id);
                    dbClient.RunQuery();
                }

                Session.SendNotification("Você mudou sua classe com sucesso, você agora é um " + Session.GetRoleplay().Class + "!");
                return;
            }
            #endregion
        }
    }
}