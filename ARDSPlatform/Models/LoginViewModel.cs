using System.ComponentModel.DataAnnotations;

namespace ARDSPlatform.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "账号不能为空！")]
        //[Display(Name = "账号")]
        public string Account { get; set; }

        [Required(ErrorMessage = "密码不能为空！")]
        [DataType(DataType.Password)]
        //[Display(Name = "密码")]
        public string Password { get; set; }

        
    }
}