﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Nekres.Music_Mixer
{
    public enum TyrianTime {
        /// <summary>
        /// Escape
        /// </summary>
        [EnumMember(Value = "none")] None,
        /// <summary>
        /// 5 minutes 05:00–06:00
        /// </summary>
        [EnumMember(Value = "dawn")] Dawn,
        /// <summary>
        /// 70 minutes 06:00–20:00
        /// </summary>
        [EnumMember(Value = "day")] Day,
        /// <summary>
        /// 5 minutes 20:00–21:00
        /// </summary>
        [EnumMember(Value = "dusk")] Dusk,
        /// <summary>
        /// 40 minutes 21:00–05:00
        /// </summary>
        [EnumMember(Value = "night")] Night
    }
    internal static class TyrianTimeExtension
    {
        /// <summary>
        /// Resolves a given day cycle to its two day cycle context which is either Night or Day.
        /// </summary>
        /// <param name="time">The time to resolve the day cycle context of.</param>
        /// <returns>The full day cycle that is the context.</returns>
        public static TyrianTime Resolve(this TyrianTime time) {
            switch (time) {
                case TyrianTime.Dawn:
                case TyrianTime.Day:
                    return TyrianTime.Day;
                case TyrianTime.Dusk:
                case TyrianTime.Night:
                    return TyrianTime.Night;
                default:
                    return TyrianTime.None;
            }
        }

        /// <summary>
        /// Compares the context of two day cycles.
        /// </summary>
        /// <param name="source">The source day cycle.</param>
        /// <param name="other">The other day cycle to compare against.</param>
        /// <returns><see langword="True"/> if both day cycles belong to the same full day cycle. Otherwise <see langword="false"/>.</returns>
        public static bool ContextEquals(this TyrianTime source, TyrianTime other) {
            return source.Resolve() == other.Resolve();
        }
    }
    internal static class TyrianTimeUtil
    {
        private static IReadOnlyDictionary<TyrianTime, (TimeSpan,TimeSpan)> _dayCycleIntervals = new Dictionary<TyrianTime, (TimeSpan, TimeSpan)>() {
            { TyrianTime.Dawn, (new TimeSpan(05,0,0), new TimeSpan(06,0,0)) },
            { TyrianTime.Day, (new TimeSpan(06,0,0), new TimeSpan(20,0,0)) },
            { TyrianTime.Dusk, (new TimeSpan(20,0,0), new TimeSpan(21,0,0)) },
            { TyrianTime.Night, (new TimeSpan(21,0,0), new TimeSpan(05,0,0)) }
        };

        /// <summary>
        /// Checks which Tyrian day cycle currently prevails.
        /// </summary>
        /// <returns>The current Tyrian day cycle.</returns>
        internal static TyrianTime GetCurrentDayCycle() {
            return GetDayCycle(GetCurrentTyrianTime());
        }

        /// <summary>
        /// Converts the current real time to Tyrian time.
        /// </summary>
        /// <returns>A TimeSpan representing the current Tyrian time.</returns>
        public static TimeSpan GetCurrentTyrianTime() {
            return FromRealDateTime(DateTime.UtcNow);
        }

        /// <summary>
        /// Checks which Tyrian day cycle prevails in the given Tyrian time.
        /// </summary>
        /// <param name="tyrianTime">The Tyrian time to get the dominant day cycle of.</param>
        /// <returns>The day cycle.</returns>
        public static TyrianTime GetDayCycle(TimeSpan tyrianTime) {
            foreach (var timePair in _dayCycleIntervals) {
                var key = timePair.Key;
                var value = timePair.Value;

                if (TimeBetween(tyrianTime, value.Item1, value.Item2))
                    return key;
            }
            return TyrianTime.None;
        }

        /// <summary>
        /// Converts a DateTime object to a TimeSpan representing Tyrian time.
        /// </summary>
        /// <param name="realTime">A DateTime object representing real time.</param>
        /// <returns>A TimeSpan representing the Tyrian Time conversion of the input time.</returns>
        public static TimeSpan FromRealDateTime(DateTime realTime)
        {
            TimeSpan currentDayTimespan;
            double currentCycleSeconds;

            // Retrieves a timespan that represents the time from 00:00 of the given realTime to the current time
            // of the given realTime
            currentDayTimespan = realTime - realTime.Date;

            /*
             * A single Tyrian day consists of 7200 real-time seconds, so we divide the current real-time seconds of
             * the day by 7200 and then return the remainder - this gives us the current total seconds that have passed
             * in Tyria for the current Tyrian day/cycle.
             */
            currentCycleSeconds = currentDayTimespan.TotalSeconds % 7200;

            /*
             * For every second that passes in real time, 12 pass in Tyrian time, so we simply need to multiply
             * the current cycle seconds by 12 to convert them from a 2 hour cycle to
             */
            return TimeSpan.FromSeconds(currentCycleSeconds * 12);
        }


        /// <summary>
        /// Checks if the given time is between the given start and end time.
        /// </summary>
        /// <param name="time">The time to check.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns><see langword="True"/> if time is between the given interval otherwise <see langword="false"/>.</returns>
        public static bool TimeBetween(TimeSpan time, TimeSpan start, TimeSpan end)
        {
            if (start < end)
                return start <= time && time <= end;
            return !(end < time && time < start);
        }
    }
}