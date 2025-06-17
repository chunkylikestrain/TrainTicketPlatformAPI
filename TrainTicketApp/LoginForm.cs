using Microsoft.Extensions.DependencyInjection;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace TrainTicketApp
{
   

    public partial class LoginForm : Form
    {
        private readonly IUserService _userService;

        // ① Parameterless ctor for the WinForms Designer
        public LoginForm()
        {
            InitializeComponent();
        }

        // ② DI ctor – VS will call this when resolving via AppHost.Services
        public LoginForm(IUserService userService)
            : this()
        {
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        // ③ Log In button click
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            var email = txtEmail.Text.Trim();
            var password = txtPasword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    "Please enter both email and password.",
                    "Missing Credentials",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                var dto = new LoginDto
                {
                    Email = txtEmail.Text,
                    Password = txtPasword.Text
                };

                // raw token
                var token = await _userService.LoginAsync(dto);

                // parse it
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                // get “sub” (user id) and “role” claims
                var subClaim = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
                var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;

                // Store in our session for later use
                AppSession.JwtToken = token;
                AppSession.CurrentUserId = int.Parse(jwt.Subject);

                // Navigate to the search screen
                if (roleClaim == "Admin")
                {
                    var admin = Program.AppHost.Services.GetRequiredService<AdminMainForm>();
                    admin.Show();
                }
                else
                {
                    var search = Program.AppHost.Services.GetRequiredService<SearchTrainsForm>();
                    search.Show();
                }
                this.Hide();
            }
            catch (KeyNotFoundException)
            {
                // thrown when email not found or password mismatch
                MessageBox.Show(
                    "Invalid email or password.",
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ④ “Register” link click 
        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            // resolve and show your RegisterForm
            var reg = Program.AppHost.Services.GetRequiredService<RegisterForm>();
            reg.Show();
            this.Hide();
        }
    }
}
