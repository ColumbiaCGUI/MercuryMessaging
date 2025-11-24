

# **Advancing the Mercury Messaging Framework: Architecting Novel Contributions for Real-Time Interactive Systems**

## **Executive Summary**

The development of Real-Time Interactive Systems (RISs), particularly those underpinning Extended Reality (XR) experiences, faces persistent challenges in software architecture. The complexities of integrating heterogeneous components—tracking systems, rendering pipelines, input modalities, and networked state synchronization—often lead to rigid, tightly coupled codebases that resist modification and limit reusability.1 The Relay & Responder (R\&R) pattern, realized through the Mercury Messaging Framework, offers a robust solution by decoupling logical components from the communication infrastructure that connects them.1 By facilitating structured, bidirectional, and non-spatial communication hierarchies, Mercury addresses the limitations of traditional Object-Oriented Programming (OOP) and event-driven patterns common in engines like Unity.1  
This report provides an exhaustive analysis of the Mercury framework, tracing its evolution from earlier toolkits like GoblinXNA and WF Toolkit, deconstructing its architectural mechanisms, and evaluating its application in complex research systems such as *CURVE*, *NotifiAR*, and *Remote Task Assistance*.1 The analysis identifies critical bottlenecks in the current implementation, specifically regarding developer cognition ("invisible logic"), runtime safety (infinite loops), and concurrency (main thread blocking).1  
To elevate Mercury to a tier-one contribution suitable for the ACM Symposium on User Interface Software and Technology (UIST), this report proposes four novel, sizable technical advancements: (1) A **Live Visual Authoring and Introspection Environment** to bridge the gulf of evaluation in network topology; (2) A **High-Performance Asynchronous Scheduling Runtime** to resolve main-thread concurrency blocking; (3) A **Static Analysis and Hybrid Safety Verification Layer** to fundamentally solve the problem of dynamic graph cycles; and (4) The integration of **Language Primitives and Domain-Specific Syntax** to reduce verbosity. These contributions aim to transform Mercury from a utility library into a comprehensive paradigm for engineering scalable, safe, and performant interactive systems.

## **1\. The Evolution of XR Software Architectures**

The historical trajectory of software development for Augmented Reality (AR) and Virtual Reality (VR) reveals a consistent struggle between the need for performance and the desire for architectural abstraction. Early systems, dating back to Sutherland’s seminal work and Feiner’s "Windows on the World," were often bespoke, monolithic constructs where low-level hardware integration and high-level application logic were inextricably intertwined.1 As the field matured, the necessity for reusable infrastructure became apparent, leading to the development of frameworks such as COTERIE, Studierstube, and DWARF.1

### **1.1 The Shift from Inheritance to Composition**

Traditional frameworks often relied on deep inheritance hierarchies to provide functionality. For instance, GoblinXNA, built on Microsoft’s XNA platform, provided a scene graph and rendering subsystems but required programmers to manage model loading and input abstraction manually, often forcing them to subclass platform-specific nodes to introduce new behavior.1 This object-oriented approach, while structured, created rigid dependencies. If a developer wanted to create a "Tracked AR Widget," they might inherit from a TrackedObject class. If they later needed that widget to function in a VR simulation without tracking, the inheritance hierarchy became a liability, forcing code duplication or awkward refactoring.1  
The industry's transition to the Entity-Component-System (ECS) pattern, exemplified by the Unity game engine, marked a pivotal shift towards composition over inheritance.1 In ECS, entities (GameObjects) are empty containers populated by functional components. This allows for greater flexibility; a LightSwitch entity is simply a container with a MeshRenderer, a Collider, and a SwitchController script attached.1 However, while ECS solves the problem of *object* composition, it exacerbates the problem of *inter-component* communication. How does the SwitchController tell a LightBulb entity, located elsewhere in the scene hierarchy, to turn on?

### **1.2 The Failure of Traditional Communication Patterns in XR**

In standard ECS development, developers typically resort to one of three patterns, each with significant deficiencies for complex RIS development:  
**The Observer Pattern (Events/Delegates):** Components expose public events (e.g., OnSwitchFlipped). Other components must find the sender and subscribe to these events.1 This introduces two critical issues. First, it requires a discovery phase where the listener must obtain a reference to the sender, reintroducing coupling. Second, it requires function signature matching; the listener’s handler must match the event’s delegate signature exactly. This rigidly binds the receiver to the specific implementation details of the sender, limiting reusability.1  
**The Command Pattern:** This interposes a command interface between sender and receiver. While this decouples the execution logic, it merely shifts the coupling to the command object itself. The sender is coupled to the command, and the command is coupled to the receiver.1 Furthermore, this pattern is typically unidirectional; handling a return signal (e.g., the light confirming it has turned on) requires a separate, parallel command structure, doubling the architectural complexity.1  
**The Mediator Pattern:** A central manager object coordinates interaction between components. While this centralizes logic, the mediator itself becomes a "God Object," tightly coupled to every component it manages. In dynamic XR systems where objects are spawned and destroyed at runtime (e.g., instantiated data placards in *CURVE*), maintaining the mediator’s list of references becomes a fragile, error-prone task.1

### **1.3 Precursor: The WF Toolkit**

Before the crystallization of the Mercury framework, the "Visualization Foundation Toolkit" (WF Toolkit) represented an initial attempt to standardize widget construction.1 WF Toolkit introduced the concept of WFNode, a component that provided a common interface (Initialize, Activate, Update, Reset, Destroy) for all interactive elements.1 This allowed a central WFManager to control disparate widgets without knowing their specific types.  
However, WF Toolkit revealed the limitations of strictly hierarchical control. While it effectively managed "vertical" commands from a manager to its children (a WFBundle), it lacked a robust mechanism for "horizontal" or "upward" communication.1 If a button deep inside a widget hierarchy needed to signal the manager that a task was complete, the toolkit offered no standardized path, forcing developers to break the pattern and revert to direct referencing.1 This limitation—the inability to support omnidirectional, decoupled communication—was the catalyst for the Relay & Responder pattern.

## **2\. The Relay & Responder (R\&R) Pattern**

The Relay & Responder (R\&R) pattern acts as the conceptual foundation of Mercury. It is a software design pattern designed to eliminate inter- and intra-module coupling by separating the *logic* of an application from the *flow of communication*.1

### **2.1 Pattern Architecture**

The pattern defines two primary actors:  
**The Relay:** The Relay is a routing hub. It maintains a Routing Table, a dynamic data structure that maps relationships between the Relay and other entities.1 Crucially, a Relay is agnostic to the content of the messages it handles. It functions similarly to a network router at the Transport Layer; it inspects the metadata of a message (the "envelope") to determine where it should go, but never inspects the payload.1 This agnosticism is key to decoupling; the Relay does not need to change if the message structure changes.1  
**The Responder:** The Responder is the endpoint. It consumes messages and executes application-specific logic. A Responder does not know who sent the message, only that a message of a recognizable type has arrived.1 Responders are typically attached to the same entity as a Relay or to entities referenced in the Relay’s Routing Table.

### **2.2 Decoupling through Message-Passing**

In an R\&R system, a sender invokes a message on a local Relay. The Relay propagates this message to all valid targets in its Routing Table. The sender has no reference to the receiver; it only has a reference to its own local Relay.1 This allows for radical reusability. A LightSwitch component can be written to send a generic SetActive(true) message. In one scene, the Relay might route this to a LightBulb. In another, it might route it to a DoorOpener or a SoundEffect. The LightSwitch code remains identical in all cases, unaware of the consequences of its action.1  
This architecture supports **Asymmetric Topologies** in networked experiences.1 In a collaborative system like *Remote Task Assistance*, a "Grab" action performed by a Subject Matter Expert (SME) might trigger a complex set of visualizations on their local machine (highlighting the object, showing a ghosted replica), while on the remote Technician’s machine, the same message triggers a completely different set of feedback (an arrow pointing to the object).1 With R\&R, the "Grab" code does not need conditional logic to handle these differences; the divergence is handled entirely by the configuration of the Relays on the respective machines.1

## **3\. The Mercury Messaging Framework: Technical Deconstruction**

Mercury is the reference implementation of the R\&R pattern within the Unity game engine. It translates the abstract concepts of Relays and Responders into concrete C\# components (MonoBehaviours) that integrate with the Unity Inspector and serialization systems.1

### **3.1 The Mercury Message (MmMessage)**

At the heart of the framework is the MmMessage. Unlike standard C\# events which are defined by function signatures, an MmMessage is a polymorphic data object containing three distinct components 1:

1. **Message Type (MmMethod):** An enumerated identifier (integer) that categorizes the message (e.g., SetActive, Initialize, Refresh). This allows for fast integer comparisons during routing rather than slow string lookups.1  
2. **Payload:** An optional data packet. Mercury provides generic implementations for primitives (bool, int, float, string) and a special MmMessageSerializable for complex custom objects.1  
3. **Routing Block:** A metadata structure that dictates *how* the message should propagate through the Relay network.

The **Routing Block** is the engine of Mercury’s control logic. It contains bitmask flags that filter which Responders are eligible to receive the message 1:

* **Directionality:** Specifies traversal direction (e.g., Children, Parents, Self). This allows a single MmInvoke to cascade down a hierarchy, bubble up to managers, or broadcast locally.1  
* **Active State:** Filters based on the Unity GameObject.activeSelf state (e.g., ActiveOnly vs All). This is crucial for UI management where hidden widgets should not process input.1  
* **Selected State:** Used in conjunction with Switch Nodes to target only the currently "selected" child in a state machine.1  
* **Tags:** Application-specific labels that allow for semantic filtering (e.g., routing a message only to Responders tagged "Physics" or "UI").1

### **3.2 The Relay Node (MmRelayNode)**

The MmRelayNode component maintains the Routing Table. In the Unity Editor, this table allows developers to drag-and-drop other Relay Nodes or Responders to define connections.1 This effectively creates a **Non-Spatial Hierarchy**. While Unity enforces a strict parent-child transform hierarchy, Mercury allows a Relay to be logically connected to any other Relay, regardless of their positions in the scene graph.1  
The core logic resides in the MmInvoke method. When called, MmInvoke iterates through the Routing Table. For each entry, it performs a bitwise comparison between the message’s Routing Block and the entry’s filter settings. If the mask matches, the message is forwarded.1 This centralized routing logic replaces the decentralized conditional logic (if-else chains) typically found in event handlers, reducing code complexity and "cyclomatic complexity" in application logic.1

### **3.3 The Responder (MmResponder)**

The MmResponder acts as the interface between Mercury and the application code. It implements a standard MmInvoke receiver method. A typical Responder contains a switch statement on the MmMethod type, dispatching execution to specific internal functions.1  
A critical innovation in Mercury is the **Switch Node** (MmRelaySwitchNode). This is a specialized Relay that acts as a Finite State Machine (FSM).1 It enforces that only one child node in its Routing Table is "Selected" at any time. This allows developers to manage complex application states (e.g., transitioning from "Tutorial" to "Game" to "ScoreScreen") using the same messaging infrastructure used for simple button clicks. The dissertation notes that this unifies "UI Widgets" and "Application State Management" under a single architectural paradigm.1

### **3.4 Performance Characteristics**

Performance is paramount in RIS. The dissertation provides empirical data comparing Mercury against Unity’s native messaging solutions. In benchmarks consisting of 100,000 invocations, Mercury (0.00122 ms) proved significantly faster than Unity’s SendMessage (0.00771 ms)—approximately **6.3x faster**.1 While slower than a direct C\# function call or event delegate (0.00005 ms), this cost is a linear, fixed overhead for the routing logic.  
The performance advantage over SendMessage stems from Mercury’s avoidance of reflection. SendMessage uses string-based reflection to find methods at runtime, which is computationally expensive and prone to errors (renaming a function breaks the link without compiler warnings). Mercury uses direct references stored in the Routing Table and integer-based type checking, offering type safety and speed closer to direct calls while maintaining decoupling.1  
For extreme performance scenarios, such as the physics synchronization in *Bounce\!*, Mercury introduces the MmQuickNode.1 The MmQuickNode bypasses the full routing logic (Active/Selected/Tag checks) and blindly iterates the Routing Table. This creates a "fast path" for high-frequency updates (e.g., 90Hz transform data) where validation overhead is unacceptable.1

## **4\. Application in Research Systems: Case Studies**

The validity of Mercury is demonstrated through its deployment in diverse, complex research systems. These case studies illustrate how the framework solves specific architectural challenges that defeat traditional patterns.

### **4.1 *NotifiAR*: Modular User Study Management**

*NotifiAR* investigated the efficacy of AR notifications using different visual behaviors (pivoting vs. wall-aligned placards).1 The system required rigorous experimental control, necessitating a separation between the "Experimental Artifact" (the notification) and the "Study Logic" (the timer and logger).  
**The Challenge:** Tightly coupling the study manager to the artifacts would require rewriting the manager for every new condition. If the "Pivoting" notification exposed different events than the "Wall-Aligned" one, the manager would need conditional logic to handle each type.1  
**The Mercury Solution:** Mercury decoupled these layers using a four-component architecture 1:

1. **Task Manager:** Loads trial data (JSON/XML) and iterates through a sequence of tasks. It uses an MmRelaySwitchNode to activate the correct condition for the current trial.1  
2. **Application Manager:** Controls the global state (Instructions, Break, Experiment).  
3. **Widget Manager:** A generalized controller for the experimental artifacts.  
4. **Data Collector:** A logging module that listens for generic "Data" messages.

The crucial innovation was the **Iterating Subtask Responder**.1 In the *Precueing* study (a related project), trials were recursive—a "Task" was composed of 31 "Subtasks" (movement actions). Mercury handled this by allowing Task objects to be recursive. A TaskResponder could sideload subtask completion logic, emitting a "Complete" message only when the entire sequence was finished. This allowed the global Task Manager to remain ignorant of the internal granularity of the trial, simply waiting for a generic "Complete" signal.1

### **4.2 *CURVE*: Urban Data and Heterogeneous Types**

*CURVE* (Collaborative Urban Visualization Environment) allowed users to explore urban datasets (Yelp, Twitter, NYC 311\) in VR.1  
**The Challenge:** The system needed to handle heterogeneous data types. A Yelp record (ratings, photos) is structurally different from a 311 record (timestamp, complaint type). In a strongly typed language like C\#, passing these distinct objects usually requires casting, generics, or separate message channels.  
**The Mercury Solution:** Mercury introduced MmMessageSerializable and the IMmSerializable interface.1 By implementing this interface, any custom class (e.g., TwitterData, YelpData) could be wrapped in a generic MmMessage. The Relay Node transports this wrapper without inspecting the contents. On the receiving end, the Responder unwraps it back to the original type. This allowed the SelectionManager in *CURVE* to trigger a "Load Data" event with a generic payload, which the specific TwitterController or YelpController would interpret correctly.1 This is a form of **polymorphic messaging** that bypasses the need for rigid inheritance hierarchies.

### **4.3 *Bounce\!*: High-Frequency Physics Synchronization**

*Bounce\!* was a collaborative VR game where users manipulated physics-based ropes to bounce a ball.1  
**The Challenge:** The physics simulation required synchronizing the transform of the ball and ropes at 90Hz across the network. Standard Unity networking (NetworkTransform) imposes a 29Hz update limit, which is insufficient for haptic-grade VR interactions.1 Furthermore, standard Mercury routing introduced too much latency for this loop.  
**The Mercury Solution:** This project necessitated the MmQuickNode. By placing MmQuickNode components on the ball and ropes, the system achieved the necessary throughput.1 The system used an authoritative server model: the server simulated the physics and broadcast the transforms via Mercury. The clients received these messages and updated their local visual proxies. The latency was managed to be below 15ms (round trip), which the study found to be the threshold for effective collaboration.1

### **4.4 *Remote Task Assistance*: Asymmetric Topologies**

In this system, a remote expert guided a local technician in assembling physical objects.  
**The Challenge:** The system required **Asymmetric Topology**. When the expert "grabbed" a virtual object, they saw a ghosted replica to aid in placement. The technician, however, should *not* see the ghost, but rather an arrow indicating where to move the real object.1 Hardcoding this ("if user is expert do X, if technician do Y") leads to unmaintainable branching logic.  
**The Mercury Solution:** Mercury handled this via topology configuration. The "Grab" message was sent to a local Relay. On the expert's machine, this Relay was connected to a GhostVisualizer. On the technician's machine, the corresponding Relay was connected to an ArrowVisualizer.1 The code sending the "Grab" message was identical on both; the divergence in behavior was defined entirely by the non-spatial wiring of the Relays. This perfectly illustrates the power of decoupling logic from topology.1

## **5\. Critical Analysis: Limitations of the Paradigm**

Despite its successes, the dissertation material candidly identifies significant limitations that prevent Mercury from being a "perfect" solution. These limitations act as the springboard for the proposed UIST contributions.

### **5.1 The "Invisible Logic" and Mental Model**

The most severe criticism of Mercury is that it obscures the flow of execution. In a standard script, a developer can read lightBulb.TurnOn() and know exactly what will happen. In Mercury, they read Relay.MmInvoke(...). To understand what happens next, they must:

1. Check the Inspector to see what is in the Routing Table.  
2. Check the message's Routing Block filters.  
3. Check the state of the target objects (Active/Selected) at runtime.

This creates a "Gulf of Evaluation".1 The logic is distributed across data files (scenes/prefabs) rather than centralized in code. The dissertation states, "It can be hard to maintain a mental model of all of this... even with a UML diagram, we’d need to represent different types of propagation possibilities".1 This opacity makes debugging difficult; if a message fails to arrive, tracing the point of failure requires manually inspecting every node in the chain.

### **5.2 Graph Cycles (Infinite Loops)**

The flexibility of the Routing Table allows for the creation of circular dependencies (Node A $\\to$ Node B $\\to$ Node A). An MmInvoke in such a cycle triggers an infinite recursion, causing a stack overflow and crashing the application.1 The current implementation lacks built-in prevention mechanisms to avoid performance penalties. The framework relies on developer vigilance, which is brittle in large teams or complex graphs.1

### **5.3 Concurrency and Blocking**

Mercury executes synchronously on the main thread. If a message triggers a cascade of complex operations in 50 different Responders, the main thread blocks until all 50 complete. In VR, where dropping a frame below 11ms induces simulator sickness, this is a critical liability.1 The dissertation notes that "blocking calls anywhere in the R\&R network would stop everything in the program".1 While thread safety is ensured for memory, *execution scheduling* is not handled.

### **5.4 Verbosity**

The syntax for sending messages is verbose. Instantiating new MmRoutingBlock and specifying enums for every call adds friction to the development process, discouraging adoption for rapid prototyping.1 The dissertation suggests that "programmers may choose to opt for easier development... moving towards event-based design when using MRTK as it was now the lowest-hanging fruit".1

## **6\. Proposed Contributions for UIST**

To address these limitations and present a framework worthy of UIST, this report proposes four major technical contributions. These go beyond simple "features" and represent fundamental advancements in the engineering of interactive systems.

### **6.1 Contribution I: Live Visual Authoring and Introspection Environment**

To solve the "Invisible Logic" problem, we propose a **Live Visual Authoring Environment** integrated into the Unity Editor. This tool transforms the abstract R\&R network into a concrete, manipulable node graph.  
Technical Architecture:  
This is not merely a static visualizer but a bi-directional reflection of the runtime state.

* **Graph Visualization:** The tool renders Relay Nodes as nodes and Routing Table connections as edges. The edges are color-coded based on the filter criteria (e.g., Blue for "Children Only", Red for "Active Only").  
* **Live Introspection:** The graph lights up in real-time as messages propagate. When MmInvoke is called, the "path" of the message is highlighted. If a message stops at a node, the tool displays a "Blockage Indicator" explaining *why* (e.g., "Rejected by Active Filter").  
* **Runtime Manipulation:** Developers can drag connections between nodes during gameplay to rewire the application logic on the fly. This enables "live coding" of the interaction topology.

**Evaluation Strategy:** A user study comparing debugging times for a "broken" routing path using the standard Inspector vs. the Visual Environment. We hypothesize a statistically significant reduction in time-to-fix and cognitive load (NASA-TLX).

### **6.2 Contribution II: Static Analysis and Hybrid Safety Verification**

To solve the "Graph Cycles" and type safety issues, we propose a **Hybrid Safety Layer**.  
Static Analysis (Edit-Time):  
We implement an editor-time scanner that builds a directed graph of the scene’s Relay Nodes. It applies Tarjan’s Algorithm for Strongly Connected Components to identify cycles before the application runs. If a cycle is detected, the editor flagging the offending nodes prevents the scene from playing.  
Runtime Loop Prevention (Dynamic):  
For cycles formed dynamically (e.g., by instantiating prefabs), we introduce a low-overhead Bloom Filter mechanism. Each message is assigned a unique MessageID. Relay Nodes maintain a small Bloom filter of recently processed IDs. Before propagating, a node checks the filter. If the ID is present, the message is dropped. This provides $O(1)$ cycle detection with negligible memory footprint, solving the performance concerns raised in the dissertation.1  
**Type Safety Handshake:** To prevent runtime casting errors with MmMessageSerializable, we introduce a capability handshake. Responders use C\# attributes (\`\`) to advertise their capabilities. The Static Analyzer validates that Relays only route messages to Responders capable of handling them.

### **6.3 Contribution III: High-Performance Asynchronous Scheduler**

To address the concurrency blocking issue, we propose replacing the recursive MmInvoke with a **Task-Based Asynchronous Scheduler**.  
Technical Architecture:  
Instead of immediate execution, MmInvoke places the message into a Frame-Budgeted Priority Queue.

* **Time-Slicing:** The Scheduler is allocated a budget (e.g., 2ms per frame). It processes messages until the budget is exhausted, then defers the remainder to the next frame. This guarantees that the messaging system never causes a frame drop.  
* **Async/Await Integration:** Relay Nodes are refactored to support async propagation. If a Responder returns a Task, the Relay awaits it before continuing, allowing for non-blocking IO (e.g., network requests) within the message chain.1  
* **Parallel Routing:** For independent branches of the graph, the Scheduler utilizes Unity’s C\# Job System to process routing logic on worker threads, merging the results back to the main thread for the final Responder execution.

### **6.4 Contribution IV: Language Primitives and Domain-Specific Syntax**

To reduce verbosity, we propose extending C\# via **Operator Overloading** and **Extension Methods** to create a Domain-Specific Language (DSL) for Mercury.  
Proposed Syntax:  
Instead of:  
Relay.MmInvoke(MmMethod.SetActive, true, new MmRoutingBlock(...))  
We introduce a custom operator (e.g., :\>) or fluent syntax:  
Relay :\> SetActive(true).ToChildren().ActiveOnly();  
Variable Arguments:  
We propose implementing a params based message type (MmMessageObjectArray) to allow sending arbitrary lists of arguments without creating custom message classes.1 This brings the ease of use of standard function calls to the decoupled world of R\&R.

### **Table 1: Summary of Proposed Contributions**

| Contribution Domain | Current Limitation | Proposed Novel Contribution | UIST Relevance |
| :---- | :---- | :---- | :---- |
| **Developer Experience** | Mental model is difficult to maintain; connections are invisible in code. | **Live Visual Authoring & Introspection Environment** (Bi-directional graph editor). | Novel Tool / Interface for Programming |
| **System Robustness** | Risk of infinite loops; manual cycle detection; type safety risks. | **Static Analysis & Runtime Safety Layer** (Tarjan's algo, Bloom filters). | Engineering Reliability / Safety |
| **Performance** | Synchronous execution blocks main thread; impacts VR framerate. | **High-Performance Async Scheduler** (Time-slicing, Task-based parallelism). | System Performance / Latency |
| **Language Design** | Verbose syntax increases friction for rapid prototyping. | **DSL and Language Primitives** (Fluent syntax, operator overloading). | Toolkits / API Design |

## **7\. Conclusion**

The Mercury Messaging Framework represents a significant step forward in the architecture of Real-Time Interactive Systems. By enforcing the separation of concerns through the Relay & Responder pattern, it solves the endemic problems of coupling and rigidity that plague XR development. The successful deployment of Mercury in systems ranging from urban data visualization (*CURVE*) to physics-based collaboration (*Bounce\!*) and psychophysical user studies (*Precueing*) validates its core utility.  
However, true impact requires addressing the friction points of cognition, safety, and performance. The proposed contributions—Visual Authoring, Hybrid Safety Verification, Asynchronous Scheduling, and Language Integration—specifically target these bottlenecks. Implementing these advancements will evolve Mercury from a useful library into a foundational contribution to the field of Human-Computer Interaction engineering, bridging the gap between high-level architectural theory and the gritty reality of real-time system development.

#### **Works cited**

1. mercury.pdf