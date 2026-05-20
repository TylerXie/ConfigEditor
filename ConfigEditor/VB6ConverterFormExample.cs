using System;
using System.IO;
using System.Windows.Forms;

namespace ConfigEditor
{
    /// <summary>
    /// Example code showing how to integrate VB6ToCSharpConverter into a Windows Forms application.
    /// This is NOT a complete form, just example code and helper methods.
    /// </summary>
    public class VB6ConverterFormExample
    {
        private readonly VB6ToCSharpConverter _converter;

        public VB6ConverterFormExample()
        {
            _converter = new VB6ToCSharpConverter();
        }

        /// <summary>
        /// Example method: Browse for VB6 file and display success/error dialog
        /// </summary>
        public void BrowseAndConvertVB6File()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select VB6 File to Convert";
                dialog.Filter = "VB6 Files (*.bas;*.cls)|*.bas;*.cls|All Files (*.*)|*.*";
                dialog.DefaultExt = ".bas";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ConvertSelectedFile(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// Example method: Convert file and display result
        /// </summary>
        public void ConvertSelectedFile(string vb6FilePath)
        {
            try
            {
                // Show progress
                string statusMessage = $"Converting: {Path.GetFileName(vb6FilePath)}...";
                // TODO: Update UI with status

                // Convert the file
                string outputFile = _converter.ConvertFile(vb6FilePath);

                // Show success message
                string successMessage = $"✓ Conversion successful!\n\n" +
                    $"Input:  {vb6FilePath}\n" +
                    $"Output: {outputFile}";
                MessageBox.Show(successMessage, "Conversion Complete", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Optionally open the converted file
                if (File.Exists(outputFile))
                {
                    // System.Diagnostics.Process.Start("notepad.exe", outputFile);
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"File not found: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Conversion failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Example method: Convert multiple files from a directory
        /// </summary>
        public void ConvertDirectoryOfVB6Files(string directory)
        {
            if (!Directory.Exists(directory))
            {
                MessageBox.Show("Directory not found.", "Error");
                return;
            }

            try
            {
                string[] vb6Files = Directory.GetFiles(directory, "*.bas");
                vb6Files = vb6Files.Length > 0 ? vb6Files : 
                    Directory.GetFiles(directory, "*.cls");

                if (vb6Files.Length == 0)
                {
                    MessageBox.Show("No VB6 files found in directory.", "Info");
                    return;
                }

                int successCount = 0;
                int failureCount = 0;

                foreach (string file in vb6Files)
                {
                    try
                    {
                        string outputFile = _converter.ConvertFile(file);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        // Log error but continue with other files
                    }
                }

                string resultMessage = $"Conversion complete!\n\n" +
                    $"Successful: {successCount}\n" +
                    $"Failed: {failureCount}";
                MessageBox.Show(resultMessage, "Batch Conversion Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during batch conversion:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================================
        // USAGE EXAMPLES FOR FORM DESIGNER CODE
        // =====================================================================
        //
        // To use this in a Windows Forms application, you would need:
        //
        // 1. Add buttons to the form:
        //    - btnBrowse (Browse for VB6 file)
        //    - btnConvert (Convert selected file)
        //    - btnConvertFolder (Convert entire folder)
        //
        // 2. Add text boxes:
        //    - txtVB6FilePath (Display selected file path)
        //    - txtOutputPath (Display output directory)
        //
        // 3. Add event handlers:
        //    btnBrowse.Click += (s, e) => BrowseVB6File();
        //    btnConvert.Click += (s, e) => ConvertSelectedFile(txtVB6FilePath.Text);
        //
        // Example InitializeComponent code:
        // =====================================================================
        //
        // private void InitializeComponent()
        // {
        //     this.btnBrowse = new Button();
        //     this.btnConvert = new Button();
        //     this.txtVB6FilePath = new TextBox();
        //     this.lblInfo = new Label();
        //
        //     this.btnBrowse.Text = "Browse VB6 File";
        //     this.btnBrowse.Click += (s, e) => BrowseVB6File();
        //     this.btnBrowse.Location = new Point(10, 10);
        //     this.Controls.Add(this.btnBrowse);
        //
        //     this.txtVB6FilePath.Location = new Point(100, 12);
        //     this.txtVB6FilePath.Width = 300;
        //     this.Controls.Add(this.txtVB6FilePath);
        //
        //     this.btnConvert.Text = "Convert";
        //     this.btnConvert.Click += (s, e) => 
        //     {
        //         if (File.Exists(txtVB6FilePath.Text))
        //             ConvertSelectedFile(txtVB6FilePath.Text);
        //     };
        //     this.btnConvert.Location = new Point(410, 10);
        //     this.Controls.Add(this.btnConvert);
        //
        //     this.Text = "VB6 to C# Converter";
        //     this.Width = 600;
        //     this.Height = 150;
        // }
    }

    // =====================================================================
    // ALTERNATIVE: COMMAND-LINE INTEGRATION
    // =====================================================================
    // If you want to add a command-line version, create a separate console app:
    //
    // class VB6ConverterCLI
    // {
    //     static int Main(string[] args)
    //     {
    //         if (args.Length == 0)
    //         {
    //             PrintHelp();
    //             return 0;
    //         }
    //
    //         string vb6File = args[0];
    //         string outputDir = args.Length > 1 ? args[1] : null;
    //
    //         var converter = new VB6ToCSharpConverter();
    //
    //         try
    //         {
    //             string output = converter.ConvertFile(vb6File, outputDir);
    //             Console.WriteLine($"✓ Success: {output}");
    //             return 0;
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.Error.WriteLine($"✗ Error: {ex.Message}");
    //             return 1;
    //         }
    //     }
    //
    //     static void PrintHelp()
    //     {
    //         Console.WriteLine("VB6ToCSharp Converter");
    //         Console.WriteLine("Usage: VB6ToCSharp.exe <vb6_file> [output_dir]");
    //     }
    // }
}
