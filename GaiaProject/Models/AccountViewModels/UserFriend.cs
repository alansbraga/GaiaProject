﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GaiaProject.Models.AccountViewModels
{
    public class UserFriend
    {
        [Key]
        public int Id { get; set; }
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string UserName { get; set; }
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string UserId { get; set; }
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string UserNameTo { get; set; }
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string UserIdTo { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string Remark { get; set; }
    }
}
