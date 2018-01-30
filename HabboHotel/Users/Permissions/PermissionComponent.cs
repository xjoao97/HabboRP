using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Users.Permissions
{
    /// <summary>
    /// Permissions for a specific Player.
    /// </summary>
    public sealed class PermissionComponent
    {
        /// <summary>
        /// Permission rights are stored here.
        /// </summary>
        private readonly List<string> _permissions;

        private readonly List<string> _commands;

        private readonly bool _hasSpecialRights;

        public PermissionComponent(bool HasSpecialRights)
        {
            this._hasSpecialRights = HasSpecialRights;
            this._permissions = new List<string>();
            this._commands = new List<string>();
        }

        /// <summary>
        /// Initialize the PermissionComponent.
        /// </summary>
        /// <param name="Player"></param>
        public bool Init(Habbo Player)
        {
            if (this._permissions == null)
                return true;

            if (this._commands == null)
                return true;

            if (this._permissions.Count > 0)
                this._permissions.Clear();

            if (this._commands.Count > 0)
                this._commands.Clear();

            this._permissions.AddRange(PlusEnvironment.GetGame().GetPermissionManager().GetPermissionsForPlayer(Player));
            this._commands.AddRange(PlusEnvironment.GetGame().GetPermissionManager().GetCommandsForPlayer(Player));
            return true;
        }

        /// <summary>
        /// Checks if the user has the specified right.
        /// </summary>
        /// <param name="Right"></param>
        /// <returns></returns>
        public bool HasRight(string Right)
        {
            if (this._hasSpecialRights)
                return true;

            return this._permissions.Contains(Right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public bool HasCommand(string Command)
        {
            if (this._hasSpecialRights)
                return true;

            return this._commands.Contains(Command);
        }

        /// <summary>
        /// Dispose of the permissions list.
        /// </summary>
        public void Dispose()
        {
            this._permissions.Clear();
        }
    }
}
