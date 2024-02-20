// Copyright 2024 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Calendars;
using NUnit.Framework;
using System;
using System.Globalization;

namespace NodaTime.Test
{
    // Most of the Week functionality should already be covered by
    // SimpleWeekYearRuleTest.cs. Here we should test that the
    // struct itself behaves correctly.
    public partial class WeekTest
    {
        [Test]
        [TestCase(2000, 1)]
        [TestCase(2000, 52)]
        [TestCase(-9998, 1)]
        [TestCase(9999, 52)]
        public void ValidConstruction(int weekYear, int weekOfWeekYear)
        {
            var week = new Week(weekYear, weekOfWeekYear);
            Assert.AreEqual(weekYear, week.WeekYear);
            Assert.AreEqual(weekOfWeekYear, week.WeekOfWeekYear);
            Assert.AreEqual(WeekYearRules.Iso, week.WeekYearRule);
            Assert.AreEqual(CalendarSystem.Iso, week.Calendar);
        }

        [Test]
        [TestCase(-9999, 1)]
        [TestCase(10000, 1)]
        [TestCase(2000, 0)]
        [TestCase(2000, 53)]
        public void InvalidConstruction(int weekYear, int weekOfWeekYear)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Week(weekYear, weekOfWeekYear));
        }

        [Test]
        public void OnDayOfWeek_Valid()
        {
            var rule = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var week = new Week(2014, 53, rule);

            var actualDate = week.OnDayOfWeek(IsoDayOfWeek.Wednesday);
            var expectedDate = new LocalDate(2014, 12, 31);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [Test]
        public void OnDayOfWeek_Invalid()
        {
            var rule = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var week = new Week(2014, 53, rule);

            Assert.Throws<ArgumentOutOfRangeException>(() => week.OnDayOfWeek(IsoDayOfWeek.Thursday));
        }

        [Test]
        [TestCase(2014, 52, true, 2014, 12, 22, 2014, 12, 28)]
        [TestCase(2015, 01, true, 2014, 12, 29, 2015, 01, 04)]
        [TestCase(2015, 02, true, 2015, 01, 05, 2015, 01, 11)]
        [TestCase(2014, 52, false, 2014, 12, 22, 2014, 12, 28)]
        [TestCase(2014, 53, false, 2014, 12, 29, 2014, 12, 31)]
        [TestCase(2015, 01, false, 2015, 01, 01, 2015, 01, 04)]
        [TestCase(2015, 02, false, 2015, 01, 05, 2015, 01, 11)]
        public void ToDateInterval(int weekYear, int weekOfWeekYear, bool iso, int startYear, int startMonth, int startDay, int endYear, int endMonth, int endDay)
        {
            var rule = iso ? WeekYearRules.Iso : WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var week = new Week(weekYear, weekOfWeekYear, rule);

            var interval = new DateInterval(new LocalDate(startYear, startMonth, startDay), new LocalDate(endYear, endMonth, endDay));

            Assert.AreEqual(interval, week.ToDateInterval());
        }

        [Test]
        public void Equals_EqualValues()
        {
            IWeekYearRule rule = WeekYearRules.Iso;
            CalendarSystem calendar = CalendarSystem.Julian;
            Week week1 = new Week(2011, 1, rule, calendar);
            Week week2 = new Week(2011, 1, rule, calendar);
            Assert.AreEqual(week1, week2);
            Assert.AreEqual(week1.GetHashCode(), week2.GetHashCode());
            Assert.IsTrue(week1 == week2);
            Assert.IsFalse(week1 != week2);
            Assert.IsTrue(week1.Equals(week2)); // IEquatable implementation
        }

        [Test]
        public void Equals_DifferentWeeks()
        {
            IWeekYearRule rule = WeekYearRules.Iso;
            CalendarSystem calendar = CalendarSystem.Julian;
            Week week1 = new Week(2011, 1, rule, calendar);
            Week week2 = new Week(2011, 2, rule, calendar);
            Assert.AreNotEqual(week1, week2);
            Assert.AreNotEqual(week1.GetHashCode(), week2.GetHashCode());
            Assert.IsFalse(week1 == week2);
            Assert.IsTrue(week1 != week2);
            Assert.IsFalse(week1.Equals(week2)); // IEquatable implementation
        }

        [Test]
        public void Equals_DifferentRules()
        {
            IWeekYearRule rule1 = WeekYearRules.Iso;
            IWeekYearRule rule2 = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            CalendarSystem calendar = CalendarSystem.Julian;
            Week week1 = new Week(2011, 1, rule1, calendar);
            Week week2 = new Week(2011, 1, rule2, calendar);
            Assert.AreNotEqual(week1, week2);
            Assert.AreNotEqual(week1.GetHashCode(), week2.GetHashCode());
            Assert.IsFalse(week1 == week2);
            Assert.IsTrue(week1 != week2);
            Assert.IsFalse(week1.Equals(week2)); // IEquatable implementation
        }

        [Test]
        public void Equals_DifferentCalendars()
        {
            IWeekYearRule rule = WeekYearRules.Iso;
            Week week1 = new Week(2011, 1, rule, CalendarSystem.Julian);
            Week week2 = new Week(2011, 1, rule, CalendarSystem.Iso);
            Assert.AreNotEqual(week1, week2);
            Assert.AreNotEqual(week1.GetHashCode(), week2.GetHashCode());
            Assert.IsFalse(week1 == week2);
            Assert.IsTrue(week1 != week2);
            Assert.IsFalse(week1.Equals(week2)); // IEquatable implementation
        }

        [Test]
        public void Equals_DifferentToNull()
        {
            Week week = new Week(2011, 1);
            Assert.IsFalse(week.Equals(null));
        }

        [Test]
        public void Equals_DifferentToOtherType()
        {
            Week week = new Week(2011, 1);
            Assert.IsFalse(week.Equals(Instant.FromUnixTimeTicks(0)));
        }

        [Test]
        public void CompareTo_SameRuleAndCalendar()
        {
            Week week1 = new Week(2011, 1);
            Week week2 = new Week(2011, 1);
            Week week3 = new Week(2011, 2);

            TestHelper.TestOperatorComparisonEquality(week1, week2, week3);
        }

        [Test]
        public void CompareTo_RulesWithSameParameters()
        {
            IWeekYearRule rule1 = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            IWeekYearRule rule2 = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            IWeekYearRule rule3 = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            // Rules are different references...

            Assert.AreNotSame(rule1, rule2);
            Assert.AreNotSame(rule1, rule3);
            Assert.AreNotSame(rule2, rule3);

            Week week1 = new Week(2011, 1, rule1);
            Week week2 = new Week(2011, 1, rule2);
            Week week3 = new Week(2011, 2, rule3);

            // ... but they are equal according to SimpleWeekYearRule.Equals

            TestHelper.TestOperatorComparisonEquality(week1, week2, week3);
        }

        [Test]
        public void CompareTo_DifferentRules_Throws()
        {
            IWeekYearRule rule1 = WeekYearRules.Iso;
            IWeekYearRule rule2 = WeekYearRules.FromCalendarWeekRule(CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            Week week1 = new Week(2011, 1, rule1);
            Week week2 = new Week(2011, 2, rule2);

            Assert.Throws<ArgumentException>(() => week1.CompareTo(week2));
            Assert.Throws<ArgumentException>(() => ((IComparable)week1).CompareTo(week2));
            Assert.Throws<ArgumentException>(() => (week1 > week2).ToString());
        }

        [Test]
        public void CompareTo_DifferentCalendars_Throws()
        {
            CalendarSystem islamic = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base15, IslamicEpoch.Astronomical);
            Week week1 = new Week(2011, 1, WeekYearRules.Iso);
            Week week2 = new Week(1500, 1, WeekYearRules.Iso, islamic);

            Assert.Throws<ArgumentException>(() => week1.CompareTo(week2));
            Assert.Throws<ArgumentException>(() => ((IComparable)week1).CompareTo(week2));
            Assert.Throws<ArgumentException>(() => (week1 > week2).ToString());
        }

        /// <summary>
        /// IComparable.CompareTo works properly with Week inputs with same calendar.
        /// </summary>
        [Test]
        public void IComparableCompareTo_SameCalendar()
        {
            var instance = new Week(2012, 3);
            var i_instance = (IComparable)instance;

            var later = new Week(2012, 6);
            var earlier = new Week(2012, 1);
            var same = new Week(2012, 3);

            Assert.That(i_instance.CompareTo(later), Is.LessThan(0));
            Assert.That(i_instance.CompareTo(earlier), Is.GreaterThan(0));
            Assert.That(i_instance.CompareTo(same), Is.EqualTo(0));
        }

        /// <summary>
        /// IComparable.CompareTo returns a positive number for a null input.
        /// </summary>
        [Test]
        public void IComparableCompareTo_Null_Positive()
        {
            var instance = new Week(2012, 3);
            var comparable = (IComparable)instance;
            var result = comparable.CompareTo(null!);
            Assert.Greater(result, 0);
        }

        /// <summary>
        /// IComparable.CompareTo throws an ArgumentException for non-null arguments 
        /// that are not a Week.
        /// </summary>
        [Test]
        public void IComparableCompareTo_WrongType_ArgumentException()
        {
            var instance = new Week(2012, 3);
            var i_instance = (IComparable)instance;
            var arg = new LocalDateTime(2012, 3, 6, 15, 42);
            Assert.Throws<ArgumentException>(() =>
            {
                i_instance.CompareTo(arg);
            });
        }
    }
}
