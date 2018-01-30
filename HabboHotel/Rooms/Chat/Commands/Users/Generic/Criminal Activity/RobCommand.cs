using System;
using System.Linq;
using System.Text;
using System.Drawing;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal
{
    class RobCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_criminal_activity_rob"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Rouba a carteira de um cidadão [dinheiro, maconha, cocaína ou cigarro]."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null || TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            int LevelDifference = Math.Abs(Session.GetRoleplay().Level - TargetClient.GetRoleplay().Level);

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);

            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode roubar alguém enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode roubar alguém enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().StaffOnDuty || Session.GetRoleplay().AmbassadorOnDuty)
            {
                Session.SendWhisper("Você não pode roubar alguém enquanto você está de plantão!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode roubar alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode roubar alguém que está na prisão!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().StaffOnDuty)
            {
                Session.SendWhisper("Você não pode roubar um funcionário que esteja de plantão!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().AmbassadorOnDuty)
            {
                Session.SendWhisper("Você não pode roubar um embaixador que está de plantão!", 1);
                return;
            }

            if (TargetClient.GetHabbo().VIPRank > 1)
            {
                Session.SendWhisper("Você não pode roubar esse membro da equipe!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode roubar alguém que está ausente!", 1);
                return;
            }

            if (!Room.RobEnabled && !RoleplayManager.PurgeStarted)
            {
                Session.SendWhisper("Você não pode roubar nesta sala!", 1);
                return;
            }

            if (Session.GetRoleplay().DrivingCar)
            {
                Session.SendWhisper("Você não pode roubar alguém ao dirigir um veículo!", 1);
                return;
            }

            if (Session.GetRoleplay().IsNoob)
            {
                Session.SendWhisper("Você não pode completar esta ação enquanto estiver sob Deus Proteção!", 1);
                return;
            }

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode se roubar!", 1);
                return;
            }

            if (TargetClient.MachineId == Session.MachineId)
            {
                Session.SendWhisper("GAROTO IXPERTINHO! Você não pode roubar outra das suas contas!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Level < 2)
            {
                Session.SendWhisper("Você não pode completar esta ação porque o nível do usuário é abaixo de 2!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Level < 2)
            {
            Session.SendWhisper("Você não pode completar esta ação, pois você ainda é Nível 1!", 1);
            return;
            }
				
            if (Session.GetRoleplay().TryGetCooldown("roubar"))
                return;

            if (LevelDifference > 6)
            {
                Session.SendWhisper("Você não pode roubar esse usuário, pois sua diferença de nível é maior que 5!", 1);
                return;
            }
            #endregion

            #region Execute
            CryptoRandom Random = new CryptoRandom();
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            bool Success = false;
            string RobbedItems = "";
            int DrugsChance = Random.Next(1, 101);

            if (Distance <= 1)
            {
                if (TargetClient.GetHabbo().Credits > 50)
                {
                    int AmountToRob;
                    if (TargetClient.GetHabbo().Credits > 100)
                        AmountToRob = 100;
                    else
                        AmountToRob = TargetClient.GetHabbo().Credits;

                    int MaxAmount = Convert.ToInt32(Math.Floor((Double)AmountToRob / 2));
                    int MinAmount = Convert.ToInt32(Math.Floor((double)AmountToRob / 10));

                    int Amount = Random.Next(MinAmount, MaxAmount + 1);

                    Session.GetHabbo().Credits += Amount;
                    Session.GetHabbo().UpdateCreditsBalance();

                    TargetClient.GetHabbo().Credits -= Amount;
                    TargetClient.GetHabbo().UpdateCreditsBalance();

                    Success = true;
                    RobbedItems += "R$" + String.Format("{0:N0}", Amount) + ", ";
                }

                if (DrugsChance <= 15)
                {
                    if (TargetClient.GetRoleplay().Weed > 30)
                    {
                        int AmountToRob;
                        if (TargetClient.GetRoleplay().Weed > 100)
                            AmountToRob = 100;
                        else
                            AmountToRob = TargetClient.GetRoleplay().Weed;

                        int MaxAmount = Convert.ToInt32(Math.Floor((Double)AmountToRob / 5));
                        int MinAmount = Convert.ToInt32(Math.Floor((double)AmountToRob / 20));

                        int Amount = Random.Next(MinAmount, MaxAmount + 1);

                        Session.GetRoleplay().Weed += Amount;
                        TargetClient.GetRoleplay().Weed -= Amount;

                        Success = true;
                        RobbedItems += "e " + String.Format("{0:N0}", Amount) + "g de maconha, ";
                    }

                    if (TargetClient.GetRoleplay().Cocaine > 30)
                    {
                        int AmountToRob;
                        if (TargetClient.GetRoleplay().Cocaine > 100)
                            AmountToRob = 100;
                        else
                            AmountToRob = TargetClient.GetRoleplay().Cocaine;

                        int MaxAmount = Convert.ToInt32(Math.Floor((Double)AmountToRob / 5));
                        int MinAmount = Convert.ToInt32(Math.Floor((double)AmountToRob / 20));

                        int Amount = Random.Next(MinAmount, MaxAmount + 1);

                        Session.GetRoleplay().Cocaine += Amount;
                        TargetClient.GetRoleplay().Cocaine -= Amount;

                        Success = true;
                        RobbedItems += "e " + String.Format("{0:N0}", Amount) + "g de cocaína, ";
                    }

                    if (TargetClient.GetRoleplay().Cigarettes > 30)
                    {
                        int AmountToRob;
                        if (TargetClient.GetRoleplay().Cigarettes > 100)
                            AmountToRob = 100;
                        else
                            AmountToRob = TargetClient.GetRoleplay().Cigarettes;

                        int MaxAmount = Convert.ToInt32(Math.Floor((Double)AmountToRob / 5));
                        int MinAmount = Convert.ToInt32(Math.Floor((double)AmountToRob / 20));

                        int Amount = Random.Next(MinAmount, MaxAmount + 1);

                        Session.GetRoleplay().Cigarettes += Amount;
                        TargetClient.GetRoleplay().Cigarettes -= Amount;

                        Success = true;
                        RobbedItems += "e " + String.Format("{0:N0}", Amount) + "g de cigarros, ";
                    }
                }
                
                if (!Success)
                {
                    Session.SendWhisper("Desculpe, mas esta pessoa é muito pobre para roubar!", 1);
                    return;
                }

                if (Success)
                {
                    if (!Session.GetRoleplay().WantedFor.Contains("roubar"))
                        Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "roubar os cidadão(s), ";

                    Session.Shout("*Coloca as mãos nos bolsos de " + TargetClient.GetHabbo().Username + " e rouba " + RobbedItems.TrimEnd(',', ' ') + "*", 4);
					
					
					
				
					
            lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
				foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;
                
                    client.SendMessage(new RoomNotificationComposer("staff_notice", "message", "[Notícia Urgente] " + Session.GetHabbo().Username + " roubou " + RobbedItems.TrimEnd(',', ' ') + " de " + TargetClient.GetHabbo().Username + ", tome cuidado!"));
                }
            }
			
                Session.SendWhisper("Você roubou " + RobbedItems.TrimEnd(',', ' ') + " de " + TargetClient.GetHabbo().Username + ", todos, inclusive a polícia recebeu a notícia!", 1);
                if (Session.GetRoleplay().TryGetCooldown("roubar"))
                return;
					
					
					

                    TargetClient.SendWhisper("Você foi roubado e perdeu " + RobbedItems.TrimEnd(',', ' ') + " para " + Session.GetHabbo().Username + ", todos, inclusive a polícia recebeu a notícia!", 1);
					
                    Session.GetRoleplay().CooldownManager.CreateCooldown("roubar", 1000, 300);
                    Session.GetRoleplay().SpecialCooldowns.TryUpdate("roubar", 300, Session.GetRoleplay().SpecialCooldowns["roubar"]);
                }
            }
            else
            {
                Session.SendWhisper("Você precisa se aproximar de " + TargetClient.GetHabbo().Username + " para roubá-lo!", 1);
                return;
            }
            #endregion
        }
    }
}