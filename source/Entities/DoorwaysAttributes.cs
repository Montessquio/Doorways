﻿using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorwaysFramework.Entities
{

    /// <summary>
    /// Signals to Doorways that this class can be loaded
    /// as a Fucine Importable. It must be a child class of
    /// an existing Fucine class or a Doorways entity class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DoorwaysObjectAttribute : Attribute { }

    /// <summary>
    /// A shorthand signal to Doorways that this enum
    /// can be used as a Fucine DeckSpec.
    /// Optionally, a default item may be specified.
    /// If no default item is specified, the
    /// deck will loop by default.
    /// <para />
    /// In order to specify deck text (for portal deckspecs)
    /// add the <see cref="DrawTextAttribute"/> attribute
    /// to each enum item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class DeckAttribute : Attribute 
    {
        public DeckAttribute() { }
        public DeckAttribute(string defaultKey)
        {

        }
    }

    /// <summary>
    /// Add this to a <see cref="DeckAttribute"/>
    /// enum in order to specify the text that
    /// will be shown then that card is pulled
    /// when the deck is drawn from as a portal
    /// deck.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class DrawTextAttribute : Attribute
    {
        string text;
        public DrawTextAttribute(string text)
        {
            this.text = text;
        }
    }

    /// <summary>
    /// Classes that derive from DoorwaysFactory
    /// can procedurally generate as many entities
    /// as they like at load time.
    /// </summary>
    public abstract class DoorwaysFactory
    {
        /// <summary>
        /// Retrieve all the entities generated by this factory.
        /// </summary>
        public abstract IEnumerable<IEntityWithId> GetAll();
    }

    /// <summary>
    /// Add this to any property in your
    /// <see cref="DoorwaysObjectAttribute"/>
    /// class. Doorways will then cause the
    /// property's getter and/or setter to be
    /// run every time they would be invoked
    /// by an instance on the table, instead
    /// of once at run-time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DynamicAttribute : Attribute
    {

    }
}