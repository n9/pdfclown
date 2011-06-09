package org.pdfclown.samples.cli;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Scanner;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.interaction.viewer.ViewerPreferences;
import org.pdfclown.documents.interchange.metadata.Information;
import org.pdfclown.files.File;
import org.pdfclown.files.SerializationModeEnum;

/**
  Abstract sample.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.0
*/
public abstract class Sample
{
  // <class>
  // <dynamic>
  // <fields>
  private String inputPath;
  private String outputPath;
  // </fields>

  // <interface>
  // <public>
  /**
    Executes the sample.

    @return Whether the sample has been completed.
  */
  public abstract boolean run(
    );
  // </public>

  // <protected>
  protected void buildAccessories(
    Document document,
    String title,
    String subject
    )
  {
    // Viewer preferences.
    ViewerPreferences view = new ViewerPreferences(document); // Instantiates viewer preferences inside the document context.
    document.setViewerPreferences(view); // Assigns the viewer preferences object to the viewer preferences function.
    view.setDisplayDocTitle(true);

    // Document metadata.
    Information info = new Information(document);
    document.setInformation(info);
    info.setAuthor("Stefano Chizzolini");
    info.setCreationDate(new Date());
    info.setCreator(getClass().getName());
    info.setTitle("PDF Clown - " + title + " sample");
    info.setSubject("Sample about " + subject + " using PDF Clown");
  }

  protected String getIndentation(
    int level
    )
  {
    StringBuilder indentationBuilder = new StringBuilder();
    for(int i = 0; i < level; i++)
    {indentationBuilder.append(' ');}
    return indentationBuilder.toString();
  }

  /**
    Prompts a message to the user.

    @param message Text to show.
  */
  protected void prompt(
    String message
    )
  {Utils.prompt(message);}

  /**
    Gets the user's choice from the given request.

    @param message Description of the request to show to the user.
    @return User choice.
  */
  protected String promptChoice(
    String message
    )
  {
    System.out.print("\n" + message);
    Scanner in = new Scanner(System.in);
    try
    {return in.nextLine();}
    catch(Exception e)
    {return null;}
  }

  /**
    Gets the user's choice from the given options.

    @param options Available options to show to the user.
    @return Chosen option key.
  */
  protected String promptChoice(
    Map<String,String> options
    )
  {
    System.out.println();
    List<Map.Entry<String,String>> optionEntries = new ArrayList<Map.Entry<String,String>>(options.entrySet());
    Collections.sort(
      optionEntries,
      new Comparator<Map.Entry<String,String>>()
      {
        @Override
        public int compare(Map.Entry<String,String> o1, Map.Entry<String,String> o2)
        {return o1.getKey().compareTo(o2.getKey());};
      }
      );
    for(Map.Entry<String,String> option : optionEntries)
    {
      System.out.println(
          (option.getKey().equals("") ? "ENTER" : "[" + option.getKey() + "]")
            + " " + option.getValue()
          );
    }
    System.out.print("Please select: ");
    Scanner in = new Scanner(System.in);
    try
    {return in.nextLine();}
    catch(Exception e)
    {return null;}
  }

  protected String promptFileChoice(
    String fileExtension,
    String listDescription,
    String inputDescription
    )
  {
    Scanner in = new Scanner(System.in);

    System.out.println("\n" + listDescription + ":");
    SampleResources resources = new SampleResources(new java.io.File(inputPath + java.io.File.separator + "pdf" + java.io.File.separator));

    // Get the list of available PDF files!
    List<String> filePaths = Arrays.asList(resources.filter(fileExtension));
    Collections.sort(filePaths);

    // Display files!
    resources.printList((String[])filePaths.toArray());

    // Get the user's choice!
    System.out.print(inputDescription + ": ");
    try
    {return inputPath + java.io.File.separator + "pdf" + java.io.File.separator + filePaths.get(Integer.parseInt(in.nextLine()));}
    catch(Exception e)
    {return inputPath + java.io.File.separator + "pdf" + java.io.File.separator + filePaths.get(0);}
  }

  /**
    Prompts the user for advancing to the next page.

    @param page Next page.
    @param skip Whether the prompt has to be skipped.
    @return Whether to advance.
  */
  protected boolean promptNextPage(
    Page page,
    boolean skip
    )
  {
    int pageIndex = page.getIndex();
    if(pageIndex > 0 && !skip)
    {
      Map<String,String> options = new HashMap<String,String>();
      options.put("", "Scan next page");
      options.put("Q", "End scanning");
      if(!promptChoice(options).equals(""))
        return false;
    }

    System.out.println("\nScanning page " + (pageIndex+1) + "...\n");
    return true;
  }

  /**
    Prompts the user for a page index to select.

    @param inputDescription Message prompted to the user.
    @param pageCount Page count.
    @return Selected page index.
  */
  protected int promptPageChoice(
    String inputDescription,
    int pageCount
    )
  {return promptPageChoice(inputDescription, 0, pageCount);}

  /**
    Prompts the user for a page index to select.

    @param inputDescription Message prompted to the user.
    @param startIndex First page index, inclusive.
    @param endIndex Last page index, exclusive.
    @return Selected page index.
  */
  protected int promptPageChoice(
    String inputDescription,
    int startIndex,
    int endIndex
    )
  {
    int pageIndex;
    try
    {pageIndex = Integer.parseInt(promptChoice(inputDescription + " [" + (startIndex + 1) + "-" + endIndex + "]: ")) - 1;}
    catch(Exception e)
    {pageIndex = startIndex;}
    if(pageIndex < startIndex)
    {pageIndex = startIndex;}
    else if(pageIndex >= endIndex)
    {pageIndex = endIndex - 1;}

    return pageIndex;
  }

  protected String promptPdfFileChoice(
    String inputDescription
    )
  {return promptFileChoice("pdf", "Available PDF files", inputDescription);}

  /**
    Gets the source path used to load input PDF files.
  */
  protected String getInputPath(
    )
  {return inputPath;}

  /**
    Gets the target path used to serialize output PDF files.
  */
  protected String getOutputPath(
    )
  {return outputPath;}

  /**
    Serializes the given PDF Clown file object.

    @param file File to serialize.
  */
  protected void serialize(
    File file
    )
  {serialize(file, true);}

  /**
    Serializes the given PDF Clown file object.

    @param file File to serialize.
    @param chooseMode Whether to allow user choice of serialization mode.
  */
  protected void serialize(
    File file,
    boolean chooseMode
    )
  {serialize(file, getClass().getSimpleName(), chooseMode);}

  /**
    Serializes the given PDF Clown file object.

    @param file File to serialize.
    @param fileName Output file name.
    @param chooseMode Whether to allow user choice of serialization mode.
  */
  protected void serialize(
    File file,
    String fileName,
    boolean chooseMode
    )
  {
    System.out.println();
    SerializationModeEnum serializationMode = SerializationModeEnum.Incremental;
    if(chooseMode)
    {
      Scanner in = new Scanner(System.in);
      System.out.println("[0] Standard serialization");
      System.out.println("[1] Incremental update");
      // Get the user's choice.
      System.out.print("Please select a serialization mode: ");
      try
      {serializationMode = SerializationModeEnum.values()[Integer.parseInt(in.nextLine())];}
      catch(Exception e)
      {/* Default. */}
    }

    java.io.File outputFile = new java.io.File(outputPath + java.io.File.separator + fileName + "." + serializationMode + ".pdf");

    // Save the file!
    try
    {
      file.save(
        outputFile,
        serializationMode
        );
    }
    catch(Exception e)
    {
      System.out.println("File writing failed: " + e.getMessage());
      e.printStackTrace();
    }

// Alternatively, defining an appropriate target stream:
/*
    OutputStream outputStream;
    BufferedOutputStream baseOutputStream;
    try
    {
      outputFile.createNewFile();
      baseOutputStream = new BufferedOutputStream(
        new FileOutputStream(outputFile)
        );
      outputStream = new org.pdfclown.bytes.OutputStream(baseOutputStream);
    }
    catch(Exception e)
    {throw new RuntimeException(outputFile.getPath() + " file couldn't be created.",e);}

    try
    {
      file.save(
        outputStream,
        serializationMode
        );

      baseOutputStream.flush();
      baseOutputStream.close();
    }
    catch(Exception e)
    {throw new RuntimeException(outputFile.getPath() + " file writing has failed.",e);}
*/
    System.out.println("\nOutput: " + outputFile.getPath());
  }
  // </protected>

  // <internal>
  final void initialize(
    String inputPath,
    String outputPath
    )
  {
    this.inputPath = inputPath;
    this.outputPath = outputPath;
  }
  // </internal>
  // </interface>
  // </dynamic>
  // </class>
}
