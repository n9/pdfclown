/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.objects
{
  /**
    <summary>PDF integer number object [PDF:1.6:3.2.2].</summary>
  */
  public sealed class PdfInteger
    : PdfAtomicObject<int>,
      IPdfNumber
  {
    #region dynamic
    #region constructors
    public PdfInteger(
      int value
      )
    {RawValue = value;}
    #endregion

    #region interface
    #region public
    public override int CompareTo(
      PdfDirectObject obj
      )
    {return PdfNumber.Compare(this,obj);}

    public override bool Equals(
      object obj
      )
    {return PdfNumber.Equal(this,obj);}

    public override int GetHashCode(
      )
    {return PdfNumber.GetHashCode(this);}

    public int IntValue
    {
      get
      {return this.RawValue;}
    }

    public override void WriteTo(
      IOutputStream stream
      )
    {stream.Write(RawValue.ToString());}

    #region IPdfNumber
    float IPdfAtomicObject<float>.RawValue
    {
      get
      {return this.RawValue;}
      set
      {this.RawValue = (int)value;}
    }

    object IPdfAtomicObject<float>.Value
    {
      get
      {return this.Value;}
      set
      {this.Value = value;}
    }
    #endregion
    #endregion
    #endregion
    #endregion
  }
}