using HarvestSystems.Domain.Time;
using NUnit.Framework;

namespace HarvestSystems.Tests.EditMode
{
    public sealed class GameClockTests
    {
        [Test]
        public void AdvanceDay_IncrementsDayBeforePublishingEvent()
        {
            var clock = new GameClock();
            GameDate publishedDay = default;
            clock.DayAdvanced += day => publishedDay = day;

            clock.AdvanceDay();

            Assert.That(clock.CurrentDay, Is.EqualTo(2));
            Assert.That(clock.Time, Is.EqualTo(new GameTime(6, 0)));
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
            Assert.That(clock.Time, Is.EqualTo(new GameTime(0, 0)));
        }

        [Test]
        public void Constructor_RejectsNonPositiveStartingDay()
        {
            Assert.That(() => new GameClock(0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void AdvanceMinutes_RejectsNonPositiveDuration()
        {
            var clock = new GameClock();

            Assert.That(() => clock.AdvanceMinutes(0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
