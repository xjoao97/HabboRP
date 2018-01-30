using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class EquipCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_equip"; }
        }

        public string Parameters
        {
            get { return "%weapon%"; }
        }

        public string Description
        {
            get { return "Equipa a arma desejada."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir o nome de arma!", 1);
                return;
            }

            string GunName = Params[1].ToLower();
            Weapon BaseWeapon = WeaponManager.getWeapon(GunName);

            if (BaseWeapon == null)
            {
                Session.SendWhisper("Esta arma não existe!", 1);
                return;
            }

            if (Session.GetRoomUser().Frozen)
                return;

            if (Session.GetRoleplay().EquippedWeapon != null)
            {
                if (Session.GetRoleplay().EquippedWeapon.Name == BaseWeapon.Name)
                {
                    Session.SendWhisper("Você já possui esta arma equipada!", 1);
                    return;
                }
            }

            if (!Session.GetRoleplay().OwnedWeapons.ContainsKey(GunName))
            {
                Session.SendWhisper("Você não possui essa arma!", 1);
                return;
            }

            if (!Session.GetRoleplay().OwnedWeapons[GunName].CanUse)
            {
                Session.SendWhisper("Você não pode usar esta arma até pagar uma multa de R$" + String.Format("{0:N0}", Session.GetRoleplay().OwnedWeapons[GunName].CostFine) + "!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode equipar uma arma enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode equipar uma arma enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Você não consegue alcançar sua " + GunName + ", você está algemado", 1);
                return;
            }
            
            if (BaseWeapon.LevelRequirement > Session.GetRoleplay().Level)
            {
                Session.SendWhisper("Desculpa! Esta arma só pode ser usada por: Nível " + BaseWeapon.LevelRequirement + ".", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("equipar", true))
                return;

            if (Session.GetRoleplay().DrivingCar)
            {
                Session.SendWhisper("Por favor, pare de dirigir o seu veículo para equipar uma arma!", 1);
                return;
            }
            #endregion

            #region Execute
            var Weapon = Session.GetRoleplay().OwnedWeapons[GunName];

            string EquipMessage = Weapon.EquipText;
            EquipMessage = EquipMessage.Replace("[NAME]", Weapon.PublicName);

            Session.Shout(EquipMessage, 4);
           // Session.SendWhisper("Sua " + Weapon.PublicName + " tem " + Weapon.clip + "/" + Weapon.clip + " balas no pente.", 1);

            Session.GetRoleplay().EquippedWeapon = Weapon;
            Session.GetRoleplay().CooldownManager.CreateCooldown("equipar", 1000, 3);

            if (Session.GetRoomUser().CurrentEffect != Weapon.EffectID)
                Session.GetRoomUser().ApplyEffect(Weapon.EffectID);

            if (Session.GetRoomUser().CarryItemID != Weapon.HandItem)
                Session.GetRoomUser().CarryItem(Weapon.HandItem);

            return;
            #endregion
        }
    }
}