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
      Console.Write(message);
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
      <param name="file">PDF file to serialize.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file
      )
    {return Serialize(file, null, null, null);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">PDF file to serialize.</param>
      <param name="serializationMode">Serialization mode.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file,
      files::SerializationModeEnum? serializationMode
      )
    {return Serialize(file, null, null, serializationMode);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">PDF file to serialize.</param>
      <param name="fileName">Output file name.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file,
      string fileName
      )
    {return Serialize(file, fileName, null, null);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">PDF file to serialize.</param>
      <param name="fileName">Output file name.</param>
      <param name="serializationMode">Serialization mode.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file,
      string fileName,
      files::SerializationModeEnum? serializationMode
      )
    {return Serialize(file, fileName, null, null, serializationMode);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">PDF file to serialize.</param>
      <param name="title">Document title.</param>
      <param name="subject">Document subject.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file,
      string title,
      string subject
      )
    {return Serialize(file, title, subject, null);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">PDF file to serialize.</param>
      <param name="title">Document title.</param>
      <param name="subject">Document subject.</param>
      <param name="serializationMode">Serialization mode.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file,
      string title,
      string subject,
      files::SerializationModeEnum? serializationMode
      )
    {return Serialize(file, GetType().Name, title, subject, serializationMode);}

    /**
      <summary>Serializes the given PDF Clown file object.</summary>
      <param name="file">PDF file to serialize.</param>
      <param name="fileName">Output file name.</param>
      <param name="title">Document title.</param>
      <param name="subject">Document subject.</param>
      <param name="serializationMode">Serialization mode.</param>
      <returns>Serialization path.</returns>
    */
    protected string Serialize(
      files::File file,
      string fileName,
      string title,
      string subject,
      files::SerializationModeEnum? serializationMode
      )
    {
      ApplyDocumentSettings(file.Document, title, subject);

      Console.WriteLine();

      if(!serializationMode.HasValue)
      {
        if(file.Reader == null) // New file.
        {serializationMode = files::SerializationModeEnum.Standard;}
        else // Existing file.
        {
          Console.WriteLine("[0] Standard serialization");
          Console.WriteLine("[1] Incremental update");
          Console.Write("Please select a serialization mode: ");
          try
          {serializationMode = (files::SerializationModeEnum)Int32.Parse(Console.ReadLine());}
          catch
          {serializationMode = files::SerializationModeEnum.Standard;}
        }
      }

      string outputFilePath = outputPath + fileName + "." + serializationMode + ".pdf";

      // Save the file!
      /*
        NOTE: You can also save to a generic target stream (see Save() method overloads).
      */
      try
      {file.Save(outputFilePath, serializationMode.Value);}
      catch(Exception e)
      {
        Console.WriteLine("File writing failed: " + e.Message);
        Console.WriteLine(e.StackTrace);
      }
      Console.WriteLine("\nOutput: "+ outputFilePath);

      return outputFilePath;
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

    #region private
    private void ApplyDocumentSettings(
      Document document,
      string title,
      string subject
      )
    {
      if(title == null)
        return;

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
    #endregion
    #endregion
    #endregion
  }
}