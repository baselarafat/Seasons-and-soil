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
            int publishedDay = 0;
            clock.DayAdvanced += day => publishedDay = day;

            clock.AdvanceDay();

            Assert.That(clock.CurrentDay, Is.EqualTo(2));
            Assert.That(publishedDay, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_RejectsNonPositiveStartingDay()
        {
            Assert.That(() => new GameClock(0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
