// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmHierarchyValidator.cs - Editor-time validation for MercuryMessaging hierarchies
// Part of DX4: Developer Experience Improvements

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MercuryMessaging.Editor
{
    /// <summary>
    /// Provides editor-time validation for MercuryMessaging hierarchies.
    /// Validates routing table registration, parent relationships, and common setup issues.
    /// </summary>
    /// <remarks>
    /// Access via menu: MercuryMessaging > Validate Hierarchy
    /// </remarks>
    public static class MmHierarchyValidator
    {
        private const string MENU_PATH = "MercuryMessaging/Validate Hierarchy";
        private const string VALIDATE_SELECTION_PATH = "MercuryMessaging/Validate Selected";

        /// <summary>
        /// Validate all relay nodes in the current scene.
        /// </summary>
        [MenuItem(MENU_PATH)]
        public static void ValidateAllRelayNodes()
        {
            var issues = new List<ValidationIssue>();
            var relayNodes = Object.FindObjectsByType<MmRelayNode>(FindObjectsSortMode.None);

            if (relayNodes.Length == 0)
            {
                Debug.Log("[MmValidator] No MmRelayNode components found in scene.");
                return;
            }

            foreach (var relay in relayNodes)
            {
                ValidateRelayNode(relay, issues);
            }

            ReportIssues(issues, relayNodes.Length);
        }

        /// <summary>
        /// Validate only the selected GameObject's relay node.
        /// </summary>
        [MenuItem(VALIDATE_SELECTION_PATH)]
        public static void ValidateSelectedRelayNodes()
        {
            var selection = Selection.gameObjects;
            if (selection.Length == 0)
            {
                Debug.LogWarning("[MmValidator] No GameObjects selected.");
                return;
            }

            var issues = new List<ValidationIssue>();
            int validatedCount = 0;

            foreach (var go in selection)
            {
                var relay = go.GetComponent<MmRelayNode>();
                if (relay != null)
                {
                    ValidateRelayNode(relay, issues);
                    validatedCount++;
                }
            }

            if (validatedCount == 0)
            {
                Debug.LogWarning("[MmValidator] No MmRelayNode components found in selection.");
                return;
            }

            ReportIssues(issues, validatedCount);
        }

        [MenuItem(VALIDATE_SELECTION_PATH, true)]
        private static bool ValidateSelectedRelayNodesValidate()
        {
            return Selection.gameObjects.Length > 0;
        }

        /// <summary>
        /// Validate a single relay node.
        /// </summary>
        public static List<ValidationIssue> ValidateNode(MmRelayNode relay)
        {
            var issues = new List<ValidationIssue>();
            ValidateRelayNode(relay, issues);
            return issues;
        }

        private static void ValidateRelayNode(MmRelayNode relay, List<ValidationIssue> issues)
        {
            // Check 1: Orphan relay node (has parent transform but no parent relay)
            ValidateParentRelationship(relay, issues);

            // Check 2: Child relay nodes not registered in routing table
            ValidateChildRegistration(relay, issues);

            // Check 3: Responders on same GameObject are in routing table
            ValidateResponderRegistration(relay, issues);
        }

        private static void ValidateParentRelationship(MmRelayNode relay, List<ValidationIssue> issues)
        {
            var parentTransform = relay.transform.parent;
            if (parentTransform == null) return; // Root node, no parent expected

            var parentRelay = parentTransform.GetComponentInParent<MmRelayNode>();
            if (parentRelay == null)
            {
                // Parent transform exists but has no relay node in ancestry
                issues.Add(new ValidationIssue
                {
                    Level = IssueLevel.Info,
                    Node = relay,
                    Message = $"'{relay.name}' has no parent MmRelayNode in hierarchy. " +
                             "This may be intentional (root node) or a missing relay node on an ancestor."
                });
                return;
            }

            // Check if this node is registered in the parent's routing table
            bool isRegistered = false;
            foreach (var item in parentRelay.RoutingTable)
            {
                if (item.Responder == relay)
                {
                    isRegistered = true;
                    break;
                }
            }

            if (!isRegistered)
            {
                issues.Add(new ValidationIssue
                {
                    Level = IssueLevel.Warning,
                    Node = relay,
                    Message = $"'{relay.name}' is not registered in parent '{parentRelay.name}' routing table. " +
                             "Messages from parent will not reach this node. " +
                             "Fix: Call parentRelay.MmAddToRoutingTable() or use MmSetParent()."
                });
            }
        }

        private static void ValidateChildRegistration(MmRelayNode relay, List<ValidationIssue> issues)
        {
            foreach (Transform child in relay.transform)
            {
                var childRelay = child.GetComponent<MmRelayNode>();
                if (childRelay == null) continue;

                // Check if child is in parent's routing table
                bool isRegistered = false;
                foreach (var item in relay.RoutingTable)
                {
                    if (item.Responder == childRelay)
                    {
                        isRegistered = true;
                        break;
                    }
                }

                if (!isRegistered)
                {
                    issues.Add(new ValidationIssue
                    {
                        Level = IssueLevel.Warning,
                        Node = relay,
                        Message = $"Child relay '{childRelay.name}' not in '{relay.name}' routing table. " +
                                 "Messages to children will not reach this node. " +
                                 "Fix: Call MmAddToRoutingTable(childRelay, MmLevelFilter.Child)."
                    });
                }
            }
        }

        private static void ValidateResponderRegistration(MmRelayNode relay, List<ValidationIssue> issues)
        {
            var responders = relay.GetComponents<MmResponder>();
            foreach (var responder in responders)
            {
                // Skip relay nodes (they are not "responders" in this context)
                if (responder is MmRelayNode) continue;

                // Check if responder is in routing table
                bool isRegistered = false;
                foreach (var item in relay.RoutingTable)
                {
                    if (item.Responder == responder)
                    {
                        isRegistered = true;
                        break;
                    }
                }

                if (!isRegistered)
                {
                    issues.Add(new ValidationIssue
                    {
                        Level = IssueLevel.Warning,
                        Node = relay,
                        Message = $"Responder '{responder.GetType().Name}' on '{relay.name}' not in routing table. " +
                                 "This responder will not receive messages. " +
                                 "Fix: Call MmRefreshResponders() after adding components at runtime."
                    });
                }
            }
        }

        private static void ReportIssues(List<ValidationIssue> issues, int nodeCount)
        {
            if (issues.Count == 0)
            {
                Debug.Log($"[MmValidator] ✓ All {nodeCount} relay node(s) validated successfully. No issues found.");
                return;
            }

            int warnings = 0, infos = 0;
            foreach (var issue in issues)
            {
                switch (issue.Level)
                {
                    case IssueLevel.Warning:
                        Debug.LogWarning($"[MmValidator] {issue.Message}", issue.Node);
                        warnings++;
                        break;
                    case IssueLevel.Info:
                        Debug.Log($"[MmValidator] {issue.Message}", issue.Node);
                        infos++;
                        break;
                }
            }

            Debug.Log($"[MmValidator] Validation complete: {nodeCount} node(s), {warnings} warning(s), {infos} info(s).");
        }

        /// <summary>
        /// Represents a validation issue found during hierarchy validation.
        /// </summary>
        public class ValidationIssue
        {
            public IssueLevel Level { get; set; }
            public MmRelayNode Node { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Severity level of a validation issue.
        /// </summary>
        public enum IssueLevel
        {
            Info,
            Warning
        }
    }
}
