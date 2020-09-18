﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Performance.Tests
{
    using System;
    using System.Globalization;
    using BenchmarkDotNet.Attributes;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Collections;

    [Config(typeof(SdkBenchmarkConfiguration))]
    public class MasterKeyAuthorizationBenchmark
    {
        private readonly IComputeHash authKeyHashFunction;
        private readonly INameValueCollection testHeaders;

        public MasterKeyAuthorizationBenchmark()
        {
            this.authKeyHashFunction = new StringHMACSHA256Hash(MockDocumentClient.GenerateRandomKey());
            Headers headers = new Headers
            {
                [HttpConstants.HttpHeaders.XDate] = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture)
            };

            this.testHeaders = headers.CosmosMessageHeaders;
        }

        [Benchmark]
        public void CreateSignatureGeneration()
        {
            this.TestSignature("POST", "dbs/testdb/colls/testcollection/dbs", "dbs");
        }

        [Benchmark]
        public void ReadSignatureGeneration()
        {
            this.TestSignature("GET", "dbs/testdb/colls/testcollection/dbs/item1", "dbs");
        }

        private void TestSignature(string verb,
            string resourceId,
            string resourceType)
        {
            AuthorizationHelper.GenerateKeyAuthorizationSignature(verb, resourceId, resourceType, this.testHeaders, this.authKeyHashFunction, out AuthorizationHelper.ArrayOwner payload);
            payload.Dispose();
        }
    }
}
