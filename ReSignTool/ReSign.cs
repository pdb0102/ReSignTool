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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Mono.Cecil;
using Mono.Collections.Generic;

namespace ReSignTool {
	public class ReSign {
		private Dictionary<string, SigningKey> keys;
		private Dictionary<string, DllReference> references;
		private List<DllToSign> signdlls;
		private bool only_sign_on_match;
		private string output_dir;
		private string reference_dir;
		private string configuration_file;

		public ReSign() {
			keys = new Dictionary<string,SigningKey>(StringComparer.InvariantCultureIgnoreCase);
			references = new Dictionary<string, DllReference>(StringComparer.InvariantCultureIgnoreCase);
			signdlls = new List<DllToSign>();
		}

		public ReSign(string configuration_file) : this() {
			this.configuration_file = configuration_file;
		}

		public bool Verbose { get; set; }
		public bool Debug { get; set; }
		public AssemblyDefinition HaveReferenceAssembly(string name) {
			if (references.ContainsKey(name)) {
				return references[name].Module.Assembly;
			}

			return null;
		}

		public string ConfigurationFile {
			get {
				return configuration_file;
			}

			set {
				configuration_file = value;
			}
		}

		public string ReferenceDir {
			get {
				return reference_dir;
			}
		}

		public bool Process(out string error) {
			WriterParameters wp;

			try {
				if (string.IsNullOrEmpty(configuration_file)) {
					error = "No configuration file set";
					return false;
				}

				if (!File.Exists(configuration_file)) {
					error = "Configuration file '" + configuration_file + "' not found";
					return false;
				}

				if (!ReadConfiguration(out error)) {
					error = "Processing configuration: " + error;
					return false;
				}

				if (!Directory.Exists(output_dir)) {
					if (Verbose) {
						Console.Write("Info: Creating output directory '" + output_dir + "'");
					}
					Directory.CreateDirectory(output_dir);
				} else {
					if (Verbose) {
						Console.WriteLine("Info: Using output directory '" + output_dir + "'");
					}
				}
				if (Verbose) {
					if (only_sign_on_match) {
						Console.WriteLine("Info: Only signing and writing assemblies where referenced assemblies were replaced");
					} else {
						Console.WriteLine("Info: Signing and writing all assemblies");
					}
				}

				foreach (DllToSign module in signdlls) {
					foreach (KeyValuePair<string, DllReference> reference in references) {
						if (ReplaceModuleReference(module, reference.Value)) {
							module.ReferenceFixed = true;
						}
					}
				}

				// Fix up any attributes that might contain a reference to any changed assemblies
				foreach (DllToSign module in signdlls) {
					if (keys.ContainsKey(module.Key)) {
						wp = new WriterParameters();
						wp.StrongNameKeyPair = keys[module.Key].KeyPair;
					} else {
						wp = null;
					}

					if (module.ReferenceFixed || !only_sign_on_match) {
						if (wp != null) {
							if (Verbose) {
								Console.WriteLine("Info: Signing and writing updated module '" + Path.GetFileName(module.Filename) + "'");
							}
							module.Module.Write(Path.Combine(output_dir, Path.GetFileName(module.Filename)), wp);
						} else {
							if (Verbose) {
								if ((module.Module.Attributes & ModuleAttributes.StrongNameSigned) == ModuleAttributes.StrongNameSigned) {
									Console.WriteLine("Info: Delay-signing and writing updated module '" + Path.GetFileName(module.Filename) + "'");
								} else {
									Console.WriteLine("Info: Writing updated module '" + Path.GetFileName(module.Filename) + "'");
								}
							}
							module.Module.Write(Path.Combine(output_dir, Path.GetFileName(module.Filename)));
						}
					}
				}
				error = string.Empty;
				return true;
			} catch (Exception ex) {
				error = ex.ToString();
				return false;
			}
		}
											
		private bool ReplaceModuleReference(DllToSign module, DllReference reference) {
			bool changed;
			ModuleDefinition md_ref;
			ModuleDefinition md_sign;

			md_ref = reference.Module;
			md_sign = module.Module;

			changed = false;

			for (int i = 0; i < md_sign.AssemblyReferences.Count; i++) {
				// Find any references to our 'reference' DLLs and update them
				if (md_sign.AssemblyReferences[i].Name == Path.GetFileNameWithoutExtension(md_ref.Name)) {
					if (Debug) {
						Console.WriteLine("Debug: Replacing reference to {0}, [{1}, {2}] with [{3}, {4}] in module {5}", 
							md_sign.AssemblyReferences[i].Name,
							md_sign.AssemblyReferences[i].Version,
							PrintHex(md_sign.AssemblyReferences[i].PublicKeyToken),
							md_ref.Assembly.Name.Version,
							PrintHex(md_ref.Assembly.Name.PublicKeyToken),
							Path.GetFileNameWithoutExtension(module.Filename)
						);
					}
					// We have a match - we need to replace they token and version
					md_sign.AssemblyReferences[i].PublicKeyToken = md_ref.Assembly.Name.PublicKeyToken;
					md_sign.AssemblyReferences[i].Version = md_ref.Assembly.Name.Version;
					module.ReferenceFixed = true;
				}

				// Find any references to our 'to sign' DLLs and replace the key token with the one we'll use to sign
				for (int j = 0; j < signdlls.Count; j++) {
					if (md_sign.AssemblyReferences[i].Name == Path.GetFileNameWithoutExtension(signdlls[j].Module.Name)) {
						if (Debug) {
							Console.WriteLine("Debug: Swapping key token for {0} from {1} to {2} in module {3}",
								Path.GetFileNameWithoutExtension(signdlls[j].Module.Name),
								PrintHex(md_sign.AssemblyReferences[i].PublicKeyToken),
								PrintHex(PublicKeyTokenGenerator.Token(keys[signdlls[j].Key].KeyPair)),
								md_sign.Name
							);
						}
						md_sign.AssemblyReferences[i].PublicKeyToken = PublicKeyTokenGenerator.Token(keys[signdlls[j].Key].KeyPair);
					}
				}
			}
			
			return changed;
		}

		private bool ReadConfiguration(out string error) {
			XmlNamespaceManager ns_mgr;
			XmlDocument doc;
			XmlNodeList nodes;
			XmlNode node;
			SigningKey key;
			DllReference reference;
			DllToSign sign;
			ResignAssemblyResolver resolver;

			error = string.Empty;
			resolver = new ResignAssemblyResolver(this);
			try {
				doc = new XmlDocument();
				doc.Load(configuration_file);
				ns_mgr = new XmlNamespaceManager(doc.NameTable);

				node = doc.SelectSingleNode("/resign/sign");
				if (node != null) {
					reference_dir = GetAttribute(node, "referencedir", string.Empty);
				}

				nodes = doc.SelectNodes("/resign/signingkey");
				foreach(XmlNode key_node in nodes) {
					key = new SigningKey();
					key.Name = GetAttribute(key_node, "name", string.Empty);
					key.Filename = GetAttribute(key_node, "file", string.Empty);
					key.Container = GetAttribute(key_node, "container", string.Empty);
					if (!string.IsNullOrEmpty(key.Filename) && !File.Exists(key.Filename)) {
						error = "signingkey tag '" + key.Name + "' references non-existing file '" + key.Filename + "'";
						return false;
					}

					keys.Add(key.Name, key);
				}

				nodes = doc.SelectNodes("/resign/references/dll");
				foreach (XmlNode ref_node in nodes) {
					reference = new DllReference();
					reference.Name = GetAttribute(ref_node, "name", string.Empty);
					reference.Filename = GetAttribute(ref_node, "path", string.Empty);
					if (!File.Exists(reference.Filename)) {
						error = "references/dll tag '" + reference.Name + "' references non-existing file '" + reference.Filename + "'";
						return false;
					}
					references.Add(Path.GetFileNameWithoutExtension(reference.Name), reference);
				}

				nodes = doc.SelectNodes("/resign/sign/dll");
				foreach (XmlNode dll_node in nodes) {
					sign = new DllToSign(resolver);
					sign.Filename = GetAttribute(dll_node, "path", string.Empty);
					if (!File.Exists(sign.Filename)) {
						error = "dll tag references non-existing path '" + sign.Filename + "'";
						return false;
					}

					sign.Key = GetAttribute(dll_node, "key", string.Empty);
					if (!keys.Keys.Contains(sign.Key)) {
						error = "dll tag '" + sign.Filename + "' references undefined key '" + sign.Key;
						return false;
					}
					signdlls.Add(sign);
				}

				node = doc.SelectSingleNode("/resign/sign");
				output_dir = GetAttribute(node, "outputdir", string.Empty);
				only_sign_on_match = GetAttribute(node, "changedonly", true);

				return true;
			} catch (Exception ex) {
				error = ex.ToString();
				return false;
			}
		}


		private static string PrintHex(byte[] hex) {
			StringBuilder sb;

			sb = new StringBuilder();
			for (int i = 0; i < hex.Length; i++) {
				sb.Append(string.Format("{0:X2}", hex[i]));
			}
			return sb.ToString();
		}

		private static string GetText(XmlNamespaceManager ns_mgr, XmlNode node, string child_xpath, string default_value) {
			XmlNode child;

			if (node == null) {
				return default_value;
			}

			child = node.SelectSingleNode(child_xpath, ns_mgr);
			if (child != null) {
				return child.InnerText.Trim();
			}
			return default_value;
		}

		private static DateTime GetDate(XmlNamespaceManager ns_mgr, XmlNode node, string child_xpath) {
			DateTime dt;
			string s;

			s = GetText(ns_mgr, node, child_xpath, null);
			if (s != null) {
				if (DateTime.TryParse(s, out dt)) {
					return dt;
				}
			}
			return DateTime.Now;
		}

		private static T GetAttribute<T>(XmlNode node, string attr_name, T default_value) {
			XmlNode n;
			if (node == null) {
				return default_value;
			}

			n = node.Attributes.GetNamedItem(attr_name);
			if (n == null) {
				return default_value;
			}

			var s = n.Value;
			if (s != null) {
				return (T)Convert.ChangeType(s, typeof(T));
			}
			return default_value;
		}


	}
}
