using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Services;
using TrainTicketApp;
using Microsoft.Extensions.Configuration;


namespace TrainTicketApp
{
    static class Program
    {
        public static IHost? AppHost;

        [STAThread]
        static void Main()
        {
            // 0) build a config reader
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ApplicationConfiguration.Initialize();

            // 1) Build the Host
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    // read the real connection string
                    var conn = config.GetConnectionString("DefaultConnection");

                    services.AddDbContext<TrainTicketDbContext>(opts =>
                        opts.UseSqlServer(conn));

                    services.AddSingleton<IUserService, UserService>();
                    services.AddScoped<ITrainService, TrainService>();
                    services.AddScoped<ISeatService, SeatService>();
                    services.AddScoped<IBookingService, BookingService>();
                    services.AddScoped<IPaymentService, PaymentService>();
                    services.AddScoped<IUserService, UserService>();

                    //forms:
                    services.AddTransient<LoginForm>();
                    services.AddTransient<RegisterForm>();
                    services.AddTransient<MainForm>();
                    services.AddTransient<SearchTrainsForm>();
                    services.AddTransient<SelectSeatForm>();
                    services.AddTransient<PaymentForm>();
                    services.AddTransient<AdminMainForm>();
                    services.AddTransient<BookingForm>();
                    services.AddTransient<ManageTrainForm>();
                    services.AddTransient<UpsertTrainForm>();
                    services.AddTransient<ManageSeatMapForm>();
                    services.AddTransient<UpsertSeatMapForm>();
                    services.AddTransient<ViewBookingReportForm>();
                    services.AddScoped<BookingConfirmationForm>();

                })

                .Build();
                

            //grab the first form from DI and run it
            var login = AppHost.Services.GetRequiredService<SearchTrainsForm>();
            Application.Run(login);
        }
    }
}

