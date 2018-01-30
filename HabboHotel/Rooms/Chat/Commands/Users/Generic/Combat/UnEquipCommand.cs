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
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class UnEquipCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_combat_equip_undo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desequipa qualquer arma que você tenha equipado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (Session.GetRoomUser().Frozen)
                return;

            if (Session.GetRoleplay().EquippedWeapon == null)
            {
                Session.SendWhisper("Você não possui uma arma equipada!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("desequipar", true))
                return;

            #endregion

            #region Execute

            CryptoRandom Random = new CryptoRandom();
            int Chance = Random.Next(1, 101);

            if (Chance <= 8)
            {
                Session.Shout("*Coloca sua " + Session.GetRoleplay().EquippedWeapon.PublicName + " no bolso*", 4);
                return;
            }
            else
            {
                string UnEquipMessage = Session.GetRoleplay().EquippedWeapon.UnEquipText;
                UnEquipMessage = UnEquipMessage.Replace("[NAME]", Session.GetRoleplay().EquippedWeapon.PublicName);

                Session.Shout(UnEquipMessage, 4);

                if (Session.GetRoomUser().CurrentEffect == Session.GetRoleplay().EquippedWeapon.EffectID)
                    Session.GetRoomUser().ApplyEffect(0);

                if (Session.GetRoomUser().CarryItemID == Session.GetRoleplay().EquippedWeapon.HandItem)
                    Session.GetRoomUser().CarryItem(0);

                Session.GetRoleplay().CooldownManager.CreateCooldown("desequipar", 1000, 3);
                Session.GetRoleplay().EquippedWeapon = null;
                return;
            }

            #endregion
        }
    }
}