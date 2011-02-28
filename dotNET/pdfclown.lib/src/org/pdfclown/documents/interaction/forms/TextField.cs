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
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.interaction.forms
{
  /**
    <summary>Text field [PDF:1.6:8.6.3].</summary>
  */
  [PDF(VersionEnum.PDF12)]
  public sealed class TextField
    : Field
  {
    #region dynamic
    #region constructors
    /**
      <summary>Creates a new text field within the given document context.</summary>
    */
    public TextField(
      string name,
      Widget widget,
      string value
      ) : base(
        PdfName.Tx,
        name,
        widget
        )
    {Value = value;}

    public TextField(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets whether the field can contain multiple lines of text.</summary>
    */
    public bool IsMultiline
    {
      get
      {return ((Flags & FlagsEnum.Multiline) == FlagsEnum.Multiline);}
      set
      {
        FlagsEnum flags = Flags;
        if(value)
        {flags |= FlagsEnum.Multiline;}
        else
        {flags ^= FlagsEnum.Multiline;}
        Flags = flags;
      }
    }

    /**
      <summary>Gets/Sets whether the field is intended for entering a secure password.</summary>
    */
    public bool IsPassword
    {
      get
      {return ((Flags & FlagsEnum.Password) == FlagsEnum.Password);}
      set
      {
        FlagsEnum flags = Flags;
        if(value)
        {flags |= FlagsEnum.Password;}
        else
        {flags ^= FlagsEnum.Password;}
        Flags = flags;
      }
    }

    /**
      <summary>Gets/Sets the maximum length of the field's text, in characters.</summary>
    */
    public int MaxLength
    {
      get
      {
        PdfInteger maxLengthObject = (PdfInteger)File.Resolve(
          GetInheritableAttribute(PdfName.MaxLen)
          );
        if(maxLengthObject == null)
          return Int32.MaxValue;

        return maxLengthObject.RawValue;
      }
      set
      {throw new NotImplementedException();}
    }

    /**
      <summary>Gets/Sets whether text entered in the field is spell-checked.</summary>
    */
    public bool SpellChecked
    {
      get
      {return !((Flags & FlagsEnum.DoNotSpellCheck) == FlagsEnum.DoNotSpellCheck);}
      set
      {
        FlagsEnum flags = Flags;
        if(value)
        {flags ^= FlagsEnum.DoNotSpellCheck;}
        else
        {flags |= FlagsEnum.DoNotSpellCheck;}
        Flags = flags;
      }
    }

    public override object Value
    {
      get
      {return base.Value;}
      set
      {BaseDataObject[PdfName.V] = new PdfTextString((string)value);}
    }
    #endregion
    #endregion
    #endregion
  }
}