﻿using SecretHistories.Fucine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Entities
{
    public delegate string FnCanonicalize(string rawId);

    /// <summary>
    /// This interface implies that any
    /// class which implements it submits
    /// all its internal fields for ID
    /// canonicalization, which involves
    /// prefixing each ID with the enclosing
    /// mod's namespace and canonicalizing
    /// absolute IDs.
    /// </summary>
    public interface INamespacedIDEntity : IEntityWithId
    {
        /// <summary>
        /// Doorways will try to call this method
        /// on any IEntityWithId instances that
        /// implement it. 
        /// <para/>
        /// Implementations of this method should
        /// replace any IDs and references to IDs
        /// with the result of `fnCanonicalize` for
        /// that ID.
        /// <para>
        /// <b>This method may be called more than once!</b>
        /// It is up to the implementor to ensure
        /// that IDs remain properly canonicalized even across
        /// multiple calls. For the majority of cases,
        /// a simple fnOnce flag which fast-exits
        /// if the method has already been run once
        /// will suffice.
        /// <para/>
        /// All Doorways base entity
        /// classes have their own implementations
        /// to make things easy for mod developers.
        /// If you're trying to register a class
        /// that doesn't derive from a Doorways
        /// base entity class, it's on you to ensure
        /// that all your custom type's internal
        /// ID fields are properly canonicalized
        /// in to this method.
        /// </summary>
        void CanonicalizeIds(FnCanonicalize fnCanonicalize, string prefix);
    }
}
