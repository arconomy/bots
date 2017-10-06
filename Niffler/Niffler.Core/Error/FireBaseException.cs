//package net.thegreshams.firebase4j.error;

//import org.apache.log4j.Logger;

using System;

namespace Niffler.Error
{
    public class FirebaseException : Exception
    {

        //protected static final Logger LOGGER = Logger.getRootLogger();
        private static readonly long SerialVersionUID = 1L;

        public FirebaseException(String message) : base(message) { }
        public FirebaseException(String message, Exception cause) : base(message, cause) { }
    }
}

