/*
  Copyright 2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.objects;

using System;
using System.Collections.Generic;

namespace org.pdfclown.documents.contents.layers
{
  /**
    <summary>Layer entity.</summary>
  */
  [PDF(VersionEnum.PDF15)]
  public class LayerEntity
    : PropertyList
  {
    #region types
    /**
      <summary>Membership visibility policy.</summary>
    */
    public enum VisibilityPolicyEnum
    {
      /**
        <summary>Visible only if all of the visibility layers are ON.</summary>
      */
      AllOn,
      /**
        <summary>Visible if any of the visibility layers are ON.</summary>
      */
      AnyOn,
      /**
        <summary>Visible if any of the visibility layers are OFF.</summary>
      */
      AnyOff,
      /**
        <summary>Visible only if all of the visibility layers are OFF.</summary>
      */
      AllOff
    }
    #endregion

    #region dynamic
    #region constructors
    internal LayerEntity(
      Document context,
      PdfName typeName
      ) : base(
        context,
        new PdfDictionary(
          new PdfName[]
          {PdfName.Type},
          new PdfDirectObject[]
          {typeName}
        ))
    {}

    internal LayerEntity(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the default membership.</summary>
      <remarks>This collection corresponds to the hierarchical relation between this layer entity
      and its ascendants.</remarks>
    */
    public virtual LayerMembership Membership
    {
      get
      {return null;}
    }

    /**
      <summary>Gets the layers whose states determine the visibility of content controlled by this
      entity.</summary>
    */
    public virtual IList<Layer> VisibilityLayers
    {
      get
      {return null;}
    }

    /**
      <summary>Gets/Sets the visibility policy of this entity.</summary>
    */
    public virtual VisibilityPolicyEnum VisibilityPolicy
    {
      get
      {return VisibilityPolicyEnum.AllOn;}
      set
      {}
    }
    #endregion
    #endregion
    #endregion
  }

  internal static class VisibilityPolicyEnumExtension
  {
    public static PdfName GetName(
      this LayerMembership.VisibilityPolicyEnum visibilityPolicy
      )
    {
      switch(visibilityPolicy)
      {
        case LayerMembership.VisibilityPolicyEnum.AllOn:
          return PdfName.AllOn;
        case LayerMembership.VisibilityPolicyEnum.AnyOn:
          return PdfName.AnyOn;
        case LayerMembership.VisibilityPolicyEnum.AnyOff:
          return PdfName.AnyOff;
        case LayerMembership.VisibilityPolicyEnum.AllOff:
          return PdfName.AllOff;
        default:
          throw new NotImplementedException();
      }
    }

    public static LayerMembership.VisibilityPolicyEnum ToEnum(
      PdfName name
      )
    {
      if(name == null || name.Equals(PdfName.AllOn))
        return LayerMembership.VisibilityPolicyEnum.AllOn;
      else if(name.Equals(PdfName.AnyOn))
        return LayerMembership.VisibilityPolicyEnum.AnyOn;
      else if(name.Equals(PdfName.AnyOff))
        return LayerMembership.VisibilityPolicyEnum.AnyOff;
      else if(name.Equals(PdfName.AllOff))
        return LayerMembership.VisibilityPolicyEnum.AllOff;
      else
        throw new NotSupportedException("Visibility policy unknown: " + name);
    }
  }
}