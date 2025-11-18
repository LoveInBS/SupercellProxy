using SupercellProxy.Libraries.TweetNaCl;

namespace SupercellProxy.Crypto.Piranha.Wrappers
{
    public class PublicBox
    {
        //private const int KEYBYTES = 32;
        //private const int NONCEBYTES = 24;
        private const int ZEROBYTES = 32;

        private const int BOXZEROBYTES = 16;
        private const int BEFORENMBYTES = 32;

        private readonly byte[] PrecomputedSharedKey = new byte[BEFORENMBYTES];

        public PublicBox(byte[] privatekey, byte[] publickey)
        {
            // generate a precomputed sharedkey, enhances performance drastically
            Curve25519xsalsa20poly1305.crypto_box_beforenm(this.PrecomputedSharedKey, publickey, privatekey);
        }

        public byte[] Create(byte[] plain, byte[] nonce)
        {
            int plainLength = plain.Length;
            byte[] paddedbuffer = new byte[plainLength + ZEROBYTES];
            Array.Copy(plain, 0, paddedbuffer, ZEROBYTES, plainLength);

            if (Curve25519xsalsa20poly1305.crypto_box_afternm(paddedbuffer, paddedbuffer, paddedbuffer.Length, nonce, this.PrecomputedSharedKey) != 0)
                throw new Exception("PublicBox Encryption failed");

            byte[] output = new byte[plainLength + BOXZEROBYTES];
            Array.Copy(paddedbuffer, ZEROBYTES - BOXZEROBYTES, output, 0, output.Length);
            return output;
        }

        public byte[] Open(byte[] cipher, byte[] nonce)
        {
            int cipherLength = cipher.Length;
            byte[] paddedbuffer = new byte[cipherLength + BOXZEROBYTES];
            Array.Copy(cipher, 0, paddedbuffer, BOXZEROBYTES, cipherLength);

            if (Curve25519xsalsa20poly1305.crypto_box_afternm(paddedbuffer, paddedbuffer, paddedbuffer.Length, nonce, this.PrecomputedSharedKey) != 0)
                throw new Exception("PublicBox Decryption failed");

            byte[] output = new byte[paddedbuffer.Length - ZEROBYTES];
            Array.Copy(paddedbuffer, ZEROBYTES, output, 0, output.Length);
            return output;
        }
    }
}