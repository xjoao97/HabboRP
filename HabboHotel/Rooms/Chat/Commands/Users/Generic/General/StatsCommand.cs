using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class StatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_stats"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista das suas estatísticas de roleplay."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Group job = GroupManager.GetJob(Session.GetRoleplay().JobId);
            GroupRank rank = GroupManager.GetJobRank(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank);

            StringBuilder MarriedMesssage = new StringBuilder();
            if (Session.GetRoleplay().MarriedTo != 0) MarriedMesssage.Append(PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Session.GetRoleplay().MarriedTo).Username);
            else MarriedMesssage.Append("Ninguém!");

            StringBuilder JailMessage = new StringBuilder();
            if (Session.GetRoleplay().IsJailed) JailMessage.Append("Você está preso por " + Session.GetRoleplay().JailedTimeLeft + " minutos");
            else JailMessage.Append("Você não está preso");

            StringBuilder DeadMessage = new StringBuilder();
            if (Session.GetRoleplay().IsDead) DeadMessage.Append("Você está morto por " + Session.GetRoleplay().DeadTimeLeft + " minutos");
            else DeadMessage.Append("Você nãot dead");

            StringBuilder WantedMessage = new StringBuilder();
            if (Session.GetRoleplay().IsWanted) WantedMessage.Append("Você está sendo procurado por " + Session.GetRoleplay().WantedTimeLeft + " minutos");
            else WantedMessage.Append("Você não está sendo procurado");

            StringBuilder ProbationMessage = new StringBuilder();
            if (Session.GetRoleplay().OnProbation) ProbationMessage.Append("Você está em liberdade condicional por" + Session.GetRoleplay().ProbationTimeLeft + " minutos");
            else ProbationMessage.Append("Você não está em liberdade condicional");

            StringBuilder SendhomeMessage = new StringBuilder();
            if (Session.GetRoleplay().SendHomeTimeLeft > 0) SendhomeMessage.Append("Você está enviado para casa por " + Session.GetRoleplay().SendHomeTimeLeft + " minutos");
            else SendhomeMessage.Append("Você não foi enviado para casa");

            StringBuilder PhoneType = new StringBuilder();
            if (Session.GetRoleplay().PhoneType == 0) PhoneType.Append("Você não tem um telefone");
            if (Session.GetRoleplay().PhoneType == 1) PhoneType.Append("Você tem um Nokia Tijolão. Mensagens custam [3 creditos de celuar cada]");
            if (Session.GetRoleplay().PhoneType == 2) PhoneType.Append("Você tem um iPhone 4s. Mensagens custam [2 creditos de celuar cada]");
            if (Session.GetRoleplay().PhoneType == 3) PhoneType.Append("Você tem o último iPhone 7. Mensagens custam [1 creditos de celuar cada]");

            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            GroupRank GangRank = GroupManager.GetGangRank(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank);

            string grank = "\n";
            if (Session.GetRoleplay().GangId > 1000)
            {
                if (GangRank != null)
                    grank = "Rank da Gangue: " + GangRank.Name + "\n\n";
            }

            StringBuilder CarType = new StringBuilder();
            if (Session.GetRoleplay().CarType == 0)
                CarType.Append("Você não tem");
            else if (Session.GetRoleplay().CarType == 1)
                CarType.Append("Você tem um Toyota Corolla. Ele usa 3 combustível por cada 10 segundos");
            else if (Session.GetRoleplay().CarType == 2)
                CarType.Append("Você tem um Honda Accord. Ele usa 2 combustível por cada 10 segundos");
            else
                CarType.Append("Você tem o fantastico Nissan GTR. Ele usa 1 combustível por cada 10 segundos");

            StringBuilder CarFuel = new StringBuilder();
            if (Session.GetRoleplay().CarType == 0)
                CarFuel.Append("");
            else
            {
                if (Session.GetRoleplay().CarFuel > 0)
                    CarFuel.Append("Combustível: Você tem " + String.Format("{0:N0}", Session.GetRoleplay().CarFuel) + " galões!\n");
                else
                    CarFuel.Append("Combustível: Você não tem combustível!\n");
            }

            StringBuilder MessageToSend = new StringBuilder().Append(
                                   "<---------- Suas Estatísticas ---------->\n\n" +

                                   "<----- Informações [Básicas] ----->\n" +
                                   "Level: " + Session.GetRoleplay().Level + "/" + RoleplayManager.LevelCap + "\n" +
                                   "Level XP: " + String.Format("{0:N0}", Session.GetRoleplay().LevelEXP) + "/" + String.Format("{0:N0}", ((!LevelManager.Levels.ContainsKey(Session.GetRoleplay().Level + 1) ? 100000 : LevelManager.Levels[Session.GetRoleplay().Level + 1]))) + "\n" +
                                   "Classe: " + Session.GetRoleplay().Class + "\n\n" +

                                   "<----- Informações [Trabalho] ----->\n" +
                                   "Emprego: " + job.Name + " no cargo " + rank.Name + "\n" +
                                   "Pagamento: R$" + rank.Pay + " por 15 minutos\n" + 
                                   "Enviado para casa: " + SendhomeMessage + "\n" +
                                   "Minutos trabalhados: " + String.Format("{0:N0}", Session.GetRoleplay().TimeWorked) + "\n\n" +

                                   "<----- Informações [Humanas] ----->\n" +
                                   "Sangue: " + String.Format("{0:N0}", Session.GetRoleplay().CurHealth) + "/" + Session.GetRoleplay().MaxHealth + "\n" +
                                   "Energia: " + Session.GetRoleplay().CurEnergy + "/" + Session.GetRoleplay().MaxEnergy + "\n" +
                                   "Fome: " + Session.GetRoleplay().Hunger + "/100\n" +
                                   "Higiene: " + Session.GetRoleplay().Hygiene + "/100\n\n" +

                                   "<----- Informações [Níveis] ----->\n" +
                                   "Inteligência: " + Session.GetRoleplay().Intelligence + "/" + RoleplayManager.IntelligenceCap + " <---> XP: " + String.Format("{0:N0}", Session.GetRoleplay().IntelligenceEXP) + " / " + String.Format("{0:N0}", (!LevelManager.IntelligenceLevels.ContainsKey(Session.GetRoleplay().Intelligence + 1) ? 100000 : LevelManager.IntelligenceLevels[Session.GetRoleplay().Intelligence + 1])) + "\n" +
                                   "Força: " + Session.GetRoleplay().Strength + "/" + RoleplayManager.StrengthCap + " <---> XP: " + String.Format("{0:N0}", Session.GetRoleplay().StrengthEXP) + " / " + String.Format("{0:N0}", (!LevelManager.StrengthLevels.ContainsKey(Session.GetRoleplay().Strength + 1) ? 100000 : LevelManager.StrengthLevels[Session.GetRoleplay().Strength + 1])) + "\n" +
                                   "Vigor: " + Session.GetRoleplay().Stamina + "/" + RoleplayManager.StaminaCap + " <---> XP: " + String.Format("{0:N0}", Session.GetRoleplay().StaminaEXP) + " / " + String.Format("{0:N0}", (!LevelManager.StaminaLevels.ContainsKey(Session.GetRoleplay().Stamina + 1) ? 100000 : LevelManager.StaminaLevels[Session.GetRoleplay().Stamina + 1])) + "\n\n" +

                                   "<----- Informação [Comportamento] ----->\n" +
                                   "Preso: " + JailMessage + "\n" +
                                   "Morto: " + DeadMessage + "\n" +
                                   "Procurado: " + WantedMessage + "\n" +
                                   "Liberdade condicional: " + ProbationMessage + "\n\n" +

                                   "<----- Informações [Afiliações] ----->\n" +
                                   "Casado(a) com: " + MarriedMesssage + "\n" +
                                   "Gangue: " + (Gang == null ? "Nenhuma" : Gang.Name) + "\n" +
                                   grank +

                                   "<----- Informações [Outros] ----->\n" +
                                   "Socos: " + String.Format("{0:N0}", Session.GetRoleplay().Punches) + "\n" +
                                   "Matou: " + String.Format("{0:N0}", Session.GetRoleplay().Kills) + " cidadãos\n" +
                                   "Matou com soco " + String.Format("{0:N0}", Session.GetRoleplay().HitKills) + " cidadãos\n" +
                                   "Matou com armas: " + String.Format("{0:N0}", Session.GetRoleplay().GunKills) + " cidadãos\n" +
                                   "Morreu: " + String.Format("{0:N0}", Session.GetRoleplay().Deaths) + " vezes\n" +
                                   "Morreu sendo PM: " + String.Format("{0:N0}", Session.GetRoleplay().CopDeaths) + "vezes\n" +
                                   "Prendeu: " + String.Format("{0:N0}", Session.GetRoleplay().Arrests) + " criminosos\n" +
                                   "Foi preso: " + String.Format("{0:N0}", Session.GetRoleplay().Arrested) + " vezes\n" +
                                   "Fugas da prisão: " + String.Format("{0:N0}", Session.GetRoleplay().Evasions) + "\n\n" +

                                   "<----- Informações [Bancárias] ----->\n" +
                                   "Conta de Cheques: R$" + String.Format("{0:N0}", Session.GetRoleplay().BankChequings) + "\n" +
                                   "Conta de Poupança: R$" + String.Format("{0:N0}", Session.GetRoleplay().BankSavings) + "\n\n" +

                                   "<----- Informações [Inventário] ----->\n" +
                                   "Celular: " + PhoneType + "\n" +
                                   "Carro: " + CarType + "\n" +
                                   CarFuel +
                                   "Balas: " + String.Format("{0:N0}", Session.GetRoleplay().Bullets) + "\n" +
                                   "Dinamites: " + String.Format("{0:N0}", Session.GetRoleplay().Dynamite) + "\n" +
                                   "Cigarros: " + String.Format("{0:N0}", Session.GetRoleplay().Cigarettes) + " cigarros\n" +
                                   "Maconha: " + String.Format("{0:N0}", Session.GetRoleplay().Weed) + " gramas\n" +
                                   "Cocaína: " + String.Format("{0:N0}", Session.GetRoleplay().Cocaine) + " gramas\n\n" +

                                   "<----- Informações [Agrícolas] ----->\n" +
                                   "Use o comando ':agricultura' para ver as suas estatísticas!\n\n" +

                                   "<----- Informações [Armas] ----->\n" +
                                   "Use o comando ':armas' para ver as que você possui!\n");

            Session.SendMessage(new MOTDNotificationComposer(MessageToSend.ToString()));
        }
    }
}