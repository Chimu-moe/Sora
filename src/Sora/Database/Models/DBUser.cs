using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Crypto = Sora.Utilities.Crypto;
using Hex = Sora.Utilities.Hex;

#nullable enable

namespace Sora.Database.Models
{
    public enum PasswordVersion
    {
        V0, // Md5 (never use this!)
        V1, // Md5 + BCrypt
        V2, // Md5 + SCrypt
    }

    public enum UserStatusFlags
    {
        None = 0,
        Suspended = 1 << 1, /* Disallow Login with a Reason! */
        Restricted = 1 << 2,
        Silenced = 1 << 3,
        Donator = 1 << 4,
    }

    [Table("Users")]
    public class DbUser
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(32)")]
        public string UserName { get; set; }

        [Required]
        [Column(TypeName = "varchar(64)")]
        public string EMail { get; set; }

        [Required]
        public string Password { get; set; }

        public byte[]? PasswordSalt { get; set; }

        [Required]
        public PasswordVersion PasswordVersion { get; set; }

        [Required]
        [Column("Permissions")]
        public string DbPermissions { get; set; }

        [Required]
        [NotMapped]
        public Permission Permissions
        {
            get => Permission.From(DbPermissions);
            set => DbPermissions = value.ToString();
        }

        public string? Achievements { get; set; }

        [Required]
        [DefaultValue(UserStatusFlags.None)]
        public UserStatusFlags Status { get; set; }

        public DateTime? StatusUntil { get; set; }
        public string? StatusReason { get; set; }

        public static Task<DbUser?> GetDbUser(SoraDbContext ctx, int userId)
            => ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);

        public static Task<DbUser?> GetDbUser(SoraDbContext ctx, string userName)
            => ctx.Users.FirstOrDefaultAsync(u => u.UserName == userName);

        public static Task<DbUser?> GetDbUser(SoraDbContext ctx, DbUser user)
            => GetDbUser(ctx, user.Id);

        public bool IsPassword(string passMd5)
        {
            switch (PasswordVersion)
            {
                case PasswordVersion.V0:
                    return passMd5 == Password;
                case PasswordVersion.V1:
                    return Crypto.BCrypt.validate_password(passMd5, Password);
                case PasswordVersion.V2:
                    return Crypto.SCrypt.validate_password(passMd5, Password, PasswordSalt);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static async Task<DbUser> RegisterUser(
            SoraDbContext ctx,
            Permission permission,
            string userName, string eMail, string password, bool md5 = true,
            PasswordVersion pwVersion = PasswordVersion.V2, int userId = 0) /* 0 = increased. */
        {
            if (ctx.Users.Any(u => string.Equals(u.UserName, userName, StringComparison.CurrentCultureIgnoreCase)))
                return null;

            byte[] salt = null;
            string pw;
            switch (pwVersion)
            {
                case PasswordVersion.V0:
                    pw = !md5 ? Hex.ToHex(Crypto.GetMd5(password)) : password;
                    break;
                case PasswordVersion.V1:
                    pw = !md5 ? Hex.ToHex(Crypto.GetMd5(password)) : password;
                    pw = Crypto.BCrypt.generate_hash(pw);
                    break;
                case PasswordVersion.V2:
                    pw = !md5 ? Hex.ToHex(Crypto.GetMd5(password)) : password;
                    (pw, salt) = Crypto.SCrypt.generate_hash(pw);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pwVersion), pwVersion,
                        "PasswordVersion MUST be either v0,v1 or v2!");
            }

            var user = new DbUser
            {
                UserName = userName,
                Password = pw,
                EMail = eMail,
                PasswordSalt = salt,
                PasswordVersion = pwVersion,
                Permissions = permission,
            };

            if (userId > 0)
                user.Id = userId;

            ctx.Add(user);
            await ctx.SaveChangesAsync();

            return user;
        }
    }
}