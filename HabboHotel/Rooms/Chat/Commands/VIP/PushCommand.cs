using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class PushCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_push"; }
        }

        public string Parameters
        {
            get { return "%alvo%"; }
        }

        public string Description
        {
            get { return "Empurre outro usuário."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome de usuário do usuário que deseja empurrar.", 1);
                return;
            }

            if (!Room.PushEnabled && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("Você não pode empurrar nesta sala!", 1);
                return;
            }

            if (Session.GetRoleplay().StaffOnDuty || Session.GetRoleplay().AmbassadorOnDuty && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("Você não pode empurrar alguém enquanto você está de plantão!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode se empurrar!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode empurrar alguém que não está jogando o jogo agora!", 1);
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("Opa, você não pode empurrar um usuário que possui o modo de teleporte ativado.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().StaffOnDuty)
            {
                Session.SendWhisper("Você não pode empurrar alguém que está de plantão!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().AmbassadorOnDuty)
            {
                Session.SendWhisper("Você não pode empurrar um embaixador que está de plantão!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("empurrar"))
                return;

            if (Session.GetRoleplay().Team != null && TargetClient.GetRoleplay().Team != null)
            {
                if (Session.GetRoleplay().Team == TargetClient.GetRoleplay().Team)
                {
                    Session.SendWhisper("Você não pode empurrar o membro da sua equipe enquanto estiver dentro de um evento!", 1);
                    return;
                }
            }

            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            if (!((Math.Abs(TargetUser.X - ThisUser.X) >= 2) || (Math.Abs(TargetUser.Y - ThisUser.Y) >= 2)))
            {
                if (!HabboRoleplay.Misc.RoleplayManager.PushPullOnArrows)
                {
                    if (TargetClient.GetRoleplay().Game == null || TargetClient.GetRoleplay().Team == null)
                    {
                        List<Item> RoomArrow = Room.GetRoomItemHandler().GetFloor.Where(x => x != null && x.GetBaseItem() != null && x.GetBaseItem().InteractionType == InteractionType.ARROW && (ThisUser.RotBody == 0 ? TargetUser.SetY - 1 == x.GetY : (ThisUser.RotBody == 1 ? TargetUser.SetX + 1 == x.GetX && TargetUser.Y - 1 == x.GetY : (ThisUser.RotBody == 2 ? TargetUser.SetX + 1 == x.GetX : (ThisUser.RotBody == 3 ? TargetUser.X + 1 == x.GetX && TargetUser.Y + 1 == x.GetY : (ThisUser.RotBody == 4 ? TargetUser.SetY + 1 == x.GetY : (ThisUser.RotBody == 5 ? TargetUser.X - 1 == x.GetX && TargetUser.Y + 1 == x.GetY : (ThisUser.RotBody == 6 ? TargetUser.SetX - 1 == x.GetX : (ThisUser.RotBody == 7 ? TargetUser.X - 1 == x.GetX && TargetUser.Y - 1 == x.GetY : false))))))))).ToList();
                        if (RoomArrow.Count > 0)
                        {
                            Session.SendWhisper("Você não pode expulsar os usuários desta sala!", 1);
                            return;
                        }
                    }
                }

                if (TargetUser.RotBody == 4)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 1);
                }

                if (ThisUser.RotBody == 0)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 1);
                }

                if (ThisUser.RotBody == 6)
                {
                    TargetUser.MoveTo(TargetUser.X - 1, TargetUser.Y);
                }

                if (ThisUser.RotBody == 2)
                {
                    TargetUser.MoveTo(TargetUser.X + 1, TargetUser.Y);
                }

                if (ThisUser.RotBody == 3)
                {
                    TargetUser.MoveTo(TargetUser.X + 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 1);
                }

                if (ThisUser.RotBody == 1)
                {
                    TargetUser.MoveTo(TargetUser.X + 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 1);
                }

                if (ThisUser.RotBody == 7)
                {
                    TargetUser.MoveTo(TargetUser.X - 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 1);
                }

                if (ThisUser.RotBody == 5)
                {
                    TargetUser.MoveTo(TargetUser.X - 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 1);
                }

                Session.Shout("*Empurra " + TargetClient.GetHabbo().Username + " para longe*");
                Session.GetRoleplay().CooldownManager.CreateCooldown("puxar", 1000, 3);
            }
            else
            {
                Session.SendWhisper("Opa, " + TargetClient.GetHabbo().Username + " não está perto!", 1);
            }
        }
    }
}
