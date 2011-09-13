// 
// GdkPixbufFormat.cs
//  
// Author:
//       Maia Kozheva <sikon@ubuntu.com>
// 
// Copyright (c) 2010 Maia Kozheva <sikon@ubuntu.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;

using Gdk;

namespace Pinta.Core
{
	public class GdkPixbufFormat: IImageImporter, IImageExporter
	{
		private string filetype;

		public GdkPixbufFormat(string filetype)
		{
			this.filetype = filetype;
		}
	
		public void Import (string fileName)
		{
			Pixbuf bg;

			// Handle any EXIF orientation flags
			using (var fs = new FileStream (fileName, FileMode.Open, FileAccess.Read))
				bg = new Pixbuf (fs);

			bg = bg.ApplyEmbeddedOrientation ();

			Size imagesize = new Size (bg.Width, bg.Height);

			Document doc = PintaCore.Workspace.CreateAndActivateDocument (fileName, imagesize);
			doc.HasFile = true;
			doc.ImageSize = imagesize;
			doc.Workspace.CanvasSize = imagesize;

			Layer layer = doc.AddNewLayer (Path.GetFileName (fileName));

			using (Cairo.Context g = new Cairo.Context (layer.Surface)) {
				CairoHelper.SetSourcePixbuf (g, bg, 0, 0);
				g.Paint ();
			}

			bg.Dispose ();
		}
		
		protected virtual void DoSave (Pixbuf pb, string fileName, string fileType)
		{
			pb.Save (fileName, fileType);
		}

		public void Export (Document document, string fileName)
		{
			Cairo.ImageSurface surf = document.GetFlattenedImage ();
	
			Pixbuf pb = surf.ToPixbuf ();
			DoSave(pb, fileName, filetype);

			(pb as IDisposable).Dispose ();
			(surf as IDisposable).Dispose ();
		}
	}
}
