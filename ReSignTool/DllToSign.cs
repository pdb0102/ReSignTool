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

using System;

using Mono.Cecil;

namespace ReSignTool {
	public class DllToSign {
		private string key;
		private string filename;
		private ModuleDefinition module;
		private ResignAssemblyResolver resolver;

		public DllToSign(ResignAssemblyResolver resolver) {
			this.resolver = resolver;
		}

		public DllToSign(string key, string file) {
			Key = key;
			Filename = file;
			GetInfo();
		}

		public string Key {
			get {
				return key;
			}

			set {
				key = value;
			}
		}

		public string Filename {
			get {
				return filename;
			}

			set {
				filename = value;
				GetInfo();
			}
		}

		public bool ReferenceFixed { get; set; }

		public ModuleDefinition Module {
			get {
				return module;
			}
		}

		public byte[] PublicKey { get; set; }
		public Version Version { get; set; }

		private void GetInfo() {
			module = ModuleDefinition.ReadModule(filename, new ReaderParameters() { ReadingMode = ReadingMode.Immediate, AssemblyResolver = resolver } );
			Version = module.Assembly.Name.Version;
			PublicKey = module.Assembly.Name.PublicKeyToken;
		}
	}
}
