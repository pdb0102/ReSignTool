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
	public class DllReference {
		private string name;
		private string filename;
		private ModuleDefinition module;

		public DllReference() {
		}

		public DllReference(string name, string file) {
			Name = name;
			Filename = file;
			GetInfo();
		}

		public string Name {
			get {
				return name;
			}

			set {
				name = value;
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

		public ModuleDefinition Module {
			get {
				return module;
			}
		}

		public byte[] PublicKey { get; set; }
		public Version Version { get; set; }

		private void GetInfo() {
			module = ModuleDefinition.ReadModule(filename);
			Version = module.Assembly.Name.Version;
			PublicKey = module.Assembly.Name.PublicKeyToken;
		}
	}
}
