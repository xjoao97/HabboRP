using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus
{
    class PlusStaticGameSettings
    {
        /// <summary>
        ///     The amount of credits a user will recieve every x minutes
        /// </summary>
        public const int UserCreditsUpdateAmount = 100;

        /// <summary>
        ///     The amount of pixels a user will recieve every x minutes
        /// </summary>
        public const int UserPixelsUpdateAmount = 100;

        /// <summary>
        ///     The time a user will have to wait for Credits/Pixels update in minutes
        /// </summary>
        public const int UserCreditsUpdateTimer = 15;

        /// <summary>
        ///     The maximum amount of furniture that can be placed in a room.
        /// </summary>
        public const int RoomFurnitureLimit = 7500;

        /// <summary>
        ///     The maximum amount of pet instances that can be placed into a room.
        /// </summary>
        public const int RoomPetPlacementLimit = 25;

        /// <summary>
        ///     The amount of pets a user can place in a room that isn't owned by them.
        /// </summary>
        public const int UserPetPlacementRoomLimit = 5;

        /// <summary>
        ///     The maximum life time of a room promotion.
        /// </summary>
        public const int RoomPromotionLifeTime = 120;

        /// <summary>
        ///     The maximum amount of friends a user can have.
        /// </summary>
        public const int MessengerFriendLimit = 5000;

        /// <summary>
        ///     The amount of credits a group costs.
        /// </summary>
        public const int GroupPurchaseAmount = 250;

        /// <summary>
        ///     The maximum amount of banned words that a client can send per session.
        /// </summary>
        public const int BannedPhrasesAmount = 6;

        /// <summary>
        /// The maximum amount of members a group can exceed before being eligible for deletion.
        /// </summary>
        public const int GroupMemberDeletionLimit = 500;
    }
}
