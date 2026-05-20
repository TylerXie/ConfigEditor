using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ConfigEditor
{
    /// <summary>
    /// Converts VB6 files to C# classes.
    /// Converts Sub and Function declarations to C# methods.
    /// </summary>
    public class VB6ToCSharpConverter
    {
        /// <summary>
        /// Converts a VB6 file to a C# class file.
        /// </summary>
        /// <param name="vb6FilePath">Path to the VB6 file</param>
        /// <param name="outputDirectory">Directory where the C# file will be created</param>
        /// <returns>Path to the generated C# file</returns>
        public string ConvertFile(string vb6FilePath, string outputDirectory = null)
        {
            if (!File.Exists(vb6FilePath))
                throw new FileNotFoundException($"VB6 file not found: {vb6FilePath}");

            // Read VB6 file
            string vb6Content = File.ReadAllText(vb6FilePath, Encoding.UTF8);

            // Generate C# content
            string csharpContent = ConvertContent(vb6Content, Path.GetFileNameWithoutExtension(vb6FilePath));

            // Determine output path
            if (outputDirectory == null)
            {
                outputDirectory = Path.GetDirectoryName(vb6FilePath);
            }

            string fileName = Path.GetFileNameWithoutExtension(vb6FilePath);
            string outputFilePath = Path.Combine(outputDirectory, fileName + ".cs");

            // Write C# file
            File.WriteAllText(outputFilePath, csharpContent, Encoding.UTF8);

            return outputFilePath;
        }

        /// <summary>
        /// Converts VB6 content to C# content.
        /// </summary>
        private string ConvertContent(string vb6Content, string className)
        {
            var sb = new StringBuilder();

            // Add using statements
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine();

            // Add namespace and class declaration
            sb.AppendLine("namespace ConfigEditor");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {SanitizeClassName(className)}");
            sb.AppendLine("    {");

            // Convert methods
            string methodsContent = ConvertMethods(vb6Content);
            sb.Append(methodsContent);

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>
        /// Converts VB6 Sub and Function declarations to C# methods.
        /// </summary>
        private string ConvertMethods(string vb6Content)
        {
            var sb = new StringBuilder();

            // Split by lines for processing
            string[] lines = vb6Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int i = 0;
            while (i < lines.Length)
            {
                string line = lines[i];
                string trimmedLine = line.Trim();

                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("'"))
                {
                    i++;
                    continue;
                }

                // Check for Sub or Function declaration
                if (trimmedLine.StartsWith("Sub ", StringComparison.OrdinalIgnoreCase) ||
                    trimmedLine.StartsWith("Function ", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract method and its body
                    var methodLines = new List<string>();
                    methodLines.Add(line);

                    i++;
                    while (i < lines.Length)
                    {
                        string bodyLine = lines[i];
                        string trimmedBodyLine = bodyLine.Trim();

                        if (trimmedBodyLine.StartsWith("End Sub", StringComparison.OrdinalIgnoreCase) ||
                            trimmedBodyLine.StartsWith("End Function", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        methodLines.Add(bodyLine);
                        i++;
                    }

                    // Convert the method
                    string convertedMethod = ConvertMethod(methodLines);
                    sb.Append(convertedMethod);
                }

                i++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a single VB6 method to C# method.
        /// </summary>
        private string ConvertMethod(List<string> methodLines)
        {
            if (methodLines.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            string signature = methodLines[0].Trim();

            // Parse method signature
            bool isFunction = signature.StartsWith("Function", StringComparison.OrdinalIgnoreCase);
            string returnType = isFunction ? "object" : "void";

            // Extract method name and parameters
            string methodDeclaration = ExtractMethodDeclaration(signature, ref returnType);

            // Add method
            sb.AppendLine($"        public {returnType} {methodDeclaration}");
            sb.AppendLine("        {");

            // Convert method body
            for (int i = 1; i < methodLines.Count; i++)
            {
                string line = methodLines[i];
                string convertedLine = ConvertLine(line);
                sb.AppendLine(convertedLine);
            }

            sb.AppendLine("        }");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Extracts method declaration (name and parameters) from VB6 signature.
        /// </summary>
        private string ExtractMethodDeclaration(string signature, ref string returnType)
        {
            // Remove Sub/Function keyword
            string declaration = Regex.Replace(signature, @"^(Sub|Function)\s+", "", RegexOptions.IgnoreCase).Trim();

            // Extract return type if Function (e.g., "Function Foo() As String")
            Match asMatch = Regex.Match(declaration, @"\s+As\s+(\w+)\s*$", RegexOptions.IgnoreCase);
            if (asMatch.Success)
            {
                returnType = ConvertVBTypeToCSharp(asMatch.Groups[1].Value);
                declaration = declaration.Substring(0, asMatch.Index);
            }

            // Convert parameter list
            declaration = ConvertParameterList(declaration);

            return declaration;
        }

        /// <summary>
        /// Converts VB6 parameter list to C# parameter list.
        /// </summary>
        private string ConvertParameterList(string paramList)
        {
            // Handle parameters like: Foo(ByVal x As Integer, ByRef y As String)
            // Convert to: Foo(int x, ref string y)

            if (!paramList.Contains("("))
                return paramList + "()";

            int parenStart = paramList.IndexOf("(");
            int parenEnd = paramList.LastIndexOf(")");

            if (parenEnd <= parenStart)
                return paramList;

            string methodName = paramList.Substring(0, parenStart).Trim();
            string paramPart = paramList.Substring(parenStart + 1, parenEnd - parenStart - 1);

            // Parse parameters
            List<string> csharpParams = new List<string>();
            string[] parameters = SplitParameters(paramPart);

            foreach (string param in parameters)
            {
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                string csharpParam = ConvertParameter(param.Trim());
                csharpParams.Add(csharpParam);
            }

            return $"{methodName}({string.Join(", ", csharpParams)})";
        }

        /// <summary>
        /// Splits VB6 parameter list by comma, respecting parentheses.
        /// </summary>
        private string[] SplitParameters(string paramString)
        {
            var parameters = new List<string>();
            int parenDepth = 0;
            var currentParam = new StringBuilder();

            foreach (char c in paramString)
            {
                if (c == '(')
                    parenDepth++;
                else if (c == ')')
                    parenDepth--;
                else if (c == ',' && parenDepth == 0)
                {
                    parameters.Add(currentParam.ToString());
                    currentParam.Clear();
                    continue;
                }

                currentParam.Append(c);
            }

            if (currentParam.Length > 0)
                parameters.Add(currentParam.ToString());

            return parameters.ToArray();
        }

        /// <summary>
        /// Converts a single VB6 parameter to C# format.
        /// </summary>
        private string ConvertParameter(string vbParam)
        {
            // Handle: [ByVal|ByRef] name As Type
            string refModifier = "";

            if (vbParam.StartsWith("ByRef ", StringComparison.OrdinalIgnoreCase))
            {
                refModifier = "ref ";
                vbParam = vbParam.Substring(6).Trim();
            }
            else if (vbParam.StartsWith("ByVal ", StringComparison.OrdinalIgnoreCase))
            {
                vbParam = vbParam.Substring(6).Trim();
            }

            // Extract name and type
            Match match = Regex.Match(vbParam, @"^(\w+)\s+As\s+(.+)$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string vbType = match.Groups[2].Value;
                string csharpType = ConvertVBTypeToCSharp(vbType);
                return $"{refModifier}{csharpType} {name}";
            }

            // If no "As" clause, treat as object
            return $"{refModifier}object {vbParam}";
        }

        /// <summary>
        /// Converts VB6 type names to C# type names.
        /// </summary>
        private string ConvertVBTypeToCSharp(string vbType)
        {
            var typeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Integer", "int" },
                { "Long", "long" },
                { "Single", "float" },
                { "Double", "double" },
                { "String", "string" },
                { "Boolean", "bool" },
                { "Byte", "byte" },
                { "Date", "DateTime" },
                { "Currency", "decimal" },
                { "Variant", "object" },
                { "Object", "object" },
                { "Void", "void" }
            };

            if (typeMap.TryGetValue(vbType.Trim(), out string csharpType))
                return csharpType;

            // Handle array types (e.g., String())
            if (vbType.EndsWith("()"))
            {
                string baseType = vbType.Substring(0, vbType.Length - 2);
                return ConvertVBTypeToCSharp(baseType) + "[]";
            }

            return vbType; // Return as-is if not recognized
        }

        /// <summary>
        /// Converts a single line of VB6 code to C#.
        /// </summary>
        private string ConvertLine(string line)
        {
            string originalLine = line;
            string trimmed = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed))
                return line;

            if (trimmed.StartsWith("'"))
                return line.Replace("'", "//");

            // Convert assignments (=)
            if (trimmed.Contains("=") && !trimmed.Contains("=="))
            {
                // Be careful not to convert comparison operators
            }

            // Convert string concatenation (&) to C# (+)
            trimmed = trimmed.Replace(" & ", " + ");

            // Convert MsgBox to MessageBox
            trimmed = Regex.Replace(trimmed, @"\bMsgBox\b", "MessageBox.Show", RegexOptions.IgnoreCase);

            // Add semicolon if missing and line doesn't end with {, }, or is incomplete
            if (!trimmed.EndsWith(";") && !trimmed.EndsWith("{") && !trimmed.EndsWith("}") && 
                !trimmed.EndsWith(",") && !trimmed.EndsWith("(") && !trimmed.EndsWith("\\"))
            {
                trimmed += ";";
            }

            // Maintain original indentation
            int indentCount = line.Length - line.TrimStart().Length;
            return new string(' ', indentCount) + trimmed;
        }

        /// <summary>
        /// Sanitizes a class name to be valid C# identifier.
        /// </summary>
        private string SanitizeClassName(string name)
        {
            // Replace invalid characters with underscores
            string sanitized = Regex.Replace(name, @"[^\w]", "_");

            // Ensure it starts with a letter or underscore
            if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;

            return sanitized;
        }
    }
}
