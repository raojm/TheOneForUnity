﻿using UnityEngine;
using System;

namespace TheOneUnity.Sdk.Exceptions
{
    /// <summary>
    /// Thrown when changing from/to unacceptable values.
    /// </summary>
    public class ObservableInvalidValueException : Exception
    {
        //  Properties ------------------------------------

        
        //  Fields ----------------------------------------

        
        //  Constructor Methods ---------------------------
        public ObservableInvalidValueException(object oldValue, object newValue) :
            base($"Value cannot change from {oldValue} to {newValue}.")
        {
        }

        //  Methods ---------------------------------------
        public static void Throw (object value)
        {
            throw new SwitchDefaultException (value);
        }
        
        
        //  Event Handlers --------------------------------
        
        
    }
}