/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.tokens;

using System.Collections.Generic;

namespace org.pdfclown.objects
{
  /**
    <summary>Visitor object.</summary>
  */
  public class Visitor
    : IVisitor
  {
    public virtual bool Visit(
      ObjectStream obj,
      object data
      )
    {
      foreach(PdfDataObject value in obj.Values)
      {
        if(!value.Accept(this, data))
          return false;
      }
      return true;
    }

    public virtual bool Visit(
      PdfArray obj,
      object data
      )
    {
      foreach(PdfDirectObject item in obj)
      {
        if(item == null)
          continue;

        if(!item.Accept(this, data))
          return false;
      }
      return true;
    }

    public virtual bool Visit(
      PdfBoolean obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfDate obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfDictionary obj,
      object data
      )
    {
      foreach(PdfDirectObject value in obj.Values)
      {
        if(value == null)
          continue;

        if(!value.Accept(this, data))
          return false;
      }
      return true;
    }

    public virtual bool Visit(
      PdfIndirectObject obj,
      object data
      )
    {
      PdfDataObject dataObject = obj.DataObject;
      return dataObject != null ? dataObject.Accept(this, data) : true;
    }

    public virtual bool Visit(
      PdfInteger obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfName obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfReal obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfReference obj,
      object data
      )
    {return obj.IndirectObject.Accept(this, data);}

    public virtual bool Visit(
      PdfStream obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfString obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      PdfTextString obj,
      object data
      )
    {return true;}

    public virtual bool Visit(
      XRefStream obj,
      object data
      )
    {return true;}
  }
}