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


namespace ReSignTool {
	static class Program {
		[STAThread]
		static int Main(string[] args) {
			ReSign rs;
			bool verbose;
			bool debug;
			string error;
			string config_file;


			verbose = false;
			debug = false;
			config_file = null;
			for (int i = 0; i < args.Length; i++) {
				if (args[i] == "-v") {
					verbose = true;
				} else if (args[i] == "-d") {
					debug = true;
				} else {
					config_file = args[i];
				}
			}

			if ((args.Length == 0) || (config_file == null)) {
				Console.WriteLine("Usage: ReSignTool [-v] [-d] <xml-configuration-file>");
				return 1;
			}

			rs = new ReSign(config_file);
			rs.Verbose = verbose;
			rs.Debug = debug;
			if (!rs.Process(out error)) {
				Console.WriteLine("Error: Re-signing failed with error: " + error);
				return 2;
			}

			return 0;
		}
	}
}
