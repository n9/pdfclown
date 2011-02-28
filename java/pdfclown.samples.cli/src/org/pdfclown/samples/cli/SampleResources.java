package org.pdfclown.samples.cli;

import java.io.File;
import java.io.FilenameFilter;

/**
  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
*/
class SampleResources
{
  private File resourceDir;

  private class ExtensionFilter
    implements FilenameFilter
  {
    private String extension;

    private ExtensionFilter(
      String extension
      )
    {this.extension = "." + extension;}

    public boolean accept(
      File dir,
      String name
      )
    {return name.endsWith(extension);}
  }

  public SampleResources(
    File resourceDir
    )
  {this.resourceDir = resourceDir;}

  public String[] filter(
    String extension
    )
  {return resourceDir.list(new ExtensionFilter(extension));}

  public void printList(
    String[] filePaths
    )
  {
    for(
      int i = 0;
      i < filePaths.length;
      i++
      )
    {System.out.println("[" + i + "] " + filePaths[i]);}
  }
}