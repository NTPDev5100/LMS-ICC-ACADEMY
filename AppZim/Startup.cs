using AppZim.Areas.Admin.Controllers;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
[assembly: OwinStartup(typeof(AppZim.Startup))]
namespace AppZim
{
    public class Startup
    {
        private IEnumerable<IDisposable> GetHangfireServers()
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage("Server=LAPTOP-4EUEEHBQ;Database=icc.monamedia.net;User ID=sa;Password=123;MultipleActiveResultSets=true;Persist Security Info=true;TrustServerCertificate=True;", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            yield return new BackgroundJobServer();
        }

        [Obsolete]
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
            app.UseHangfireAspNet(GetHangfireServers);
            app.UseHangfireDashboard();
            //Mỗi tháng tự tạo chiến dịch
            RecurringJob.AddOrUpdate(() => CampaignSaleController.CreatePeriod(), Cron.Monthly, TimeZoneInfo.Local);
            //Cuối ngày kiểm tra các chiến dịch hết hạn để xét điều kiện xuống rank
            RecurringJob.AddOrUpdate(() => CampaignSaleController.CheckRevenueOfCampaign(), "55 23 * * *", TimeZoneInfo.Local);
        }
    }
}