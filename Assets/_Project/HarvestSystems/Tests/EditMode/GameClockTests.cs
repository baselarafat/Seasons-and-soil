using HarvestSystems.Domain.Time;
using NUnit.Framework;

namespace HarvestSystems.Tests.EditMode
{
    public sealed class GameClockTests
    {
        [TestCase(1, 1, Season.Spring, 1)]
        [TestCase(7, 1, Season.Spring, 7)]
        [TestCase(8, 1, Season.Summer, 1)]
        [TestCase(15, 1, Season.Autumn, 1)]
        [TestCase(22, 1, Season.Winter, 1)]
        [TestCase(29, 2, Season.Spring, 1)]
        public void CalendarDate_ResolvesSeasonBoundaries(
            int absoluteDay,
            int expectedYear,
            Season expectedSeason,
            int expectedDayOfSeason)
        {
            var clock = new GameClock(startingDay: absoluteDay);

            Assert.That(
                clock.CalendarDate,
                Is.EqualTo(new SeasonDate(expectedYear, expectedSeason, expectedDayOfSeason)));
        }

        [Test]
        public void AdvanceDay_IncrementsDayBeforePublishingEvent()
        {
            var clock = new GameClock();
            GameDate publishedDay = default;
            clock.DayAdvanced += day => publishedDay = day;

            clock.AdvanceDay();

            Assert.That(clock.CurrentDay, Is.EqualTo(2));
            Assert.That(clock.Time, Is.EqualTo(new GameTime(6, 0)));
            Assert.That(clock.CalendarDate, Is.EqualTo(new SeasonDate(1, Season.Spring, 2)));
            Assert.That(publishedDay, Is.EqualTo(new GameDate(1, 2)));
        }

        [Test]
        public void AdvanceMinutes_WithinDay_ChangesTimeWithoutChangingDate()
        {
            var clock = new GameClock();
            int dayEvents = 0;
            clock.DayAdvanced += _ => dayEvents++;

            clock.AdvanceMinutes(90);

            Assert.That(clock.Date, Is.EqualTo(new GameDate(1, 1)));
            Assert.That(clock.Time, Is.EqualTo(new GameTime(7, 30)));
            Assert.That(dayEvents, Is.Zero);
        }

        [Test]
        public void AdvanceMinutes_AcrossMultipleDays_PublishesEveryDayInOrder()
        {
            var clock = new GameClock(startingMinuteOfDay: 23 * 60, daysPerYear: 4);
            var publishedDates = new System.Collections.Generic.List<GameDate>();
            clock.DayAdvanced += publishedDates.Add;

            clock.AdvanceMinutes((2 * GameClock.MinutesPerDay) + 120);

            Assert.That(publishedDates, Is.EqualTo(new[]
            {
                new GameDate(1, 2),
                new GameDate(1, 3),
                new GameDate(1, 4)
            }));
            Assert.That(clock.Time, Is.EqualTo(new GameTime(1, 0)));
        }

        [Test]
        public void AdvanceMinutes_RollsDateIntoNextYear()
        {
            var clock = new GameClock(startingDay: 4, startingMinuteOfDay: 23 * 60, daysPerYear: 4);

            clock.AdvanceMinutes(60);

            Assert.That(clock.Date, Is.EqualTo(new GameDate(2, 1)));
            Assert.That(clock.CalendarDate, Is.EqualTo(new SeasonDate(2, Season.Spring, 1)));
            Assert.That(clock.Time, Is.EqualTo(new GameTime(0, 0)));
        }

        [Test]
        public void AdvanceDay_RollsIntoNextSeason()
        {
            var clock = new GameClock(startingDay: 7);

            clock.AdvanceDay();

            Assert.That(clock.CalendarDate, Is.EqualTo(new SeasonDate(1, Season.Summer, 1)));
        }

        [Test]
        public void Constructor_RejectsNonPositiveStartingDay()
        {
            Assert.That(() => new GameClock(0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void Constructor_RejectsYearThatCannotBeSplitIntoFourEqualSeasons()
        {
            Assert.That(
                () => new GameClock(daysPerYear: 30),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void AdvanceMinutes_RejectsNonPositiveDuration()
        {
            var clock = new GameClock();

            Assert.That(() => clock.AdvanceMinutes(0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
