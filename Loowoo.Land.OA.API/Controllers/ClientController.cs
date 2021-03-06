﻿using Loowoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Loowoo.Land.OA.API.Controllers
{
    public class ClientController : Controller
    {
        private string _lastVersion;
        public ClientController()
        {
            var versions = System.IO.Directory.GetFiles(_clientPath).Select(filePath => filePath.Substring(filePath.LastIndexOf("android-")).Replace("android-", "").Replace(".apk", ""));
            _lastVersion = versions.OrderByDescending(v => v).FirstOrDefault();
        }

        private string _clientPath
        {
            get { return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Client"); }
        }

        public string LastVersion()
        {
            return _lastVersion;
        }

        public FileResult QrCode()
        {
            var url = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + Url.Action("download");
            var img = QrCodeHelper.GenerateQrCode(url, 5);
            using (var ms = new System.IO.MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return File(ms.ToArray(), "image/jpeg");
            }
        }

        public ActionResult Download()
        {
            if (Request.UserAgent.Contains("Android"))
            {
                var filePath = System.IO.Path.Combine(_clientPath, $"android-{_lastVersion}.apk");
                return File(filePath, "application/octet-stream", $"oa-{_lastVersion}.apk");
            }
            else
            {
                return Redirect(AppSettings.Get("iOSDownloadUrl"));
            }
        }
    }
}