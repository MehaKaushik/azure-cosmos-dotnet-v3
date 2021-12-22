﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Microsoft.Azure.Cosmos.Tracing.TraceData;

    internal static class TraceJoiner
    {
        public static ITrace JoinTraces(params ITrace[] traces)
        {
            if (traces == null)
            {
                throw new ArgumentNullException(nameof(traces));
            }

            return JoinTraces(traces.ToList());
        }

        public static ITrace JoinTraces(IReadOnlyList<ITrace> traces)
        {
            if (traces == null)
            {
                throw new ArgumentNullException(nameof(traces));
            }

            TraceForest traceForest = new TraceForest(traces.ToList());
            return traceForest;
        }

        private sealed class TraceForest : ITrace
        {
            private static readonly CallerInfo EmptyInfo = new CallerInfo(string.Empty, string.Empty, 0);

            private readonly Dictionary<string, object> data;

            private readonly List<ITrace> children;

            public TraceForest(IReadOnlyList<ITrace> children)
            {
                this.children = new List<ITrace>(children);
                this.data = new Dictionary<string, object>();

                HashSet<(string, Uri)> regionsList = new HashSet<(string, Uri)>();
                foreach (ITrace trace in children)
                {
                    regionsList.UnionWith(trace.RegionsContacted);
                }

                this.UpdateRegionContacted(regionsList);
            }

            public string Name => "Trace Forest";

            public Guid Id => Guid.Empty;

            public CallerInfo CallerInfo => EmptyInfo;

            public DateTime StartTime => DateTime.MinValue;

            public TimeSpan Duration => TimeSpan.MaxValue;

            public TraceLevel Level => TraceLevel.Info;

            public TraceComponent Component => TraceComponent.Unknown;

            public ITrace Parent => null;

            public IReadOnlyList<ITrace> Children => this.children;

            public IReadOnlyDictionary<string, object> Data => this.data;

            public HashSet<(string, Uri)> RegionsContacted { get; private set; }

            public void AddDatum(string key, TraceDatum traceDatum)
            {
                this.data[key] = traceDatum;
                this.UpdateRegionContacted(traceDatum);
            }

            public void AddDatum(string key, object value)
            {
                this.data[key] = value;
            }

            public void Dispose()
            {
            }

            public ITrace StartChild(string name, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            {
                return this.StartChild(name, TraceComponent.Unknown, TraceLevel.Info, memberName, sourceFilePath, sourceLineNumber);
            }

            public ITrace StartChild(string name, TraceComponent component, TraceLevel level, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            {
                ITrace child = Trace.GetRootTrace(name, component, level, memberName, sourceFilePath, sourceLineNumber);
                this.AddChild(child);
                return child;
            }

            public void AddChild(ITrace trace)
            {
                this.children.Add(trace);
                if (trace.RegionsContacted != null)
                {
                    this.UpdateRegionContacted(trace.RegionsContacted);
                }
            }

            public void UpdateRegionContacted(TraceDatum traceDatum)
            {
                if (traceDatum is ClientSideRequestStatisticsTraceDatum clientSideRequestStatisticsTraceDatum)
                {
                    if (clientSideRequestStatisticsTraceDatum.RegionsContacted == null || clientSideRequestStatisticsTraceDatum.RegionsContacted.Count == 0)
                    {
                        return;
                    }
                    this.UpdateRegionContacted(clientSideRequestStatisticsTraceDatum.RegionsContacted);
                }
            }

            public void UpdateRegionContacted(HashSet<(string, Uri)> newRegionContacted)
            {
                if (this.RegionsContacted == null)
                {
                    this.RegionsContacted = newRegionContacted;
                }
                else
                {
                    this.RegionsContacted.UnionWith(newRegionContacted);

                }

                if (this.Parent != null)
                {
                    this.Parent.UpdateRegionContacted(newRegionContacted);
                }
            }
        }
    }
}
