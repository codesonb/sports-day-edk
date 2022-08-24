using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

using System.Xml;

namespace Launcher
{
    static class ExtDocument
    {

        public static FlowDocument Clone(this FlowDocument from)
        {
            var str = XamlWriter.Save(from);
            var stringReader = new StringReader(str);
            var xmlReader = XmlReader.Create(stringReader);
            var cloneDoc = XamlReader.Load(xmlReader) as FlowDocument;
            return cloneDoc;
        }


        /// <summary>
        /// Adds one flowdocument to another.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public static void CopyAppendTo(this FlowDocument from, FlowDocument to)
        {
            TextRange range = new TextRange(from.ContentStart, from.ContentEnd);
            using (MemoryStream stream = new MemoryStream())
            {
                XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);
                TextRange range2 = new TextRange(to.ContentEnd, to.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);
            }
        }

        /// <summary>
        /// Adds a block to a flowdocument.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public static void CopyAppendTo(this Block from, FlowDocument to)
        {
            if (from != null)
            {
                TextRange range = new TextRange(from.ContentStart, from.ContentEnd);

                using (MemoryStream stream = new MemoryStream())
                {
                    XamlWriter.Save(range, stream);
                    range.Save(stream, DataFormats.XamlPackage);
                    TextRange textRange2 = new TextRange(to.ContentEnd, to.ContentEnd);
                    textRange2.Load(stream, DataFormats.XamlPackage);
                }
            }
        }


    }
}
