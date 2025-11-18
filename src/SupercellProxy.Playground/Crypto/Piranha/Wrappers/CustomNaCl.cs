namespace SupercellProxy.Crypto.Piranha.Wrappers
{
    /// <summary>
    /// Custom written NaCl version
    /// </summary>
    public class CustomNaCl
    {
        /// <summary>
        /// Decrypts a public-box ciphertext
        /// </summary>
        /// <param name="cipher">Ciphertext to decrypt</param>
        /// <param name="nonce">24-byte Nonce</param>
        /// <param name="secretKey">32-byte SecretKey</param>
        /// <param name="publicKey">32-byte PublicKey</param>
        /// <returns>Decrypted plaintext</returns>
        public static byte[] OpenPublicBox(byte[] cipher, byte[] nonce, byte[] secretKey, byte[] publicKey)
        {
            return new PublicBox(secretKey, publicKey).Open(cipher, nonce);
        }

        /// <summary>
        /// Encrypts a public-box plaintext
        /// </summary>
        /// <param name="plaintext">Plaintext to encrypt</param>
        /// <param name="nonce">24-byte Nonce</param>
        /// <param name="secretKey">32-byte SecretKey</param>
        /// <param name="publicKey">32-byte PublicKey</param>
        /// <returns>Encrypted ciphertext</returns>
        public static byte[] CreatePublicBox(byte[] plaintext, byte[] nonce, byte[] secretKey, byte[] publicKey)
        {
            return new PublicBox(secretKey, publicKey).Create(plaintext, nonce);
        }

        /// <summary>
        /// Decrypts a secret-box ciphertext
        /// </summary>
        /// <param name="cipher">Ciphertext to decrypt</param>
        /// <param name="nonce">24-byte Nonce</param>
        /// <param name="sharedKey">32-byte SharedKey</param>
        /// <returns>Decrypted plaintext</returns>
        public static byte[] OpenSecretBox(byte[] cipher, byte[] nonce, byte[] sharedKey)
        {
            return new SecretBox(sharedKey).Open(cipher, nonce);
        }

        /// <summary>
        /// Encrypts a secret-box plaintext
        /// </summary>
        /// <param name="plaintext">Plaintext to encrypt</param>
        /// <param name="nonce">24-byte Nonce</param>
        /// <param name="sharedKey">32-byte SharedKey</param>
        /// <returns>Encrypted ciphertext</returns>
        public static byte[] CreateSecretBox(byte[] plaintext, byte[] nonce, byte[] sharedKey)
        {
            return new SecretBox(sharedKey).Create(plaintext, nonce);
        }
    }
}