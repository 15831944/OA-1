﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loowoo.Land.OA.Models
{
    /// <summary>
    /// 部门
    /// </summary>
    [Table("department")]
    public class Department
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ParentId { get; set; }

        public string Name { get; set; }

        public int AttendanceGroupId { get; set; }

        [JsonIgnore]
        public virtual AttendanceGroup AttendanceGroup { get; set; }

        public int Sort { get; set; }

        public bool Deleted { get; set; }
    }
}
