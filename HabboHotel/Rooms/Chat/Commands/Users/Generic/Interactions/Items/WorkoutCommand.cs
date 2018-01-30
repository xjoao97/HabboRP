using System;
using System.Linq;
using System.Text;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.Food;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class WorkoutCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_workout"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Começa a malhar na Academia."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetRoomUser() == null)
                return;

            if (Session.GetRoleplay().IsWorkingOut)
            {
                Session.SendWhisper("Você já está malhando!", 1);
                return;
            }

            if (Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você não pode fazer exercícios enquanto trabalha!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode fazer exercícios enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode fazer exercícios enquanto está preso!", 1);
                return;
            }

            if (!Room.GymEnabled)
            {
                Session.SendWhisper("Você deve estar dentro da academia para se exercitar!", 1);
                return;
            }

            if (Session.GetRoleplay().CurEnergy <= 0)
            {
                Session.SendWhisper("Você não tem energia suficiente para treinar!", 1);
                return;
            }

            bool HasTreadmill = Room.GetRoomItemHandler().GetFloor.Where(x => (x.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || x.GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer") && x.Coordinate == Session.GetRoomUser().Coordinate).ToList().Count > 0;

            if (!HasTreadmill)
            { 
                Session.SendWhisper("Você deve estar em uma esteira ou crosstrainer para treinar!!", 1);
                return;
            }

            Item Treadmill = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => (x.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || x.GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer") && x.Coordinate == Session.GetRoomUser().Coordinate);

            if (Treadmill == null)
            {
                Session.SendWhisper("Você deve estar em uma esteira ou crosstrainer para treinar!!", 1);
                return;
            }

            bool Strength = false;
            if (Treadmill.GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill")
                Strength = true;

            if (Strength && Session.GetRoleplay().Strength >= RoleplayManager.StrengthCap)
            {
                Session.SendWhisper("Você atingiu o nível máximo de força de: " + RoleplayManager.StrengthCap + "!", 1);
                return;
            }

            if (!Strength && Session.GetRoleplay().Stamina >= RoleplayManager.StaminaCap)
            {
                Session.SendWhisper("Você alcançou o nível de resistência máxima de: " + RoleplayManager.StaminaCap + "!", 1);
                return;
            }
            #endregion

            #region Execute
            if (Session.GetRoomUser().isSitting)
            {
                Session.GetRoomUser().Z += 0.35;
                Session.GetRoomUser().RemoveStatus("sit");
                Session.GetRoomUser().isSitting = false;
                Session.GetRoomUser().UpdateNeeded = true;
            }
            else if (Session.GetRoomUser().isLying)
            {
                Session.GetRoomUser().Z += 0.35;
                Session.GetRoomUser().RemoveStatus("lay");
                Session.GetRoomUser().isLying = false;
                Session.GetRoomUser().UpdateNeeded = true;
            }

            Treadmill.ExtraData = "1";
            Treadmill.InteractingUser = Session.GetHabbo().Id;
            Treadmill.UpdateState(false, true);
            Treadmill.RequestUpdate(1, true);

            if (!Strength)
                Session.GetRoomUser().ApplyEffect(195);
            else
                Session.GetRoomUser().ApplyEffect(194);

            object[] Data = { Treadmill.Id, Strength };

            Session.GetRoomUser().SetRot(Treadmill.Rotation, false);
            Session.GetRoleplay().IsWorkingOut = true;

            Session.GetRoleplay().TimerManager.CreateTimer("workout", 1000, true, Data);

            if (Strength)
            {
                Session.Shout("*Começa a correr na esteira para treinar sua Força*", 4);
                Session.SendWhisper("Tempo de exercício necessário: " + String.Format("{0:N0}", Session.GetRoleplay().StrengthEXP) + "/" + String.Format("{0:N0}", ((!LevelManager.StrengthLevels.ContainsKey(Session.GetRoleplay().Strength + 1) ? 10400 : LevelManager.StrengthLevels[Session.GetRoleplay().Strength + 1]))), 1);
            }
            else
            {
                Session.Shout("*Começa correndo no crosstrainer para exercitar seu Sangue*", 4);
                Session.SendWhisper("Tempo de exercício necessário: " + String.Format("{0:N0}", Session.GetRoleplay().StaminaEXP) + "/" + String.Format("{0:N0}", ((!LevelManager.StaminaLevels.ContainsKey(Session.GetRoleplay().Stamina + 1) ? 25200 : LevelManager.StaminaLevels[Session.GetRoleplay().Stamina + 1]))), 1);
            }
            #endregion
        }
    }
}