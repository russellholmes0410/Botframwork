﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.FunctionalTests.Configuration
{
    internal static class EnvironmentConfig
    {
        public static string TestAppId()
        {
            var testAppId = Environment.GetEnvironmentVariable("TESTAPPID");
            if (string.IsNullOrWhiteSpace(testAppId))
            {
                throw new Exception("Environment variable 'TestAppId' not found.");
            }

            return testAppId;
        }

        public static string TestAppPassword()
        {
            var testPassword = Environment.GetEnvironmentVariable("TESTPASSWORD");

            if (string.IsNullOrWhiteSpace(testPassword))
            {
                throw new Exception("Environment variable 'TestPassword' not found.");
            }

            return testPassword;
        }
    }
}
