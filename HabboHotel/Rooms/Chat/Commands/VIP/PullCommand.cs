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
    class PullCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_pull"; }
        }

        public string Parameters
        {
            get { return "%alvo%"; }
        }

        public string Description
        {
            get { return "Puxe outro usuário para você."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome de usuário do usuário que deseja puxar.", 1);
                return;
            }

            if (!Room.PullEnabled && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("Você não pode puxar este quarto!", 1);
                return;
            }

            if (Session.GetRoleplay().StaffOnDuty || Session.GetRoleplay().AmbassadorOnDuty && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("Você não pode puxar alguém enquanto você está plantão!", 1);
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
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez eles não estejam online ou no quarto.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode se puxar!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode puxar alguém está ausente!", 1);
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("Opa, você não pode puxar um usuário enquanto ele está com o modo teleporte ativado!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().StaffOnDuty)
            {
                Session.SendWhisper("Você não pode puxar um membro da equipe que está de plantão!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().AmbassadorOnDuty)
            {
                Session.SendWhisper("Você não pode puxar um embaixador que está de plantão!", 1);
                return;
            }

            if (Session.GetRoleplay().Team != null && TargetClient.GetRoleplay().Team != null)
            {
                if (Session.GetRoleplay().Team == TargetClient.GetRoleplay().Team)
                {
                    Session.SendWhisper("Você não pode puxar o membro da sua equipe enquanto estiver dentro de um evento!", 1);
                    return;
                }
            }

            if (Session.GetRoleplay().TryGetCooldown("pull"))
                return;

            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            if (!HabboRoleplay.Misc.RoleplayManager.PushPullOnArrows)
            {
                if (TargetClient.GetRoleplay().Game == null || TargetClient.GetRoleplay().Team == null)
                {
                    List<Item> RoomArrow = Room.GetRoomItemHandler().GetFloor.Where(x => x != null && x.GetBaseItem() != null && x.GetBaseItem().InteractionType == InteractionType.ARROW && (ThisUser.RotBody == 0 ? TargetUser.SetY - 1 == x.GetY : (ThisUser.RotBody == 1 ? TargetUser.SetX + 1 == x.GetX && TargetUser.Y - 1 == x.GetY : (ThisUser.RotBody == 2 ? TargetUser.SetX + 1 == x.GetX : (ThisUser.RotBody == 3 ? TargetUser.X + 1 == x.GetX && TargetUser.Y + 1 == x.GetY : (ThisUser.RotBody == 4 ? TargetUser.SetY + 1 == x.GetY : (ThisUser.RotBody == 5 ? TargetUser.X - 1 == x.GetX && TargetUser.Y + 1 == x.GetY : (ThisUser.RotBody == 6 ? TargetUser.SetX - 1 == x.GetX : (ThisUser.RotBody == 7 ? TargetUser.X - 1 == x.GetX && TargetUser.Y - 1 == x.GetY : false))))))))).ToList();
                    if (RoomArrow.Count > 0)
                    {
                        Session.SendWhisper("Você não pode retirar os usuários desta sala!", 1);
                        return;
                    }
                }
            }

            string PushDirection = "down";
            if (TargetClient.GetHabbo().CurrentRoomId == Session.GetHabbo().CurrentRoomId && (Math.Abs(ThisUser.X - TargetUser.X) < 3 && Math.Abs(ThisUser.Y - TargetUser.Y) < 3))
            {
                if (ThisUser.RotBody == 0)
                    PushDirection = "up";
                if (ThisUser.RotBody == 2)
                    PushDirection = "right";
                if (ThisUser.RotBody == 4)
                    PushDirection = "down";
                if (ThisUser.RotBody == 6)
                    PushDirection = "left";

                if (PushDirection == "up")
                    TargetUser.MoveTo(ThisUser.X, ThisUser.Y - 1);

                if (PushDirection == "right")
                    TargetUser.MoveTo(ThisUser.X + 1, ThisUser.Y);

                if (PushDirection == "down")
                    TargetUser.MoveTo(ThisUser.X, ThisUser.Y + 1);

                if (PushDirection == "left")
                    TargetUser.MoveTo(ThisUser.X - 1, ThisUser.Y);

                Session.Shout("*Puxa " + TargetClient.GetHabbo().Username + " em minha direção*", 4);
                Session.GetRoleplay().CooldownManager.CreateCooldown("pull", 1000, 3);
                return;
            }
            else
            {
                Session.SendWhisper("Esse usuário não está perto o suficiente, tente se aproximar!", 1);
                return;
            }
        }
    }
}
