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
    <summary>This sample demonstrates how to alter the graphic contents (in this case:
    the line width) of pages within a PDF document.</summary>
    <remarks>This implementation is just a humble exercise: see the API documentation
    to perform all the possible access functionalities.</remarks>
  */
  public class ContentTweakingSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Content tweaking.
      foreach(Page page in document.Pages)
      {
        // Get the page contents!
        Contents contents = page.Contents;
        contents.Insert(0,new SetLineWidth(10)); // Forces the override of line width's initial value (1.0) [PDF:1.6:4.3] setting it at 10 user-space units.
        foreach(ContentObject obj in contents)
        {NormalizeLineWidth(obj);}
        // Update the page contents!
        contents.Flush();
      }

      // 3. Serialize the PDF file!
      Serialize(file, true, "Content tweaking", "content tweaking inside existing pages");

      return true;
    }

    private void NormalizeLineWidth(
      ContentObject content
      )
    {
      if(content is SetLineWidth)
      {
        SetLineWidth setLineWidth = (SetLineWidth)content;
        // Force lines under 10 user-space units to be set to 10!
        if(setLineWidth.Value < 10)
        {setLineWidth.Value = 10;}
      }
      else if(content is ContainerObject)
      {
        foreach(ContentObject obj in ((ContainerObject)content).Objects)
        {NormalizeLineWidth(obj);}
      }
    }
  }
}