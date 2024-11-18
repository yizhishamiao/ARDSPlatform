using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARDSPlatform.Models
{
    [Table("tb_login_history")]
    public class TbLoginHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginHistoryId { get; set; }

        public int UserId { get; set; }

        [StringLength(45)]
        public string LoginIp { get; set; }

        public DateTime LoginTime { get; set; }
    }
}