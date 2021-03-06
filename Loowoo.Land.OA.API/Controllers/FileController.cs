﻿using Loowoo.Common;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Loowoo.Land.OA.API.Controllers
{
    public class FileController : ControllerBase
    {
        [HttpGet]
        public HttpResponseMessage Index(int id, string action = "preview")
        {
            var file = Core.FileManager.GetModel(id);
            if (file == null)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent("文件未找到")
                };
            }
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(file.PhysicalPath, FileMode.Open, FileAccess.Read);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            var fileName = HttpUtility.UrlPathEncode(file.FileName);
            if (Request.Headers.UserAgent.ToString().Contains("irefox"))
            {
                fileName = file.FileName;
            }
            if (action == "download")
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName,
                };
            }
            else
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = fileName,
                };
            }
            return response;
        }

        [HttpGet]
        public HttpResponseMessage Download(int id)
        {
            return Index(id, "download");
        }

        [HttpPost]
        public OA.Models.File Upload(string name = null, int id = 0, int infoId = 0, int formId = 0, bool inline = false)
        {
            var files = HttpContext.Current.Request.Files;
            if (files.Count == 0)
            {
                throw new Exception("没有上传文件");
            }
            var inputFile = string.IsNullOrWhiteSpace(name) ? files[0] : files[name];
            if (inputFile == null)
            {
                throw new Exception("未找到指定的name");
            }

            var file = OA.Models.File.Upload(inputFile);
            file.InfoId = infoId;
            file.ID = id;
            file.Inline = inline;
            Core.FileManager.Save(file);

            return file;
        }

        [HttpPost]
        public void Update(OA.Models.File model)
        {
            Core.FileManager.Save(model);
        }

        [HttpGet]
        public void UpdateRelation(int[] fileIds, int infoId)
        {
            Core.FileManager.Relation(fileIds, infoId);
        }

        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            Core.FileManager.Delete(id);
            return Ok();
        }

        [HttpGet]
        public object List(int? infoId = null, int? formId = null, int? page = null, int? rows = null, FileType? type = null, bool? inline = null)
        {
            var parameter = new FileParameter
            {
                InfoId = infoId,
                FormId = formId,
                Type = type,
                Inline = inline,
                Page = new PageParameter(page, rows)
            };
            var list = Core.FileManager.GetList(parameter);
            return new PagingResult
            {
                List = list,
                Page = parameter.Page
            };
        }

        public HttpResponseMessage GetPreviewFile(int infoId)
        {
            var files = Core.FileManager.GetList(new FileParameter
            {
                InfoId = infoId,
                Inline = true,
            });
            var file = files.OrderByDescending(e => e.ID).FirstOrDefault();
            if (file == null)
            {
                return Index(0);
            }
            //如果是word文档，则需要转为pdf 并替换原来的word文件
            if (file.IsWordFile)
            {
                var pdfFile = Core.FileManager.GetList(new FileParameter { ParentId = file.ID }).ToList().Where(e => e.FileName.EndsWith("pdf")).FirstOrDefault();
                if (pdfFile == null)
                {
                    var docPath = Path.Combine(Environment.CurrentDirectory, file.AbsolutelyPath);
                    var pdfPath = docPath + ".pdf";
                    if (Core.FileManager.TryConvertToPdf(docPath, pdfPath))
                    {
                        pdfFile = new OA.Models.File
                        {
                            FileName = file.FileName + ".pdf",
                            InfoId = file.InfoId,
                            SavePath = file.SavePath + ".pdf",
                            Size = file.Size,
                            Inline = true,
                            ParentId = file.ID
                        };
                        Core.FileManager.Save(pdfFile);
                    }
                    else
                    {
                        return Index(file.ID);
                    }
                }
                return Index(pdfFile.ID);
            }
            return Index(file.ID);
        }

        [HttpGet]
        public string WordEditUrl(int id)
        {
            var url = "/word/get?id=" + id;
            var file = Core.FileManager.GetModel(id);
            if (file == null)
            {
                throw new Exception("未找到该文件");
            }
            if (file.IsWordFile)
            {
                return PageOffice.PageOfficeLink.OpenWindow(url, string.Empty);
            }
            else
            {
                return url;
            }
        }
    }
}
