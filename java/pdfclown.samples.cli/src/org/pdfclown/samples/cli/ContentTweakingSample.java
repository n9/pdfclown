package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.Contents;
import org.pdfclown.documents.contents.objects.CompositeObject;
import org.pdfclown.documents.contents.objects.ContentObject;
import org.pdfclown.documents.contents.objects.SetLineWidth;
import org.pdfclown.files.File;

import java.util.List;

/**
  This sample demonstrates <b>how to alter the graphic contents</b> (in this case: the line width)
  of pages within a PDF document.
  <h3>Remarks</h3>
  <p>This implementation is just a humble exercise: see the API documentation
  to perform all the possible access functionalities.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.6
  @version 0.1.0
*/
public class ContentTweakingSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    File file;
    {
      String filePath = promptPdfFileChoice("Please select a PDF file");

      // 1. Open the PDF file!
      try
      {file = new File(filePath);}
      catch(Exception e)
      {throw new RuntimeException(filePath + " file access error.",e);}
    }

    Document document = file.getDocument();

    // 2. Content tweaking.
    for(Page page : document.getPages())
    {
      // Get the page contents!
      Contents contents = page.getContents();
      contents.add(0,new SetLineWidth(10)); // Forces the override of line width's initial value (1.0) [PDF:1.6:4.3] setting it at 10 user-space units.
      for(ContentObject obj : contents)
      {normalizeLineWidth(obj);}
      // Update the page contents!
      contents.flush();
    }

    // (boilerplate metadata insertion -- ignore it)
    buildAccessories(document,"Content tweaking","content tweaking inside existing pages");

    // 3. Serialize the PDF file!
    serialize(file);
    
    return true;
  }

  private void normalizeLineWidth(
    ContentObject content
    )
  {
    if(content instanceof SetLineWidth)
    {
      SetLineWidth setLineWidth = (SetLineWidth)content;
      // Force lines under 10 user-space units to be set to 10!
      if(setLineWidth.getValue() < 10)
      {setLineWidth.setValue(10);}
    }
    else if(content instanceof CompositeObject)
    {
      List<ContentObject> objects = ((CompositeObject)content).getObjects();
      for(ContentObject obj : objects)
      {normalizeLineWidth(obj);}
    }
  }
}