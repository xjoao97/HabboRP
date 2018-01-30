using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Items.Data.Moodlight;

namespace Plus.HabboRoleplay.Misc
{
    public static class DayNightManager
    {
        /// <summary>
        /// Gets the intensity based on time of day.
        /// </summary>
        /// <returns></returns>
        public static int GetIntensities()
        {
            DateTime TimeNow = DateTime.Now;
            TimeSpan TimeOfDay = TimeNow.TimeOfDay;
            int Intensity = 255;

            #region Hours

            if (TimeOfDay.Hours == 00)
                Intensity = 160;
            else if (TimeOfDay.Hours == 01)
                Intensity = 150;
            else if (TimeOfDay.Hours == 02)
                Intensity = 130;
            else if (TimeOfDay.Hours == 03)
                Intensity = 130;
            else if (TimeOfDay.Hours == 04)
                Intensity = 120;
            else if (TimeOfDay.Hours == 05)
                Intensity = 120;
            else if (TimeOfDay.Hours == 06)
                Intensity = 150;
            else if (TimeOfDay.Hours == 07)
                Intensity = 210;
            else if (TimeOfDay.Hours == 08)
                Intensity = 215;
            else if (TimeOfDay.Hours == 09)
                Intensity = 215;
            else if (TimeOfDay.Hours == 10)
                Intensity = 225;
            else if (TimeOfDay.Hours == 11)
                Intensity = 235;
            else if (TimeOfDay.Hours == 12)
                Intensity = 245;
            else if (TimeOfDay.Hours == 13)
                Intensity = 255;
            else if (TimeOfDay.Hours == 14)
                Intensity = 255;
            else if (TimeOfDay.Hours == 15)
                Intensity = 255;
            else if (TimeOfDay.Hours == 16)
                Intensity = 255;
            else if (TimeOfDay.Hours == 17)
                Intensity = 255;
            else if (TimeOfDay.Hours == 18)
                Intensity = 235;
            else if (TimeOfDay.Hours == 19)
                Intensity = 175;
            else if (TimeOfDay.Hours == 20)
                Intensity = 175;
            else if (TimeOfDay.Hours == 21)
                Intensity = 170;
            else if (TimeOfDay.Hours == 22)
                Intensity = 160;
            else if (TimeOfDay.Hours == 23)
                Intensity = 160;

            #endregion

            return Intensity;
        }

        /// <summary>
        /// Gets the taxi wait time based on time of day.
        /// </summary>
        /// <returns></returns>
        public static int GetTaxiTime()
        {
            DateTime TimeNow = DateTime.Now;
            TimeSpan TimeOfDay = TimeNow.TimeOfDay;
            int TaxiTime = 0;

            #region Hours

            if (!RoleplayManager.DayNightTaxiTime)
                TaxiTime = 0;

            if (TimeOfDay.Hours == 00)
                TaxiTime = 3;
            else if (TimeOfDay.Hours == 01)
                TaxiTime = 4;
            else if (TimeOfDay.Hours == 02)
                TaxiTime = 5;
            else if (TimeOfDay.Hours == 03)
                TaxiTime = 5;
            else if (TimeOfDay.Hours == 04)
                TaxiTime = 4;
            else if (TimeOfDay.Hours == 05)
                TaxiTime = 3;
            else if (TimeOfDay.Hours == 06)
                TaxiTime = 2;
            else if (TimeOfDay.Hours == 07)
                TaxiTime = 1;
            else if (TimeOfDay.Hours == 08)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 09)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 10)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 11)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 12)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 13)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 14)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 15)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 16)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 17)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 18)
                TaxiTime = 0;
            else if (TimeOfDay.Hours == 19)
                TaxiTime = 1;
            else if (TimeOfDay.Hours == 20)
                TaxiTime = 1;
            else if (TimeOfDay.Hours == 21)
                TaxiTime = 2;
            else if (TimeOfDay.Hours == 22)
                TaxiTime = 3;
            else if (TimeOfDay.Hours == 23)
                TaxiTime = 3;

            #endregion

            return TaxiTime;
        }

        /// <summary>
        /// Runs the day and night cycle.
        /// </summary>
        /// <param name="Intensity"></param>
        public static void ActiveCycle(int Intensity = 255)
        {
            try
            {
                DateTime TimeNow = DateTime.Now;
                TimeSpan TimeOfDay = TimeNow.TimeOfDay;
                string LandscapeData = "1.1";

                foreach (Room Room in PlusEnvironment.GetGame().GetRoomManager().GetRooms())
                {
                    #region Null checks

                    if (Room == null)
                        continue;

                    if (Room.GetRoomItemHandler() == null)
                        continue;

                    if (Room.GetRoomItemHandler().GetWall == null)
                        continue;

                    #endregion

                    #region Apply Landscape

                    if (TimeOfDay.Hours == 00)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 01)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 02)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 03)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 04)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 05)
                        LandscapeData = "1.1";
                    else if (TimeOfDay.Hours == 06)
                        LandscapeData = "1.1";
                    else if (TimeOfDay.Hours == 07)
                        LandscapeData = "5.1";
                    else if (TimeOfDay.Hours == 08)
                        LandscapeData = "5.1";
                    else if (TimeOfDay.Hours == 09)
                        LandscapeData = "5.1";
                    else if (TimeOfDay.Hours == 10)
                        LandscapeData = "4.1";
                    else if (TimeOfDay.Hours == 11)
                        LandscapeData = "4.1";
                    else if (TimeOfDay.Hours == 12)
                        LandscapeData = "4.1";
                    else if (TimeOfDay.Hours == 13)
                        LandscapeData = "2.1";
                    else if (TimeOfDay.Hours == 14)
                        LandscapeData = "2.1";
                    else if (TimeOfDay.Hours == 15)
                        LandscapeData = "2.1";
                    else if (TimeOfDay.Hours == 16)
                        LandscapeData = "2.1";
                    else if (TimeOfDay.Hours == 17)
                        LandscapeData = "2.1";
                    else if (TimeOfDay.Hours == 18)
                        LandscapeData = "5.1";
                    else if (TimeOfDay.Hours == 19)
                        LandscapeData = "5.1";
                    else if (TimeOfDay.Hours == 20)
                        LandscapeData = "5.1";
                    else if (TimeOfDay.Hours == 21)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 22)
                        LandscapeData = "6.1";
                    else if (TimeOfDay.Hours == 23)
                        LandscapeData = "6.1";

                    foreach (Item Item in Room.GetRoomItemHandler().GetWall)
                    {
                        if (Item == null)
                            continue;

                        if (Item.GetBaseItem() == null)
                            continue;

                        if (Item.GetBaseItem().ItemName == "window_skyscraper")
                        {
                            Room.Landscape = LandscapeData;
                            Room.RoomData.Landscape = LandscapeData;

                            Room.SendMessage(new RoomPropertyComposer("landscape", LandscapeData));
                        }
                    }

                    #endregion

                    #region Apply Moodlight

                    if (Room.RoomData.DriveEnabled)
                    {
                        foreach (Item Item in Room.GetRoomItemHandler().GetWall)
                        {
                            if (Item == null)
                                continue;

                            if (Item.GetBaseItem() == null)
                                continue;

                            if (Item.GetBaseItem().InteractionType == InteractionType.MOODLIGHT)
                            {
                                Room.MoodlightData = new MoodlightData(Item.Id);

                                if (Room.MoodlightData == null)
                                    continue;

                                Room.MoodlightData.Enabled = true;
                                Room.MoodlightData.Enable();
                                Room.MoodlightData.CurrentPreset = 1;
                                Room.MoodlightData.UpdatePreset(1, "#000000", Intensity, false, true);

                                if (Item.ExtraData == null)
                                    continue;

                                Item.ExtraData = Room.MoodlightData.GenerateExtraData();
                                Item.UpdateState();
                            }
                        }
                    }

                    #endregion
                }
            }
            catch(Exception e)
            {
                Logging.LogCriticalException("Error in ActiveCycle() void: " + e);
            }
        }

        /// <summary>
        /// Set the day/night intensity based on time of day.
        /// </summary>
        public static void SetTime()
        {
            if (!RoleplayManager.DayNightSystem)
                return;

            int Intensity = 255;

            Intensity = GetIntensities();
            ActiveCycle(Intensity);
        }
    }
}