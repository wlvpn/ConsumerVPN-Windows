using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace WLVPN.Extensions
{
    public static class FlowDocumentExtensions
    {
        /// <summary>
        /// Loads textual data into the rich text box to display.
        /// </summary>
        /// <param name="flowDocument">The flow document which contains alignment and pagination information.</param>
        /// <param name="filepath">The filepath for the file to load text from.</param>
        public static void LoadFromFile(this FlowDocument flowDocument, string filepath)
        {
            if (File.Exists(filepath))
            {
                TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);

                using (FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    textRange.Load(fileStream, DataFormats.Rtf);
                }
            }
        }
    }
}
