using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace RB3Tool.x360
{
    /// <summary>
    /// RC4 Kerberos extensions
    /// </summary>
    [DebuggerStepThrough]
    public static class KerbExtenz
    {
        /// <summary>
        /// Runs a Kerberos implemented RC4 decryption
        /// </summary>
        /// <param name="xConfounder">The confounder input</param>
        /// <param name="xPayload">The payload input</param>
        /// <param name="x">RC4 reference</param>
        /// <param name="xData">Data to be decrypted</param>
        /// <param name="xConLen">Length of the Confounder</param>
        /// <returns></returns>
        public static bool KerberosDecrypt(this RC4 x, byte[] xData, out byte[] xConfounder, int xConLen, out byte[] xPayload)
        {
            xPayload = new byte[0];
            xConfounder = new byte[0];
            try
            {
                var xOut = new DJsIO(x.RunAlgorithm(xData), true) { Position = 0 };
                xConfounder = xOut.ReadBytes(xConLen);
                xPayload = xOut.ReadBytes(xData.Length - xConLen);
                xOut.Dispose();
            }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Runs a Kerberos implemented RC4 Encryption
        /// </summary>
        /// <param name="xConfounder">Outputs new Confounder</param>
        /// <param name="xPayload">Outputs the payload</param>
        /// <param name="x">RC4 Reference</param>
        /// <returns></returns>
        public static byte[] KerberosEncrypt(this RC4 x, ref byte[] xConfounder, ref byte[] xPayload)
        {
            var xIn = new List<byte>();
            xIn.AddRange(xConfounder);
            xIn.AddRange(xPayload);
            return x.RunAlgorithm(xIn.ToArray());
        }
    }

    /// <summary>
    /// Cryptographic exceptions
    /// </summary>
    [DebuggerStepThrough]
    public static class CryptoExcepts
    {
        [CompilerGenerated]
        static readonly Exception xCryptoSign = new Exception("Error when taking the signature");
        [CompilerGenerated]
        static readonly Exception xCryptoVeri = new Exception("Unknown error when trying to verify signature");
        [CompilerGenerated]
        static readonly Exception xRC4KeyError = new Exception("Unknown error when trying to set key");
        [CompilerGenerated]
        static readonly Exception xRC4AlgoError = new Exception("Unknown error when trying to run algorithm");
        [CompilerGenerated]
        static readonly Exception xKVError = new Exception("Error Parsing Keyvault");
        [CompilerGenerated]
        static readonly Exception xKVSize = new Exception("Keyvault not the correct size");
        [CompilerGenerated]
        static readonly Exception xCertConflict = new Exception("Certificate not the same as the keys");
        [CompilerGenerated]
        static readonly Exception xParamError = new Exception("Params invalid");

        /// <summary>Signing error</summary>
        public static Exception CryptoSign { get { return xCryptoSign; } }
        /// <summary>Verifying error</summary>
        public static Exception CryptoVeri { get { return xCryptoVeri; } }
        /// <summary>KeyVault error</summary>
        public static Exception KVSize { get { return xKVSize; } }
        /// <summary>Certificate error</summary>
        public static Exception CertConflict { get { return xCertConflict; } }
        /// <summary>Invalid parameter</summary>
        public static Exception ParamError { get { return xParamError; } }
    }

    /// <summary>
    /// Initializes a new instance of an RC4 class
    /// </summary>
    [DebuggerStepThrough]
    public sealed class RC4
    {
        // RC4 Key
        byte[] xKeyRC4_Bb; // Before key algo
        byte[] xKeyRC4_Ba; // After after key algo

        /// <summary>
        /// Initializes this class with a specified binary key
        /// </summary>
        /// <param name="xKey"></param>
        public RC4(byte[] xKey) { KeyBinary = xKey; }

        /// <summary>
        /// Gets or sets the key used in this RC4 instance
        /// </summary>
        public byte[] KeyBinary
        {
            get { return xKeyRC4_Bb; }
            set
            {
                if (xKeyRC4_Bb == value)
                    return;
                xKeyRC4_Bb = value;
                xKeyRC4_Ba = new byte[0x100];
                var j = 0;
                for (short i = 0; i < 0x100; i++)
                    xKeyRC4_Ba[i] = (byte)i;
                for (short i = 0; i < 0x100; i++)
                {
                    j = (j + xKeyRC4_Ba[i] + value[i % value.Length]) % 0x100;
                    swap(ref xKeyRC4_Ba, i, j);
                }
            }
        }

        static void swap(ref byte[] xIn, int i, int j)
        {
            var temp = xIn[i];
            xIn[i] = xIn[j];
            xIn[j] = temp;
        }

        /// <summary>
        /// Encrypts or decrypts the data
        /// </summary>
        /// <param name="xData"></param>
        /// <returns></returns>
        public byte[] RunAlgorithm(byte[] xData)
        {
            int i = 0, j = 0;
            var xReturn = new byte[xData.Length];
            var n_LocBox = new byte[0x100];
            xKeyRC4_Ba.CopyTo(n_LocBox, 0);
            for (var offset = 0; offset < xData.Length; offset++)
            {
                i = (i + 1) % 0x100;
                j = (j + n_LocBox[i]) % 0x100;
                swap(ref n_LocBox, i, j);
                xReturn[offset] = (byte)(xData[offset] ^ n_LocBox[(n_LocBox[i] + n_LocBox[j]) % 0x100]);
            }
            return xReturn;
        }
    }

    /// <summary>
    /// Scrambling methods used in this .DLL
    /// </summary>
    [DebuggerStepThrough]
    public static class ScrambleMethods
    {
        /// <summary>
        /// Swaps bytes in 8 byte blocks, xReverse true if reverse each 8 byte blocks
        /// </summary>
        /// <param name="xPiece"></param>
        /// <param name="xReverse"></param>
        /// <returns></returns>
        public static byte[] StockScramble(byte[] xPiece, bool xReverse)
        {
            if ((xPiece.Length % 8) != 0)
                throw new Exception("Input not divisible by 8");
            var xStream = new DJsIO(xPiece, true);
            for (var i = 0; i < (xPiece.Length / 2); i += 8)
            {
                xStream.Position = i;
                var xPart1 = xStream.ReadBytes(8);
                xStream.Position = (xPiece.Length - i - 8);
                var xPart2 = xStream.ReadBytes(8);
                xStream.Position = i;
                xStream.Write(xPart2);
                xStream.Position = (xPiece.Length - i - 8);
                xStream.Write(xPart1);
                xStream.Flush();
            }
            if (!xReverse) return xPiece;
            for (var i = 0; i < xPiece.Length; i += 8)
                Array.Reverse(xPiece, i, 8);
            return xPiece;
        }

        /// <summary>
        /// Reverses all bytes
        /// </summary>
        /// <param name="xPiece"></param>
        /// <returns></returns>
        public static byte[] DevScramble(byte[] xPiece)
        {
            Array.Reverse(xPiece);
            return xPiece;
        }
    }

    /// <summary>
    /// Create and verify PKS1 Signatures of SHA1 digest
    /// </summary>
    [DebuggerStepThrough]
    public static class RSAQuick
    {
        /// <summary>
        /// Generate a PKS1 Signature of SHA1 digest
        /// </summary>
        /// <param name="xParam"></param>
        /// <param name="xHash"></param>
        /// <returns></returns>
        public static byte[] SignatureGenerate(RSAParameters xParam, byte[] xHash)
        {
            var xRSACrypto = new RSACryptoServiceProvider();
            var xRSASigFormat = new RSAPKCS1SignatureFormatter();
            xRSACrypto.ImportParameters(xParam);
            xRSASigFormat.SetHashAlgorithm("SHA1");
            xRSASigFormat.SetKey(xRSACrypto);
            try { return xRSASigFormat.CreateSignature(xHash); }
            catch { throw CryptoExcepts.CryptoSign; }
        }

        /// <summary>
        /// Verifies a PKS1 signature of SHA1 digest
        /// </summary>
        /// <param name="xParam">Keys</param>
        /// <param name="xHash">Hash to sign</param>
        /// <param name="xSignature">Outputs the signature</param>
        /// <returns></returns>
        public static bool SignatureVerify(RSAParameters xParam, byte[] xHash, byte[] xSignature)
        {
            var xRSACrypto = new RSACryptoServiceProvider();
            var xRSASigDeformat = new RSAPKCS1SignatureDeformatter();
            xRSACrypto.ImportParameters(xParam);
            xRSASigDeformat.SetHashAlgorithm("SHA1");
            xRSASigDeformat.SetKey(xRSACrypto);
            try { return xRSASigDeformat.VerifySignature(xHash, xSignature); }
            catch { throw CryptoExcepts.CryptoVeri; }
        }
    }

    /// <summary>
    /// Initializes a new instance of a SHA1 hash
    /// </summary>
    [DebuggerStepThrough]
    public static class SHA1Quick
    {
        /// <summary>
        /// Computes a SHA1 hash on specified data
        /// </summary>
        /// <param name="xData"></param>
        /// <returns></returns>
        public static byte[] ComputeHash(byte[] xData)
        {
            return new SHA1CryptoServiceProvider().ComputeHash(xData);
        }
    }
}