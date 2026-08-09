using NUnit.Framework;
using PunishingTower.Data;
using UnityEngine;

namespace PunishingTower.Tests
{
    public class DataObjectTests
    {
        [Test]
        public void OrbData_DefaultColorIsRed()
        {
            var orb = ScriptableObject.CreateInstance<OrbData>();

            Assert.AreEqual(OrbColor.Red, orb.Color);
            Object.DestroyImmediate(orb);
        }

        [Test]
        public void OrbData_AssignIdentity_StoresFields()
        {
            var orb = ScriptableObject.CreateInstance<OrbData>();
#if UNITY_EDITOR
            orb.AssignIdentity("orb_red", "Red Orb");
#endif
            Assert.AreEqual("orb_red", orb.Id);
            Assert.AreEqual("Red Orb", orb.DisplayName);
            Object.DestroyImmediate(orb);
        }
    }
}
