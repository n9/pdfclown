/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library"
  (the Program): see the accompanying README files for more info.

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

package org.pdfclown.objects;

import org.pdfclown.tokens.ObjectStream;
import org.pdfclown.tokens.XRefStream;

/**
  Visitor interface.
  
  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.2
  @version 0.1.2, 09/24/12
*/
public interface IVisitor
{
  /**
    Visits an object array.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfArray object,
    Object data
    );

  /**
    Visits a boolean object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfBoolean object,
    Object data
    );

  /**
    Visits a date object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfDate object,
    Object data
    );
  
  /**
    Visits an object dictionary.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfDictionary object,
    Object data
    );
  
  /**
    Visits an indirect object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfIndirectObject object,
    Object data
    );
  
  /**
    Visits an integer-number object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfInteger object,
    Object data
    );
  
  /**
    Visits a name object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfName object,
    Object data
    );
  
  /**
    Visits an object stream.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    ObjectStream object,
    Object data
    );
  
  /**
    Visits a real-number object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfReal object,
    Object data
    );
  
  /**
    Visits a reference object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfReference object,
    Object data
    );
  
  /**
    Visits a stream object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfStream object,
    Object data
    );
  
  /**
    Visits a string object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfString object,
    Object data
    );
  
  /**
    Visits a text string object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    PdfTextString object,
    Object data
    );
  
  /**
    Visits a cross-reference stream object.
  
    @param object Visited object.
    @param data Supplemental data.
    @return Whether visit can be continued.
  */
  boolean visit(
    XRefStream object,
    Object data
    );
}
