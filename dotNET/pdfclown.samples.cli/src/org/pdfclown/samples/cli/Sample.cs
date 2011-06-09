using bytes = org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.interaction;
using org.pdfclown.documents.interchange.metadata;
using org.pdfclown.documents.interaction.viewer;
using files = org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.IO;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>Abstract sample.</summary>
  */
  public abstract class Sample
  {
    #region dynamic
    #region fields
    private string inputPath;
    private string outputPath;
    #endregion

    #region interface
    #region public
    /**
      <summary>Executes the sample.</summary>
      <returns>Whether the sample has been completed.</returns>
    */
    public abstract bool Run(
      );
    #endregion

    #region protected
    protected void BuildAccessories(
      Document document,
      string title,
      string subject
      )
    {
      // Viewer preferences.
      ViewerPreferences view = new ViewerPreferences(document); // Instantiates viewer preferences inside the document context.
      document.ViewerPreferences = view; // Assigns the viewer preferences object to the viewer preferences function.
      view.DisplayDocTitle = true;

      // Document metadata.
      Information info = new Information(document);
      document.Information = info;
      info.Author = "Stefano Chizzolini";
      info.CreationDate = DateTime.Now;
      info.Creator = GetType().FullName;
      info.Title = "PDF Clown - " + title + " sample";
      info.Subject = "Sample about " + subject + " using PDF Clown";
    }

    protected string GetIndentation(
      int level
      )
    {return new String(' ',level);}

    protected string InputPath
    {
      get
      {return inputPath;}
    }

    protected string OutputPath
    {
      get
      {return outputPath;}
    }

    /**
      <summary>Prompts a message to the user.</summary>
      <param name="message">Text to show.</param>
    */
    protected void Prompt(
      string message
      )
    {Utils.Prompt(message);}

    /**
      <summary>Gets the user's choice from the given request.</summary>
      <param name="message">Description of the request to show to the user.</param>
      <returns>User choice.</returns>
    */
    protected string PromptChoice(
      string message
      )
    {
      Console.Write("\n" + message);
      try
      {return Console.ReadLine();}
      catch
      {return null;}
    }

    /**
      <summary>Gets the user's choice from the given options.</summary>
      <param name="options">Available options to show to the user.</param>
      <returns>Chosen option key.</returns>
    */
    protected string PromptChoice(
      IDictionary<string,string> options
      )
    {
      Console.WriteLine();
      foreach(KeyValuePair<string,string> option in options)
      {
        Console.WriteLine(
            (option.Key.Equals("") ? "ENTER" : "[" + option.Key + "]")
              + " " + option.Value
            );
      }
      Console.Write("Please select: ");
      return Console.ReadLine();
    }

    protected string PromptFileChoice(
      string fileExtension,
      string listDescription,
      string inputDescription
      )
    {
      Console.WriteLine("\n" + listDescription + ":");

      // Get the list of available PDF files!
      string[] filePaths = Directory.GetFiles(inputPath + "pdf" + Path.DirectorySeparatorChar,"*." + fileExtension);

      // Display files!
      for(
        int i = 0;
        i < filePaths.Length;
        i++
        )
      {Console.WriteLine("[" + i + "] {0}", System.IO.Path.GetFileName(filePaths[i]));}

      // Get the user's choice!
      Console.Write(inputDescription + ": ");
      try
      {return filePaths[Int32.Parse(Console.ReadLine())];}
      catch
      {return filePaths[0];}
    }

    /**
      <summary>Prompts the user for advancing to the next page.</summary>
      <param name="page">Next page.</param>
      <param name="skip">Whether the prompt has to be skipped.</param>
      <returns>Whether to advance.</returns>
    */
    protected bool PromptNextPage(
      Page page,
      bool skip
      )
    {
      int pageIndex = page.Index;
      if(pageIndex > 0 && !skip)
      {
        IDictionary<string,string> options = new Dictionary<string,string>();
        options[""] = "Scan next page";
        options["Q"] = "End scanning";
        if(!PromptChoice(options).Equals(""))
          return false;
      }

      Console.WriteLine("\nScanning page " + (pageIndex+1) + "...\n");
      return true;
    }

    /**
      <summary>Prompts the user for a page index to select.</summary>
      <param name="inputDescription">Message prompted to the user.</param>
      <param name="pageCount">Page count.</param>
      <returns>Selected page index.</returns>
    */
    protected int PromptPageChoice(
      string inputDescription,
      int pageCount
      )
    {return PromptPageChoice(inputDescription, 0, pageCount);}

    /**
      <summary>Prompts the user for a page index to select.</summary>
      <param name="inputDescription">Message prompted to the user.</param>
      <param name="startIndex">First page index, inclusive.</param>
      <param name="endIndex">Last page index, exclusive.</param>
      <returns>Selected page index.</returns>
    */
    protected int PromptPageChoice(
      string inputDescription,
      int startIndex,
      int endIndex
      )
    {
      int pageIndex;
      try
      {pageIndex = Int32.Parse(PromptChoice(inputDescription + " [" + (startIndex + 1) + "-" + endIndex + "]: ")) - 1;}
      catch
      {pageIndex = startIndex;}
      if(pageIndex < startIndex)
      {pageIndex = startIndex;}
      else if(pageIndex >= endIndex)
      {pageIndex = endIndex - 1;}

      return pageIndex;
    }

    protected string PromptPdfFileChoice(
      string inputDescription
      )
    {return PromptFileChoice("pdf", "Available PDF files", inputDescription);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">File to serialize.</param>
    */
    protected void Serialize(
      files::File file
      )
    {Serialize(file, true);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">File to serialize.</param>
      <param name="chooseMode">Whether to allow user choice of serialization mode.</param>
    */
    protected void Serialize(
      files::File file,
      bool chooseMode
      )
    {Serialize(file, GetType().Name, chooseMode);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">File to serialize.</param>
      <param name="fileName">Output file name.</param>
      <param name="chooseMode">Whether to allow user choice of serialization mode.</param>
    */
    protected void Serialize(
      files::File file,
      string fileName,
      bool chooseMode
      )
    {
      Console.WriteLine();
      files::SerializationModeEnum serializationMode = files::SerializationModeEnum.Incremental;
      if(chooseMode)
      {
        Console.WriteLine("[0] Standard serialization");
        Console.WriteLine("[1] Incremental update");
        // Get the user's choice.
        Console.Write("Please select a serialization mode: ");
        try
        {serializationMode = (files::SerializationModeEnum)Int32.Parse(Console.ReadLine());}
        catch
        {/* Default. */}
      }

      string outputFilePath = outputPath + fileName + "." + serializationMode + ".pdf";

      // Save the file!
      try
      {
        file.Save(
          outputFilePath,
          serializationMode
          );
      }
      catch(Exception e)
      {
        Console.WriteLine("File writing failed: " + e.Message);
        Console.WriteLine(e.StackTrace);
      }

// Alternatively, defining an appropriate target stream:
/*
      FileStream outputStream = new System.IO.FileStream(
        outputFilePath,
        System.IO.FileMode.Create,
        System.IO.FileAccess.Write
        );
      file.Save(
        new bytes::Stream(outputStream),
        serializationMode
        );
      outputStream.Flush();
      outputStream.Close();
*/
      Console.WriteLine("\nOutput: "+ outputFilePath);
    }
    #endregion

    #region internal
    internal void Initialize(
      string inputPath,
      string outputPath
      )
    {
      this.inputPath = inputPath;
      this.outputPath = outputPath;
    }
    #endregion
    #endregion
    #endregion
  }
}