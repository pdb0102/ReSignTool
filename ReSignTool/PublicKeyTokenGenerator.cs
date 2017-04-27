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
using System.Reflection;
using System.Security.Cryptography;

namespace ReSignTool {
	public class PublicKeyTokenGenerator {
		private byte[] public_key;

		public PublicKeyTokenGenerator(byte[] public_key) {
			this.public_key = public_key;
		}

		private static byte[] ExtractAndReverseBytes(byte[] hash) {
			byte[] publicKeyToken;

			publicKeyToken = new byte[8];
			Array.Copy(hash, hash.Length - publicKeyToken.Length, publicKeyToken, 0, publicKeyToken.Length);
			Array.Reverse(publicKeyToken, 0, publicKeyToken.Length);

			return publicKeyToken;
		}

		private byte[] ComputeSHA1Hash() {
			SHA1Managed sha1;
			byte[] hash;

			sha1 = new SHA1Managed();
			hash = sha1.ComputeHash(public_key);
			sha1.Dispose();

			return hash;
		}

		public static byte[] Token(byte[] public_key) {
			PublicKeyTokenGenerator gen;

			gen = new PublicKeyTokenGenerator(public_key);
			return ExtractAndReverseBytes(gen.ComputeSHA1Hash());
		}

		public static byte[] Token(StrongNameKeyPair keypair) {
			return Token(keypair.PublicKey);
		}
	}
}
