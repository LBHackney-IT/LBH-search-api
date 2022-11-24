using Amazon.XRay.Recorder.Handlers.AwsSdk;
using FluentValidation.AspNetCore;
using Hackney.Core.HealthCheck;
using Hackney.Core.JWT;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Hackney.Core.ElasticSearch;
using Hackney.Core.ElasticSearch.Interfaces;
using HousingSearchApi.V1.Gateways.Interfaces;
using HousingSearchApi.V1.Gateways;
using HousingSearchApi.V1.Infrastructure.Extensions;
using HousingSearchApi.V1.Infrastructure.Factories;
using HousingSearchApi.V1.Infrastructure.Filtering;
using HousingSearchApi.V1.Infrastructure.Sorting;
using HousingSearchApi.V1.Interfaces.Factories;
using HousingSearchApi.V1.Interfaces.Filtering;
using HousingSearchApi.V1.Infrastructure.Core;

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

        private const string ApiName = "Housing Search";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services
                .AddMvc()
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
                    fv.LocalizationEnabled = false;
                })
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
            services.AddTokenFactory();

            RegisterGateways(services);
            RegisterUseCases(services);
            services.ConfigureElasticSearch(Configuration);
            services.AddElasticSearchHealthCheck();

            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped(typeof(IFilterQueryBuilder<>), typeof(FilterQueryBuilder<>));
            services.AddScoped<IWildCardAppenderAndPrepender, WildCardAppenderAndPrepender>();
            services.AddScoped<IQueryFactory, QueryFactory>();
            services.AddScoped<IIndexSelector, IndexSelector>();

            services.AddLogCallAspect();
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<ISearchGateway, SearchGateway>();
            services.AddScoped<IGetAccountGateway, GetAccountGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddScoped<IGetPersonListUseCase, GetPersonListUseCase>();
            services.AddScoped<IGetAccountListUseCase, GetAccountListUseCase>();
            services.AddScoped<IGetTenureListUseCase, GetTenureListUseCase>();
            services.AddScoped<IGetTenureListByPrnListUseCase, GetTenureListUseByPrnListCase>();
            services.AddScoped<IElasticSearchWrapper, ElasticSearchWrapper>();
            services.AddScoped<IPagingHelper, PagingHelper>();
            services.AddScoped<ISortFactory, SortFactory>();
            services.AddScoped<IFilterFactory, FilterFactory>();
            services.AddScoped<IGetAssetListUseCase, GetAssetListUseCase>();
            services.AddScoped<IGetAssetListSetsUseCase, GetAssetListSetsUseCase>();
            services.AddScoped<IGetTransactionListUseCase, GetTransactionListUseCase>();
            services.AddScoped<IGetStaffListUseCase, GetStaffListUseCase>();
            services.AddScoped<IGetProcessListUseCase, GetProcessListUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders("x-correlation-id"));

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
