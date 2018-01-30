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
using System.Threading;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal
{
    class SmokeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_criminal_activity_smoke"; }
        }

        public string Parameters
        {
            get { return "%droga%"; }
        }

        public string Description
        {
            get { return "Permite fumar um cigarro ou maconha."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            string Type = "maconha";
            if (Params.Length > 1)
                Type = Params[1].ToLower();

            if (Type == "maconha")
            {
                if (Session.GetRoleplay().Weed < 10)
                {
                    Session.SendWhisper("Você precisa de pelo menos 10g de maconha para fumar!", 1);
                    return;
                }

                if (Session.GetRoleplay().TryGetCooldown("maconha", false))
                {
                    Session.SendWhisper("Você já está drogado!", 1);
                    return;
                }

                if (Session.GetRoleplay().IsDead)
                {
                    Session.SendWhisper("Você não pode completar esta ação enquanto está morto!", 1);
                    return;
                }

                if (Session.GetRoleplay().IsJailed)
                {
                    Session.SendWhisper("Você não pode completar esta ação enquanto está preso!", 1);
                    return;
                }
            }

            if (Type == "cig" || Type == "cigs" || Type == "cigarro" || Type == "cigarros")
            {
                if (Session.GetRoleplay().Cigarettes < 1)
                {
                    Session.SendWhisper("Você precisa de pelo menos um cigarro para fumar!", 1);
                    return;
                }

                if (Session.GetRoleplay().CurHealth >= Session.GetRoleplay().MaxHealth)
                {
                    Session.SendWhisper("Sua vida já está cheia!", 1);
                    return;
                }

                if (Session.GetRoleplay().TryGetCooldown("cigarro"))
                    return;

                if (Session.GetRoleplay().Game != null)
                {
                    Session.SendWhisper("Você não pode fumar cigarros enquanto estiver dentro de um evento!", 1);
                    return;
                }

                if (Session.GetRoleplay().IsDead)
                {
                    Session.SendWhisper("Você não pode completar esta ação enquanto está morto!", 1);
                    return;
                }

                if (Session.GetRoleplay().IsJailed)
                {
                    Session.SendWhisper("Você não pode completar esta ação enquanto está preso!", 1);
                    return;
                }
            }
            #endregion

            #region Execute
            switch (Type)
            {
                #region Weed
                case "weed":
				case "maconha":
                    {
                        Session.GetRoleplay().Weed -= 10;
                        Session.GetRoleplay().HighOffWeed = true;
                        Session.GetRoleplay().CooldownManager.CreateCooldown("maconha", 1000, 45);
                        Session.Shout("*Pega 10 g de maconha, dobrando-o com papel embrulhado e depois fumando*", 4);

                        if (!Session.GetRoleplay().WantedFor.Contains("fumando substancias ilegais"))
                            Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "fumar maconha, ";
                        break;
                    }
                #endregion

                #region Cigarettes
                case "cig":
                case "cigs":
                case "cigarette":
                case "cigarettes":
				case "cigarros":
				case "cigarro":
                    {
                        Session.GetRoleplay().CooldownManager.CreateCooldown("cigarro", 1000, 5);
                        Session.GetRoleplay().Cigarettes--;
                        if ((Session.GetRoleplay().CurHealth + 3) >= Session.GetRoleplay().MaxHealth)
                            Session.GetRoleplay().CurHealth = Session.GetRoleplay().MaxHealth;
                        else
                            Session.GetRoleplay().CurHealth += 3;

                        if ((Session.GetRoleplay().CurEnergy + 5) >= Session.GetRoleplay().MaxEnergy)
                            Session.GetRoleplay().CurEnergy = Session.GetRoleplay().MaxEnergy;
                        else
                            Session.GetRoleplay().CurEnergy += 5;

                        Session.Shout("*Retira um cigarro do bolso e fuma [+3 Health, +5 Energy]*", 4);

                        if (!Room.RoomData.DriveEnabled)
                        {
                            if (!Session.GetRoleplay().WantedFor.Contains("fumar substâncias legais dentro de um estabelecimento"))
                                Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "fumar substancias legais dentro de um estabelecimento, ";
                        }
                        break;
                    }
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("Você só pode fumar 'maconha' e 'cigarro'!", 1);
                        break;
                    }
                #endregion
            }
            #endregion
        }
    }
}