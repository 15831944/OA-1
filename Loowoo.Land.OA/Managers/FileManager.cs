﻿using Loowoo.Common;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Web;

namespace Loowoo.Land.OA.Managers
{
    public class FileManager : ManagerBase
    {
        public void Save(File file)
        {
            if (file.ID > 0)
            {
                file.UpdateTime = DateTime.Now;
            }
            DB.Files.AddOrUpdate(file);
            DB.SaveChanges();
        }

        public File GetModel(int id)
        {
            return DB.Files.FirstOrDefault(e => e.ID == id);
        }

        public void Delete(int id)
        {
            var entity = DB.Files.FirstOrDefault(e => e.ID == id);
            if (entity != null)
            {
                DB.Files.Remove(entity);
                DB.SaveChanges();
            }
        }

        public void Relation(int[] fileIds, int infoId)
        {
            var entities = DB.Files.Where(e => fileIds.Contains(e.ID));
            foreach (var entity in entities)
            {
                entity.InfoId = infoId;
            }
            DB.SaveChanges();
        }

        public IEnumerable<File> GetList(FileParameter parameter)
        {
            var query = DB.Files.AsQueryable();
            if (parameter.InfoId.HasValue)
            {
                query = query.Where(e => e.InfoId == parameter.InfoId.Value);
            }
            if(parameter.Inline.HasValue)
            {
                query = query.Where(e => e.Inline == parameter.Inline.Value);
            }
            if(parameter.Type.HasValue)
            {
                string[] fileExt = null;
                switch(parameter.Type.Value)
                {

                }
                query = query.Where(e => e.FileName.EndsWith(parameter.Type.Value.ToString()));
            }
            query = query.OrderBy(e => e.UpdateTime).SetPage(parameter.Page);
            return query;
        }

        public void ConvertToPdf(object docPath,object pdfPath)
        {
            var word = new Application();
            object oMissing = System.Reflection.Missing.Value;
            var doc = word.Documents.Open(ref docPath, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                    ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            doc.Activate();

            object fileFormat = WdSaveFormat.wdFormatPDF;

            // Save document into PDF Format
            doc.SaveAs(ref pdfPath,
                ref fileFormat, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            // Close the Word document, but leave the Word application open.
            // doc has to be cast to type _Document so that it will find the
            // correct Close method.                
            object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
            doc.Close(ref saveChanges, ref oMissing, ref oMissing);
            doc = null;
        }


    }
}