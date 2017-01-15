
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Security.Cryptography.X509Certificates;

namespace Thought.vCards
{

    /// <summary>
    ///     A certificate attached to a vCard.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A vCard can be associated with a public key or
    ///         authentication certificate.  This is typically
    ///         a public X509 certificate that allows people to
    ///         use the key for validating messages.
    ///     </para>
    /// </remarks>
    [Serializable]
    public class vCardCertificate
    {

        private string keyType;
        private byte[] data;


        /// <summary>
        ///     Creates a new instance of the <see cref="vCardCertificate"/> class.
        /// </summary>
        public vCardCertificate()
        {
            this.keyType = string.Empty;
        }


        /// <summary>
        ///     Creates a new instance of the <see cref="vCardCertificate"/>
        ///     class using the specified key type and raw certificate data.
        /// </summary>
        /// <param name="keyType">
        ///     A string that identifies the type of certificate,
        ///     such as X509.
        /// </param>
        /// <param name="data">
        ///     The raw certificate data stored as a byte array.
        /// </param>
        public vCardCertificate(string keyType, byte[] data)
        {
            if (string.IsNullOrEmpty(keyType))
                throw new ArgumentNullException("keyType");

            if (data == null)
                throw new ArgumentNullException("data");

            this.keyType = keyType;
            this.data = data;
        }


        /// <summary>
        ///     Creates a vCard certificate based on an X509 certificate.
        /// </summary>
        /// <param name="x509">
        ///     An initialized X509 certificate.
        /// </param>
        public vCardCertificate(X509Certificate2 x509)
        {
            if (x509 == null)
                throw new ArgumentNullException("x509");

            this.data = x509.RawData;
            this.keyType = "X509";

        }


        /// <summary>
        ///     The raw data of the certificate as a byte array.
        /// </summary>
        /// <remarks>
        ///     Most certificates consist of 8-bit binary data
        ///     that is encoded into a text format using BASE64
        ///     or a similar system.  This property provides
        ///     access to the computer-friendly, decoded data.
        /// </remarks>
        public byte[] Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }


        /// <summary>
        ///     A short string that identifies the type of certificate.
        /// </summary>
        /// <remarks>
        ///     The most common type is X509.
        /// </remarks>
        public string KeyType
        {
            get
            {
                return this.keyType ?? string.Empty;
            }
            set
            {
                this.keyType = value;
            }
        }

    }

}