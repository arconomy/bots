//package net.thegreshams.firebase4j.model;

//import java.util.LinkedHashMap;
//import java.util.Map;

//import org.apache.log4j.Logger;

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Niffler.Model
{
    public class FirebaseResponse : HttpResponseMessage
    {

        //protected static readonly Logger LOGGER = Logger.getRootLogger();

        ///////////////////////////////////////////////////////////////////////////////
        //
        // PROPERTIES & CONSTRUCTORS
        //
        ///////////////////////////////////////////////////////////////////////////////

        private readonly bool success;
        private readonly int code;
        private readonly IDictionary<string, object> body;
        private readonly string rawBody;

        public FirebaseResponse(bool success, int code, IDictionary<string, object> body, string rawBody)
        {
            this.success = success;
            this.code = code;

            if (body == null)
            {
                Console.WriteLine("INFO: body was null; replacing with empty map");
                body = new Dictionary<string, object>();
            }
            this.body = body;

            if (rawBody == null)
            {
                Console.WriteLine("INFO: rawBody was null; replacing with empty string");
                rawBody = "";
            }
            this.rawBody = rawBody.Trim();
        }

        ///////////////////////////////////////////////////////////////////////////////
        //
        // PUBLIC API
        //
        ///////////////////////////////////////////////////////////////////////////////


        /**
         * Returns whether or not the response from the Firebase-client was successful
         * 
         * @return true if response from the Firebase-client was successful
         */
        public bool GetSuccess()
        {
            return this.success;
        }

        /**
         * Returns the HTTP status code returned from the Firebase-client
         * 
         * @return an integer representing an HTTP status code
         */
        public int GetCode()
        {
            return this.code;
        }

        /**
         * Returns a map of the data returned by the Firebase-client
         * 
         * @return a map of strings to Objects
         */
        public IDictionary<string, object> GetBody()
        {
            return this.body;
        }

        /**
         * Returns the raw data response returned by the Firebase-client
         * 
         * @return a string of the JSON-response from the client
         */
        public string GetRawBody()
        {
            return this.rawBody;
        }

        public override string ToString()
        {
            return nameof(FirebaseResponse) + "[ "
                + "(Success:" + this.success + ") "
                + "(Code:" + this.code + ") "
                + "(Body:" + this.body + ") "
                + "(Raw-body:" + this.rawBody + ") "
                + "]";
        }

    }
}