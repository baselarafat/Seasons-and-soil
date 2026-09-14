using System.Collections;
using HarvestSystems.Domain.Common;
using HarvestSystems.Unity.Composition;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HarvestSystems.Tests.PlayMode
{
    public sealed class Phase1CompositionTests
    {
        [UnityTest]
        public IEnumerator Bootstrap_CreatesInitializedSimulationWithSixPlots()
        {
            var root = new GameObject("Test Bootstrap");
            Phase1DemoBootstrap bootstrap = root.AddComponent<Phase1DemoBootstrap>();

            yield return null;

            Assert.That(bootstrap.GameController, Is.Not.Null);
            Assert.That(bootstrap.GameController.Simulation, Is.Not.Null);
            Assert.That(bootstrap.GameController.Simulation.Clock.CurrentDay, Is.EqualTo(1));

            for (int i = 1; i <= 6; i++)
            {
                Assert.That(
                    bootstrap.GameController.Simulation.GetPlot(new StableId($"plot.demo_{i}")),
                    Is.Not.Null);
            }

            Object.Destroy(root);
        }
    }
}
