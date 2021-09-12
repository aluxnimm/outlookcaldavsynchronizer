// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace CalDavSynchronizer.Utilities
{
    public struct X11Colors
    {
        public static readonly X11Color Aliceblue = new X11Color("aliceblue", 0xf0f8ff);
        public static readonly X11Color Antiquewhite = new X11Color("antiquewhite", 0xfaebd7);
        public static readonly X11Color Aqua = new X11Color("aqua", 0x00ffff);
        public static readonly X11Color Aquamarine = new X11Color("aquamarine", 0x7fffd4);
        public static readonly X11Color Azure = new X11Color("azure", 0xf0ffff);
        public static readonly X11Color Beige = new X11Color("beige", 0xf5f5dc);
        public static readonly X11Color Bisque = new X11Color("bisque", 0xffe4c4);
        public static readonly X11Color Black = new X11Color("black", 0x000000);
        public static readonly X11Color Blanchedalmond = new X11Color("blanchedalmond", 0xffebcd);
        public static readonly X11Color Blue = new X11Color("blue", 0x0000ff);
        public static readonly X11Color Blueviolet = new X11Color("blueviolet", 0x8a2be2);
        public static readonly X11Color Brown = new X11Color("brown", 0xa52a2a);
        public static readonly X11Color Burlywood = new X11Color("burlywood", 0xdeb887);
        public static readonly X11Color Cadetblue = new X11Color("cadetblue", 0x5f9ea0);
        public static readonly X11Color Chartreuse = new X11Color("chartreuse", 0x7fff00);
        public static readonly X11Color Chocolate = new X11Color("chocolate", 0xd2691e);
        public static readonly X11Color Coral = new X11Color("coral", 0xff7f50);
        public static readonly X11Color Cornflowerblue = new X11Color("cornflowerblue", 0x6495ed);
        public static readonly X11Color Cornsilk = new X11Color("cornsilk", 0xfff8dc);
        public static readonly X11Color Crimson = new X11Color("crimson", 0xdc143c);
        public static readonly X11Color Cyan = new X11Color("cyan", 0x00ffff);
        public static readonly X11Color Darkblue = new X11Color("darkblue", 0x00008b);
        public static readonly X11Color Darkcyan = new X11Color("darkcyan", 0x008b8b);
        public static readonly X11Color Darkgoldenrod = new X11Color("darkgoldenrod", 0xb8860b);
        public static readonly X11Color Darkgray = new X11Color("darkgray", 0xa9a9a9);
        public static readonly X11Color Darkgreen = new X11Color("darkgreen", 0x006400);
        public static readonly X11Color Darkgrey = new X11Color("darkgrey", 0xa9a9a9);
        public static readonly X11Color Darkkhaki = new X11Color("darkkhaki", 0xbdb76b);
        public static readonly X11Color Darkmagenta = new X11Color("darkmagenta", 0x8b008b);
        public static readonly X11Color Darkolivegreen = new X11Color("darkolivegreen", 0x556b2f);
        public static readonly X11Color Darkorange = new X11Color("darkorange", 0xff8c00);
        public static readonly X11Color Darkorchid = new X11Color("darkorchid", 0x9932cc);
        public static readonly X11Color Darkred = new X11Color("darkred", 0x8b0000);
        public static readonly X11Color Darksalmon = new X11Color("darksalmon", 0xe9967a);
        public static readonly X11Color Darkseagreen = new X11Color("darkseagreen", 0x8fbc8f);
        public static readonly X11Color Darkslateblue = new X11Color("darkslateblue", 0x483d8b);
        public static readonly X11Color Darkslategray = new X11Color("darkslategray", 0x2f4f4f);
        public static readonly X11Color Darkslategrey = new X11Color("darkslategrey", 0x2f4f4f);
        public static readonly X11Color Darkturquoise = new X11Color("darkturquoise", 0x00ced1);
        public static readonly X11Color Darkviolet = new X11Color("darkviolet", 0x9400d3);
        public static readonly X11Color Deeppink = new X11Color("deeppink", 0xff1493);
        public static readonly X11Color Deepskyblue = new X11Color("deepskyblue", 0x00bfff);
        public static readonly X11Color Dimgray = new X11Color("dimgray", 0x696969);
        public static readonly X11Color Dimgrey = new X11Color("dimgrey", 0x696969);
        public static readonly X11Color Dodgerblue = new X11Color("dodgerblue", 0x1e90ff);
        public static readonly X11Color Firebrick = new X11Color("firebrick", 0xb22222);
        public static readonly X11Color Floralwhite = new X11Color("floralwhite", 0xfffaf0);
        public static readonly X11Color Forestgreen = new X11Color("forestgreen", 0x228b22);
        public static readonly X11Color Fuchsia = new X11Color("fuchsia", 0xff00ff);
        public static readonly X11Color Gainsboro = new X11Color("gainsboro", 0xdcdcdc);
        public static readonly X11Color Ghostwhite = new X11Color("ghostwhite", 0xf8f8ff);
        public static readonly X11Color Gold = new X11Color("gold", 0xffd700);
        public static readonly X11Color Goldenrod = new X11Color("goldenrod", 0xdaa520);
        public static readonly X11Color Gray = new X11Color("gray", 0x808080);
        public static readonly X11Color Green = new X11Color("green", 0x008000);
        public static readonly X11Color Greenyellow = new X11Color("greenyellow", 0xadff2f);
        public static readonly X11Color Grey = new X11Color("grey", 0x808080);
        public static readonly X11Color Honeydew = new X11Color("honeydew", 0xf0fff0);
        public static readonly X11Color Hotpink = new X11Color("hotpink", 0xff69b4);
        public static readonly X11Color Indianred = new X11Color("indianred", 0xcd5c5c);
        public static readonly X11Color Indigo = new X11Color("indigo", 0x4b0082);
        public static readonly X11Color Ivory = new X11Color("ivory", 0xfffff0);
        public static readonly X11Color Khaki = new X11Color("khaki", 0xf0e68c);
        public static readonly X11Color Lavender = new X11Color("lavender", 0xe6e6fa);
        public static readonly X11Color Lavenderblush = new X11Color("lavenderblush", 0xfff0f5);
        public static readonly X11Color Lawngreen = new X11Color("lawngreen", 0x7cfc00);
        public static readonly X11Color Lemonchiffon = new X11Color("lemonchiffon", 0xfffacd);
        public static readonly X11Color Lightblue = new X11Color("lightblue", 0xadd8e6);
        public static readonly X11Color Lightcoral = new X11Color("lightcoral", 0xf08080);
        public static readonly X11Color Lightcyan = new X11Color("lightcyan", 0xe0ffff);
        public static readonly X11Color Lightgoldenrodyellow = new X11Color("lightgoldenrodyellow", 0xfafad2);
        public static readonly X11Color Lightgray = new X11Color("lightgray", 0xd3d3d3);
        public static readonly X11Color Lightgreen = new X11Color("lightgreen", 0x90ee90);
        public static readonly X11Color Lightgrey = new X11Color("lightgrey", 0xd3d3d3);
        public static readonly X11Color Lightpink = new X11Color("lightpink", 0xffb6c1);
        public static readonly X11Color Lightsalmon = new X11Color("lightsalmon", 0xffa07a);
        public static readonly X11Color Lightseagreen = new X11Color("lightseagreen", 0x20b2aa);
        public static readonly X11Color Lightskyblue = new X11Color("lightskyblue", 0x87cefa);
        public static readonly X11Color Lightslategray = new X11Color("lightslategray", 0x778899);
        public static readonly X11Color Lightslategrey = new X11Color("lightslategrey", 0x778899);
        public static readonly X11Color Lightsteelblue = new X11Color("lightsteelblue", 0xb0c4de);
        public static readonly X11Color Lightyellow = new X11Color("lightyellow", 0xffffe0);
        public static readonly X11Color Lime = new X11Color("lime", 0x00ff00);
        public static readonly X11Color Limegreen = new X11Color("limegreen", 0x32cd32);
        public static readonly X11Color Linen = new X11Color("linen", 0xfaf0e6);
        public static readonly X11Color Magenta = new X11Color("magenta", 0xff00ff);
        public static readonly X11Color Maroon = new X11Color("maroon", 0x800000);
        public static readonly X11Color Mediumaquamarine = new X11Color("mediumaquamarine", 0x66cdaa);
        public static readonly X11Color Mediumblue = new X11Color("mediumblue", 0x0000cd);
        public static readonly X11Color Mediumorchid = new X11Color("mediumorchid", 0xba55d3);
        public static readonly X11Color Mediumpurple = new X11Color("mediumpurple", 0x9370db);
        public static readonly X11Color Mediumseagreen = new X11Color("mediumseagreen", 0x3cb371);
        public static readonly X11Color Mediumslateblue = new X11Color("mediumslateblue", 0x7b68ee);
        public static readonly X11Color Mediumspringgreen = new X11Color("mediumspringgreen", 0x00fa9a);
        public static readonly X11Color Mediumturquoise = new X11Color("mediumturquoise", 0x48d1cc);
        public static readonly X11Color Mediumvioletred = new X11Color("mediumvioletred", 0xc71585);
        public static readonly X11Color Midnightblue = new X11Color("midnightblue", 0x191970);
        public static readonly X11Color Mintcream = new X11Color("mintcream", 0xf5fffa);
        public static readonly X11Color Mistyrose = new X11Color("mistyrose", 0xffe4e1);
        public static readonly X11Color Moccasin = new X11Color("moccasin", 0xffe4b5);
        public static readonly X11Color Navajowhite = new X11Color("navajowhite", 0xffdead);
        public static readonly X11Color Navy = new X11Color("navy", 0x000080);
        public static readonly X11Color Oldlace = new X11Color("oldlace", 0xfdf5e6);
        public static readonly X11Color Olive = new X11Color("olive", 0x808000);
        public static readonly X11Color Olivedrab = new X11Color("olivedrab", 0x6b8e23);
        public static readonly X11Color Orange = new X11Color("orange", 0xffa500);
        public static readonly X11Color Orangered = new X11Color("orangered", 0xff4500);
        public static readonly X11Color Orchid = new X11Color("orchid", 0xda70d6);
        public static readonly X11Color Palegoldenrod = new X11Color("palegoldenrod", 0xeee8aa);
        public static readonly X11Color Palegreen = new X11Color("palegreen", 0x98fb98);
        public static readonly X11Color Paleturquoise = new X11Color("paleturquoise", 0xafeeee);
        public static readonly X11Color Palevioletred = new X11Color("palevioletred", 0xdb7093);
        public static readonly X11Color Papayawhip = new X11Color("papayawhip", 0xffefd5);
        public static readonly X11Color Peachpuff = new X11Color("peachpuff", 0xffdab9);
        public static readonly X11Color Peru = new X11Color("peru", 0xcd853f);
        public static readonly X11Color Pink = new X11Color("pink", 0xffc0cb);
        public static readonly X11Color Plum = new X11Color("plum", 0xdda0dd);
        public static readonly X11Color Powderblue = new X11Color("powderblue", 0xb0e0e6);
        public static readonly X11Color Purple = new X11Color("purple", 0x800080);
        public static readonly X11Color Red = new X11Color("red", 0xff0000);
        public static readonly X11Color Rosybrown = new X11Color("rosybrown", 0xbc8f8f);
        public static readonly X11Color Royalblue = new X11Color("royalblue", 0x4169e1);
        public static readonly X11Color Saddlebrown = new X11Color("saddlebrown", 0x8b4513);
        public static readonly X11Color Salmon = new X11Color("salmon", 0xfa8072);
        public static readonly X11Color Sandybrown = new X11Color("sandybrown", 0xf4a460);
        public static readonly X11Color Seagreen = new X11Color("seagreen", 0x2e8b57);
        public static readonly X11Color Seashell = new X11Color("seashell", 0xfff5ee);
        public static readonly X11Color Sienna = new X11Color("sienna", 0xa0522d);
        public static readonly X11Color Silver = new X11Color("silver", 0xc0c0c0);
        public static readonly X11Color Skyblue = new X11Color("skyblue", 0x87ceeb);
        public static readonly X11Color Slateblue = new X11Color("slateblue", 0x6a5acd);
        public static readonly X11Color Slategray = new X11Color("slategray", 0x708090);
        public static readonly X11Color Slategrey = new X11Color("slategrey", 0x708090);
        public static readonly X11Color Snow = new X11Color("snow", 0xfffafa);
        public static readonly X11Color Springgreen = new X11Color("springgreen", 0x00ff7f);
        public static readonly X11Color Steelblue = new X11Color("steelblue", 0x4682b4);
        public static readonly X11Color Tan = new X11Color("tan", 0xd2b48c);
        public static readonly X11Color Teal = new X11Color("teal", 0x008080);
        public static readonly X11Color Thistle = new X11Color("thistle", 0xd8bfd8);
        public static readonly X11Color Tomato = new X11Color("tomato", 0xff6347);
        public static readonly X11Color Turquoise = new X11Color("turquoise", 0x40e0d0);
        public static readonly X11Color Violet = new X11Color("violet", 0xee82ee);
        public static readonly X11Color Wheat = new X11Color("wheat", 0xf5deb3);
        public static readonly X11Color White = new X11Color("white", 0xffffff);
        public static readonly X11Color Whitesmoke = new X11Color("whitesmoke", 0xf5f5f5);
        public static readonly X11Color Yellow = new X11Color("yellow", 0xffff00);
        public static readonly X11Color Yellowgreen = new X11Color("yellowgreen", 0x9acd32);
    }
}