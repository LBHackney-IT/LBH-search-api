using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Elasticsearch.Net;
using Hackney.Core.HealthCheck;
using Hackney.Core.Logging;
using Hackney.Core.Middleware.CorrelationId;
using Hackney.Core.Middleware.Exception;
using Hackney.Core.Middleware.Logging;
using HousingSearchApi.V1.HealthCheck;
using HousingSearchApi.V1.Infrastructure;
using HousingSearchApi.V1.Interfaces;
using HousingSearchApi.V1.Interfaces.Sorting;
using HousingSearchApi.V1.UseCase;
using HousingSearchApi.V1.UseCase.Interfaces;
using HousingSearchApi.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nest;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HousingSearchApi
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AWSSDKHandler.RegisterXRayForAllServices();
        }

        public IConfiguration Configuration { get; }
        private static List<ApiVersionDescription> _apiVersions { get; set; }
        //TODO update the below to the name of your API
        private const string ApiName = "Your API Name";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services
                .AddMvc()
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new UrlSegmentApiVersionReader(); // read the version number from the url segment header)
            });

            services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Token",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Your Hackney API Key",
                        Name = "X-Api-Key",
                        Type = SecuritySchemeType.ApiKey
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Token" }
                        },
                        new List<string>()
                    }
                });

                //Looks at the APIVersionAttribute [ApiVersion("x")] on controllers and decides whether or not
                //to include it in that version of the swagger document
                //Controllers must have this [ApiVersion("x")] to be included in swagger documentation!!
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    apiDesc.TryGetMethodInfo(out var methodInfo);

                    var versions = methodInfo?
                        .DeclaringType?.GetCustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions).ToList();

                    return versions?.Any(v => $"{v.GetFormattedApiVersion()}" == docName) ?? false;
                });

                //Get every ApiVersion attribute specified and create swagger docs for them
                foreach (var apiVersion in _apiVersions)
                {
                    var version = $"v{apiVersion.ApiVersion.ToString()}";
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = $"{ApiName}-api {version}",
                        Version = version,
                        Description = $"{ApiName} version {version}. Please check older versions for depreciated endpoints."
                    });
                }

                c.CustomSchemaIds(x => x.FullName);
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });
            services.ConfigureLambdaLogging(Configuration);

            RegisterGateways(services);
            RegisterUseCases(services);
            ConfigureElasticsearch(services);

            services.AddScoped<IWildCardAppenderAndPrepender, WildCardAppenderAndPrepender>();

            services.AddLogCallAspect();
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<ISearchPersonsGateway, SearchPersonsGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddScoped<IGetPersonListUseCase, GetPersonListUseCase>();
            services.AddScoped<ISearchPersonElasticSearchHelper, SearchPersonElasticSearchHelper>();
            services.AddScoped<ISearchPersonsQueryContainerOrchestrator, SearchPersonsQueryContainerOrchestrator>();
            services.AddScoped<IPagingHelper, PagingHelper>();
            services.AddScoped<IPersonListSortFactory, PersonListSortFactory>();
        }

        private static void ConfigureElasticsearch(IServiceCollection services)
        {
            var url = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL") ?? "http://localhost:9200";
            var pool = new SingleNodeConnectionPool(new Uri(url));
            var connectionSettings =
                new ConnectionSettings(pool)
                    .PrettyJson().ThrowExceptions().DisableDirectStreaming();
            var esClient = new ElasticClient(connectionSettings);

            services.TryAddSingleton<IElasticClient>(esClient);

            services.AddElasticSearchHealthCheck();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseXRay("housing-search-api");

            app.UseCorrelationId();
            app.UseLoggingScope();
            app.UseCustomExceptionHandler(logger);
            app.UseLogCall();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Get All ApiVersions,
            var api = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            _apiVersions = api.ApiVersionDescriptions.ToList();

            //Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in _apiVersions)
                {
                    //Create a swagger endpoint for each swagger version
                    c.SwaggerEndpoint($"{apiVersionDescription.GetFormattedApiVersion()}/swagger.json",
                        $"{ApiName}-api {apiVersionDescription.GetFormattedApiVersion()}");
                }
            });
            app.UseSwagger();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHealthChecks("/api/v1/healthcheck/ping", new HealthCheckOptions()
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteResponse
                });
            });
        }
    }
}
