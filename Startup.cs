using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HelloDynamoDb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var localMode = Configuration.GetValue<bool>("DynamoDb:LocalMode");
            var localServiceUrl = Configuration.GetValue<string>("DynamoDb:LocalServiceUrl");
            if (localMode)
            {
                services.AddSingleton<IAmazonDynamoDB>(sp =>
                    {
                        var config = new AmazonDynamoDBConfig { ServiceURL = localServiceUrl };
                        return new AmazonDynamoDBClient(config);
                    });
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }
            services.AddSingleton<IDynamoDBContext>(sp =>
                {
                    var client = sp.GetRequiredService<IAmazonDynamoDB>();
                    return new DynamoDBContext(client);
                });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
