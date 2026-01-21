# MercuryMessaging Source Generators

Phase 4 Performance Optimization: Source generators for compile-time dispatch optimization.

## Overview

The `MercuryMessaging.Generators` project contains Roslyn source generators that analyze responder classes marked with `[MmGenerateDispatch]` and generate optimized `MmInvoke` implementations at compile time.

### Performance Impact

- **Before**: Virtual dispatch through base class (~8-10 ticks per message)
- **After**: Direct switch dispatch (~2-4 ticks per message)
- **Improvement**: ~2-4x faster message dispatch
- **Custom Methods**: ~300-500ns (dictionary) → ~100-150ns (generated switch)

## Building the Generator

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### Build Steps

```bash
# Navigate to generator project
cd SourceGenerators/MercuryMessaging.Generators

# Restore dependencies
dotnet restore

# Build the generator
dotnet build -c Release

# Output: bin/Release/netstandard2.0/MercuryMessaging.Generators.dll
```

## Unity Integration

### Option 1: Manual DLL Import (Recommended for Development)

1. Build the generator DLL:
   ```bash
   dotnet build -c Release
   ```

2. Copy the DLL to Unity:
   ```
   SourceGenerators/MercuryMessaging.Generators/bin/Release/netstandard2.0/MercuryMessaging.Generators.dll
   → Assets/MercuryMessaging/Protocol/Analyzers/MercuryMessaging.Generators.dll
   ```

3. In Unity, select the DLL and set the **Asset Labels** to include `RoslynAnalyzer`:
   - Select the DLL in Project window
   - In Inspector, click the label icon (bottom of Inspector)
   - Add label: `RoslynAnalyzer`

4. Unity will automatically run the generator on compilation.

### Option 2: NuGet Package

1. Build as NuGet package:
   ```bash
   dotnet pack -c Release
   ```

2. Reference the package in your Unity project's manifest or via NuGet.

## Usage

### Basic Usage with ReceivedMessage Overrides

Mark your responder class with `[MmGenerateDispatch]`:

```csharp
using MercuryMessaging;

[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageInt msg)
    {
        Debug.Log($"Received int: {msg.value}");
    }

    protected override void ReceivedMessage(MmMessageString msg)
    {
        Debug.Log($"Received string: {msg.value}");
    }
}
```

**Important**: The class must be declared `partial` for the generator to extend it.

### Custom Method Handlers with [MmHandler] (NEW)

Use the `[MmHandler]` attribute to define handlers for custom method IDs (>= 1000):

```csharp
using MercuryMessaging;

[MmGenerateDispatch]
public partial class MyCustomResponder : MmBaseResponder
{
    // Handle custom method ID 1000
    [MmHandler(1000)]
    private void OnCustomColor(MmMessage msg)
    {
        var colorMsg = (ColorMessage)msg;
        GetComponent<Renderer>().material.color = colorMsg.color;
    }

    // Handle custom method ID 1001 with a descriptive name
    [MmHandler(1001, Name = "ScaleHandler")]
    private void OnCustomScale(MmMessage msg)
    {
        var scaleMsg = (ScaleMessage)msg;
        transform.localScale = scaleMsg.scale;
    }
}
```

**Benefits over MmExtendableResponder:**
- **Compile-time dispatch**: ~100-150ns vs ~300-500ns (dictionary lookup)
- **No Awake() boilerplate**: No need to call `RegisterCustomHandler()` at runtime
- **Static validation**: Generator reports errors for invalid method IDs
- **IDE-friendly**: Full IntelliSense support

**[MmHandler] Attribute Properties:**
- `methodId` (required): Custom method ID (must be >= 1000)
- `Name` (optional): Descriptive name for debugging and generated code comments

### Mixed Usage

Combine standard message handlers with custom method handlers:

```csharp
[MmGenerateDispatch]
public partial class MixedResponder : MmBaseResponder
{
    // Standard typed message handler
    protected override void ReceivedMessage(MmMessageInt msg)
    {
        Debug.Log($"Standard int: {msg.value}");
    }

    // Custom method handler
    [MmHandler(1000)]
    private void OnCustomAction(MmMessage msg)
    {
        Debug.Log("Custom action triggered");
    }
}
```

### Generated Code

The generator creates a partial class file (`MyResponder.g.cs`) with an optimized `MmInvoke`:

```csharp
public partial class MyCustomResponder
{
    public override void MmInvoke(MmMessage message)
    {
        // Type-based dispatch for ReceivedMessage handlers
        switch (message.MmMessageType)
        {
            case MmMessageType.MmInt:
                ReceivedMessage((MmMessageInt)message);
                return;
        }

        // Custom method dispatch for [MmHandler] methods
        var methodId = (int)message.MmMethod;
        if (methodId >= 1000)
        {
            switch (methodId)
            {
                case 1000: // OnCustomColor
                    OnCustomColor(message);
                    return;
                case 1001: // ScaleHandler
                    OnCustomScale(message);
                    return;
            }
        }

        base.MmInvoke(message);
    }
}
```

### Attribute Options

```csharp
[MmGenerateDispatch(
    IncludeStandardMethods = true,  // Generate dispatch for ReceivedSetActive, etc.
    EnableDebugLogging = false,     // Add debug logging (disable in production)
    GenerateNullChecks = true       // Generate null checks for safety
)]
public partial class MyResponder : MmBaseResponder
{
    // ...
}
```

## Supported Handlers

### Typed Message Handlers

The generator recognizes overrides of `ReceivedMessage` with these parameter types:

| Parameter Type | MmMessageType |
|---------------|---------------|
| MmMessage | MmVoid |
| MmMessageBool | MmBool |
| MmMessageInt | MmInt |
| MmMessageFloat | MmFloat |
| MmMessageString | MmString |
| MmMessageVector3 | MmVector3 |
| MmMessageVector4 | MmVector4 |
| MmMessageQuaternion | MmQuaternion |
| MmMessageTransform | MmTransform |
| MmMessageTransformList | MmTransformList |
| MmMessageByteArray | MmByteArray |
| MmMessageSerializable | MmSerializable |
| MmMessageGameObject | MmGameObject |

### Standard Method Handlers

When `IncludeStandardMethods = true`, the generator also recognizes:

- `ReceivedSetActive()` → `MmMethod.SetActive`
- `ReceivedInitialize()` → `MmMethod.Initialize`
- `ReceivedRefresh()` → `MmMethod.Refresh`
- `ReceivedSwitch(int)` → `MmMethod.Switch`
- `ReceivedComplete()` → `MmMethod.Complete`

### Custom Method Handlers ([MmHandler])

Methods marked with `[MmHandler(methodId)]` where `methodId >= 1000`:

- Method must accept exactly one parameter of type `MmMessage` (or derived type)
- Method can be private, protected, or public
- Multiple handlers can coexist with unique method IDs

## Generator Diagnostics

The generator reports errors for common mistakes:

| ID | Severity | Description |
|----|----------|-------------|
| MMG001 | Error | MmHandler method ID is invalid (must be >= 1000) |
| MMG002 | Error | Duplicate MmHandler method ID |
| MMG003 | Error | Invalid MmHandler method signature |
| MMG004 | Warning | [MmHandler] without [MmGenerateDispatch] on class |

## Troubleshooting

### Generator Not Running

1. Ensure DLL has `RoslynAnalyzer` label in Unity
2. Check Unity Console for compilation errors
3. Ensure class is declared `partial`
4. Restart Unity IDE

### No Generated Code

1. Verify at least one `ReceivedMessage` override OR `[MmHandler]` method exists
2. Check parameter types match supported types
3. View generated files: `Library/Bee/artifacts/`

### Compilation Errors in Generated Code

1. Ensure namespace declarations are correct
2. Verify MercuryMessaging assembly is referenced
3. Check for conflicting partial class definitions

### MMG001: Invalid Method ID

Custom methods must use IDs >= 1000. Values 0-999 are reserved for framework methods.

```csharp
// Wrong
[MmHandler(500)]  // Error: Must be >= 1000
private void OnHandler(MmMessage msg) { }

// Correct
[MmHandler(1000)]
private void OnHandler(MmMessage msg) { }
```

### MMG002: Duplicate Method ID

Each custom method ID can only have one handler per class.

```csharp
// Wrong
[MmHandler(1000)]
private void OnFirst(MmMessage msg) { }
[MmHandler(1000)]  // Error: Duplicate
private void OnSecond(MmMessage msg) { }

// Correct
[MmHandler(1000)]
private void OnFirst(MmMessage msg) { }
[MmHandler(1001)]
private void OnSecond(MmMessage msg) { }
```

### MMG003: Invalid Signature

Handler methods must accept exactly one MmMessage parameter.

```csharp
// Wrong
[MmHandler(1000)]
private void OnHandler() { }  // Error: No parameter
[MmHandler(1001)]
private void OnHandler(string s) { }  // Error: Wrong type

// Correct
[MmHandler(1000)]
private void OnHandler(MmMessage msg) { }
```

## Files

```
SourceGenerators/
├── README.md                          # This file
└── MercuryMessaging.Generators/
    ├── MercuryMessaging.Generators.csproj
    ├── MmDispatchGenerator.cs         # Main generator implementation
    ├── AnalyzerReleases.Shipped.md    # Shipped diagnostics
    └── AnalyzerReleases.Unshipped.md  # Unshipped diagnostics
```

## Development

To debug the generator:

1. Add to `.csproj`:
   ```xml
   <PropertyGroup>
     <IsRoslynComponent>true</IsRoslynComponent>
     <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
   </PropertyGroup>
   ```

2. View generated files in `obj/Debug/netstandard2.0/generated/`

## Migration from MmExtendableResponder

If you're using `MmExtendableResponder` with `RegisterCustomHandler()`:

**Before (runtime registration):**
```csharp
public class MyResponder : MmExtendableResponder
{
    protected override void Awake()
    {
        base.Awake();
        RegisterCustomHandler((MmMethod)1000, OnCustomColor);
        RegisterCustomHandler((MmMethod)1001, OnCustomScale);
    }

    private void OnCustomColor(MmMessage msg) { /* ... */ }
    private void OnCustomScale(MmMessage msg) { /* ... */ }
}
```

**After (compile-time generation):**
```csharp
[MmGenerateDispatch]
public partial class MyResponder : MmBaseResponder  // Note: MmBaseResponder, not MmExtendableResponder
{
    [MmHandler(1000)]
    private void OnCustomColor(MmMessage msg) { /* ... */ }

    [MmHandler(1001)]
    private void OnCustomScale(MmMessage msg) { /* ... */ }
}
```

**Benefits:**
- No Awake() boilerplate
- ~2-3x faster custom method dispatch
- Compile-time error detection
- Smaller memory footprint (no dictionary)

## License

Same as MercuryMessaging framework (BSD 3-Clause).
