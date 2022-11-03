using Newtonsoft.Json.Linq;
using SecretHistories.Commands;
using SecretHistories.Constants;
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
using UnityEngine;
using Logger = sh.monty.doorways.logging.Logger;

namespace sh.monty.doorways.CoreExtensions.Tables
{
    /// <summary>
    /// A type of TabletopSphere which can be constructed
    /// at runtime instead of from unity editor injections.
    /// <para />
    /// This class indirectly inherits MonoBehaviour.
    /// <para />
    /// When this script Awake()ns, it will create its own
    /// choreographer and define the GameObject it is
    /// attached to as its own token container.
    /// </summary>
    [IsEmulousEncaustable(typeof(Sphere))]
    internal class AuxTabletopSphere : TabletopSphere
    {
        private float tokenHeartbeatIntervalMultiplier = 0f;

        [DontEncaust]
        public override float TokenHeartbeatIntervalMultiplier
        {
            get
            {
                return tokenHeartbeatIntervalMultiplier;
            }
        }

        /// <summary>
        /// Allows the user to change how fast time passes for the tokens in this sphere.
        /// Set it to <c>0f</c> to freeze time for this sphere.
        /// </summary>
        /// <param name="value"></param>
        public void SetTokenHeartbeatIntervalMultiplier(float value)
        {
            tokenHeartbeatIntervalMultiplier = value;
        }

        public void SetTabletopBackground(TabletopBackground value)
        {
            typeof(TabletopSphere).GetField("_background", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, value);
        }

        public void SetCanvasGroupFader(CanvasGroupFader value)
        {
            base.canvasGroupFader = value;
        }

        public void SetTabletopChoreographer(TabletopChoreographer value)
        {
            typeof(TabletopSphere).GetField("_tabletopChoreographer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, value);
        }

        public void SetEnRouteSphere(EnRouteSphere value)
        {
            base.SendViaContainer = value;
        }
    }
}
