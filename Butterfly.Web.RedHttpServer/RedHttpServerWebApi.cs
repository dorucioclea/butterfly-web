﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Butterfly.Util;
using Butterfly.Web.WebApi;
using Red;

namespace Butterfly.RedHttpServer {

    /// <inheritdoc/>
    public class RedHttpServerWebApi : BaseWebApi {

        public readonly Red.RedHttpServer server;

        public RedHttpServerWebApi(Red.RedHttpServer server) {
            this.server = server;
        }


        public override void Compile() {
            foreach (var webHandler in this.webHandlers) {
                async Task<HandlerType> handler(Request req, Response res) {
                    try {
                        await webHandler.listener(new RedHttpServerWebRequest(req), new RedHttpServerWebResponse(res));
                        return HandlerType.Final;
                    }
                    catch (Exception e) {
                        logger.Error(e);
                        return HandlerType.Error;
                    }
                }

                if (webHandler.method == HttpMethod.Get) {
                    string newPath = Regex.Replace(webHandler.path, "{([^/]+?)}", m => $":{m.Groups[1].Value}");
                    this.server.Get(newPath, handler);
                }
                else if (webHandler.method == HttpMethod.Post) {
                    this.server.Post(webHandler.path, handler);
                }
                else if (webHandler.method == HttpMethod.Put) {
                    this.server.Put(webHandler.path, handler);
                }
                else if (webHandler.method == HttpMethod.Delete) {
                    this.server.Delete(webHandler.path, handler);
                }
                else if (webHandler.method == null) {
                    string newPath = Regex.Replace(webHandler.path, "{([^/]+?)}", m => $":{m.Groups[1].Value}");
                    this.server.Get(newPath, handler);
                    this.server.Post(webHandler.path, handler);
                    this.server.Put(webHandler.path, handler);
                    this.server.Delete(webHandler.path, handler);
                }
            }
        }

        public override void Dispose() {
        }
    }

    public class RedHttpServerWebRequest : BaseHttpRequest {

        public readonly Request request;

        public RedHttpServerWebRequest(Request request) {
            this.request = request;
        }

        public override Stream InputStream => this.request.BodyStream;

        public override string ClientIp => this.request?.AspNetRequest?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public override string Method => this.request?.AspNetRequest?.Method?.ToUpper();

        public override Uri RequestUrl => this.request?.Context?.AspNetContext?.Request?.ToUri();

        public override Dictionary<string, string> Headers => this.request.Headers.ToDictionary(x => x.Key.ToUpper(), x => x.Value.ToString());

        public override Dictionary<string, string> PathParams => this.request?.Context?.ExtractAllUrlParameters();

        public override Dictionary<string, string> QueryParams => this.RequestUrl.ParseQuery();

    }

    public class RedHttpServerWebResponse : IHttpResponse {

        public readonly Response response;

        public RedHttpServerWebResponse(Response response) {
            this.response = response;
        }

        public string GetHeader(string name) {
            return this.response.AspNetResponse.Headers[name];
        }

        public void SetHeader(string name, string value) {
            this.response.AspNetResponse.Headers[name] = value;
        }

        public int StatusCode {
            get => this.response.AspNetResponse.StatusCode;
            set => this.response.AspNetResponse.StatusCode = value;
        }

        public string StatusText {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void SendRedirect(string url) {
            this.response.Redirect(url);
        }

        public Stream OutputStream => this.response.Context.AspNetContext.Response.Body;

        public Task WriteAsTextAsync(string value, string contentType = "text/plain") {
            if (!string.IsNullOrEmpty(contentType)) this.SetHeader("Content-Type", contentType);
            return this.response.SendString(value);
        }

    }
}
