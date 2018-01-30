using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Collections.Concurrent;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class RPWeaponsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_rpweapons"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Fornece-lhe uma lista das suas armas de jogo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você esqueceu de inserir um usuário para verificar!", 1);
                return;
            }

            #region Variables
            ConcurrentDictionary<string, Weapon> Weapons = new ConcurrentDictionary<string, Weapon>();

            uint id = 0;
            string Username = Params[1];
            GameClients.GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            #endregion

            #region Generate Weapons Data
            if (TargetClient == null)
            {
                DataTable Weps = null;
                Weapons.Clear();

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` where `username` = '" + Username + "' LIMIT 1");
                    var UserRow = dbClient.getRow();

                    if (UserRow == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    int UserId = Convert.ToInt32(UserRow["id"]);

                    dbClient.SetQuery("SELECT * FROM `rp_weapons_owned` WHERE `user_id` = '" + UserId + "'");
                    Weps = dbClient.getTable();

                    if (Weps == null)
                    {
                        Session.SendWhisper("Esta pessoa não possui armas!", 1);
                        return;
                    }
                    else
                    {
                        foreach (DataRow Row in Weps.Rows)
                        {
                            id++;

                            string basename = Convert.ToString(Row["base_weapon"]);
                            string name = Convert.ToString(Row["name"]);
                            int mindam = Convert.ToInt32(Row["min_damage"]);
                            int maxdam = Convert.ToInt32(Row["max_damage"]);
                            int range = Convert.ToInt32(Row["range"]);
                            bool canuse = Convert.ToBoolean(Row["can_use"]);

                            if (!Weapons.ContainsKey(basename))
                            {
                                Weapon BaseWeapon = WeaponManager.getWeapon(basename);

                                if (BaseWeapon != null)
                                {
                                    Weapon Weapon = new Weapon(id, basename, name, BaseWeapon.FiringText, BaseWeapon.EquipText, BaseWeapon.UnEquipText, BaseWeapon.ReloadText, BaseWeapon.Energy, BaseWeapon.EffectID, BaseWeapon.HandItem, range, mindam, maxdam, BaseWeapon.ClipSize, BaseWeapon.ReloadTime, BaseWeapon.Cost, BaseWeapon.CostFine, BaseWeapon.Stock, BaseWeapon.LevelRequirement, canuse);

                                    if (Weapon != null)
                                        Weapons.TryAdd(basename, Weapon);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Username = TargetClient.GetHabbo().Username;
                Weapons = TargetClient.GetRoleplay().OwnedWeapons;
            }
            #endregion

            #region Execute
            if (Weapons.Count <= 0)
            {
                Session.SendWhisper("Esta pessoa não possui armas.", 1);
                return;
            }
            else
            {
                StringBuilder Message = new StringBuilder().Append("<----- " + Username + " - Armas ----->\n\n");

                lock (TargetClient.GetRoleplay().OwnedWeapons.Values)
                {
                    foreach (Weapon Weapon in TargetClient.GetRoleplay().OwnedWeapons.Values)
                    {
                        Message.Append(Weapon.PublicName + " ---> Alcance: " + Weapon.Range + " e Damage: " + Weapon.MinDamage + " - " + Weapon.MaxDamage + ".\n\n");
                        //*Message.Append("Balas: " + Weapon.Clip + "/" + Weapon.ClipSize + ".\n\n");
                    }
                }
                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            }
            #endregion
        }
    }
}