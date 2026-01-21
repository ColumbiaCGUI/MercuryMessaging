# Time Travel Debugging - Task Checklist

Implementation tasks for message-centric time travel debugging.

---

## Phase 1: Recording Infrastructure (40 hours)

### Task 1.1: Message Recorder Core
- [ ] Create `MmMessageRecorder` class with static API
- [ ] Implement `RecordedMessage` struct with all required fields
- [ ] Implement `FilterDecision` struct with rejection reason generation
- [ ] Add `IsRecording` toggle for enabling/disabling
- [ ] Add circular buffer storage (default 10,000 messages)
- [ ] Implement `Clear()` and `Export()` methods

**Status:** NOT STARTED
**Estimated:** 12 hours

---

### Task 1.2: MmRelayNode Integration
- [ ] Add recording hook to `MmRelayNode.MmInvoke()`
- [ ] Capture routing decisions for each filter type
- [ ] Record rejection reasons with specific filter information
- [ ] Ensure minimal overhead when recording disabled
- [ ] Add frame number and timestamp to recordings
- [ ] Test with existing MercuryMessaging tests

**Status:** NOT STARTED
**Estimated:** 16 hours

---

### Task 1.3: Filter Decision Capture
- [ ] Hook into Level filter evaluation
- [ ] Hook into Active filter evaluation
- [ ] Hook into Selected filter evaluation
- [ ] Hook into Tag filter evaluation
- [ ] Hook into Network filter evaluation
- [ ] Generate human-readable rejection reasons
- [ ] Include expected vs actual values in reasons

**Status:** NOT STARTED
**Estimated:** 12 hours

---

## Phase 2: Timeline UI (50 hours)

### Task 2.1: Editor Window Framework
- [ ] Create `MmTimeTravelWindow` EditorWindow
- [ ] Add to Mercury menu (Window > Mercury > Time Travel Debugger)
- [ ] Implement basic layout (timeline, details, query)
- [ ] Add recording toggle button
- [ ] Add clear button
- [ ] Add export button

**Status:** NOT STARTED
**Estimated:** 10 hours

---

### Task 2.2: Timeline Scrubber
- [ ] Implement timeline bar with frame markers
- [ ] Add current position indicator
- [ ] Implement drag-to-scrub interaction
- [ ] Add playback controls (play, pause, step forward/back)
- [ ] Add frame number input field
- [ ] Add keyboard shortcuts (arrow keys, space)
- [ ] Optimize rendering for large timelines (virtualization)

**Status:** NOT STARTED
**Estimated:** 16 hours

---

### Task 2.3: Message Details Panel
- [ ] Display current message metadata
- [ ] Show source relay node with hyperlink to scene
- [ ] List reached responders with checkmarks
- [ ] List rejected responders with X marks
- [ ] Show rejection reason for each rejected responder
- [ ] Add expand/collapse for long lists
- [ ] Syntax highlighting for message content

**Status:** NOT STARTED
**Estimated:** 14 hours

---

### Task 2.4: Visual Indicators
- [ ] Implement ✓/✗ icons for reached/rejected
- [ ] Color-code by rejection reason type
- [ ] Add hover tooltips with full details
- [ ] Highlight current message in timeline
- [ ] Add message type icons (Initialize, SetActive, etc.)

**Status:** NOT STARTED
**Estimated:** 10 hours

---

## Phase 3: Query System (40 hours)

### Task 3.1: Query Builder API
- [ ] Create `MessageQuery` fluent builder class
- [ ] Implement `Method()` filter
- [ ] Implement `Target()` filter (by name, path, or component type)
- [ ] Implement `InFrameRange()` filter
- [ ] Implement `RejectedOnly()` filter
- [ ] Implement `RejectedBy(FilterType)` filter
- [ ] Implement `Execute()` method returning `QueryResult`

**Status:** NOT STARTED
**Estimated:** 12 hours

---

### Task 3.2: Query Result Analysis
- [ ] Create `QueryResult` class with matched messages
- [ ] Generate analysis report summarizing findings
- [ ] Identify patterns in rejections
- [ ] Suggest potential fixes for common issues
- [ ] Format results for display and export

**Status:** NOT STARTED
**Estimated:** 12 hours

---

### Task 3.3: Query UI
- [ ] Add query input field to editor window
- [ ] Add autocomplete for target names
- [ ] Display query results in scrollable list
- [ ] Add "Find" button to execute query
- [ ] Show result count and summary
- [ ] Allow clicking result to jump to frame
- [ ] Add "Copy Results" button

**Status:** NOT STARTED
**Estimated:** 12 hours

---

### Task 3.4: Common Query Presets
- [ ] Add dropdown for common queries
- [ ] "Why didn't message reach X?" preset
- [ ] "What messages did X receive?" preset
- [ ] "Which messages were rejected by Tag?" preset
- [ ] "Show all Initialize messages" preset
- [ ] Allow saving custom presets

**Status:** NOT STARTED
**Estimated:** 4 hours

---

## Phase 4: Testing & Documentation (20 hours)

### Task 4.1: Automated Tests
- [ ] Test recording captures all MmInvoke calls
- [ ] Test rejection reasons are accurate for each filter type
- [ ] Test circular buffer eviction works correctly
- [ ] Test query builder returns correct results
- [ ] Test UI updates correctly when scrubbing
- [ ] Test performance overhead <5%

**Status:** NOT STARTED
**Estimated:** 10 hours

---

### Task 4.2: Performance Benchmarks
- [ ] Measure recording overhead (frame time)
- [ ] Measure memory usage at different capacities
- [ ] Measure query response time
- [ ] Measure UI responsiveness during scrubbing
- [ ] Document results in performance report

**Status:** NOT STARTED
**Estimated:** 4 hours

---

### Task 4.3: Documentation
- [ ] Write user guide for Time Travel Debugger
- [ ] Document query syntax and examples
- [ ] Create debugging workflow tutorial
- [ ] Add tooltips to all UI elements
- [ ] Update CLAUDE.md with new tool reference

**Status:** NOT STARTED
**Estimated:** 6 hours

---

## Summary

| Phase | Tasks | Hours | Status |
|-------|-------|-------|--------|
| 1. Recording Infrastructure | 3 | 40 | NOT STARTED |
| 2. Timeline UI | 4 | 50 | NOT STARTED |
| 3. Query System | 4 | 40 | NOT STARTED |
| 4. Testing & Documentation | 3 | 20 | NOT STARTED |
| **Total** | **14** | **150** | **0% Complete** |

---

*Last Updated: 2025-12-17*
