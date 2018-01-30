using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class SetStatCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_set_stat"; }
        }

        public string Parameters
        {
            get { return "%usuário% %status% %quantidade%"; }
        }

        public string Description
        {
            get { return "Define o status de usuários para a quantidade desejada."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 4 && Params[0].ToLower() != "sethp" && Params[0].ToLower() != "setenergy" && Params[0].ToLower() != "sethunger" && Params[0].ToLower() != "sethygiene")
            {
                Session.SendWhisper("Você deve digitar o nome de usuário, status e quantidade que você deseja dar.", 1);
                return;
            }

            if (Params.Length != 3 && (Params[0].ToLower() == "sethp" || Params[0].ToLower() == "setenergy" || Params[0].ToLower() == "sethunger" || Params[0].ToLower() == "sethygiene"))
            {
                Session.SendWhisper("Você deve digitar o nome de usuário, status e quantidade que você deseja dar.", 1);
                return;
            }

            var TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Este usuário não foi encontrado! Talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null || TargetClient.GetRoomUser() == null)
            {
                Session.SendWhisper("Este usuário não foi encontrado! Talvez ele esteja offline.", 1);
                return;
            }

            string Type = "";
            if (Params[0].ToLower() == "sethp")
                Type = "hp";
            else if (Params[0].ToLower() == "setenergy")
                Type = "energia";
            else if (Params[0].ToLower() == "sethunger")
                Type = "fome";
            else if (Params[0].ToLower() == "sethygiene")
                Type = "higiene";
            else
                Type = Params[2].ToLower();

            switch (Type)
            {
                #region Health
                case "hp":
                case "health":
				case "sangue":
                    {
                        string Amount = Params[0].ToLower() == "sethp" ? Params[2] : Params[3];

                        int HPAmount;
                        if (int.TryParse(Amount, out HPAmount))
                        {
                            if (HPAmount < 0)
                                HPAmount = 0;

                            if (TargetClient.GetHabbo().VIPRank > 1 && HPAmount == 0)
                            {
                                Session.SendWhisper("Você não pode matar outros super funcionários!", 1);
                                return;
                            }

                            TargetClient.GetRoleplay().CurHealth = HPAmount;

                            Session.SendWhisper("Definido com sucesso sua saúde " + TargetClient.GetHabbo().Username + ", para " + HPAmount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Energy
                case "energy":
				case "energia":
                    {
                        string Amount = Params[0].ToLower() == "setenergy" ? Params[2] : Params[3];

                        int EnergyAmount;
                        if (int.TryParse(Amount, out EnergyAmount))
                        {
                            if (EnergyAmount < 0)
                                EnergyAmount = 0;
                            
                            TargetClient.GetRoleplay().CurEnergy = EnergyAmount;

                            Session.SendWhisper("Definido com sucesso sua Energia " + TargetClient.GetHabbo().Username + ", para " + TargetClient.GetRoleplay().CurEnergy + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Stamina
                case "stamina":
                case "stam":
				case "vigor":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            if (Amount < 0)
                                Amount = 0;

                            if (Amount > RoleplayManager.StaminaCap)
                                Amount = RoleplayManager.StaminaCap;

                            TargetClient.GetRoleplay().Stamina = Amount;
                            TargetClient.GetRoleplay().StaminaEXP = LevelManager.StaminaLevels[TargetClient.GetRoleplay().Stamina];
                            TargetClient.GetRoleplay().MaxEnergy = ((Amount * 5) + 100);
                            Session.SendWhisper("Definido com sucesso seu Vigor " + TargetClient.GetHabbo().Username + ", para " + Amount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Intelligence
                case "intelligence":
                case "intel":
                case "int":
				case "inteligencia":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            if (Amount < 0)
                                Amount = 0;

                            if (Amount > RoleplayManager.IntelligenceCap)
                                Amount = RoleplayManager.IntelligenceCap;

                            TargetClient.GetRoleplay().Intelligence = Amount;
                            TargetClient.GetRoleplay().IntelligenceEXP = LevelManager.IntelligenceLevels[TargetClient.GetRoleplay().Intelligence];
                            Session.SendWhisper("Definido com sucesso sua Inteligência " + TargetClient.GetHabbo().Username + ", para " + Amount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Strength
                case "strength":
                case "str":
				case "forca":
				case "musculo":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            if (Amount < 0)
                                Amount = 0;

                            TargetClient.GetRoleplay().Strength = Amount;

                            if (LevelManager.StrengthLevels.ContainsKey(TargetClient.GetRoleplay().Strength))
                                TargetClient.GetRoleplay().StrengthEXP = LevelManager.StrengthLevels[TargetClient.GetRoleplay().Strength];

                            Session.SendWhisper("Definido com sucesso sua Força " + TargetClient.GetHabbo().Username + ", para " + Amount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Level
                case "level":
				case "nivel":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            if (Amount < 1)
                                Amount = 1;

                            if (!LevelManager.Levels.ContainsKey(Amount) || Amount > RoleplayManager.LevelCap)
                            {
                                Session.SendWhisper("Desculpe, mas o nível selecionado não existe!", 1);
                                return;
                            }

                            TargetClient.GetRoleplay().Level = Amount;
                            TargetClient.GetRoleplay().LevelEXP = LevelManager.Levels[TargetClient.GetRoleplay().Level];

                            Session.SendWhisper("Definido com sucesso seu Nível " + TargetClient.GetHabbo().Username + ", para " + Amount + "!", 1);
                            TargetClient.SendWhisper("Um administrador definiu seu nível de personagem para " + Amount + "!", 1);

                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region EXP
                case "xp":
                case "exp":
				case "experiencia":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            TargetClient.GetRoleplay().LevelEXP += Amount;
                            Session.SendWhisper("Deu " + TargetClient.GetHabbo().Username + " " + Amount + " XP! Seu XP agora é: " + TargetClient.GetRoleplay().LevelEXP + "!", 1);
                            TargetClient.SendWhisper("Um administrador lhe deu " + Amount + " XP!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Hunger
                case "hunger":
				case "fome":
                    {
                        string Amount = Params[0].ToLower() == "sethunger" ? Params[2] : Params[3];

                        int HungerAmount;
                        if (int.TryParse(Amount, out HungerAmount))
                        {
                            TargetClient.GetRoleplay().Hunger = HungerAmount;
                            Session.SendWhisper("Definido com sucesso sua fome " + TargetClient.GetHabbo().Username + ", para " + HungerAmount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Hygiene
                case "hygiene":
				case "higiene":
                    {
                        string Amount = Params[0].ToLower() == "sethygiene" ? Params[2] : Params[3];

                        int HygieneAmount;
                        if (int.TryParse(Amount, out HygieneAmount))
                        {
                            TargetClient.GetRoleplay().Hygiene = HygieneAmount;
                            Session.SendWhisper("Definido com sucesso sua Higiene " + TargetClient.GetHabbo().Username + ", para " + HygieneAmount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Minutes
                case "minutes":
				case "minutos":
				case "ttrabalho":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            TargetClient.GetRoleplay().TimeWorked = Amount;
                            Session.SendWhisper("Definiu com sucesso o tempo de trabalho de " + TargetClient.GetHabbo().Username + " para " + Amount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                #region Noob Timer
                case "noobtime":
                case "noobtimer":
				case "temponoob":
                    {
                        int Amount;
                        if (int.TryParse(Params[3], out Amount))
                        {
                            if (Amount <= 0)
                            {
                                TargetClient.GetRoleplay().IsNoob = false;
                                TargetClient.GetRoleplay().NoobTimeLeft = 0;
                            }
                            else
                            {
                                if (!TargetClient.GetRoleplay().IsNoob)
                                    TargetClient.GetRoleplay().IsNoob = true;

                                TargetClient.GetRoleplay().NoobTimeLeft = Amount;

                                if (!TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("noob"))
                                    TargetClient.GetRoleplay().TimerManager.CreateTimer("noob", 1000, true);
                            }

                            Session.SendWhisper("Definido com sucesso seu tempo Noob " + TargetClient.GetHabbo().Username + " para " + Amount + "!", 1);
                        }
                        else
                            Session.SendWhisper("Por favor insira um número válido!", 1);
                        break;
                    }
                #endregion

                case "list":
				case "lista":
                    {
                        Session.SendWhisper("Você pode escolher entre as seguintes estatísticas: saúde, energia, força, inteligência, fome, higiene, minutos, tempo noob, nível e xp!", 1);
                        break;
                    }
                default:
                    {
                        Session.SendWhisper("Desculpe, mas esse stat não existe! Digite ':status lista' para ver todas as opções!", 1);
                        break;
                    }
            }
        }
    }
}
