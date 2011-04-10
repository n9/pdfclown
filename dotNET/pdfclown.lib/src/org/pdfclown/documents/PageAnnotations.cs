/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.objects;
using org.pdfclown.util.collections.generic;

using System;
using System.Collections;
using System.Collections.Generic;

namespace org.pdfclown.documents
{
  /**
    <summary>Page annotations [PDF:1.6:3.6.2].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class PageAnnotations
    : PdfObjectWrapper<PdfArray>,
      IList<Annotation>
  {
    #region types
    public class Enumerator
      : IEnumerator<Annotation>
    {
      /** <summary>Collection size.</summary> */
      private int count;
      /** <summary>Index of the next item.</summary> */
      private int index = 0;

      /** <summary>Current annotation.</summary> */
      private Annotation current;
      /** <summary>Annotation collection.</summary> */
      private PageAnnotations annotations;

      internal Enumerator(
        PageAnnotations annotations
        )
      {
        this.annotations = annotations;
        count = annotations.Count;
      }

      Annotation IEnumerator<Annotation>.Current
      {get{return current;}}

      public object Current
      {get{return ((IEnumerator<Annotation>)this).Current;}}

      public bool MoveNext(
        )
      {
        if(index == count)
          return false;

        current = annotations[index++];
        return true;
      }

      public void Reset(
        )
      {throw new NotSupportedException();}

      public void Dispose(
        )
      {}
    }
    #endregion

    #region dynamic
    #region fields
    private Page page;
    #endregion

    #region constructors
    internal PageAnnotations(
      PdfDirectObject baseObject,
      Page page
      ) : base(baseObject)
    {this.page = page;}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets the page associated to these annotations.</summary>
    */
    public Page Page
    {get{return page;}}

    #region IList<Annotation>
    public int IndexOf(
      Annotation value
      )
    {return BaseDataObject.IndexOf(value.BaseObject);}

    public void Insert(
      int index,
      Annotation value
      )
    {BaseDataObject.Insert(index,value.BaseObject);}

    public void RemoveAt(
      int index
      )
    {BaseDataObject.RemoveAt(index);}

    public Annotation this[
      int index
      ]
    {
      get
      {return Annotation.Wrap(BaseDataObject[index]);}
      set
      {BaseDataObject[index] = value.BaseObject;}
    }

    #region ICollection<Annotation>
    public void Add(
      Annotation value
      )
    {
      // Assign the annotation to the page!
      value.BaseDataObject[PdfName.P] = value.BaseObject;

      BaseDataObject.Add(value.BaseObject);
    }

    public void Clear(
      )
    {BaseDataObject.Clear();}

    public bool Contains(
      Annotation value
      )
    {return BaseDataObject.Contains(value.BaseObject);}

    public void CopyTo(
      Annotation[] values,
      int index
      )
    {throw new NotImplementedException();}

    public int Count
    {get{return BaseDataObject.Count;}}

    public bool IsReadOnly
    {get{return false;}}

    public bool Remove(
      Annotation value
      )
    {return BaseDataObject.Remove(value.BaseObject);}

    #region IEnumerable<Annotation>
    public IEnumerator<Annotation> GetEnumerator(
      )
    {return new Enumerator(this);}

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator(
      )
    {return this.GetEnumerator();}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
  }
}