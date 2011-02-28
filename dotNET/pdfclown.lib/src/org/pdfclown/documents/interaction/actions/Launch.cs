/*
  Copyright 2008-2010 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.fileSpecs;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;

namespace org.pdfclown.documents.interaction.actions
{
  /**
    <summary>'Launch an application' action [PDF:1.6:8.5.3].</summary>
  */
  [PDF(VersionEnum.PDF11)]
  public sealed class Launch
    : Action
  {
    #region types
    /**
      <summary>Windows-specific launch parameters [PDF:1.6:8.5.3].</summary>
    */
    public class WinParametersObject
      : PdfObjectWrapper<PdfDictionary>
    {
      #region types
      /**
        <summary>Operation [PDF:1.6:8.5.3].</summary>
      */
      public enum OperationEnum
      {
        /**
          <summary>Open.</summary>
        */
        Open,
        /**
          <summary>Print.</summary>
        */
        Print
      };
      #endregion

      #region static
      #region fields
      private static readonly Dictionary<OperationEnum,PdfString> OperationEnumCodes;
      #endregion

      #region constructors
      static WinParametersObject()
      {
        OperationEnumCodes = new Dictionary<OperationEnum,PdfString>();
        OperationEnumCodes[OperationEnum.Open] = new PdfString("open");
        OperationEnumCodes[OperationEnum.Print] = new PdfString("print");
      }
      #endregion

      #region interface
      #region private
      /**
        <summary>Gets the code corresponding to the given value.</summary>
      */
      private static PdfString ToCode(
        OperationEnum value
        )
      {return OperationEnumCodes[value];}

      /**
        <summary>Gets the operation corresponding to the given value.</summary>
      */
      private static OperationEnum ToOperationEnum(
        PdfString value
        )
      {
        foreach(KeyValuePair<OperationEnum,PdfString> operation in OperationEnumCodes)
        {
          if(operation.Value.Equals(value))
            return operation.Key;
        }
        return OperationEnum.Open;
      }
      #endregion
      #endregion
      #endregion

      #region dynamic
      #region constructors
      public WinParametersObject(
        Document context,
        string fileName
        ) : base(
          context.File,
          new PdfDictionary()
          )
      {FileName = fileName;}

      public WinParametersObject(
        Document context,
        string fileName,
        OperationEnum operation
        ) : this(
          context,
          fileName
          )
      {Operation = operation;}

      public WinParametersObject(
        Document context,
        string fileName,
        string parameterString
        ) : this(
          context,
          fileName
          )
      {ParameterString = parameterString;}

      internal WinParametersObject(
        PdfDirectObject baseObject,
        PdfIndirectObject container
        ) : base(baseObject,container)
      {}
      #endregion

      #region interface
      #region public
      public override object Clone(
        Document context
        )
      {throw new NotImplementedException();}

      /**
        <summary>Gets/Sets the default directory.</summary>
      */
      public string DefaultDirectory
      {
        get
        {
          /*
            NOTE: 'D' entry may be undefined.
          */
          PdfString defaultDirectoryObject = (PdfString)BaseDataObject[PdfName.D];
          if(defaultDirectoryObject == null)
            return null;

          return (string)defaultDirectoryObject.Value;
        }
        set
        {BaseDataObject[PdfName.D] = new PdfString(value);}
      }

      /**
        <summary>Gets/Sets the file name of the application to be launched
        or the document to be opened or printed.</summary>
      */
      public string FileName
      {
        get
        {
          /*
            NOTE: 'F' entry MUST exist.
          */
          return (string)((PdfString)BaseDataObject[PdfName.F]).Value;
        }
        set
        {BaseDataObject[PdfName.F] = new PdfString(value);}
      }

      /**
        <summary>Gets/Sets the operation to perform.</summary>
      */
      public OperationEnum Operation
      {
        get
        {return ToOperationEnum((PdfString)BaseDataObject[PdfName.O]);}
        set
        {BaseDataObject[PdfName.O] = ToCode(value);}
      }

      /**
        <summary>Gets/Sets the parameter string to be passed to the application.</summary>
      */
      public string ParameterString
      {
        get
        {
          /*
            NOTE: 'P' entry may be undefined.
          */
          PdfString parameterStringObject = (PdfString)BaseDataObject[PdfName.P];
          if(parameterStringObject == null)
            return null;

          return (string)parameterStringObject.Value;
        }
        set
        {BaseDataObject[PdfName.P] = new PdfString(value);}
      }
      #endregion
      #endregion
      #endregion
    }
    #endregion

    #region dynamic
    #region constructors
    /**
      <summary>Creates a new action within the given document context.</summary>
    */
    public Launch(
      Document context
      ) : base(
        context,
        PdfName.Launch
        )
    {}

    internal Launch(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      ) : base(baseObject, container, null)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets the application to be launched or the document to be opened or printed.</summary>
    */
    public FileSpec FileSpec
    {
      get
      {
        /*
          NOTE: 'F' entry may be undefined.
        */
        PdfDirectObject fileSpecObject = BaseDataObject[PdfName.F];
        if(fileSpecObject == null)
          return null;

        return new FileSpec(fileSpecObject,Container,null);
      }
      set
      {BaseDataObject[PdfName.F] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the action options.</summary>
    */
    public OptionsEnum Options
    {
      get
      {
        OptionsEnum options = 0;

        PdfDirectObject optionsObject = BaseDataObject[PdfName.NewWindow];
        if(optionsObject != null
          && (bool)((PdfBoolean)optionsObject).Value)
        {options |= OptionsEnum.NewWindow;}

        return options;
      }
      set
      {
        if((value & OptionsEnum.NewWindow) == OptionsEnum.NewWindow)
        {BaseDataObject[PdfName.NewWindow] = PdfBoolean.True;}
        else if((value & OptionsEnum.SameWindow) == OptionsEnum.SameWindow)
        {BaseDataObject[PdfName.NewWindow] = PdfBoolean.False;}
        else
        {BaseDataObject.Remove(PdfName.NewWindow);} // NOTE: Forcing the absence of this entry ensures that the viewer application should behave in accordance with the current user preference.
      }
    }

    /**
      <summary>Gets/Sets the Windows-specific launch parameters.</summary>
    */
    public WinParametersObject WinParameters
    {
      get
      {
        /*
          NOTE: 'Win' entry may be undefined.
        */
        PdfDictionary parametersObject = (PdfDictionary)BaseDataObject[PdfName.Win];
        if(parametersObject == null)
          return null;

        return new WinParametersObject(parametersObject,Container);
      }
      set
      {BaseDataObject[PdfName.Win] = value.BaseObject;}
    }
    #endregion
    #endregion
    #endregion
  }
}