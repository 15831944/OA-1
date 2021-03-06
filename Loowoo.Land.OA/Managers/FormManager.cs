﻿using Loowoo.Land.OA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Loowoo.Land.OA.Managers
{
    public class FormManager : ManagerBase
    {
        public Form GetModel(int formId)
        {
            return DB.Forms.FirstOrDefault(e => e.ID == formId);
        }

        public IEnumerable<Form> GetList()
        {
            return DB.Forms.AsQueryable();
        }

        public Form GetModel(FormType type)
        {
            return DB.Forms.FirstOrDefault(e => e.ID == (int)type);
        }
    }
}