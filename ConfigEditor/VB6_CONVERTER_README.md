# VB6 to C# File Converter

## Overview

The `VB6ToCSharpConverter` is a utility class that converts VB6 source files (.bas, .cls) to C# class files (.cs). It automatically converts VB6 procedures (Sub and Function) to C# methods and handles type conversions.

## Features

### ✅ Supported Conversions

1. **Class Generation**
   - Creates a C# class with the same name as the input VB6 file
   - Generates proper namespace (ConfigEditor)
   - Includes standard using statements

2. **Procedure Conversion**
   - `Sub` procedures → `public void` methods
   - `Function` procedures → `public <ReturnType>` methods
   - Maintains method accessibility (all converted as public)

3. **Type Mapping**
   - VB6 types are automatically converted to C# equivalents
   - Supports primitive types: Integer, String, Boolean, Double, Single, etc.
   - Supports special types: Date (→ DateTime), Currency (→ decimal)
   - Unknown types are preserved as-is

4. **Parameter Handling**
   - `ByVal` parameters → value parameters
   - `ByRef` parameters → `ref` parameters
   - Multiple parameters supported
   - Type annotations properly converted

5. **Code Conversions**
   - Single-line comments: `'` → `//`
   - String concatenation: `&` → `+`
   - Message boxes: `MsgBox` → `MessageBox.Show`
   - Automatic semicolon insertion

### ❌ Limitations & Manual Adjustments Required

The following require manual review and conversion:

1. **Control Structures**
   - `If...Then...Else` → `if...else`
   - `For...Next` → `for` loops
   - `Do...Loop` → `while` or `do...while` loops
   - `Select...Case` → `switch` statements

2. **Built-in Functions**
   - String functions: `InStr`, `Mid`, `Left`, `Right`, etc.
   - Type conversion: `CInt`, `CStr`, `CDbl`, etc.
   - Array operations: `UBound`, `LBound`, etc.

3. **Variable Declarations**
   - Global and module-level variables
   - `Dim`, `Private`, `Public` modifiers
   - Array declarations

4. **Advanced Features**
   - Class/form definitions
   - Event handlers
   - Properties and Get/Set accessors
   - Inheritance and interfaces
   - Error handling (`On Error` → `try...catch`)

5. **Specific VB6 Features**
   - Late binding and variant types
   - Collection and dictionary operations
   - File I/O operations (may differ)
   - Registry operations

## Usage

### Basic Usage

```csharp
using ConfigEditor;

// Create converter instance
var converter = new VB6ToCSharpConverter();

// Convert a file
string inputFile = "C:\\path\\to\\Module1.bas";
string outputFile = converter.ConvertFile(inputFile);

// Output: C:\path\to\Module1.cs
Console.WriteLine($"Converted: {outputFile}");
```

### Convert to Specific Directory

```csharp
string inputFile = "C:\\vb6\\Module1.bas";
string outputDirectory = "C:\\csharp\\output";
string outputFile = converter.ConvertFile(inputFile, outputDirectory);

// Output: C:\csharp\output\Module1.cs
```

## Example Conversion

### VB6 Source (Module1.bas)

```vb
Sub GreetUser(ByVal name As String)
    MsgBox "Hello, " & name
End Sub

Function Add(ByVal a As Integer, ByVal b As Integer) As Integer
    Add = a + b
End Function

Function IsValid(ByRef value As Integer) As Boolean
    IsValid = (value > 0)
End Function

' Calculate the average
Function CalculateAverage(ByVal x As Double, ByVal y As Double) As Double
    CalculateAverage = (x + y) / 2
End Function
```

### Generated C# (Module1.cs)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigEditor
{
    public class Module1
    {
        public void GreetUser(string name)
        {
            MessageBox.Show("Hello, " + name);
        }

        public int Add(int a, int b)
        {
            Add = a + b;
        }

        public bool IsValid(ref int value)
        {
            IsValid = (value > 0);
        }

        // Calculate the average
        public double CalculateAverage(double x, double y)
        {
            CalculateAverage = (x + y) / 2;
        }
    }
}
```

## Type Mapping Reference

| VB6 Type | C# Type | Notes |
|----------|---------|-------|
| Integer | int | 32-bit signed integer |
| Long | long | 64-bit signed integer |
| Single | float | Single-precision floating point |
| Double | double | Double-precision floating point |
| String | string | Text string |
| Boolean | bool | True/False value |
| Byte | byte | 8-bit unsigned integer |
| Currency | decimal | Fixed-point decimal |
| Date | DateTime | Date/time value |
| Variant | object | Any type |
| Object | object | Any object reference |
| Type() | Type[] | Array of Type |

## Advanced Features

### Private/Public Visibility

All converted methods are marked as `public`. Modify visibility as needed:

```csharp
// Change to private if needed
private void GreetUser(string name) { ... }

// Change to protected for inheritance
protected void GreetUser(string name) { ... }
```

### Optional Parameters

VB6 optional parameters need manual adjustment:

```csharp
// VB6:
// Sub DoSomething(Optional ByVal value As Integer = 0)

// C# equivalent:
public void DoSomething(int value = 0) { ... }
```

## Post-Conversion Checklist

After converting, review and adjust:

- [ ] Variable declarations (add `var`, proper types)
- [ ] Control structures (if/else, loops, switch)
- [ ] Error handling (add try/catch blocks)
- [ ] Built-in function calls (use C# equivalents)
- [ ] Array operations (use C# syntax)
- [ ] Property implementations (auto-properties or get/set)
- [ ] Namespace references (adjust as needed)
- [ ] Test for runtime errors

## API Reference

### VB6ToCSharpConverter.ConvertFile()

```csharp
public string ConvertFile(string vb6FilePath, string outputDirectory = null)
```

**Parameters:**
- `vb6FilePath`: Full path to the VB6 file to convert
- `outputDirectory`: (Optional) Directory for the output C# file. If null, uses same directory as input.

**Returns:**
- Full path to the generated C# file

**Throws:**
- `FileNotFoundException`: If the VB6 file doesn't exist
- `Exception`: If conversion fails

## Example: Batch Conversion

```csharp
using System;
using System.IO;

var converter = new VB6ToCSharpConverter();
string vb6Directory = "C:\\vb6\\source";
string csDirectory = "C:\\csharp\\output";

// Convert all .bas files in directory
foreach (string file in Directory.GetFiles(vb6Directory, "*.bas"))
{
    try
    {
        string outputFile = converter.ConvertFile(file, csDirectory);
        Console.WriteLine($"✓ {Path.GetFileName(file)} -> {Path.GetFileName(outputFile)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ {Path.GetFileName(file)}: {ex.Message}");
    }
}
```

## Best Practices

1. **Review Output**: Always review the generated C# code
2. **Test Thoroughly**: Test converted code extensively
3. **Keep Originals**: Keep VB6 source files as reference
4. **Use Version Control**: Track changes with Git or similar
5. **Incremental Conversion**: Convert and test one file at a time
6. **Manual Adjustments**: Plan time for manual refinement

## Troubleshooting

### Issue: Generated file is empty or incomplete

**Solution:** Check that the VB6 file has proper Sub/Function declarations with matching End statements.

### Issue: Types not converted correctly

**Solution:** Review the type mapping table above and manually adjust if needed.

### Issue: Parameters missing or incorrect

**Solution:** Ensure VB6 parameters use "As Type" syntax for proper conversion.

### Issue: Build errors after conversion

**Solution:** This is expected. Review and adjust:
- Variable declarations
- Method calls (syntax differences)
- Namespaces and using statements
- Type compatibility

## Performance Notes

- Conversion is fast for typical modules (< 1000 lines)
- Regex-based parsing is suitable for straightforward code
- Complex VB6 code may require more manual adjustments

## Future Enhancements

Potential improvements:
- GUI tool for batch conversion
- Better error reporting
- Support for VB.NET syntax
- Integration with Visual Studio
- Support for class inheritance
- Async/await conversion
- LINQ conversion assistance

## License & Support

This converter is part of the ConfigEditor project. For issues or questions, refer to the project documentation or create an issue in the repository.
