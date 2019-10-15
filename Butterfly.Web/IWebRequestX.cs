﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Net;
using System.Net.Http.Headers;

namespace Butterfly.Web {
    public static class IWebRequestX {
        public static AuthenticationHeaderValue GetAuthenticationHeaderValue(this IWebRequest me) {
            if (me.Headers!=null && me.Headers.TryGetValue(HttpRequestHeader.Authorization.ToString().ToUpper(), out string text)) {
                if (text!=null && AuthenticationHeaderValue.TryParse(text, out AuthenticationHeaderValue result)) return result;
            }
            return null;
        }
    }
}
