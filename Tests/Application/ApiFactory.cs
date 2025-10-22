using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services;
using Moq;

namespace Tests.Application
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public Mock<IStudentService> StudentSvcMock;
        public ApiFactory()
        {
            StudentSvcMock=new Mock<IStudentService>(MockBehavior.Strict);
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var desc = services.Single(d => d.ServiceType == typeof(IStudentService));
                services.Remove(desc);

                services.AddSingleton(StudentSvcMock.Object);
            });
        }
    }
}
