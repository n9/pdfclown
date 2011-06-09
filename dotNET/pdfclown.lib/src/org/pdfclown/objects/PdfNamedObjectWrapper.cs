/*
  Copyright 2009-2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.documents;
using org.pdfclown.files;

namespace org.pdfclown.objects
{
  /**
    <summary>High-level representation of a strongly-typed PDF object
    that can be referenced also through a name. When such a name exists, the object
    is called 'named object', otherwise 'explicit object'.</summary>
    <remarks>
      <para>Some categories of objects in a PDF file can be referred to by name
      rather than by object reference. The correspondence between names and objects
      is established by the document's name dictionary [PDF:1.6:3.6.3].</para>
      <para>The name's purpose is to provide a further level of referential abstraction
      especially for references across diverse PDF documents.</para>
    </remarks>
  */
  public abstract class PdfNamedObjectWrapper<TDataObject>
    : PdfObjectWrapper<TDataObject>
    where TDataObject : PdfDataObject
  {
    #region dynamic
    #region fields
    private PdfString name;
    #endregion

    #region constructors
    protected PdfNamedObjectWrapper(
      Document context,
      TDataObject baseDataObject
      ) : this(context.File.Register(baseDataObject), null)
    {}

    /**
      <param name="baseObject">Base PDF object. MUST be a <see cref="PdfReference"/>
      everytime available.</param>
      <param name="name">Object name.</param>
    */
    protected PdfNamedObjectWrapper(
      PdfDirectObject baseObject,
      PdfString name
      ) : base(baseObject)
    {this.name = name;}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the object name.</summary>
    */
    public PdfString Name
    {
      get
      {return name;}
      internal set
      {name = value;}
    }

    /**
      <summary>Gets the object name, if available;
      otherwise, behaves like <see cref="PdfObjectWrapper.BaseObject">BaseObject</see>.</summary>
    */
    public PdfDirectObject NamedBaseObject
    {
      get
      {
        if(name != null)
          return name;
        else
          return BaseObject;
      }
    }
    #endregion
    #endregion
    #endregion
  }
}