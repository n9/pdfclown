using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.entities;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.xObjects;
using org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to apply actions to a document.</summary>
    <remarks>In this case, on document-opening a go-to-page-2 action is triggered;
    then on page-2-opening a go-to-URI action is triggered.</remarks>
  */
  public class ActionSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Applying actions...
      // 2.1. Local go-to.
      {
        DocumentActions documentActions = document.Actions;
        if(documentActions == null)
        {document.Actions = documentActions = new DocumentActions(document);}

        /*
          NOTE: This statement instructs the PDF viewer to go to page 2 on document opening.
        */
        documentActions.OnOpen = new GoToLocal(
          document,
          new LocalDestination(
            document.Pages[1], // Page 2 (zero-based index).
            Destination.ModeEnum.Fit,
            null
            )
          );
      }
      // 2.2. Remote go-to.
      {
        Page page = document.Pages[1]; // Page 2 (zero-based index).
        PageActions pageActions = page.Actions;
        if(pageActions == null)
        {page.Actions = pageActions = new PageActions(document);}
        try
        {
          /*
            NOTE: This statement instructs the PDF viewer to navigate to the given URI on page 2 opening.
          */
          pageActions.OnOpen = new GoToURI(
            document,
            new Uri("http://www.sourceforge.net/projects/clown")
            );
        }
        catch(Exception exception)
        {throw new Exception("Remote goto failed.",exception);}
      }

      // 3. Serialize the PDF file!
      Serialize(file, true, "Actions", "applying actions");

      return true;
    }
  }
}