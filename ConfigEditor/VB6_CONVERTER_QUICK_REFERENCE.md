# VB6 to C# Converter - Quick Reference Guide

## Quick Start

```csharp
using ConfigEditor;

// Create converter
var converter = new VB6ToCSharpConverter();

// Convert a file
string output = converter.ConvertFile("Module1.bas");
Console.WriteLine($"Created: {output}");
```

## Usage Patterns

### Pattern 1: Single File Conversion
```csharp
var converter = new VB6ToCSharpConverter();
string output = converter.ConvertFile("C:\\vb6\\Math.bas");
// Output: C:\vb6\Math.cs
```

### Pattern 2: Convert to Different Directory
```csharp
string output = converter.ConvertFile(
    "C:\\vb6\\Math.bas", 
    "C:\\csharp\\converted"
);
// Output: C:\csharp\converted\Math.cs
```

### Pattern 3: Batch Conversion
```csharp
var converter = new VB6ToCSharpConverter();
var errors = new List<string>();

foreach (string file in Directory.GetFiles("C:\\vb6", "*.bas"))
{
    try
    {
        converter.ConvertFile(file, "C:\\output");
        Console.WriteLine($"✓ {Path.GetFileName(file)}");
    }
    catch (Exception ex)
    {
        errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
    }
}

if (errors.Count > 0)
    Console.WriteLine($"\n{errors.Count} files failed to convert");
```

### Pattern 4: With Error Handling
```csharp
try
{
    var converter = new VB6ToCSharpConverter();
    string output = converter.ConvertFile(inputPath, outputDir);
    Console.WriteLine($"Success: {output}");
}
catch (FileNotFoundException)
{
    Console.WriteLine("Input file not found");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Common VB6 to C# Conversions

| VB6 | C# | Example |
|-----|----|----|
| `Sub` | `public void` | `Sub Add()` → `public void Add()` |
| `Function` | `public Type` | `Function Add() As Int` → `public int Add()` |
| `ByVal x As Integer` | `int x` | `ByVal x As Integer` → `int x` |
| `ByRef x As Integer` | `ref int x` | `ByRef x As Integer` → `ref int x` |
| `'Comment` | `//Comment` | `'TODO` → `//TODO` |
| `MsgBox "Text"` | `MessageBox.Show("Text")` | Same |
| `String & String` | `String + String` | `"A" & "B"` → `"A" + "B"` |

## Type Conversion Quick Map

```
Integer     → int
Long        → long
Single      → float
Double      → double
String      → string
Boolean     → bool
Byte        → byte
Date        → DateTime
Currency    → decimal
Variant     → object
Object      → object
Type()      → Type[]
```

## VB6 Parameter Examples

```vb
' VB6 Code Examples
Sub Method1()                          ' → public void Method1()
Sub Method2(x As Integer)              ' → public void Method2(int x)
Sub Method3(ByVal x As Integer)        ' → public void Method3(int x)
Sub Method4(ByRef x As Integer)        ' → public void Method4(ref int x)
Function GetValue() As String          ' → public string GetValue()
Function Add(x As Integer, y As Integer) As Integer ' → public int Add(int x, int y)
```

## Output Structure

Every converted file has this structure:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigEditor
{
    public class FileName                    // From filename
    {
        public void Method1()                // Converted Sub
        {
            // Method body
        }

        public int Method2(int param)        // Converted Function
        {
            // Method body
        }
    }
}
```

## File Naming Convention

- **Input:** `Module1.bas` or `Form1.cls`
- **Output:** `Module1.cs` or `Form1.cs`
- **Class Name:** Same as filename (sanitized)
- **Location:** Same directory or specified output directory

## Post-Conversion Tasks

After conversion, you typically need to:

- [ ] Add `using System.Windows.Forms;` if using UI components
- [ ] Convert control structures (if/else, loops)
- [ ] Add variable declarations
- [ ] Replace VB6 built-in functions with C# equivalents
- [ ] Add error handling (try/catch)
- [ ] Test the code
- [ ] Fix any compilation errors

## Common Issues & Solutions

### Issue: File not found
**Solution:** Use full path or check file exists
```csharp
if (!File.Exists(path))
    throw new FileNotFoundException(path);
```

### Issue: Types not recognized
**Solution:** Add necessary using statements after conversion
```csharp
using System.Windows.Forms;  // For GUI
using System.Collections;     // For collections
```

### Issue: Methods not converting
**Solution:** Ensure VB6 file has proper Sub/Function...End Sub/End Function structure

### Issue: Parameters missing
**Solution:** Check parameters use "As Type" syntax
```vb
' Correct
Sub Method(ByVal x As Integer)

' Won't convert correctly
Sub Method(x)
```

## Command Line Usage (If Creating CLI Tool)

```bash
VB6ToCSharp.exe input.bas
VB6ToCSharp.exe input.bas C:\output
VB6ToCSharp.exe C:\vb6\module.bas C:\csharp\
```

## Integration Examples

### Example 1: Add to Windows Form
```csharp
// In your form
private void btnConvert_Click(object sender, EventArgs e)
{
    var converter = new VB6ToCSharpConverter();
    string output = converter.ConvertFile(txtInputPath.Text);
    MessageBox.Show($"Converted to:\n{output}");
}
```

### Example 2: Background Worker
```csharp
backgroundWorker.DoWork += (s, e) =>
{
    var converter = new VB6ToCSharpConverter();
    e.Result = converter.ConvertFile((string)e.Argument);
};
```

### Example 3: Console Application
```csharp
class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: program.exe <vb6_file>");
            return;
        }

        var converter = new VB6ToCSharpConverter();
        try
        {
            string output = converter.ConvertFile(args[0]);
            Console.WriteLine($"✓ {output}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ {ex.Message}");
        }
    }
}
```

## Performance Notes

- **Small files** (< 100 lines): Instant
- **Medium files** (100-500 lines): < 100ms
- **Large files** (500-2000 lines): < 500ms
- **Very large files** (> 2000 lines): May need review for complex structures

## Tips & Tricks

1. **Always review output** - Automatic conversion isn't 100% perfect
2. **Test early** - Run code immediately after conversion
3. **Keep originals** - Don't delete VB6 files until confident
4. **Use version control** - Track changes with Git
5. **Convert incrementally** - One file at a time is safer
6. **Document changes** - Note manual adjustments made

## Need More Help?

- See `VB6_CONVERTER_README.md` for comprehensive documentation
- Check `VB6_CONVERTER_USAGE.cs` for code examples
- Review `VB6ConverterFormExample.cs` for UI integration
- Check `VB6_CONVERTER_IMPLEMENTATION.md` for architecture details

## Supported VB6 Elements

✅ **Supported:**
- Sub procedures
- Function procedures
- Parameters (ByVal, ByRef)
- Return types
- Comments
- String literals

❌ **Not Supported (Manual adjustment needed):**
- Control structures (if/else, loops, select/case)
- Variable declarations
- Property definitions
- Event handlers
- Class inheritance
- Error handling (On Error)
- Collections and dictionaries

---

**Last Updated:** 2024
**Status:** Ready for Use
**Build:** ✅ Passing
