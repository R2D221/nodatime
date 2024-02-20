// Copyright 2024 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using JetBrains.Annotations;
using NodaTime.Annotations;
using NodaTime.Calendars;
using NodaTime.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NodaTime
{
    /// <summary>
    /// Represents a 7-day period, with the start and end dates defined by
    /// the given <see cref="IWeekYearRule"/>.
    /// </summary>
    /// <threadsafety>This type is an immutable value type. See the thread safety section of the user guide for more information.</threadsafety>
    public readonly struct Week : IEquatable<Week>, IComparable<Week>, IComparable
    {
        private readonly int weekYear;
        private readonly int weekOfWeekYear;
        private readonly IWeekYearRule weekYearRule;
        private readonly CalendarOrdinal calendarOrdinal;

        /// <summary>
        /// Constructs an instance for the given week-year and week of week-year,
        /// using the specified week-year rule and calendar.
        /// </summary>
        /// <param name="weekYear">The week-year. This is different than the year, and specific to the week-year rule.</param>
        /// <param name="weekOfWeekYear">The week of the week-year.</param>
        /// <param name="weekYearRule">The rule determining how week-years are calculated.</param>
        /// <param name="calendar">Calendar system in which to create the week.</param>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid week.</exception>
        public Week(int weekYear, int weekOfWeekYear, IWeekYearRule weekYearRule, CalendarSystem calendar)
        {
            Preconditions.CheckNotNull(weekYearRule, nameof(weekYearRule));
            Preconditions.CheckNotNull(calendar, nameof(calendar));

            var maxWeeks = weekYearRule.GetWeeksInWeekYear(weekYear, calendar);

            Preconditions.CheckArgumentRange(nameof(weekOfWeekYear), weekOfWeekYear, 1, maxWeeks);

            this.weekYear = weekYear;
            this.weekOfWeekYear = weekOfWeekYear;
            this.weekYearRule = weekYearRule;
            this.calendarOrdinal = calendar.Ordinal;
        }

        /// <summary>
        /// Constructs an instance for the given week-year and week of week-year,
        /// using the specified week-year rule in the ISO calendar.
        /// </summary>
        /// <param name="weekYear"></param>
        /// <param name="weekOfWeekYear"></param>
        /// <param name="weekYearRule"></param>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid week.</exception>
        public Week(int weekYear, int weekOfWeekYear, IWeekYearRule weekYearRule)
            : this(weekYear, weekOfWeekYear, weekYearRule, CalendarSystem.Iso) { }

        /// <summary>
        /// Constructs an instance for the given week-year and week of week-year,
        /// using the ISO week-year rule and the ISO calendar.
        /// </summary>
        /// <param name="weekYear"></param>
        /// <param name="weekOfWeekYear"></param>
        /// <exception cref="ArgumentOutOfRangeException">The parameters do not form a valid week.</exception>
        public Week(int weekYear, int weekOfWeekYear)
            : this(weekYear, weekOfWeekYear, WeekYearRules.Iso, CalendarSystem.Iso) { }

        /// <summary>
        /// Gets the week-year of this value.
        /// </summary>
        /// <value>The week-year of this value.</value>
        public int WeekYear => weekYear;

        /// <summary>
        /// Gets the week of the week-year of this value.
        /// </summary>
        /// <value>The week of the week-year of this value.</value>
        public int WeekOfWeekYear => weekOfWeekYear;

        /// <summary>
        /// Gets the week-year rule associated with this week.
        /// </summary>
        /// <value>The week-year rule associated with this week.</value>
        public IWeekYearRule WeekYearRule => weekYearRule;

        /// <summary>Gets the calendar system associated with this week.</summary>
        /// <value>The calendar system associated with this week.</value>
        public CalendarSystem Calendar => CalendarSystem.ForOrdinal(calendarOrdinal);

        /// <summary>
        /// Returns a <see cref="LocalDate"/> with the week-year and week of week-year
        /// of this value, and the given day of week, according to this value's
        /// week-year rule and calendar system.
        /// </summary>
        /// <param name="dayOfWeek">The day-of-week of the new date. Valid values for this parameter may vary
        /// depending on this value's week-year and week of week-year.</param>
        /// <returns>
        /// A <see cref="LocalDate"/> corresponding to the specified values.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The <paramref name="dayOfWeek"/> is invalid for this week's rule.
        /// </exception>
        [Pure]
        public LocalDate OnDayOfWeek(IsoDayOfWeek dayOfWeek)
        {
            return weekYearRule.GetLocalDate(weekYear, weekOfWeekYear, dayOfWeek, Calendar);
        }

        /// <summary>
        /// Returns a <see cref="DateInterval"/> covering the week represented by this value.
        /// </summary>
        /// <returns>A <see cref="DateInterval"/> covering the week represented by this value.</returns>
        [Pure]
        public DateInterval ToDateInterval()
        {
            // This implementation is ugly. We should find a better way to calculate this.

            var @this = this;

            var validDates =
                Enum.GetValues(typeof(IsoDayOfWeek))
                .Cast<IsoDayOfWeek>()
                .Except([IsoDayOfWeek.None])
                .Select(dayOfWeek =>
                {
                    try { return @this.OnDayOfWeek(dayOfWeek); }
                    catch (ArgumentOutOfRangeException) { return (LocalDate?)null; }
                })
                .OfType<LocalDate>()
                .OrderBy(x => x)
                .ToArray()
                ;

            return new DateInterval(validDates[0], validDates[validDates.Length - 1]);
        }

        /// <summary>
        /// Returns a hash code for this week.
        /// See the type documentation for a description of equality semantics.
        /// </summary>
        /// <returns>A hash code for this value.</returns>
        public override int GetHashCode()
            => HashCodeHelper.Initialize()
            .Hash(weekYear)
            .Hash(weekOfWeekYear)
            .Hash(weekYearRule)
            .Hash(calendarOrdinal)
            .Value;

        /// <summary>
        /// Compares two <see cref="Week" /> values for equality.
        /// See the type documentation for a description of equality semantics.
        /// </summary>
        /// <param name="lhs">The first value to compare</param>
        /// <param name="rhs">The second value to compare</param>
        /// <returns>True if the two week values are the same, with the same week-year rule, and in the same calendar; false otherwise</returns>
        public static bool operator ==(Week lhs, Week rhs)
        {
            return
                lhs.weekYear == rhs.weekYear
                && lhs.weekOfWeekYear == rhs.weekOfWeekYear
                && lhs.weekYearRule.Equals(rhs.weekYearRule)
                && lhs.calendarOrdinal == rhs.calendarOrdinal;
        }

        /// <summary>
        /// Compares two <see cref="Week" /> values for inequality.
        /// See the type documentation for a description of equality semantics.
        /// </summary>
        /// <param name="lhs">The first value to compare</param>
        /// <param name="rhs">The second value to compare</param>
        /// <returns>False if the two week values are the same, with the same week-year rule, and in the same calendar; true otherwise</returns>
        public static bool operator !=(Week lhs, Week rhs) => !(lhs == rhs);

        /// <summary>
        /// Compares two <see cref="Week" /> values for equality.
        /// See the type documentation for a description of equality semantics.
        /// </summary>
        /// <param name="other">The value to compare this week with.</param>
        /// <returns>True if the given value is another week equal to this one; false otherwise.</returns>
        public bool Equals(Week other) => this == other;

        /// <summary>
        /// Compares two <see cref="Week" /> values for equality.
        /// See the type documentation for a description of equality semantics.
        /// </summary>
        /// <param name="obj">The object to compare this week with.</param>
        /// <returns>True if the given value is another week equal to this one; false otherwise.</returns>
        public override bool Equals(object? obj) => obj is Week other && this == other;

        /// <summary>
        /// Indicates whether this week is earlier, later or the same as another one.
        /// See the type documentation for a description of ordering semantics.
        /// </summary>
        /// <param name="other">The other week to compare this one with</param>
        /// <exception cref="ArgumentException">This value and <paramref name="other"/> don't have the
        /// same calendar system and week-year rule.</exception>
        /// <returns>A value less than zero if this week is earlier than <paramref name="other"/>;
        /// zero if this week is the same as <paramref name="other"/>; a value greater than zero if this date is
        /// later than <paramref name="other"/>.</returns>
        public int CompareTo(Week other)
        {
            Preconditions.CheckArgument(weekYearRule.Equals(other.weekYearRule), nameof(other), "Only values with the same week-year rule can be compared");
            Preconditions.CheckArgument(calendarOrdinal == other.calendarOrdinal, nameof(other), "Only values with the same calendar system can be compared");
            return TrustedCompareTo(other);
        }

        /// <summary>
        /// Performs a comparison with another week, trusting that the calendar
        /// and week-year rule of the other week is already correct.
        /// This avoids duplicate checks.
        /// </summary>
        private int TrustedCompareTo([Trusted] Week other) => (weekYear, weekOfWeekYear).CompareTo((other.weekYear, other.weekOfWeekYear));

        /// <summary>
        /// Implementation of <see cref="IComparable.CompareTo"/> to compare two Week values.
        /// See the type documentation for a description of ordering semantics.
        /// </summary>
        /// <remarks>
        /// This uses explicit interface implementation to avoid it being called accidentally. The generic implementation should usually be preferred.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// <paramref name="obj"/> is non-null but does not refer to an instance of <see cref="Week"/>, or refers
        /// to a value in a different calendar system or with a different week-year rule.
        /// </exception>
        /// <param name="obj">The object to compare this value with.</param>
        /// <returns>The result of comparing this Week with another one.
        /// If <paramref name="obj"/> is null, this method returns a value greater than 0.
        /// </returns>
        int IComparable.CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }
            Preconditions.CheckArgument(obj is Week, nameof(obj), "Object must be of type NodaTime.Week.");
            return CompareTo((Week)obj);
        }

        /// <summary>
        /// Compares two Week values to see if the left one is strictly earlier than the right one.
        /// See the type documentation for a description of ordering semantics.
        /// </summary>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <exception cref="ArgumentException"><paramref name="lhs"/> and <paramref name="rhs"/> don't have the
        /// same calendar system and week-year rule.</exception>
        /// <returns>true if the <paramref name="lhs"/> is strictly earlier than <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator <(Week lhs, Week rhs)
        {
            Preconditions.CheckArgument(lhs.weekYearRule.Equals(rhs.weekYearRule), nameof(rhs), "Only values with the same week-year rule can be compared");
            Preconditions.CheckArgument(lhs.calendarOrdinal == rhs.calendarOrdinal, nameof(rhs), "Only values with the same calendar system can be compared");
            return lhs.TrustedCompareTo(rhs) < 0;
        }

        /// <summary>
        /// Compares two Week values to see if the left one is earlier than or equal to the right one.
        /// See the type documentation for a description of ordering semantics.
        /// </summary>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <exception cref="ArgumentException"><paramref name="lhs"/> and <paramref name="rhs"/> don't have the
        /// same calendar system and week-year rule.</exception>
        /// <returns>true if the <paramref name="lhs"/> is earlier than or equal to <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator <=(Week lhs, Week rhs)
        {
            Preconditions.CheckArgument(lhs.weekYearRule.Equals(rhs.weekYearRule), nameof(rhs), "Only values with the same week-year rule can be compared");
            Preconditions.CheckArgument(lhs.calendarOrdinal == rhs.calendarOrdinal, nameof(rhs), "Only values with the same calendar system can be compared");
            return lhs.TrustedCompareTo(rhs) <= 0;
        }

        /// <summary>
        /// Compares two Week values to see if the left one is strictly later than the right one.
        /// See the type documentation for a description of ordering semantics.
        /// </summary>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <exception cref="ArgumentException"><paramref name="lhs"/> and <paramref name="rhs"/> don't have the
        /// same calendar system and week-year rule.</exception>
        /// <returns>true if the <paramref name="lhs"/> is strictly later than <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator >(Week lhs, Week rhs)
        {
            Preconditions.CheckArgument(lhs.weekYearRule.Equals(rhs.weekYearRule), nameof(rhs), "Only values with the same week-year rule can be compared");
            Preconditions.CheckArgument(lhs.calendarOrdinal == rhs.calendarOrdinal, nameof(rhs), "Only values with the same calendar system can be compared");
            return lhs.TrustedCompareTo(rhs) > 0;
        }

        /// <summary>
        /// Compares two Week values to see if the left one is later than or equal to the right one.
        /// See the type documentation for a description of ordering semantics.
        /// </summary>
        /// <param name="lhs">First operand of the comparison</param>
        /// <param name="rhs">Second operand of the comparison</param>
        /// <exception cref="ArgumentException"><paramref name="lhs"/> and <paramref name="rhs"/> don't have the
        /// same calendar system and week-year rule.</exception>
        /// <returns>true if the <paramref name="lhs"/> is later than or equal to <paramref name="rhs"/>, false otherwise.</returns>
        public static bool operator >=(Week lhs, Week rhs)
        {
            Preconditions.CheckArgument(lhs.weekYearRule.Equals(rhs.weekYearRule), nameof(rhs), "Only values with the same week-year rule can be compared");
            Preconditions.CheckArgument(lhs.calendarOrdinal == rhs.calendarOrdinal, nameof(rhs), "Only values with the same calendar system can be compared");
            return lhs.TrustedCompareTo(rhs) >= 0;
        }
    }
}
