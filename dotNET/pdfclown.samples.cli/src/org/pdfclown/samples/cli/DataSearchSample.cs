using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to visit the structure of a PDF file.</summary>
    <remarks>Note this is just a simple exercise for low-level data matching; if you need more
    advanced functionalities (e.g. text content extraction, text highlighting...), see the related
    samples instead (e.g. <see cref="AdvancedTextExtractionSample"/>, <see cref="TextHighlightSample"/>
    ...).</remarks>
  */
  public class DataSearchSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);

      // 2. Define the text pattern to look for!
      string textRegEx = PromptChoice("Please enter the pattern to look for: ");
      Regex pattern = new Regex(textRegEx, RegexOptions.IgnoreCase);

      // 3. Search.
      /*
        NOTE: In order to navigate through the file data, we can either (a) descend from the document
        root or (b) iterate through the indirect objects collection.
      */
      // 3.a. Searching from the document root object.
      Console.WriteLine("We can alternatively search starting from the document root object or iterating through the file's indirect objects collection.");
      Console.WriteLine("\nApproach 1: Search starting from the document root object...");
      Search(pattern, file.Document.BaseObject, "", new HashSet<PdfReference>());

      // 3.b. Searching through the file's indirect objects.
      Console.WriteLine("\nApproach 2: Search iterating through the file's indirect objects collection...");
      HashSet<PdfReference> visitedReferences = new HashSet<PdfReference>();
      foreach(PdfIndirectObject obj in file.IndirectObjects)
      {Search(pattern, obj.Reference, "", visitedReferences);}

      return true;
    }

    private void Search(
      Regex pattern,
      PdfDataObject obj,
      string path,
      HashSet<PdfReference> visitedReferences
      )
    {
      if(obj is PdfReference)
      {
        if(visitedReferences.Contains((PdfReference)obj))
          return;

        path += "[" + ((PdfReference)obj).IndirectReference + "]";
        visitedReferences.Add((PdfReference)obj);
        obj = File.Resolve(obj);
      }

      Match match = null;
      if(obj is PdfName
        || (obj is PdfString && !(obj is PdfDate)))
      {
        string value;
        if(obj is PdfName)
        {value = (string)((PdfName)obj).Value;}
        else
        {value = (string)((PdfString)obj).Value;}
        match = pattern.Match(value);
      }
      else if(obj is PdfDictionary)
      {
        foreach(KeyValuePair<PdfName,PdfDirectObject> entry in (PdfDictionary)obj)
        {Search(pattern, entry.Value, path + "/" + entry.Key, visitedReferences);}
      }
      else if(obj is PdfArray)
      {
        int index = 0;
        foreach(PdfDirectObject item in (PdfArray)obj)
        {Search(pattern, item, path + "/" + index++, visitedReferences);}
      }
      else if(obj is PdfStream)
      {
        PdfStream stream = (PdfStream)obj;
        if(PdfName.XObject.Equals(stream.Header[PdfName.Type])
          && PdfName.Form.Equals(stream.Header[PdfName.Subtype]))
        {
          IBuffer body = ((PdfStream)obj).Body;
          body.Seek(0);
          string content = body.ReadString((int)body.Length);
          match = pattern.Match(content);
        }
      }
      if(match != null
        && match.Success)
      {Console.WriteLine(path + ":" + match.Value);}
    }
  }
}