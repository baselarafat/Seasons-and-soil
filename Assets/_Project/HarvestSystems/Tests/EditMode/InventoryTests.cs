using HarvestSystems.Domain.Common;
using HarvestSystems.Domain.Inventory;
using NUnit.Framework;

namespace HarvestSystems.Tests.EditMode
{
    public sealed class InventoryTests
    {
        private static readonly StableId Carrot = new StableId("item.carrot");

        [Test]
        public void AddAndRemove_UpdateQuantityAndPublishDeltas()
        {
            var inventory = new Inventory();
            int observedDelta = 0;
            inventory.Changed += change => observedDelta += change.Delta;

            inventory.Add(Carrot, 3);
            bool removed = inventory.TryRemove(Carrot, 2);

            Assert.That(removed, Is.True);
            Assert.That(inventory.GetQuantity(Carrot), Is.EqualTo(1));
            Assert.That(observedDelta, Is.EqualTo(1));
        }

        [Test]
        public void TryRemove_WhenStockIsInsufficient_DoesNotMutateInventory()
        {
            var inventory = new Inventory();
            inventory.Add(Carrot, 1);

            bool removed = inventory.TryRemove(Carrot, 2);

            Assert.That(removed, Is.False);
            Assert.That(inventory.GetQuantity(Carrot), Is.EqualTo(1));
        }
    }
}
