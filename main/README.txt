PDF Clown Project



Project version: 0.1.1 - README revision: 0 (2011-11-14)

---------------
Introduction
---------------
This is the source code distribution of PDF Clown, a general-purpose library for the manipulation of PDF files implemented in multiple platforms (Java [../java/README.html], .NET [../dotNET/README.html]).


---------------
What's new?
---------------
This release [http://pdfclown.wordpress.com/2011/04/12/waiting-for-pdf-clown-0-1-1-release/] adds support to optional/layered contents, text highlighting, metadata streams (XMP), Type1/CFF font files, along with primitive object model and AcroForm fields filling enhancements.

Lots of minor improvements have been applied too.

 * [add] Primitive object model: see objects namespace (PdfObject, PdfObjectWrapper, PdfSimpleObject, PdfReal).
 * [add] Optional/layered contents: see documents.contents.layers namespace
 * [add] Text highlighting: see tools.TextExtractor, annotations.TextMarkup
 * [add] AcroForm fields filling: see documents.interaction.forms namespace
 * [add] Metadata streams (XMP): see documents.interchange.metadata.Metadata, PdfObjectWrapper.get/setMetadata(Metadata)
 * [add] Type1/CFF font files support: see fonts.CffParser
 * [add] File configuration: real number formatting (see files.File.Configuration)
 * [add] Page boxes: see documents.Page (get/setArtBox(Rectangle2D), get/setBleedBox(Rectangle2D), get/setCropBox(Rectangle2D), get/setTrimBox(Rectangle2D))
 * [add] PostScript-based parsers: see util.parsers.PostScriptParser, tokens.BaseParser, tokens.FileParser, documents.contents.tokens.ContentParser

---------------
Copyright
---------------
Copyright © 2006-2011 Stefano Chizzolini

Contacts:
 * url: http://www.stefanochizzolini.it [http://www.stefanochizzolini.it]


---------------
License
---------------
This program is free software [http://en.wikipedia.org/wiki/Free_software]; you can redistribute it and/or modify it under the terms of version 3 of the GNU Lesser General Public License as published by the Free Software Foundation.

References:
 * LGPL (GNU Lesser General Public License) version 3:
  * sources:
   * url: licenses/gnu.org/lgpl.html [licenses/gnu.org/lgpl.html]
   * url: http://www.gnu.org/licenses/lgpl.html [http://www.gnu.org/licenses/lgpl.html]
   * mail: Free Software Foundation, Inc., 51 Franklin St - Fifth Floor, Boston, MA 02110-1301 USA.
  * restrictions: none
  * extensions: none


---------------
Disclaimer
---------------
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for more details.

IN NO EVENT SHALL THE COPYRIGHT HOLDER AND CONTRIBUTORS BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE, EVEN IF THE COPYRIGHT HOLDER AND CONTRIBUTORS HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

THE SOFTWARE PROVIDED HEREIN IS ON AN "AS IS" BASIS, AND THE COPYRIGHT HOLDER AND CONTRIBUTORS HAVE NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS. THE COPYRIGHT HOLDER AND CONTRIBUTORS MAKE NO REPRESENTATIONS AND EXTEND NO WARRANTIES OF ANY KIND, EITHER IMPLIED OR EXPRESS, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE, OR THAT THE USE OF THE SOFTWARE WILL NOT INFRINGE ANY PATENT, TRADEMARK OR OTHER RIGHTS.


---------------
Community
---------------
The Community page [http://www.pdfclown.org/community.html] guides you through the resources that you can use to ask questions, request new features, report bugs, discuss and submit code contributions and keep yourself up-to-date about the project's development.


---------------
Updates
---------------
The  [http://sourceforge.net/projects/clown/] is hosted by SourceForge.net and referenced by the official PDF Clown's website [http://www.pdfclown.org/]: please AVOID downloading from any other repository if you want to be sure its updates can be trusted.

This distribution represents the result of a release cycle which tipically spans over several months: instead of waiting for the final release, you can keep your copy of the PDF Clown's code base up-to-date synchronizing it with the  [http://sourceforge.net/scm/?type=svn&group_id=176158]. You have just to choose the branch more appropriate for your needs:

 * Fix branch <https://clown.svn.sourceforge.net/svnroot/clown/branches/0.1.1-Fix [https://clown.svn.sourceforge.net/svnroot/clown/branches/0.1.1-Fix]>: corrective branch (bug fixes for existing functionalities);
 * Trunk <https://clown.svn.sourceforge.net/svnroot/clown/trunk [https://clown.svn.sourceforge.net/svnroot/clown/trunk]>: evolutionary branch (all the latest & greatest along with the same bug fixes of the above-mentioned Fix branch).

---------------
Support it!
---------------
Are you successfully using this software? Remember that behind it there are human beings who enjoyed donating some effort to craft a nice piece of code -- you can demonstrate your appreciation in several ways:

 * donate: even a little PayPal transfer [http://www.stefanochizzolini.it/en/projects/clown/#Donations] is welcome, just to cheer your success;
 * contribute: have you extended the library to cover new functionalities? have you written useful sample code or documentation you'd like to share? let us know!;
 * communicate: inform your colleagues and Web community about this project and promote the broader adoption of free software [http://en.wikipedia.org/wiki/Free_software].

---------------
Resources
---------------
 *  [../java/README.html]: PDF Clown implementation for Java
 *  [../dotNET/README.html]: PDF Clown implementation for .NET
 *  [licenses/README.html]: Licenses applied to the PDF Clown distribution
 *  [doc/README.html]: PDF Clown common guides
 *  [res/README.html]: Material supporting PDF Clown distribution
 *  [CREDITS.html]: Who's behind PDF Clown development
 *  [WHATSNEW.html]: New features of the PDF Clown Project
 *  [CHANGELOG.html]: Change chronology of the PDF Clown Project
 *  [ISSUES.html]: Known issues
 *  [TODO.html]: TODO list of the PDF Clown Project
 *  [INDEX.html]: Distribution map
 * PDF Clown home page [http://www.pdfclown.org]: Project home page
 * Navigation:
  * Current directory [.]: browse current section contents
  * Next section [../java/README.html]: move to next section
  * INDEX [INDEX.html]: move to the distribution map
