using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Timers;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class legislacaoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_legislacao"; }
        }

        public string Parameters
        {
            get { return "%"; }
        }

        public string Description
        {
            get { return "Adiciona um usuário à lista desejada para um nível desejado (1 à 6)."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int WantedLevel = 1;
            int NewWantedLevel = 1;
            bool OnProbation = false;
            Wanted NewWanted;
            #endregion

            #region Conditions
            if (Params.Length != 3)
            {
                Session.SendWhisper("Digite o nome do cidadão e a quantidade de estrelas (1-6)", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "law"))
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Jailbroken)
            {
                Session.SendWhisper("É óbvio que esta pessoa já está livre.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode adicionar estrelas em alguém que já está preso!", 1);
                return;
            }

            if (TargetClient.GetRoomUser() != null)
            {
                if (TargetClient.GetRoomUser().IsAsleep)
                {
                    Session.SendWhisper("Você não pode adicionar estrelas em alguém que está ausente!", 1);
                    return;
                }
            }
            #endregion

            #region Execute
            if (int.TryParse(Params[2], out WantedLevel))
            {
                if (WantedLevel > 6 || WantedLevel == 0)
                {
                    Session.SendWhisper("Por favor, insira um nível desejado entre 1 a 6!", 1);
                    return;
                }

                string RoomId = TargetClient.GetHabbo().CurrentRoomId.ToString() != "0" ? TargetClient.GetHabbo().CurrentRoomId.ToString() : "Desconhecido";

                if (TargetClient.GetRoleplay().OnProbation)
                {
                    OnProbation = true;
                    NewWantedLevel = WantedLevel + 1;

                    if (NewWantedLevel > 6)
                        NewWantedLevel = 6;

                    NewWanted = new Wanted(Convert.ToUInt32(TargetClient.GetHabbo().Id), RoomId, NewWantedLevel);
                }
                else
                    NewWanted = new Wanted(Convert.ToUInt32(TargetClient.GetHabbo().Id), RoomId, WantedLevel);

                if (RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    int CurrentWantedLevel = RoleplayManager.WantedList[TargetClient.GetHabbo().Id].WantedLevel;
                    if ((OnProbation && NewWantedLevel > CurrentWantedLevel) || (!OnProbation && WantedLevel > CurrentWantedLevel))
                    {
                        if (TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("wanted"))
                            TargetClient.GetRoleplay().TimerManager.ActiveTimers["wanted"].EndTimer();

                        if (!OnProbation)
                        {
                            if (TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("probation"))
                                TargetClient.GetRoleplay().TimerManager.ActiveTimers["probation"].EndTimer();

                            TargetClient.GetRoleplay().IsWanted = true;
                            TargetClient.GetRoleplay().WantedLevel = WantedLevel;
                            TargetClient.GetRoleplay().WantedTimeLeft = 10;

                            TargetClient.GetRoleplay().TimerManager.CreateTimer("procurado", 1000, false);
                            RoleplayManager.WantedList.TryUpdate(TargetClient.GetHabbo().Id, NewWanted, RoleplayManager.WantedList[TargetClient.GetHabbo().Id]);
                            Session.Shout("*Atualiza " + TargetClient.GetHabbo().Username + " como procurado, de " + CurrentWantedLevel + " estrela(s) para " + WantedLevel + " estrela(s)*", 37);
							
							TargetClient.SendWhisper("" + Session.GetHabbo().Username + " atualizou seu nível de procurado de " + CurrentWantedLevel + " estrela(s) para " + WantedLevel + " estrela(s)!", 1);
			                if (Session.GetRoleplay().TryGetCooldown("estrelas"))
                            return;
							
                            PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] O nível de procurado de " + TargetClient.GetHabbo().Username + " foi atualizado de " + CurrentWantedLevel + " estrela(s) para " + WantedLevel + " estrela(s)!");
                            return;
                        }
                        else
                        {
                            if (TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("probation"))
                                TargetClient.GetRoleplay().TimerManager.ActiveTimers["probation"].EndTimer();

                            TargetClient.GetRoleplay().OnProbation = false;
                            TargetClient.GetRoleplay().ProbationTimeLeft = 0;

                            TargetClient.GetRoleplay().IsWanted = true;
                            TargetClient.GetRoleplay().WantedLevel = NewWantedLevel;
                            TargetClient.GetRoleplay().WantedTimeLeft = 10;

                            TargetClient.GetRoleplay().TimerManager.CreateTimer("procurado", 1000, false);
                            RoleplayManager.WantedList.TryUpdate(TargetClient.GetHabbo().Id, NewWanted, RoleplayManager.WantedList[TargetClient.GetHabbo().Id]);
                            Session.Shout("*Atualiza " + TargetClient.GetHabbo().Username + " como procurado, de " + CurrentWantedLevel + " estrela(s) para " + NewWantedLevel + " estrela(s) [+1 devido à liberdade condicional]*", 37);
							
							TargetClient.SendWhisper("" + Session.GetHabbo().Username + " você está sendo procurado com um nível de " + NewWantedLevel + " estrela(s) [+1 devido à liberdade condicional]*", 1);
			                if (Session.GetRoleplay().TryGetCooldown("estrelas"))
                            return;
							
                            PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] O nível de procurado de " + TargetClient.GetHabbo().Username + " foi atualizado de " + CurrentWantedLevel + " estrela(s) para " + WantedLevel + " estrela(s) [+1 devido à liberdade condicional]!");
                            return;
                        }
                    }
                    else
                    {
                        Session.Shout("*Tenta atualizar o nível de procurado de " + TargetClient.GetHabbo().Username + ", mas nota que o nível desejado já está em " + CurrentWantedLevel + " estrela(s)*", 37);
                        return;
                    }
                }
                else
                {
                    if (TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("procurado"))
                        TargetClient.GetRoleplay().TimerManager.ActiveTimers["procurado"].EndTimer();

                    if (!OnProbation)
                    {
                        TargetClient.GetRoleplay().IsWanted = true;
                        TargetClient.GetRoleplay().WantedLevel = WantedLevel;
                        TargetClient.GetRoleplay().WantedTimeLeft = 10;
                        TargetClient.GetRoleplay().TimerManager.CreateTimer("procurado", 1000, false);
                        RoleplayManager.WantedList.TryAdd(TargetClient.GetHabbo().Id, NewWanted);
                        Session.Shout("*Adiciona " + TargetClient.GetHabbo().Username + " para a Lista de Procurados com um Nível de " + WantedLevel + " estrela(s)*", 37);
						
						TargetClient.SendWhisper("" + Session.GetHabbo().Username + " adicionou você à lista de procurados!", 1);
			            if (Session.GetRoleplay().TryGetCooldown("estrelas"))
                        return;
						
                        PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] " + TargetClient.GetHabbo().Username + " foi adicionado à Lista de Procurados com um Nível de " + WantedLevel + " estrela(s)!");
                        return;
                    }
                    else
                    {
                        if (TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("probation"))
                            TargetClient.GetRoleplay().TimerManager.ActiveTimers["probation"].EndTimer();

                        TargetClient.GetRoleplay().OnProbation = false;
                        TargetClient.GetRoleplay().ProbationTimeLeft = 0;

                        TargetClient.GetRoleplay().IsWanted = true;
                        TargetClient.GetRoleplay().WantedLevel = NewWantedLevel;
                        TargetClient.GetRoleplay().WantedTimeLeft = 10;

                        TargetClient.GetRoleplay().TimerManager.CreateTimer("procurado", 1000, false);
                        RoleplayManager.WantedList.TryAdd(TargetClient.GetHabbo().Id, NewWanted);
                        Session.Shout("*Adiciona " + TargetClient.GetHabbo().Username + " para a Lista de Procurados com um Nível de " + NewWantedLevel + " estrela(s) [+1 devido à liberdade condicional]*", 37);
						
					TargetClient.SendWhisper("" + Session.GetHabbo().Username + " você está sendo procurado com um nível de " + NewWantedLevel + " estrela(s) [+1 devido à liberdade condicional]*", 1);
			            if (Session.GetRoleplay().TryGetCooldown("estrelas"))
                        return;
						
                        PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] " + TargetClient.GetHabbo().Username + " foi adicionado à Lista de Procurados com um Nível de " + WantedLevel + " estrela(s) [+1 devido à liberdade condicional]!");
                        return;
                    }
                }
            }
            else
            {
                Session.SendWhisper("Por favor, insira um nível desejado entre 1 a 6!", 1);
                return;
            }
            #endregion
        }
    }
}