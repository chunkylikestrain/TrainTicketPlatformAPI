using System;
using BCrypt.Net;
namespace PasswordHasher
{
    class Program
    {
        static void Main(string[] args)
        {
            // either pass the password in as an argument:
            var plain = args.Length > 0 ? args[0] : "password123";

            // generate a bcrypt hash (10 rounds by default)
            string hash = BCrypt.Net.BCrypt.HashPassword(plain);

            Console.WriteLine("Plain: " + plain);
            Console.WriteLine("Hash : " + hash);
        }
    }
}
