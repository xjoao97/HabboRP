using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Events
{
    class PurchaseEventCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_purchase"; }
        }

        public string Parameters
        {
            get { return "%tipo%"; }
        }

        public string Description
        {
            get { return "Permite comprar mercadorias com pontos de evento."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            int EventsLobby = Convert.ToInt32(RoleplayData.GetData("eventslobby", "roomid"));

            if (Room.Id != EventsLobby)
            {
                Session.SendWhisper("Você deve estar dentro da Sala Principal de Eventos para ver a loja de Pontos de Eventos!", 1);
                return;
            }

            if (Params.Length == 1)
            {
                #region Store List
                StringBuilder Message = new StringBuilder().Append("<----------- Loja de Pontos de Eventos ----------->\n");
                Message.Append("Para comprar qualquer um dos seguintes itens, digite ':comprar (numero do tipo)', [Ex: ':comprar 1]'\n\n");
                Message.Append("Tipo: 1   --->   5 Pontos de eventos por r$80\n\n");
                Message.Append("Tipo: 2   --->   5 Pontos de eventoss por 200 Balas\n\n");
                Message.Append("Tipo: 3   --->   5 Pontos de eventos por 300 créditos de Celular\n\n");
                Message.Append("Tipo: 4   --->   10 Pontos de eventos por 250 galões de Gasolina\n\n");
                Message.Append("Tipo: 5   --->   25 Pontos de eventos por uma atualização de Celular (para iPhone 4s)\n\n");
                Message.Append("Tipo: 6   --->   35 Pontos de eventos por uma atualização de Celular (para iPhone 7)\n\n");
                Message.Append("Tipo: 7   --->   50 Pontos de eventos por R$1000\n\n");
                Message.Append("Tipo: 8   --->   50 Pontos de eventos por 2500 Balas\n\n");
                Message.Append("Tipo: 9   --->   50 Pontos de eventos por 3750 créditos de Celular\n\n");
                Message.Append("Tipo: 10   --->   50 Pontos de eventos por uma atualização de Carro (para Honda Accord)\n\n");
                Message.Append("Tipo: 11   --->   60 Pontos de eventos por uma mudança de nick (Não é necessário VIP)\n\n");
                Message.Append("Tipo: 12   --->   80 Pontos de eventos por uma atualização de Carro (Para Nissan GTR)\n\n");
                Message.Append("Tipo: 13   --->   100 Pontos de eventos por 3250 galões de Gasolina\n\n");
                Message.Append("Tipo: 14   --->   200 Pontos de eventos por uma alteração de Classe\n\n");
                Message.Append("Tipo: 15   --->   1000 Pontos de eventos por uma Customização de Arma\n\n");
                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
                #endregion
            }
            else
            {
                switch (Params[1].ToLower())
                {
                    #region Type 1
                    case "1":
                        {
                            if (Session.GetHabbo().EventPoints < 5)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 1", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 5;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetHabbo().Credits += 80;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 2
                    case "2":
                        {
                            if (Session.GetHabbo().EventPoints < 5)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 2", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 5;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().Bullets += 200;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 3
                    case "3":
                        {
                            if (Session.GetHabbo().EventPoints < 5)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 3", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 5;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetHabbo().Duckets += 300;
                            Session.GetHabbo().UpdateDucketsBalance();
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 4
                    case "4":
                        {
                            if (Session.GetHabbo().EventPoints < 10)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 4", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 10;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().CarFuel += 250;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 5
                    case "5":
                        {
                            if (Session.GetHabbo().EventPoints < 25)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 5", 1);
                                break;
                            }

                            if (Session.GetRoleplay().PhoneType <= 0)
                            {
                                Session.SendWhisper("Você deve ter um telefone para comprar uma atualização!", 1);
                                break;
                            }

                            if (Session.GetRoleplay().PhoneType > 2)
                            {
                                Session.SendWhisper("Você já tem um telefone melhor do que o iPhone 4s!", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 25;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().PhoneType = 2;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 6
                    case "6":
                        {
                            if (Session.GetHabbo().EventPoints < 35)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 6", 1);
                                break;
                            }

                            if (Session.GetRoleplay().PhoneType <= 1)
                            {
                                Session.SendWhisper("Você não tem um iPhone 4s para atualizar para o iPhone 7!", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 35;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().PhoneType = 3;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 7
                    case "7":
                        {
                            if (Session.GetHabbo().EventPoints < 50)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 7", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 50;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetHabbo().Credits += 1000;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 8
                    case "8":
                        {
                            if (Session.GetHabbo().EventPoints < 50)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 2", 8);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 50;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().Bullets += 2500;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 9
                    case "9":
                        {
                            if (Session.GetHabbo().EventPoints < 50)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 9", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 50;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetHabbo().Duckets += 3750;
                            Session.GetHabbo().UpdateDucketsBalance();
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 10
                    case "10":
                        {
                            if (Session.GetHabbo().EventPoints < 50)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 10", 1);
                                break;
                            }

                            if (Session.GetRoleplay().CarType <= 0)
                            {
                                Session.SendWhisper("Você deve ter um carro para comprar uma atualização!", 1);
                                break;
                            }

                            if (Session.GetRoleplay().CarType > 2)
                            {
                                Session.SendWhisper("Você já possui um carro melhor do que o Honda Accord!", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 50;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().CarType = 2;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 11
                    case "11":
                        {
                            if (Session.GetHabbo().EventPoints < 60)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 11", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 60;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetHabbo().LastNameChange = 0;
                            Session.GetHabbo().ChangingName = true;
                            Session.GetRoleplay().FreeNameChange = true;
                            Session.SendNotification("Tenha em atenção que, se o seu nome de usuário for considerado inapropriado, você será banido sem dúvida.\r\rObserve também que o pessoal NÃO permitirá que você altere seu nome de usuário novamente se você tiver um problema com o que você escolheu.\r\rFeche esta janela e clique em si mesmo para começar a escolher um novo nome de usuário!");
                            Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
                            break;
                        }
                    #endregion

                    #region Type 12
                    case "12":
                        {
                            if (Session.GetHabbo().EventPoints < 80)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 12", 1);
                                break;
                            }

                            if (Session.GetRoleplay().CarType <= 1)
                            {
                                Session.SendWhisper("Você não tem um Honda Accord para atualizar para o Nissan GTR!", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 80;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().CarType = 3;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 13
                    case "13":
                        {
                            if (Session.GetHabbo().EventPoints < 100)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 13", 1);
                                break;
                            }

                            Session.GetHabbo().EventPoints -= 100;
                            Session.GetHabbo().UpdateEventPointsBalance();

                            Session.GetRoleplay().CarFuel += 3250;
                            Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                            break;
                        }
                    #endregion

                    #region Type 14
                    case "14":
                        {
                            if (Session.GetHabbo().EventPoints < 200)
                            {
                                Session.SendWhisper("Você não tem pontos suficientes para comprar o TIPO: 14", 1);
                                break;
                            }

                            if (Params.Length < 3)
                            {
                                Session.SendWhisper("Por favor digite ':comprar 14 lista' para ver suas opções, para escolher uma opção, por exemplo. ':comprar 14 lutador'", 1);
                                break;
                            }
                            else
                            {
                                bool RunQuery = false;
                                if (Params[2].ToLower() == "lista")
                                {
                                    StringBuilder Message = new StringBuilder().Append("----- HabboRPG Classes -----\n\n");
                                    Message.Append("Os civis recebem mais dinheiro quando concluem um ciclo de trabalho!\n\n");
                                    Message.Append("Os lutadores causam um pouco mais de dano com os punhos (comando :soco x)!\n\n");
                                    Message.Append("Os Atiradores causam um pouco mais de dano com armas (comando :atirar x)!\n\n");
                                    Message.Append("Escolha com sabedoria, pois isso custará 200 Pontos de Evento!");
                                    Session.SendNotification(Message.ToString());
                                }
                                else if (Params[2].ToLower() == "atirador")
                                {
                                    Session.GetRoleplay().Class = "Atirador - [+Dano]";
                                    Session.GetHabbo().Motto = "Classe: Atirador - [+Dano]";
                                    Session.GetHabbo().Poof(true);
                                    RunQuery = true;
                                }
                                else if (Params[2].ToLower() == "lutador")
                                {
                                    Session.GetRoleplay().Class = "Lutador - [+Força]";
                                    Session.GetHabbo().Motto = "Classe: Lutador - [+Força]";
                                    Session.GetHabbo().Poof(true);
                                    RunQuery = true;
                                }
                                else if (Params[2].ToLower() == "Civil")
                                {
                                    Session.GetRoleplay().Class = "Civil - [+R$]";
                                    Session.GetHabbo().Motto = "Classe: Civil - [+R$]";
                                    Session.GetHabbo().Poof(true);
                                    RunQuery = true;
                                }
                                else
                                    Session.SendWhisper("você não escolheu uma Classe válida! Por favor escolha [Atirador] [Lutador] [Civil], ou [Lista] se você não tiver certeza!", 1);

                                if (RunQuery)
                                {
                                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.SetQuery("UPDATE `users` set `motto` = @class WHERE `id` = @userid LIMIT 1");
                                        dbClient.AddParameter("class", Session.GetRoleplay().Class);
                                        dbClient.AddParameter("userid", Session.GetHabbo().Id);
                                        dbClient.RunQuery();

                                        dbClient.SetQuery("UPDATE `rp_stats` set `class` = @class WHERE `id` = @userid LIMIT 1");
                                        dbClient.AddParameter("class", Session.GetRoleplay().Class);
                                        dbClient.AddParameter("userid", Session.GetHabbo().Id);
                                        dbClient.RunQuery();
                                    }

                                    Session.GetHabbo().EventPoints -= 200;
                                    Session.GetHabbo().UpdateEventPointsBalance();
                                    Session.Shout("*Compra um prêmio da loja de pontos do evento*", 4);
                                    Session.SendNotification("Você mudou sua classe com sucesso para " + Session.GetRoleplay().Class + "!");
                                }
                            }
                            break;
                        }
                    #endregion

                    #region Type 15
                    case "15":
                        {
                            StringBuilder Message = new StringBuilder().Append("<---------- Armas Customizadas de Evento ---------->\n\n");
                            Message.Append("Primeiro, você deve possuir a arma que deseja personalizar! Pistola, MP5K.\n\n");
                            Message.Append("A arma personalizada parecerá a MESMA como a arma regular, mas tem um 'aperto de camo de ouro personalizado'.\n\n");
                            Message.Append("A arma personalizada também virá com o seu próprio 'texto de equipar' e 'texto de disparar'.\n\n");
                            Message.Append("Esteja ciente de que a arma personalizada fará o mesmo dano, etc., como sua versão não personalizada.\n\n");
                            Message.Append("A arma personalizada é puramente para fins estéticos, para 'mostrar' suas realizações e parecer legal!\n\n");
                            Message.Append("Se você quiser comprar a arma personalizada, notifique a xxx se você tem 1000 Pontos de Eventos!");
                            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
                            break;
                        }
                    #endregion

                    #region Default
                    default:
                        {
                            Session.SendWhisper("Esse não é um dos tipos de opção, digite ':comprar' para ver todas as opções!", 1);
                            break;
                        }
                    #endregion
                }
            }
        }
    }
}