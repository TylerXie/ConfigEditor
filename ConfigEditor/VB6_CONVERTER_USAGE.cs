// Example usage of VB6ToCSharpConverter
// This file demonstrates how to use the converter in your code

using System;
using System.IO;

namespace ConfigEditor
{
    /// <summary>
    /// Example: Converting a VB6 file to C# class
    /// </summary>
    public class VB6ConverterExample
    {
        public static void ConvertVB6FileToCSharp()
        {
            // Create an instance of the converter
            var converter = new VB6ToCSharpConverter();

            // Example 1: Convert a VB6 file to C# in the same directory
            try
            {
                string vb6FilePath = "C:\\path\\to\\Module1.bas";
                string csharpFilePath = converter.ConvertFile(vb6FilePath);
                Console.WriteLine($"Converted: {vb6FilePath} -> {csharpFilePath}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Example 2: Convert a VB6 file to C# in a specific output directory
            try
            {
                string vb6FilePath = "C:\\path\\to\\Module2.bas";
                string outputDirectory = "C:\\output\\csharp";
                string csharpFilePath = converter.ConvertFile(vb6FilePath, outputDirectory);
                Console.WriteLine($"Converted: {vb6FilePath} -> {csharpFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

// ============================================================================
// FEATURES OF THE VB6 TO C# CONVERTER:
// ============================================================================
//
// 1. CLASS GENERATION
//    - Creates a new C# class with the same name as the VB6 file
//    - Adds appropriate using statements and namespace
//
// 2. METHOD CONVERSION
//    - Converts VB6 Sub procedures to C# void methods
//    - Converts VB6 Function procedures to C# methods with return types
//    - Maintains method parameters and modifiers (ByVal, ByRef -> ref)
//
// 3. TYPE CONVERSION
//    - Converts VB6 data types to C# equivalents:
//      Integer → int
//      Long → long
//      Single → float
//      Double → double
//      String → string
//      Boolean → bool
//      Date → DateTime
//      Byte → byte
//      Currency → decimal
//      Variant → object
//      Object → object
//
// 4. SYNTAX CONVERSION
//    - Converts comments: ' → //
//    - Converts string concatenation: & → +
//    - Converts MsgBox → MessageBox.Show
//    - Adds semicolons to statements
//
// 5. PARAMETER HANDLING
//    - Converts "ByVal name As Type" → "type name"
//    - Converts "ByRef name As Type" → "ref type name"
//    - Supports multiple parameters
//    - Handles complex type declarations
//
// ============================================================================
// EXAMPLE VB6 CODE AND CONVERSION:
// ============================================================================
//
// VB6 INPUT:
// ============================================================================
// Function Add(ByVal x As Integer, ByVal y As Integer) As Integer
//     Add = x + y
// End Function
//
// Sub ShowMessage(ByVal msg As String)
//     MsgBox msg
// End Sub
//
// ============================================================================
// C# OUTPUT:
// ============================================================================
// using System;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace ConfigEditor
// {
//     public class Module1
//     {
//         public int Add(int x, int y)
//         {
//             Add = x + y;
//         }
//
//         public void ShowMessage(string msg)
//         {
//             MessageBox.Show(msg);
//         }
//     }
// }
//
// ============================================================================
// USAGE IN COMMAND LINE:
// ============================================================================
//
// To create a command-line tool, use this code:
//
// class Program
// {
//     static void Main(string[] args)
//     {
//         if (args.Length == 0)
//         {
//             Console.WriteLine("Usage: VB6ToCSharp.exe <vb6_file_path> [output_directory]");
//             return;
//         }
//
//         var converter = new VB6ToCSharpConverter();
//         try
//         {
//             string inputFile = args[0];
//             string outputDir = args.Length > 1 ? args[1] : null;
//             string outputFile = converter.ConvertFile(inputFile, outputDir);
//             Console.WriteLine($"✓ Conversion successful: {outputFile}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"✗ Error: {ex.Message}");
//         }
//     }
// }
