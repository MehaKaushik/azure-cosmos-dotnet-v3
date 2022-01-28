﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.Telemetry.Diagnostics
{
    using System;
    using System.Net;

    internal static class DiagnosticsFilterHelper
    {
        private static readonly double latencyThresholdInMs = 100;
        private static readonly double requestChargeThresholdInRu = 100;

        /// <summary>
        /// Allow only when either of below is True
        /// 1) Latency is not more than 100 ms
        /// 2) Request Charge is more than 100 RUs
        /// 3) HTTP status code is not Success
        /// 
        /// </summary>
        /// <returns>true or false</returns>
        public static bool IsAllowed(
            TimeSpan latency, 
            double requestcharge, 
            HttpStatusCode statuscode)
        {
            return latency > TimeSpan.FromMilliseconds(DiagnosticsFilterHelper.latencyThresholdInMs) ||
                   requestcharge > DiagnosticsFilterHelper.requestChargeThresholdInRu ||
                   !statuscode.IsSuccess();
        }
    }
}
