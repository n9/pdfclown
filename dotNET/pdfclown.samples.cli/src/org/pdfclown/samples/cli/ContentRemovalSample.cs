using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample is a rough stub that demonstrates a basic way to remove all
    the text content from a PDF document.</summary>
    <remarks>Next releases will provide a refined way to discriminate text content
    for removal through ContentScanner class.</remarks>
  */
  public class ContentRemovalSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Content removal.
      foreach(Page page in document.Pages)
      {
        // Get the page contents!
        Contents contents = page.Contents;
        // Remove text content from page!
        RemoveText(contents);
        // Update the page contents!
        contents.Flush();
      }

      // 3. Serialize the PDF file!
      Serialize(file, true, "Content removal", "content removal from existing pages");

      return true;
    }

    private void RemoveText(
      IList<ContentObject> contents
      )
    {
      for(
        int index = 0,
          length = contents.Count;
        index < length;
        index++
        )
      {
        ContentObject content = contents[index];
        if(content is Text)
        {
          contents.RemoveAt(index);
          index--; length--;
        }
        else if(content is ContainerObject)
        {RemoveText(((ContainerObject)content).Objects);}
      }
    }
  }
}