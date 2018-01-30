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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class RPStatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_rpstats"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Fornece uma lista das suas estatísticas de jogo de metas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você esqueceu de inserir o usuário da pessoa que deseja verificar!", 1);
                return;
            }
            #region Variables
            int JobId = 1;
            int JobRank = 1;
            int MarriedTo;
            bool IsJailed = false;
            int JailTimeLeft;
            bool IsDead = false;
            int DeadTimeLeft;
            bool IsWanted = false;
            int WantedTimeLeft;
            bool OnProbation = false;
            int ProbationTimeLeft;
            int SendHomeTimeLeft;
            int Phone;
            int GangId = 1000;
            int GangRankId = 1;
            int Car;
            int Fuel;

            int Level;
            int LevelEXP;
            int BankSavings;
            int BankChequings;
            int Arrests;
            int Arrested;
            int Evasions;
            int Punches;
            int Kills;
            int HitKills;
            int GunKills;
            int Deaths;
            int CopDeaths;
            int Hunger;
            int Hygiene;
            int CurHealth;
            int MaxHealth;
            int CurEnergy;
            int MaxEnergy;
            int Intelligence;
            int Strength;
            int Stamina;
            int Bullets;
            int Dynamites;
            int Weed;
            int Cocaine;
            int Cigarettes;
            int IntelligenceEXP;
            int StrengthEXP;
            int StaminaEXP;
            int TimeWorked;
            string Class;

            string Username = Params[1];
            GameClients.GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            #endregion

            #region Variables Client Check & Set
            if (TargetClient == null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = '" + Username + "' LIMIT 1");
                    var Row = dbClient.getRow();

                    if (Row == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    int UserId = Convert.ToInt32(Row["id"]);

                    dbClient.SetQuery("SELECT * FROM `rp_stats` WHERE `id` = '" + UserId + "' LIMIT 1");
                    var Stats = dbClient.getRow();

                    if (Stats == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    JobId = Convert.ToInt32(Stats["job_id"]);
                    JobRank = Convert.ToInt32(Stats["job_rank"]);

                    MarriedTo = Convert.ToInt32(Stats["married_to"]);
                    IsJailed = PlusEnvironment.EnumToBool(Stats["is_jailed"].ToString());
                    JailTimeLeft = Convert.ToInt32(Stats["jailed_time_left"]);
                    IsDead = PlusEnvironment.EnumToBool(Stats["is_dead"].ToString());
                    DeadTimeLeft = Convert.ToInt32(Stats["dead_time_left"]);
                    IsWanted = PlusEnvironment.EnumToBool(Stats["is_wanted"].ToString());
                    WantedTimeLeft = Convert.ToInt32(Stats["wanted_time_left"]);
                    OnProbation = PlusEnvironment.EnumToBool(Stats["on_probation"].ToString());
                    ProbationTimeLeft = Convert.ToInt32(Stats["probation_time_left"]);
                    SendHomeTimeLeft = Convert.ToInt32(Stats["sendhome_time_left"]);
                    Phone = Convert.ToInt32(Stats["phone"]);
                    GangId = Convert.ToInt32(Stats["gang_id"]);
                    GangRankId = Convert.ToInt32(Stats["gang_rank"]);
                    Car = Convert.ToInt32(Stats["car"]);
                    Fuel = Convert.ToInt32(Stats["car_fuel"]);

                    Level = Convert.ToInt32(Stats["level"]);
                    LevelEXP = Convert.ToInt32(Stats["level_exp"]);
                    BankSavings = Convert.ToInt32(Stats["bank_savings"]);
                    BankChequings = Convert.ToInt32(Stats["bank_chequings"]);
                    Arrests = Convert.ToInt32(Stats["arrests"]);
                    Arrested = Convert.ToInt32(Stats["arrested"]);
                    Evasions = Convert.ToInt32(Stats["evasions"]);
                    Punches = Convert.ToInt32(Stats["punches"]);
                    Kills = Convert.ToInt32(Stats["kills"]);
                    HitKills = Convert.ToInt32(Stats["hit_kills"]);
                    GunKills = Convert.ToInt32(Stats["gun_kills"]);
                    Deaths = Convert.ToInt32(Stats["deaths"]);
                    CopDeaths = Convert.ToInt32(Stats["cop_deaths"]);
                    Hunger = Convert.ToInt32(Stats["hunger"]);
                    Hygiene = Convert.ToInt32(Stats["hygiene"]);
                    CurHealth = Convert.ToInt32(Stats["curhealth"]);
                    MaxHealth = Convert.ToInt32(Stats["maxhealth"]);
                    CurEnergy = Convert.ToInt32(Stats["curenergy"]);
                    MaxEnergy = Convert.ToInt32(Stats["maxenergy"]);
                    Intelligence = Convert.ToInt32(Stats["intelligence"]);
                    Strength = Convert.ToInt32(Stats["strength"]);
                    Stamina = Convert.ToInt32(Stats["stamina"]);
                    Bullets = Convert.ToInt32(Stats["bullets"]);
                    Dynamites = Convert.ToInt32(Stats["dynamite"]);
                    Weed = Convert.ToInt32(Stats["weed"]);
                    Cocaine = Convert.ToInt32(Stats["cocaine"]);
                    Cigarettes = Convert.ToInt32(Stats["cigarette"]);
                    IntelligenceEXP = Convert.ToInt32(Stats["intelligence_exp"]);
                    StrengthEXP = Convert.ToInt32(Stats["strength_exp"]);
                    StaminaEXP = Convert.ToInt32(Stats["stamina_exp"]);
                    TimeWorked = Convert.ToInt32(Stats["time_worked"]);
                    Class = Stats["class"].ToString();
                }
            }
            else
            {
                JobId = TargetClient.GetRoleplay().JobId;
                JobRank = TargetClient.GetRoleplay().JobRank;
                MarriedTo = TargetClient.GetRoleplay().MarriedTo;
                IsJailed = TargetClient.GetRoleplay().IsJailed;
                JailTimeLeft = TargetClient.GetRoleplay().JailedTimeLeft;
                IsDead = TargetClient.GetRoleplay().IsDead;
                DeadTimeLeft = TargetClient.GetRoleplay().DeadTimeLeft;
                IsWanted = TargetClient.GetRoleplay().IsWanted;
                WantedTimeLeft = TargetClient.GetRoleplay().WantedTimeLeft;
                OnProbation = TargetClient.GetRoleplay().OnProbation;
                ProbationTimeLeft = TargetClient.GetRoleplay().ProbationTimeLeft;
                SendHomeTimeLeft = TargetClient.GetRoleplay().SendHomeTimeLeft;
                Phone = TargetClient.GetRoleplay().PhoneType;
                GangId = TargetClient.GetRoleplay().GangId;
                GangRankId = TargetClient.GetRoleplay().GangRank;
                Car = TargetClient.GetRoleplay().CarType;
                Fuel = TargetClient.GetRoleplay().CarFuel;

                Level = TargetClient.GetRoleplay().Level;
                LevelEXP = TargetClient.GetRoleplay().LevelEXP;
                BankSavings = TargetClient.GetRoleplay().BankSavings;
                BankChequings = TargetClient.GetRoleplay().BankChequings;
                Arrests = TargetClient.GetRoleplay().Arrests;
                Arrested = TargetClient.GetRoleplay().Arrested;
                Evasions = TargetClient.GetRoleplay().Evasions;
                Punches = TargetClient.GetRoleplay().Punches;
                Kills = TargetClient.GetRoleplay().Kills;
                HitKills = TargetClient.GetRoleplay().HitKills;
                GunKills = TargetClient.GetRoleplay().GunKills;
                Deaths = TargetClient.GetRoleplay().Deaths;
                CopDeaths = TargetClient.GetRoleplay().CopDeaths;
                Hunger = TargetClient.GetRoleplay().Hunger;
                Hygiene = TargetClient.GetRoleplay().Hygiene;
                CurHealth = TargetClient.GetRoleplay().CurHealth;
                MaxHealth = TargetClient.GetRoleplay().MaxHealth;
                CurEnergy = TargetClient.GetRoleplay().CurEnergy;
                MaxEnergy = TargetClient.GetRoleplay().MaxEnergy;
                Intelligence = TargetClient.GetRoleplay().Intelligence;
                Strength = TargetClient.GetRoleplay().Strength;
                Stamina = TargetClient.GetRoleplay().Stamina;
                Bullets = TargetClient.GetRoleplay().Bullets;
                Dynamites = TargetClient.GetRoleplay().Dynamite;
                Weed = TargetClient.GetRoleplay().Weed;
                Cocaine = TargetClient.GetRoleplay().Cocaine;
                Cigarettes = TargetClient.GetRoleplay().Cigarettes;
                IntelligenceEXP = TargetClient.GetRoleplay().IntelligenceEXP;
                StrengthEXP = TargetClient.GetRoleplay().StrengthEXP;
                StaminaEXP = TargetClient.GetRoleplay().StaminaEXP;
                TimeWorked = TargetClient.GetRoleplay().TimeWorked;
                Class = TargetClient.GetRoleplay().Class;
            }
            #endregion

            Group job = GroupManager.GetJob(JobId);
            GroupRank rank = GroupManager.GetJobRank(JobId, JobRank);

            StringBuilder MarriedMesssage = new StringBuilder();
            if (MarriedTo != 0) MarriedMesssage.Append(PlusEnvironment.GetGame().GetCacheManager().GenerateUser(MarriedTo).Username);
            else MarriedMesssage.Append("Ninguém!");

            StringBuilder JailMessage = new StringBuilder();
            if (IsJailed) JailMessage.Append("Ele está preso por " + JailTimeLeft + " minutos");
            else JailMessage.Append("Ele não está preso");

            StringBuilder DeadMessage = new StringBuilder();
            if (IsDead) DeadMessage.Append("Ele está morto por " + DeadTimeLeft + " minutos");
            else DeadMessage.Append("Ele não está morto");

            StringBuilder WantedMessage = new StringBuilder();
            if (IsWanted) WantedMessage.Append("Ele está sendo procurado por" + WantedTimeLeft + " minutos");
            else WantedMessage.Append("Ele não está sendo procurado");

            StringBuilder ProbationMessage = new StringBuilder();
            if (OnProbation) ProbationMessage.Append("Ele está em liberdade condicional por " + ProbationTimeLeft + " minutos");
            else ProbationMessage.Append("Ele não está em liberdade condicional");

            StringBuilder SendhomeMessage = new StringBuilder();
            if (SendHomeTimeLeft > 0) SendhomeMessage.Append("Ele foi enviado para casa por " + SendHomeTimeLeft + " minutos");
            else SendhomeMessage.Append("Ele não foi enviado para casa");

            StringBuilder PhoneType = new StringBuilder();
            if (Phone == 0) PhoneType.Append("Ele não têm um telefone");
            if (Phone == 1) PhoneType.Append("Ele têm um Nokia Tijolão. Textos custam 3 créditos(de celular) cada");
            if (Phone == 2) PhoneType.Append("Ele têm um iPhone 4. Textos custam 2 créditos(de celular) cada");
            if (Phone == 3) PhoneType.Append("Ele têm um iPhone7. Textos custam 1 crédito(de celular) cada");

            Group Gang = GroupManager.GetGang(GangId);
            GroupRank GangRank = GroupManager.GetGangRank(GangId, GangRankId);

            string grank = "\n";
            if (GangId > 1000)
            {
                if (GangRank != null)
                    grank = "Rank da Gangue: " + GangRank.Name + "\n\n";
            }

            StringBuilder CarType = new StringBuilder();
            if (Car == 0)
                CarType.Append("Ele não têm um carro");
            else if (Car == 1)
                CarType.Append("Ele têm um Toyota Corolla. Ele usa 3 combustível por cada 10 segundos");
            else if (Car == 2)
                CarType.Append("Ele têm um Honda Accord. Ele usa 2 combustível por cada 10 segundos");
            else
                CarType.Append("Eles têm o Nissan GTR mais fantástico. Ele usa 1 combustível por cada 10 segundos");

            StringBuilder CarFuel = new StringBuilder();
            if (Car == 0)
                CarFuel.Append("");
            else
            {
                if (Fuel > 0)
                    CarFuel.Append("Gasolina: Ele tem " + String.Format("{0:N0}", Fuel) + " galões de gasolina!\n");
                else
                    CarFuel.Append("Gasolina: Ele não têm combustível restante!\n");
            }

            StringBuilder MessageToSend = new StringBuilder().Append(
                                   "<---------- " + Username + " - Estatísticas --------->\n\n" +

                                   "<----- Informações Básicas ----->\n" +
                                   "Level: " + Level + "/" + RoleplayManager.LevelCap + "\n" +
                                   "XP: " + String.Format("{0:N0}", LevelEXP) + "/" + String.Format("{0:N0}", String.Format("{0:N0}", (!LevelManager.Levels.ContainsKey(Level + 1) ? 100000 : LevelManager.Levels[Level +1]))) + "\n" +
                                   "Classe: " + Class + "\n\n" +

                                   "<----- Informações do Trabalho ----->\n" +
                                   "Trabalho: " + job.Name + " - [" + rank.Name + "]\n" +
                                   "Pagamento: R$" + String.Format("{0:N0}", rank.Pay) + " a cada 15min\n" +
                                   "Enviado p/ casa: " + SendhomeMessage + "\n" +
                                   "Minutos trabalhados: " + String.Format("{0:N0}", TimeWorked) + "\n\n" +

                                   "<----- Informações Humanas ----->\n" +
                                   "Sangue: " + String.Format("{0:N0}", CurHealth) + "/" + MaxHealth + "\n" +
                                   "Energia: " + CurEnergy + "/" + MaxEnergy + "\n" +
                                   "Fome: " + Hunger + "/100\n" +
                                   "Higiene: " + Hygiene + "/100\n\n" +

                                   "<----- Informações de Level ----->\n" +
                                   "Inteligência: " + Intelligence + "/" + RoleplayManager.IntelligenceCap + " --- Experiência: " + String.Format("{0:N0}", IntelligenceEXP) + " / " + String.Format("{0:N0}", (!LevelManager.IntelligenceLevels.ContainsKey(Intelligence + 1) ? 100000 : LevelManager.IntelligenceLevels[Intelligence + 1])) + "\n" +
                                   "Força: " + Strength + "/" + RoleplayManager.StrengthCap + " ---> Experiência: " + String.Format("{0:N0}", StrengthEXP) + " / " + String.Format("{0:N0}", (!LevelManager.StrengthLevels.ContainsKey(Strength + 1) ? 100000 : LevelManager.StrengthLevels[Strength + 1])) + "\n" +
                                   "Vigor: " + Stamina + "/" + RoleplayManager.StaminaCap + " ---> Experiência: " + String.Format("{0:N0}", StaminaEXP) + " / " + String.Format("{0:N0}", (!LevelManager.StaminaLevels.ContainsKey(Stamina + 1) ? 100000 : LevelManager.StaminaLevels[Stamina + 1])) + "\n\n" +

                                   "<----- Preso - Morto - Procurado - Liberdade ----->\n" +
                                   "Preso: " + JailMessage + "\n" +
                                   "Morto: " + DeadMessage + "\n" +
                                   "Procurado: " + WantedMessage + "\n" +
                                   "Liberdade Condicional: " + ProbationMessage + "\n\n" +

                                   "<----- Afiliações ----->\n" +
                                   "Casado(a) com: " + MarriedMesssage + "\n" +
                                   "Gangue: " + (Gang == null ? "Nenhuma" : Gang.Name) + "\n" +
                                   grank +

                                   "<----- Outras Estatísticas ----->\n" +
                                   "Socos: " + String.Format("{0:N0}", Punches) + "\n" +
                                   "Matou: " + String.Format("{0:N0}", Kills) + " pessoas no total\n" +
                                   "Matou com Soco: " + String.Format("{0:N0}", HitKills) + " pessoas\n" +
                                   "Matou com Armas: " + String.Format("{0:N0}", GunKills) + " pessoas\n" +
                                   "Morreu: " + String.Format("{0:N0}", Deaths) + " vezes\n" +
                                   "Morreu sendo PM: " + String.Format("{0:N0}", CopDeaths) + " vezes\n" +
                                   "Prendeu: " + String.Format("{0:N0}", Arrests) + " pessoas\n" +
                                   "Preso: " + String.Format("{0:N0}", Arrested) + " vezes\n" +
                                   "Fugiu da prisão: " + String.Format("{0:N0}", Evasions) + " vezes\n\n" +

                                   "<----- Banco ----->\n" +
                                   "Cheques: R$" + String.Format("{0:N0}", BankChequings) + "\n" +
                                   "Poupança: R$" + String.Format("{0:N0}", BankSavings) + "\n\n" +

                                   "<----- Inventário ----->\n" +
                                   "Celular: " + PhoneType + "\n" +
                                   "Carro: " + CarType + "\n" +
                                   CarFuel +
                                   "Balas: " + String.Format("{0:N0}", Bullets) + "\n" +
                                   "Dinamites: " + String.Format("{0:N0}", Dynamites) + "\n" +
                                   "Cigarros: " + String.Format("{0:N0}", Cigarettes) + " cigarros\n" +
                                   "Maconha: " + String.Format("{0:N0}", Weed) + " gramas\n" +
                                   "Cocaína: " + String.Format("{0:N0}", Cocaine) + " gramas\n\n" +

                                   "<----- Armas ----->\n" +
                                   "Use o comando ':uarmas <nome>' para ver as armas da pessoa!\n");

            Session.SendMessage(new MOTDNotificationComposer(MessageToSend.ToString()));
            Session.Shout("*Começa a verificar as informações de " + TargetClient.GetHabbo().Username + ".", 4);
            TargetClient.SendWhisper("" + Session.GetHabbo().Username + " está checando suas informações!", 1);
			if (Session.GetRoleplay().TryGetCooldown("ustatus"))
                return;
        }
    }
}