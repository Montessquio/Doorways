using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sh.monty.doorways.CoreExtensions.Tables
{
    /// <summary>
    /// A PermanentRootSphereSpec which never starts sealed.
    /// This allows the Doorways runtime to call ApplySpecToSphere()
    /// later, when a new Table is created.
    /// </summary>
    internal class AuxPermanentRootSphereSpec : PermanentRootSphereSpec
    {
        public string Id = null;

        public new void Awake()
        {
            typeof(PermanentRootSphereSpec)
                .GetField("_startSealed", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this, false);
            base.Awake();
        }

        public override void ApplySpecToSphere(Sphere applyToSphere) 
        {
            if(Id == null)
            {
                Id = GetId();
            }
            _sphereSpec = new SphereSpec(applyToSphere.GetType(), Id);
            _sphereSpec.EnRouteSpherePath = new FucinePath(EnRouteSpherePath);
            _sphereSpec.WindowsSpherePath = new FucinePath(WindowsSpherePath);
            applyToSphere.SetPropertiesFromSpec(_sphereSpec);
            FucineRoot.Get().AttachSphere(applyToSphere);
            Watchman.Get<HornedAxe>().RegisterSphere(applyToSphere);
            InitialiseChildTerrainFeatures(applyToSphere);
        }
    }
}
