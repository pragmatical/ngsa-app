﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Ngsa.Application
{
    /// <summary>
    /// Application secrets
    /// </summary>
    public class Secrets
    {
        public string Volume { get; set; }
        public string CosmosServer { get; set; }
        public string CosmosKey { get; set; }
        public string CosmosDatabase { get; set; }
        public string CosmosCollection { get; set; }

        /// <summary>
        /// Get the secrets from the k8s volume
        /// </summary>
        /// <param name="volume">k8s volume name</param>
        /// <returns>Secrets or null</returns>
        public static Secrets GetSecretsFromVolume(string volume)
        {
            if (string.IsNullOrWhiteSpace(volume))
            {
                throw new ArgumentNullException(nameof(volume));
            }

            // thow exception if volume doesn't exist
            if (!Directory.Exists(volume))
            {
                throw new Exception($"Volume '{volume}' does not exist");
            }

            // get k8s secrets from volume
            Secrets sec = new()
            {
                Volume = volume,
                CosmosCollection = GetSecretFromFile(volume, "CosmosCollection"),
                CosmosDatabase = GetSecretFromFile(volume, "CosmosDatabase"),
                CosmosKey = GetSecretFromFile(volume, "CosmosKey"),
                CosmosServer = GetSecretFromFile(volume, "CosmosUrl"),
            };

            ValidateSecrets(volume, sec);

            return sec;
        }

        // basic validation of Cosmos values
        private static void ValidateSecrets(string volume, Secrets sec)
        {
            if (sec == null)
            {
                throw new Exception($"Unable to read secrets from volume: {volume}");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosCollection))
            {
                throw new Exception($"CosmosCollection cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosDatabase))
            {
                throw new Exception($"CosmosDatabase cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosKey))
            {
                throw new Exception($"CosmosKey cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(sec.CosmosServer))
            {
                throw new Exception($"CosmosUrl cannot be empty");
            }

            if (!sec.CosmosServer.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                !sec.CosmosServer.Contains(".documents.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Invalid value for CosmosUrl: {sec.CosmosServer}");
            }

            if (sec.CosmosKey.Length < 64)
            {
                throw new Exception($"Invalid value for CosmosKey: {sec.CosmosKey}");
            }
        }

        // read a secret from a k8s volume
        private static string GetSecretFromFile(string volume, string key)
        {
            string val = string.Empty;

            if (File.Exists($"{volume}/{key}"))
            {
                val = File.ReadAllText($"{volume}/{key}").Trim();
            }

            return val;
        }
    }
}
