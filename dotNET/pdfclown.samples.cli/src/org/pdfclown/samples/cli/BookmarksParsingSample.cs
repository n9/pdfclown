using org.pdfclown.documents;
using org.pdfclown.documents.fileSpecs;
using actions = org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to inspect the bookmarks of a PDF document.</summary>
  */
  public class BookmarksParsingSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Get the bookmarks collection!
      Bookmarks bookmarks = document.Bookmarks;
      if(bookmarks == null)
      {Console.WriteLine("\nNo bookmark available (Outline dictionary not found).");}
      else
      {
        Console.WriteLine("\nIterating through the bookmarks collection (please wait)...\n");
        // 3. Show the bookmarks!
        PrintBookmarks(bookmarks);
      }

      return true;
    }

    private void PrintBookmarks(
      Bookmarks bookmarks
      )
    {
      if(bookmarks == null)
        return;

      foreach(Bookmark bookmark in bookmarks)
      {
        // Show current bookmark!
        Console.WriteLine("Bookmark '" + bookmark.Title + "'");
        Console.Write("    Target: ");
        PdfObjectWrapper target = bookmark.Target;
        if(target is Destination)
        {PrintDestination((Destination)target);}
        else if(target is actions::Action)
        {PrintAction((actions::Action)target);}
        else if(target == null)
        {Console.WriteLine("[not available]");}
        else
        {Console.WriteLine("[unknown type: " + target.GetType().Name + "]");}

        // Show child bookmarks!
        PrintBookmarks(bookmark.Bookmarks);
      }
    }

    private void PrintAction(
      actions::Action action
      )
    {
      /*
        NOTE: Here we have to deal with reflection as a workaround
        to the lack of type covariance support in C# (so bad -- any better solution?).
      */
      Console.WriteLine("Action [" + action.GetType().Name + "] " + action.BaseObject);
      if(action.Is(typeof(actions::GoToDestination<>)))
      {
        if(action.Is(typeof(actions::GotoNonLocal<>)))
        {
          FileSpec fileSpec = (FileSpec)action.Get("FileSpec");
          if(fileSpec != null)
          {Console.WriteLine("    Filename: " + fileSpec.Filename);}

          if(action is actions::GoToEmbedded)
          {
            actions::GoToEmbedded.TargetObject target = ((actions::GoToEmbedded)action).Target;
            Console.WriteLine("    EmbeddedFilename: " + target.EmbeddedFileName + " Relation: " + target.Relation);
          }
        }
        Console.Write("    ");
        PrintDestination((Destination)action.Get("Destination"));
      }
      else if(action is actions::GoToURI)
      {Console.WriteLine("    URI: " + ((actions::GoToURI)action).URI);}
    }

    private void PrintDestination(
      Destination destination
      )
    {
      Console.WriteLine(destination.GetType().Name + " " + destination.BaseObject);
      Console.Write("    Page ");
      object pageRef = destination.PageRef;
      if(pageRef is Page)
      {
        Page refPage = (Page)pageRef;
        Console.WriteLine((refPage.Index+1) + " [ID: " + refPage.BaseObject + "]");
      }
      else
      {Console.WriteLine((int)pageRef+1);}
    }
  }
}