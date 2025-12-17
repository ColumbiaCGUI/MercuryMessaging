// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Phase 2.1: Path Specification Parser
// Parses and validates hierarchical routing paths like "parent/sibling/child"

using System;
using System.Collections.Generic;
using System.Text;

namespace MercuryMessaging
{
    /// <summary>
    /// Represents a segment in a routing path.
    /// </summary>
    public enum PathSegment
    {
        /// <summary>Navigate to immediate parent node.</summary>
        Parent,

        /// <summary>Navigate to immediate child nodes.</summary>
        Child,

        /// <summary>Navigate to sibling nodes (same parent).</summary>
        Sibling,

        /// <summary>Navigate to self (current node).</summary>
        Self,

        /// <summary>Navigate to all ancestor nodes recursively.</summary>
        Ancestor,

        /// <summary>Navigate to all descendant nodes recursively.</summary>
        Descendant,

        /// <summary>Wildcard - expand to all nodes at this step.</summary>
        Wildcard
    }

    /// <summary>
    /// Represents a parsed routing path with validated segments.
    /// </summary>
    public class ParsedPath
    {
        /// <summary>Original path string.</summary>
        public string Original { get; private set; }

        /// <summary>Parsed segments in order.</summary>
        public PathSegment[] Segments { get; private set; }

        /// <summary>
        /// Creates a new parsed path.
        /// </summary>
        public ParsedPath(string original, PathSegment[] segments)
        {
            Original = original;
            Segments = segments;
        }

        /// <summary>
        /// Returns string representation of parsed path.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Segments.Length; i++)
            {
                if (i > 0) sb.Append("/");
                sb.Append(SegmentToString(Segments[i]));
            }
            return sb.ToString();
        }

        private static string SegmentToString(PathSegment segment)
        {
            switch (segment)
            {
                case PathSegment.Parent: return "parent";
                case PathSegment.Child: return "child";
                case PathSegment.Sibling: return "sibling";
                case PathSegment.Self: return "self";
                case PathSegment.Ancestor: return "ancestor";
                case PathSegment.Descendant: return "descendant";
                case PathSegment.Wildcard: return "*";
                default: return "unknown";
            }
        }
    }

    /// <summary>
    /// Exception thrown when path specification is invalid.
    /// </summary>
    public class MmInvalidPathException : Exception
    {
        public MmInvalidPathException(string message) : base(message) { }
    }

    /// <summary>
    /// Path specification parser for hierarchical message routing.
    /// Parses paths like "parent/sibling/child" into navigation segments.
    /// </summary>
    public static class MmPathSpecification
    {
        // Cache for parsed paths (performance optimization)
        private static Dictionary<string, ParsedPath> pathCache = new Dictionary<string, ParsedPath>();

        // Maximum cache size to prevent unbounded growth
        private const int MaxCacheSize = 100;

        /// <summary>
        /// Parses a path string into a ParsedPath object.
        /// Results are cached for performance.
        /// </summary>
        /// <param name="path">Path string like "parent/sibling/child"</param>
        /// <returns>Parsed path object</returns>
        /// <exception cref="MmInvalidPathException">If path syntax is invalid</exception>
        public static ParsedPath Parse(string path)
        {
            // Validate input
            if (string.IsNullOrEmpty(path))
            {
                throw new MmInvalidPathException("Path cannot be null or empty");
            }

            // Check cache first
            if (pathCache.TryGetValue(path, out ParsedPath cached))
            {
                return cached;
            }

            // Parse the path
            ParsedPath parsedPath = ParseInternal(path);

            // Add to cache (with size limit)
            if (pathCache.Count < MaxCacheSize)
            {
                pathCache[path] = parsedPath;
            }
            else
            {
                // Cache full - clear and restart (simple eviction strategy)
                pathCache.Clear();
                pathCache[path] = parsedPath;
            }

            return parsedPath;
        }

        /// <summary>
        /// Internal parsing implementation.
        /// </summary>
        private static ParsedPath ParseInternal(string path)
        {
            // Remove leading/trailing whitespace
            path = path.Trim();

            // Check for trailing slash
            if (path.EndsWith("/"))
            {
                throw new MmInvalidPathException(
                    $"Path cannot end with '/': {path}"
                );
            }

            // Split by separator
            string[] parts = path.Split('/');

            // Validate and convert each part
            List<PathSegment> segments = new List<PathSegment>();

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();

                // Empty segment (double slash or leading slash)
                if (string.IsNullOrEmpty(part))
                {
                    throw new MmInvalidPathException(
                        $"Empty segment at position {i} in path: {path}"
                    );
                }

                // Parse segment
                PathSegment segment;
                if (!TryParseSegment(part, out segment))
                {
                    throw new MmInvalidPathException(
                        $"Invalid segment '{part}' at position {i}. " +
                        "Valid segments: parent, child, sibling, self, ancestor, descendant, *"
                    );
                }

                segments.Add(segment);
            }

            // Validate path structure
            ValidatePath(segments, path);

            return new ParsedPath(path, segments.ToArray());
        }

        /// <summary>
        /// Tries to parse a segment string into a PathSegment enum.
        /// </summary>
        private static bool TryParseSegment(string segmentStr, out PathSegment segment)
        {
            switch (segmentStr.ToLowerInvariant())
            {
                case "parent":
                    segment = PathSegment.Parent;
                    return true;
                case "child":
                    segment = PathSegment.Child;
                    return true;
                case "sibling":
                    segment = PathSegment.Sibling;
                    return true;
                case "self":
                    segment = PathSegment.Self;
                    return true;
                case "ancestor":
                    segment = PathSegment.Ancestor;
                    return true;
                case "descendant":
                    segment = PathSegment.Descendant;
                    return true;
                case "*":
                    segment = PathSegment.Wildcard;
                    return true;
                default:
                    segment = PathSegment.Self; // Default (not used)
                    return false;
            }
        }

        /// <summary>
        /// Validates the overall path structure for logical consistency.
        /// </summary>
        private static void ValidatePath(List<PathSegment> segments, string originalPath)
        {
            if (segments.Count == 0)
            {
                throw new MmInvalidPathException("Path must contain at least one segment");
            }

            // Check for invalid wildcard usage
            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] == PathSegment.Wildcard)
                {
                    // Wildcard cannot be first segment (no context)
                    if (i == 0)
                    {
                        throw new MmInvalidPathException(
                            $"Wildcard '*' cannot be the first segment in path: {originalPath}. " +
                            "Wildcard requires a preceding relationship to expand."
                        );
                    }

                    // Wildcard must be followed by another segment
                    if (i == segments.Count - 1)
                    {
                        throw new MmInvalidPathException(
                            $"Wildcard '*' cannot be the last segment in path: {originalPath}. " +
                            "Wildcard must be followed by a relationship to navigate."
                        );
                    }

                    // Wildcard cannot be followed by another wildcard
                    if (i + 1 < segments.Count && segments[i + 1] == PathSegment.Wildcard)
                    {
                        throw new MmInvalidPathException(
                            $"Consecutive wildcards '**' not supported in path: {originalPath}. " +
                            "Use explicit segments between wildcards."
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Clears the path cache. Useful for testing or memory management.
        /// </summary>
        public static void ClearCache()
        {
            pathCache.Clear();
        }

        /// <summary>
        /// Gets the current cache size. Useful for diagnostics.
        /// </summary>
        public static int GetCacheSize()
        {
            return pathCache.Count;
        }
    }
}
