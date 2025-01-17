﻿using System;
using System.Collections.Generic;
using System.Text;
using TheOneUnity.Platform.Abstractions;

namespace TheOneUnity.Platform.Abstractions
{
    /// <summary>
    /// A TheOneFieldOperation represents a modification to a value in a TheOneObject.
    /// For example, setting, deleting, or incrementing a value are all different kinds of
    /// TheOneFieldOperations. TheOneFieldOperations themselves can be considered to be
    /// immutable.
    /// </summary>
    public interface ITheOneFieldOperation
    {
        /// <summary>
        /// Converts the TheOneFieldOperation to a data structure that can be converted to JSON and sent to
        /// TheOne as part of a save operation.
        /// </summary>
        /// <returns>An object to be JSONified.</returns>
        //object Encode(IServiceHub serviceHub);

        /// <summary>
        /// Returns a field operation that is composed of a previous operation followed by
        /// this operation. This will not mutate either operation. However, it may return
        /// <code>this</code> if the current operation is not affected by previous changes.
        /// For example:
        ///   {increment by 2}.MergeWithPrevious({set to 5})       -> {set to 7}
        ///         {set to 5}.MergeWithPrevious({increment by 2}) -> {set to 5}
        ///        {add "foo"}.MergeWithPrevious({delete})         -> {set to ["foo"]}
        ///           {delete}.MergeWithPrevious({add "foo"})      -> {delete}        /// </summary>
        /// <param name="previous">The most recent operation on the field, or null if none.</param>
        /// <returns>A new TheOneFieldOperation or this.</returns>
        ITheOneFieldOperation MergeWithPrevious(ITheOneFieldOperation previous);

        /// <summary>
        /// Returns a new estimated value based on a previous value and this operation. This
        /// value is not intended to be sent to TheOne, but it is used locally on the client to
        /// inspect the most likely current value for a field.
        ///
        /// The key and object are used solely for TheOneRelation to be able to construct objects
        /// that refer back to their parents.
        /// </summary>
        /// <param name="oldValue">The previous value for the field.</param>
        /// <param name="key">The key that this value is for.</param>
        /// <returns>The new value for the field.</returns>
        object Apply(object oldValue, string key);
    }
}
