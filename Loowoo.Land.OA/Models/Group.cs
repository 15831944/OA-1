﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loowoo.Land.OA.Models
{
    /// <summary>
    /// 组
    /// </summary>
    [Table("group")]
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string Name { get; set; }

        public GroupType Type { get; set; }

        public virtual List<GroupRight> Rights { get; set; }

        public bool HasRight(string right)
        {
            return Rights != null && Rights.Any(e => e.Name == right);
        }
    }

    public enum GroupType
    {
        System,
        User
    }
}
