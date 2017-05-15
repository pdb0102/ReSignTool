// MIT License
// 
// Copyright (c) 2017 Peter Dennis Bartok
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

// Based on:
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2015 Jb Evain
// Copyright (c) 2008 - 2011 Novell, Inc.
//
// Licensed under the MIT/X11 license.
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

namespace ReSignTool {
	public class ResignAssemblyResolver : BaseAssemblyResolver {
		readonly IDictionary<string, AssemblyDefinition> cache;
		private ReSign resign;

		public ResignAssemblyResolver(ReSign resign) {
			this.resign = resign;
			cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name) {
			AssemblyDefinition assembly;

			assembly = resign.HaveReferenceAssembly(name.Name);
			if (assembly != null) {
				name.IsRetargetable = true;
				return assembly;
			}

			if (cache.TryGetValue(name.FullName, out assembly)) {
				return assembly;
			}

			assembly = base.Resolve(name);
			cache[name.FullName] = assembly;

			return assembly;
		}

		protected void RegisterAssembly(AssemblyDefinition assembly) {
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			var name = assembly.Name.FullName;
			if (cache.ContainsKey(name))
				return;

			cache[name] = assembly;
		}

		protected override void Dispose(bool disposing) {
			foreach (var assembly in cache.Values) {
				assembly.Dispose();
			}

			cache.Clear();

			base.Dispose(disposing);
		}
	}
}
