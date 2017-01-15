
/* =======================================================================
 * vCard Library for .NET
 * Copyright (c) 2007-2009 David Pinch; http://wwww.thoughtproject.com
 * See LICENSE.TXT for licensing information.
 * ======================================================================= */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Thought.vCards
{

    /// <summary>
    ///    IM info  <see cref="vCard"/>.
    /// </summary>
    [Serializable]
    public class vCardIMPP
    {

        private string handle;
        private ItemType itemType;
        private IMServiceType serviceType;

        /// <summary>
        ///     Creates a new <see cref="vCardIMPP"/> object.
        /// </summary>
        public vCardIMPP()
        {
            this.serviceType = IMServiceType.Unspecified;
        }


        /// <summary>
        ///     Creates a new <see cref="vCardIMPP"/> object with the specified handle.
        /// </summary>
        /// <param name="handle">the im handle</param>
        /// <param name="serviceType">skype, aim, etc</param>
        /// <param name="itemType">the type of IM, defaults to Unspecified</param>
        public vCardIMPP(string handle, IMServiceType serviceType, ItemType itemType = ItemType.UNSPECIFIED)
        {
            this.handle = handle;
            this.itemType = itemType;
            this.serviceType = serviceType;
        }





        /// <summary>
        ///     The full IM handle.
        /// </summary>
        public string Handle
        {
            get
            {
                return this.handle ?? string.Empty;
            }
            set
            {
                this.handle = value;
            }
        }

        /// <summary>
        /// the IMServiceType AIM, googletalk, etc
        /// </summary>
        public IMServiceType ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }


        /// <summary>
        ///     The IM ItemType. home work, unspecified
        /// </summary>
        public ItemType ItemType
        {
            get
            {
                return this.itemType;
            }
            set
            {
                this.itemType = value;
            }
        }

        /// <summary>
        /// is PREF set on this IMPP item
        /// </summary>
        public bool IsPreferred { get; set; }
    }

    /// <summary>
    /// simple enum for various types of IM services
    /// </summary>
    public enum IMServiceType
    {
        /// <summary>
        /// unspecified
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// for Skype
        /// </summary>
        Skype,
        /// <summary>
        /// aim
        /// </summary>
        AIM,
        /// <summary>
        /// gtalk
        /// </summary>
        GoogleTalk,
        /// <summary>
        /// msn
        /// </summary>
        MSN,
        /// <summary>
        /// yahoo
        /// </summary>
        Yahoo,

        /// <summary>
        /// facebook
        /// </summary>
        Facebook,
        /// <summary>
        /// jabber
        /// </summary>
        Jabber,
        /// <summary>
        /// icq
        /// </summary>
        ICQ,
        /// <summary>
        /// qq
        /// </summary>
        QQ,
        /// <summary>
        /// gadu gadu
        /// </summary>
        GaduGadu
         


    }

    /// <summary>
    /// simple class to generate the strings for given IMServiceType
    /// </summary>
    public static class IMTypeUtils
    {

        private static Dictionary<IMServiceType, string> lookup;

          static IMTypeUtils()
        {
            lookup = new Dictionary<IMServiceType, string>();
            lookup.Add(IMServiceType.AIM, "AIM:aim");
            lookup.Add(IMServiceType.Facebook, "Facebook:xmpp");
            lookup.Add(IMServiceType.GoogleTalk, "GoogleTalk:xmpp");
            lookup.Add(IMServiceType.ICQ, "ICQ:aim");
            lookup.Add(IMServiceType.Jabber, "Jabber:xmpp");
            lookup.Add(IMServiceType.MSN, "MSN:msnim");
            lookup.Add(IMServiceType.QQ, "QQ:x-apple");
            lookup.Add(IMServiceType.Skype, "Skype:skype");
            lookup.Add(IMServiceType.Yahoo, "Yahoo:ymsgr");
            lookup.Add(IMServiceType.GaduGadu, "GaduGadu:x-apple");

        }

        /// <summary>
        /// will return the property meta info to be written for a given IM serviceType
        /// </summary>
        /// 
        /// <param name="serviceType">IMServiceType to get the subproperty info for </param>
        /// <returns>for example GoogleTalk:xmpp, or for yahoo Yahoo:ymsgr</returns>
        public static string GetIMTypePropertyFull(IMServiceType serviceType)
        {

            if (lookup.ContainsKey(serviceType))
            {
                return lookup[serviceType];
            }

            return null;

        }

        /// <summary>
        /// returns the xmpp or aim or ymsgr portion of the lookup for AIM:aim, Yahoo:ysmgr, etc
        /// </summary>
        /// <param name="serviceType">IMServiceType  to fetch the lookup for</param>
        /// <returns>xmpp or msnim</returns>
        public static string GetIMTypePropertySuffix(IMServiceType serviceType)
        {
            string suffix = null;
            if (lookup.ContainsKey(serviceType))
            {
                string full = lookup[serviceType];

                suffix = full.Substring(full.IndexOf(":") + 1);

            }

            return suffix;
        }

        /// <summary>
        /// this method will return the first part of the AIM:aim , or MSN:msnim string used for writing out the property subproperty info for IMPP values
        /// </summary>
        /// <param name="serviceType">the IM service type to fetch from the dictionary</param>
        /// <returns>AIM or QQ or Yahoo, the first string component for the lookup of serviceTypes</returns>
        public static string GetIMTypePropertyPrefix(IMServiceType serviceType)
        {
            string prefix = null;
            if (lookup.ContainsKey(serviceType))
            {
                string full = lookup[serviceType];

                  prefix = full.Substring(0, full.IndexOf(":"));
           
            }

            return prefix;
            

        }


        /// <summary>
        /// the handle is coming back with the msnim:handle, so we want to return the pure handle minus the msnim:
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static string StripHandlePrefix(IMServiceType serviceType, string handle)
        {

            string property = GetIMTypePropertyFull(serviceType);


            if (property != null)
            {
                string prefix = property.Substring(property.IndexOf(":") + 1);
                int prefixLength = prefix.Length + 1;
                if (handle.StartsWith(prefix))
                {
                    handle = handle.Substring(handle.IndexOf(prefix + ":") + prefixLength);
                }

            }

            return handle;


        }

        /// <summary>
        /// for parsing the 
        /// </summary>
        /// <param name="imType"></param>
        /// <returns></returns>
        public static IMServiceType? GetIMServiceType(string imType)
        {
            IMServiceType? serviceType = null;

            switch (imType.ToLowerInvariant())
            {
                case "aim":
                    serviceType = IMServiceType.AIM;
                    break;
                case "facebook":
                    serviceType = IMServiceType.Facebook;
                    break;
                case "googletalk":
                case "google":
                    serviceType = IMServiceType.GoogleTalk;
                    break;
                case "icq":
                    serviceType = IMServiceType.ICQ;
                    break;
                case "jabber":
                case "xmpp":
                    serviceType = IMServiceType.Jabber;
                    break;
                case "msn":
                    serviceType = IMServiceType.MSN;
                    break;
                case "qq":
                    serviceType = IMServiceType.QQ;
                    break;
                case "skype":
                    serviceType = IMServiceType.Skype;
                    break;
                case "yahoo":
                case "ymsgr":
                    serviceType = IMServiceType.Yahoo;
                    break;
                case "gadugadu":
                case "gadu":
                    serviceType = IMServiceType.GaduGadu;
                    break;
            }


            return serviceType;

        }


    }

}