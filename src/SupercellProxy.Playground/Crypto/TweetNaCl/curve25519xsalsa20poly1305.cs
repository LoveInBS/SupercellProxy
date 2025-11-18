using System.Security.Cryptography;

namespace SupercellProxy.Libraries.TweetNaCl
{
    public class Curve25519xsalsa20poly1305
    {
        public const int crypto_secretbox_PUBLICKEYBYTES = 32;
        public const int crypto_secretbox_SECRETKEYBYTES = 32;
        public const int crypto_secretbox_BEFORENMBYTES = 32;
        public const int crypto_secretbox_NONCEBYTES = 24;
        public const int crypto_secretbox_ZEROBYTES = 32;
        public const int crypto_secretbox_BOXZEROBYTES = 16;

        public static int crypto_box_getpublickey(byte[] pk, byte[] sk)
        {
            return Curve25519.crypto_scalarmult_base(pk, sk);
        }

        public static int crypto_box_keypair(byte[] pk, byte[] sk)
        {
            RandomNumberGenerator.Fill(sk);
            return Curve25519.crypto_scalarmult_base(pk, sk);
        }

        public static int crypto_box_afternm(byte[] c, byte[] m, long mlen, byte[] n, byte[] k)
        {
            return Xsalsa20poly1305.crypto_secretbox(c, m, mlen, n, k);
        }

        public static int crypto_box_beforenm(byte[] k, byte[] pk, byte[] sk)
        {
            byte[] s = new byte[32];
            byte[] sp = s, sigmap = Xsalsa20.sigma;

            Curve25519.crypto_scalarmult(sp, sk, pk);
            return Hsalsa20.crypto_core(k, null, sp, sigmap);
        }

        public static int crypto_box(byte[] c, byte[] m, long mlen, byte[] n, byte[] pk, byte[] sk)
        {
            byte[] k = new byte[crypto_secretbox_BEFORENMBYTES];
            byte[] kp = k;

            crypto_box_beforenm(kp, pk, sk);
            return crypto_box_afternm(c, m, mlen, n, kp);
        }

        public static int crypto_box_open(byte[] m, byte[] c, long clen, byte[] n, byte[] pk, byte[] sk)
        {
            byte[] k = new byte[crypto_secretbox_BEFORENMBYTES];
            byte[] kp = k;

            crypto_box_beforenm(kp, pk, sk);
            return crypto_box_open_afternm(m, c, clen, n, kp);
        }

        public static int crypto_box_open_afternm(byte[] m, byte[] c, long clen, byte[] n, byte[] k)
        {
            return Xsalsa20poly1305.crypto_secretbox_open(m, c, clen, n, k);
        }

        public static int crypto_box_afternm(byte[] c, byte[] m, byte[] n, byte[] k)
        {
            byte[] cp = c, mp = m, np = n, kp = k;
            return crypto_box_afternm(cp, mp, (long)m.Length, np, kp);
        }

        public static int crypto_box_open_afternm(byte[] m, byte[] c, byte[] n, byte[] k)
        {
            byte[] cp = c, mp = m, np = n, kp = k;
            return crypto_box_open_afternm(mp, cp, (long)c.Length, np, kp);
        }

        public static int crypto_box(byte[] c, byte[] m, byte[] n, byte[] pk, byte[] sk)
        {
            byte[] cp = c, mp = m, np = n, pkp = pk, skp = sk;
            return crypto_box(cp, mp, (long)m.Length, np, pkp, skp);
        }

        public static int crypto_box_open(byte[] m, byte[] c, byte[] n, byte[] pk, byte[] sk)
        {
            byte[] cp = c, mp = m, np = n, pkp = pk, skp = sk;
            return crypto_box_open(mp, cp, (long)c.Length, np, pkp, skp);
        }
    }
}