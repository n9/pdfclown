package org.pdfclown.samples.cli;

import java.net.URI;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.DocumentActions;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.PageActions;
import org.pdfclown.documents.interaction.actions.GoToLocal;
import org.pdfclown.documents.interaction.actions.GoToURI;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.documents.interaction.navigation.document.LocalDestination;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to apply actions</b> to a PDF document.
  <p>In this case on document-opening a go-to-page-2 action is triggered;
  then on page-2-opening a go-to-URI action is triggered.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 11/01/11
*/
public class ActionSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    // 1. Opening the PDF file...
    File file;
    {
      String filePath = promptPdfFileChoice("Please select a PDF file");
      try
      {file = new File(filePath);}
      catch(Exception e)
      {throw new RuntimeException(filePath + " file access error.",e);}
    }
    Document document = file.getDocument();

    // 2. Applying actions...
    // 2.1. Local go-to.
    {
      DocumentActions documentActions = document.getActions();
      if(documentActions == null)
      {document.setActions(documentActions = new DocumentActions(document));}
      /*
        NOTE: This statement instructs the PDF viewer to go to page 2 on document opening.
      */
      documentActions.setOnOpen(
        new GoToLocal(
          document,
          new LocalDestination(
            document.getPages().get(1), // Page 2 (zero-based index).
            Destination.ModeEnum.Fit,
            null
            )
          )
        );
    }

    // 2.2. Remote go-to.
    {
      Page page = document.getPages().get(1); // Page 2 (zero-based index).
      PageActions pageActions = page.getActions();
      if(pageActions == null)
      {page.setActions(pageActions = new PageActions(document));}
      try
      {
        /*
          NOTE: This statement instructs the PDF viewer to navigate to the given URI on page 2 opening.
        */
        pageActions.setOnOpen(
          new GoToURI(
            document,
            new URI("http://www.sourceforge.net/projects/clown")
            )
          );
      }
      catch(Exception exception)
      {throw new RuntimeException(exception);}
    }

    // 3. Serialize the PDF file!
    serialize(file, true, "Actions", "applying actions");

    return true;
  }
}