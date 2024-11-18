using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ARDSPlatform.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ARDSPlatform.DAL
{
    public class ards_DbContext : IdentityDbContext<ApplicationUser>
    {
        public ards_DbContext() : base("name=MySqlConnection")
        {
            // 可以添加一些日志输出以确认连接字符串是否正确
            Console.WriteLine("Connection string database name: " + base.Database.Connection.Database);
            Console.WriteLine("Database context created. Checking connection...");
            if (Database.Exists())
            {
                Console.WriteLine("Database connection is active.");
            }
            else
            {
                Console.WriteLine("Database connection could not be established.");
            }
        }

        public DbSet<Users> tb_users { get; set; }

        public DbSet<Patient> tb_patients { get; set; }

        public DbSet<FileUploadRecord> fileuploadrecords { get; set; }

        // 可以添加其他 DbSet 属性来表示数据库中的其他表


        public DbSet<TbLoginHistory> tb_login_history { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TbLoginHistory>().HasKey(lh => lh.LoginHistoryId);
            modelBuilder.Entity<FileUploadRecord>().HasKey(f => f.FileId);
            base.OnModelCreating(modelBuilder);
        }

    }

}

public class ApplicationUser : IdentityUser
{
    // 可以添加额外的用户属性
}
