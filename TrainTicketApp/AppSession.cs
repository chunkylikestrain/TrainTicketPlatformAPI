using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainTicketApp
{
    /// <summary>
    /// application‐wide session state (after login)
    /// </summary>
    public static class AppSession
    {
        public static int CurrentUserId { get; set; }
        public static string JwtToken { get; set; } = "";
    }
}
