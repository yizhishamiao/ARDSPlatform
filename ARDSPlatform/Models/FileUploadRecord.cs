using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ARDSPlatform.Models
{
    public class FileUploadRecord
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }
        [Required]
        [MaxLength(10)]
        public string FileType { get; set; }
        public DateTime UploadTime { get; set; }

    }
}