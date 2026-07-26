using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Interfaces;
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
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "integration-test-jwt-signing-key-at-least-32-bytes",
                    ["ServiceAuthentication:ApiKey"] = "integration-test-internal-api-key"
                });
            });

            builder.ConfigureServices(services =>
            {
                var desc = services.Single(d => d.ServiceType == typeof(IStudentService));
                services.Remove(desc);

                services.AddSingleton(StudentSvcMock.Object);
            });
        }
    }
}
