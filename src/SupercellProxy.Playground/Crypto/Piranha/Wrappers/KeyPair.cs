using SupercellProxy.Libraries.TweetNaCl;

namespace SupercellProxy.Crypto.Piranha.Wrappers
{
    internal class KeyPair
    {
        private readonly byte[] sk = new byte[32];
        private readonly byte[] pk = new byte[32];

        /// <summary>
        /// KeyPair constructor
        /// </summary>
        public KeyPair()
        {
            Curve25519xsalsa20poly1305.crypto_box_keypair(pk, sk);
        }

        /// <summary>
        /// Returns the randomly generated PublicKey
        /// </summary>
        public byte[] PublicKey
        {
            get
            {
                return pk;
            }
        }

        /// <summary>
        /// Returns the randomly generated SecretKey
        /// </summary>
        public byte[] SecretKey
        {
            get
            {
                return sk;
            }
        }
    }
}