// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Routing Optimization - Configuration Options
// Part of Phase 2.1: Advanced Message Routing

using System;

namespace MercuryMessaging
{
    /// <summary>
    /// Configuration options for advanced message routing behavior.
    /// Provides fine-grained control over routing algorithms, history tracking,
    /// and custom filtering logic.
    ///
    /// Usage:
    /// - Attach to MmMetadataBlock for per-message configuration
    /// - Set on MmRelayNode for per-node defaults
    /// - All options default to backward-compatible behavior
    /// </summary>
    [Serializable]
    public class MmRoutingOptions
    {
        #region History Tracking

        /// <summary>
        /// Enable message history tracking for improved cycle detection.
        /// When enabled, messages track which nodes they've visited using a time-windowed cache.
        /// This provides better circular routing prevention than hop counting alone.
        /// Default: false (for backward compatibility)
        /// </summary>
        public bool EnableHistoryTracking = false;

        /// <summary>
        /// Time window in milliseconds for message history cache.
        /// Messages older than this are evicted from the history cache.
        /// Larger values = better cycle detection, higher memory usage.
        /// Default: 100ms (supports ~1000 messages at typical throughput)
        /// </summary>
        public int HistoryCacheSizeMs = 100;

        #endregion

        #region Hop Limits

        /// <summary>
        /// Maximum number of hops a message can traverse before being dropped.
        /// Prevents infinite loops in circular hierarchies.
        /// Set to -1 for unlimited hops (use with caution).
        /// Default: 50 (MmMessage.MAX_HOPS_DEFAULT)
        /// </summary>
        public int MaxRoutingHops = 50;

        #endregion

        #region Lateral Routing

        /// <summary>
        /// Enable routing to sibling and cousin nodes.
        /// When disabled, only hierarchical (parent/child) routing is allowed.
        /// When enabled, LevelFilter.Siblings and LevelFilter.Cousins become available.
        /// Default: false (for backward compatibility)
        /// </summary>
        public bool AllowLateralRouting = false;

        #endregion

        #region Custom Filtering

        /// <summary>
        /// Custom predicate function for filtering relay nodes during routing.
        /// Called for each candidate relay node before message delivery.
        /// Return true to include node, false to skip.
        ///
        /// Example: Options.CustomFilter = (node) => node.tag == "Player" && node.activeInHierarchy;
        ///
        /// Note: Called frequently during routing - keep logic fast (< 10 microseconds).
        /// Default: null (no custom filtering)
        /// </summary>
        public Func<MmRelayNode, bool> CustomFilter = null;

        #endregion

        #region Performance Profiling

        /// <summary>
        /// Enable performance profiling hooks for this routing operation.
        /// When enabled, timing data is collected and logged via MmLogger.
        /// Useful for identifying routing bottlenecks.
        /// Default: false (zero overhead when disabled)
        /// </summary>
        public bool EnableProfiling = false;

        /// <summary>
        /// Log profiling data only if routing time exceeds this threshold (milliseconds).
        /// Helps filter out fast routes and focus on slow ones.
        /// Default: 1.0ms (logs routes taking > 1ms)
        /// </summary>
        public float ProfilingThresholdMs = 1.0f;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates routing options with default settings (backward-compatible).
        /// </summary>
        public MmRoutingOptions()
        {
            // All defaults set via field initializers above
        }

        /// <summary>
        /// Creates routing options with specified settings.
        /// </summary>
        public MmRoutingOptions(
            bool enableHistoryTracking = false,
            int historyCacheSizeMs = 100,
            int maxRoutingHops = 50,
            bool allowLateralRouting = false,
            Func<MmRelayNode, bool> customFilter = null,
            bool enableProfiling = false,
            float profilingThresholdMs = 1.0f)
        {
            EnableHistoryTracking = enableHistoryTracking;
            HistoryCacheSizeMs = historyCacheSizeMs;
            MaxRoutingHops = maxRoutingHops;
            AllowLateralRouting = allowLateralRouting;
            CustomFilter = customFilter;
            EnableProfiling = enableProfiling;
            ProfilingThresholdMs = profilingThresholdMs;
        }

        /// <summary>
        /// Creates a copy of the routing options.
        /// Note: CustomFilter is shallow-copied (same delegate reference).
        /// </summary>
        public MmRoutingOptions Clone()
        {
            return new MmRoutingOptions(
                EnableHistoryTracking,
                HistoryCacheSizeMs,
                MaxRoutingHops,
                AllowLateralRouting,
                CustomFilter,
                EnableProfiling,
                ProfilingThresholdMs
            );
        }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Creates routing options with history tracking enabled (for cycle detection).
        /// </summary>
        public static MmRoutingOptions WithHistoryTracking(int cacheSizeMs = 100)
        {
            return new MmRoutingOptions(
                enableHistoryTracking: true,
                historyCacheSizeMs: cacheSizeMs
            );
        }

        /// <summary>
        /// Creates routing options with lateral routing enabled (siblings/cousins).
        /// </summary>
        public static MmRoutingOptions WithLateralRouting()
        {
            return new MmRoutingOptions(
                allowLateralRouting: true
            );
        }

        /// <summary>
        /// Creates routing options with custom filter predicate.
        /// </summary>
        public static MmRoutingOptions WithCustomFilter(Func<MmRelayNode, bool> filter)
        {
            return new MmRoutingOptions(
                customFilter: filter
            );
        }

        /// <summary>
        /// Creates routing options with performance profiling enabled.
        /// </summary>
        public static MmRoutingOptions WithProfiling(float thresholdMs = 1.0f)
        {
            return new MmRoutingOptions(
                enableProfiling: true,
                profilingThresholdMs: thresholdMs
            );
        }

        /// <summary>
        /// Creates routing options with all advanced features enabled (for testing/debugging).
        /// Warning: Higher overhead - use only during development.
        /// </summary>
        public static MmRoutingOptions AllFeaturesEnabled()
        {
            return new MmRoutingOptions(
                enableHistoryTracking: true,
                historyCacheSizeMs: 200,
                maxRoutingHops: 100,
                allowLateralRouting: true,
                customFilter: null,
                enableProfiling: true,
                profilingThresholdMs: 0.1f
            );
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates routing options and returns error message if invalid.
        /// Returns null if options are valid.
        /// </summary>
        public string Validate()
        {
            if (HistoryCacheSizeMs < 0)
                return "HistoryCacheSizeMs must be >= 0";

            if (MaxRoutingHops < -1)
                return "MaxRoutingHops must be >= -1 (-1 = unlimited)";

            if (MaxRoutingHops == 0)
                return "MaxRoutingHops cannot be 0 (use -1 for unlimited or >= 1 for limit)";

            if (ProfilingThresholdMs < 0)
                return "ProfilingThresholdMs must be >= 0";

            // Warning: unlimited hops with lateral routing can cause infinite loops
            if (MaxRoutingHops == -1 && AllowLateralRouting && !EnableHistoryTracking)
            {
                return "Warning: Unlimited hops with lateral routing requires history tracking to prevent infinite loops";
            }

            return null; // Valid
        }

        #endregion

        #region Debug Helpers

        public override string ToString()
        {
            return $"MmRoutingOptions(" +
                   $"History={EnableHistoryTracking}, " +
                   $"Cache={HistoryCacheSizeMs}ms, " +
                   $"MaxHops={MaxRoutingHops}, " +
                   $"Lateral={AllowLateralRouting}, " +
                   $"CustomFilter={CustomFilter != null}, " +
                   $"Profiling={EnableProfiling})";
        }

        #endregion
    }
}
