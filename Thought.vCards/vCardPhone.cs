
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;

namespace Thought.vCards
{

    /// <summary>
    ///     Telephone information for a <see cref="vCard"/>.
    /// </summary>
    /// <seealso cref="vCardPhoneCollection"/>
    /// <seealso cref="vCardPhoneTypes"/>
    [Serializable]
    public class vCardPhone
    {

        private string fullNumber;
        private vCardPhoneTypes phoneType;


        /// <summary>
        ///     Creates a new <see cref="vCardPhone"/> object.
        /// </summary>
        public vCardPhone()
        {
        }


        /// <summary>
        ///     Creates a new <see cref="vCardPhone"/> object with the specified number.
        /// </summary>
        /// <param name="fullNumber">
        ///     The phone number.
        /// </param>
        public vCardPhone(string fullNumber)
        {
            this.fullNumber = fullNumber;
        }


        /// <summary>
        ///     Creates a new <see cref="vCardPhone"/> with the specified number and subtype.
        /// </summary>
        /// <param name="fullNumber">The phone number.</param>
        /// <param name="phoneType">The phone subtype.</param>
        public vCardPhone(string fullNumber, vCardPhoneTypes phoneType)
        {
            this.fullNumber = fullNumber;
            this.phoneType = phoneType;
        }


        /// <summary>
        ///     The full telephone number.
        /// </summary>
        public string FullNumber
        {
            get
            {
                return this.fullNumber ?? string.Empty;
            }
            set
            {
                this.fullNumber = value;
            }
        }


        /// <summary>
        ///     Indicates a BBS number.
        /// </summary>
        /// <seealso cref="IsModem"/>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsBBS
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.BBS) == vCardPhoneTypes.BBS;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.BBS;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.BBS;
                }
            }
        }


        /// <summary>
        ///     Indicates a car number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsCar
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Car) == vCardPhoneTypes.Car;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Car;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Car;
                }
            }
        }


        /// <summary>
        ///     Indicates a cellular number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsCellular
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Cellular) == vCardPhoneTypes.Cellular;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Cellular;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Cellular;
                }
            }
        }

        /// <summary>
        ///     Indicates an iphone
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsiPhone
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.IPhone) == vCardPhoneTypes.IPhone;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.IPhone;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.IPhone;
                }
            }
        }

        /// <summary>
        ///     Indicates a main number
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsMain
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Main) == vCardPhoneTypes.Main;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Main;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Main;
                }
            }
        }


        /// <summary>
        ///     Indicates a fax number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsFax
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Fax) == vCardPhoneTypes.Fax;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Fax;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Fax;
                }
            }
        }


        /// <summary>
        ///     Indicates a home number.
        /// </summary>
        /// <seealso cref="IsWork"/>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsHome
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Home) == vCardPhoneTypes.Home;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Home;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Home;
                }
            }
        }


        /// <summary>
        ///     Indicates an ISDN number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsISDN
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.ISDN) == vCardPhoneTypes.ISDN;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.ISDN;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.ISDN;
                }
            }
        }


        /// <summary>
        ///     Indicates a messaging service number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsMessagingService
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.MessagingService) ==
                    vCardPhoneTypes.MessagingService;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.MessagingService;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.MessagingService;
                }
            }
        }


        /// <summary>
        ///     Indicates a modem number.
        /// </summary>
        /// <seealso cref="IsBBS"/>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsModem
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Modem) == vCardPhoneTypes.Modem;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Modem;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Modem;
                }
            }
        }


        /// <summary>
        ///     Indicates a pager number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsPager
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Pager) == vCardPhoneTypes.Pager;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Pager;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Pager;
                }
            }
        }


        /// <summary>
        ///     Indicates a preferred number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsPreferred
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Preferred) == vCardPhoneTypes.Preferred;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Preferred;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Preferred;
                }
            }
        }


        /// <summary>
        ///     Indicates a video number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsVideo
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Video) == vCardPhoneTypes.Video;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Video;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Video;
                }
            }
        }


        /// <summary>
        ///     Indicates a voice number.
        /// </summary>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsVoice
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Voice) == vCardPhoneTypes.Voice;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Voice;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Voice;
                }
            }
        }


        /// <summary>
        ///     Indicates a work number.
        /// </summary>
        /// <seealso cref="IsHome"/>
        /// <seealso cref="vCardPhoneTypes"/>
        public bool IsWork
        {
            get
            {
                return (this.phoneType & vCardPhoneTypes.Work) == vCardPhoneTypes.Work;
            }
            set
            {
                if (value)
                {
                    this.phoneType = this.phoneType | vCardPhoneTypes.Work;
                }
                else
                {
                    this.phoneType = this.phoneType & ~vCardPhoneTypes.Work;
                }
            }
        }


        /// <summary>
        ///     The phone subtype.
        /// </summary>
        /// <seealso cref="IsVideo"/>
        /// <seealso cref="IsVoice"/>
        /// <seealso cref="IsWork"/>
        public vCardPhoneTypes PhoneType
        {
            get
            {
                return this.phoneType;
            }
            set
            {
                this.phoneType = value;
            }
        }

    }

}