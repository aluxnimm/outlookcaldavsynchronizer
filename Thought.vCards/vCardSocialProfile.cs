
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
    public class vCardSocialProfile
    {

        private string username;
        private SocialProfileServiceType serviceType;
        private string fullProfileUrl;

        /// <summary>
        ///     Creates a new <see cref="vCardIMPP"/> object.
        /// </summary>
        public vCardSocialProfile()
        {
            this.serviceType = SocialProfileServiceType.Unspecified;
        }


        /// <summary>
        ///     Creates a new <see cref="vCardIMPP"/> object with the specified handle.
        /// </summary>
        /// <param name="username">the social profile username handle</param>
        /// <param name="serviceType">Facebook, Twitter, Flickr, etc</param>
        /// <param name="fullProfileUrl">the full url for the profile => http://twitter.com/username </param>
        public vCardSocialProfile(string username, SocialProfileServiceType serviceType, string fullProfileUrl)
        {
            this.username = username;
            this.serviceType = serviceType;
            this.fullProfileUrl = fullProfileUrl;
        }


        /// <summary>
        /// the full profile url of the socialProfile (http://twitter.com/username)
        /// </summary>
        public string ProfileUrl
        {
            get
            {
                return this.fullProfileUrl ?? string.Empty;
            }
            set
            {
                this.fullProfileUrl = value;
            }
        }


        /// <summary>
        ///     The username on the socialProfile
        /// </summary>
        public string Username
        {
            get
            {
                return this.username ?? string.Empty;
            }
            set
            {
                this.username = value;
            }
        }

        /// <summary>
        /// the IMServiceType AIM, googletalk, etc
        /// </summary>
        public SocialProfileServiceType ServiceType
        {
            get { return serviceType; }
            set { serviceType = value; }
        }

 
    }

    /// <summary>
    /// simple enum for various types of SocialProfile services
    /// </summary>
    public enum SocialProfileServiceType
    {
        /// <summary>
        /// unspecified
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// Facebook
        /// </summary>
        Facebook,
        /// <summary>
        /// LinkedIn
        /// </summary>
        LinkedIn,
        /// <summary>
        /// Twitter
        /// </summary>
        Twitter,
        /// <summary>
        /// Flickr
        /// </summary>
        Flickr,
        /// <summary>
        /// Myspace
        /// </summary>
        Myspace
 


    }


    /// <summary>
    /// utisl to handle string to enum conversion in one spot for SocialProfile types
    /// </summary>
    public static class SocialProfileTypeUtils
    {



        /// <summary>
        /// for parsing the type string
        /// </summary>
        /// <param name="profileType"></param>
        /// <returns>nullable ServiceType for matching string of serviceType</returns>
        public static SocialProfileServiceType? GetSocialProfileServiceType(string profileType)
        {
            SocialProfileServiceType? serviceType = null;

            switch (profileType.ToLowerInvariant())
            {
                case "facebook":
                    serviceType = SocialProfileServiceType.Facebook;
                    break;
                case "flickr":
                    serviceType = SocialProfileServiceType.Flickr;
                    break;
                case "linkedin":
                    serviceType = SocialProfileServiceType.LinkedIn;
                    break;
                case "myspace":
                    serviceType = SocialProfileServiceType.Myspace;
                    break;
                case "twitter":
                    serviceType = SocialProfileServiceType.Twitter;
                    break;
 
            }


            return serviceType;

        }

        /// <summary>
        /// for returning the socialProfile type string that will be used in the type=twitter for reading socialProfile vCard data
        /// </summary>
        /// <param name="serviceType">the SocialProfile Type to get the lowercase string for to include in the type value</param>
        /// <returns>facebook,twitter,etc</returns>
        public static string GetSocialProfileServicePropertyType(SocialProfileServiceType serviceType)
        {
            string profileType = null;

            switch (serviceType)
            {
                case SocialProfileServiceType.Facebook :
                    profileType = "facebook";
                    break;
                case SocialProfileServiceType.Flickr:
                    profileType = "flickr";
                    break;
                case SocialProfileServiceType.LinkedIn:
                    profileType = "linkedin";
                    break;
                case SocialProfileServiceType.Myspace:
                    profileType = "myspace";
                    break;
                case SocialProfileServiceType.Twitter:
                    profileType = "twitter";
                    break;

            }

            return profileType;

        }

    }

 

}