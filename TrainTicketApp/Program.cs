using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows.Forms;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Services;
using TrainTicketApp;

namespace TrainTicketApp
{
    static class Program
    {
        public static IHost? AppHost;

        [STAThread]
        static void Main()
        {
            
            ApplicationConfiguration.Initialize();

            // build Host & DI container
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<TrainTicketDbContext>(opts =>
                        opts.UseSqlServer("DefaultConnection"));

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
            var login = AppHost.Services.GetRequiredService<LoginForm>();
            Application.Run(login);
        }
    }
}

