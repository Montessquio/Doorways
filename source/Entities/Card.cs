﻿using Doorways.Entities.Mixins;
using HarmonyLib;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Fucine.DataImport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniverseLib;
using static UnityEngine.EventSystems.EventTrigger;

namespace Doorways.Entities
{
    public abstract class Card : Element, INamespacedIDEntity, IInheritOverride<Element>
    {
        // This exact field is inherited from AbstractEntity<>
        //public virtual string Id { get; protected set; } = null;

        public abstract new string Label { get; set; }
        public abstract new string Description { get; set; }

        private string _icon;
        public virtual new string Icon
        {
            get
            {
                if (string.IsNullOrEmpty(_icon))
                {
                    return Id;
                }

                return _icon;
            }
            set
            {
                _icon = value;
            }
        }
        public virtual new string VerbIcon { get; set; } = "";
        public virtual new string DecayTo { get; set; } = "";
        public virtual new string BurnTo { get; set; } = "";
        public virtual new string DrownTo { get; set; } = "";
        public virtual new string UniquenessGroup { get; set; } = "";
        public virtual new bool Resaturate { get; set; } = false;
        public virtual new bool IsHidden { get; set; } = false;
        public virtual new bool NoArtNeeded { get; set; } = false;
        public virtual new bool Metafictional { get; set; } = false;
        public virtual new string ManifestationType { get; private set; } = "Card";
        public virtual new List<string> Achievements { get; set; } = new List<string>();
        public virtual new bool Unique { get; set; } = false;
        public virtual new float Lifetime { get; set; } = 0;
        public virtual new string Inherits { get; set; } = "";
        public virtual new AspectsDictionary Aspects { get; set; } = new AspectsDictionary();
        public virtual new List<Slot> Slots { get; set; } = new List<Slot>();
        public virtual new List<RecipeLink> Induces { get; set; } = new List<RecipeLink>();
        public virtual new Dictionary<string, List<XTrigger>> XTriggers { get; set; } = new Dictionary<string, List<XTrigger>>();
        public new AspectsDictionary AspectsIncludingSelf
        {
            get
            {
                AspectsDictionary aspectsDictionary = new AspectsDictionary();
                foreach (string key in Aspects.Keys)
                {
                    aspectsDictionary.Add(key, Aspects[key]);
                }

                if (!aspectsDictionary.ContainsKey(Id))
                {
                    aspectsDictionary.Add(Id, 1);
                }

                return aspectsDictionary;
            }
        }
        public new bool Decays => Lifetime > 0f;
        public new bool IsAspect { get; } = false;

        public Card(EntityData importDataForEntity, ContentImportLog log) : base(importDataForEntity, log) { }

        public Card(ContentImportLog log) : base(new EntityData(), log) { }

        public Card() : base(new EntityData(), new ContentImportLog()) { }

        public virtual void CanonicalizeIds(FnCanonicalize Canonicalize, string prefix)
        {
            if (Id == null || Id == "id")
            {
                Id = this.GetActualType().Name;
            }

            Id = Canonicalize(Id);
            DecayTo = Canonicalize(DecayTo);
            BurnTo = Canonicalize(BurnTo);
            DrownTo = Canonicalize(DrownTo);
            UniquenessGroup = Canonicalize(UniquenessGroup);
            Achievements = Achievements.ConvertAll(id => Canonicalize(id));
            {
                var a = new AspectsDictionary();
                foreach (var kv in Aspects)
                {
                    a.Add(Canonicalize(kv.Key), kv.Value);
                }
                Aspects = a;
            }
            foreach(var s in Slots)
            {
                s.CanonicalizeIds(Canonicalize, prefix);
            }

            foreach(var l in Induces)
            {
                l.CanonicalizeIds(Canonicalize, prefix);
            }
            {
                var x = new Dictionary<string, List<XTrigger>>();
                foreach (var kv in XTriggers)
                {
                    kv.Value.ForEach((t) => t.CanonicalizeIds(Canonicalize, prefix));
                    x.Add(Canonicalize(kv.Key), kv.Value);
                }
                XTriggers = x;
            }
        }
    }
}
