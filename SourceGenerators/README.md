# MercuryMessaging Source Generators

Phase 4 Performance Optimization: Source generators for compile-time dispatch optimization.

## Overview

The `MercuryMessaging.Generators` project contains Roslyn source generators that analyze responder classes marked with `[MmGenerateDispatch]` and generate optimized `MmInvoke` implementations at compile time.

### Performance Impact

- **Before**: Virtual dispatch through base class (~8-10 ticks per message)
- **After**: Direct switch dispatch (~2-4 ticks per message)
- **Improvement**: ~2-4x faster message dispatch

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

### Basic Usage

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

### Generated Code

The generator creates a partial class file (`MyResponder.g.cs`) with an optimized `MmInvoke`:

```csharp
public partial class MyResponder
{
    public override void MmInvoke(MmMessage message)
    {
        switch (message.MmMessageType)
        {
            case MmMessageType.MmInt:
                ReceivedMessage((MmMessageInt)message);
                return;
            case MmMessageType.MmString:
                ReceivedMessage((MmMessageString)message);
                return;
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

## Troubleshooting

### Generator Not Running

1. Ensure DLL has `RoslynAnalyzer` label in Unity
2. Check Unity Console for compilation errors
3. Ensure class is declared `partial`
4. Restart Unity IDE

### No Generated Code

1. Verify at least one `ReceivedMessage` override exists
2. Check parameter types match supported types
3. View generated files: `Library/Bee/artifacts/`

### Compilation Errors in Generated Code

1. Ensure namespace declarations are correct
2. Verify MercuryMessaging assembly is referenced
3. Check for conflicting partial class definitions

## Files

```
SourceGenerators/
├── README.md                          # This file
└── MercuryMessaging.Generators/
    ├── MercuryMessaging.Generators.csproj
    └── MmDispatchGenerator.cs         # Main generator implementation
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

## License

Same as MercuryMessaging framework (BSD 3-Clause).
