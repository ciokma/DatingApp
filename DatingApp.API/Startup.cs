using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DatingApp.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;
using DatingApp.API.Dtos;

namespace DatingApp.API
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
            //Creando la conexion
            //services.AddDbContext<DataContext>(x =>x.UseSqlite("ConnectionString"));
            //Esta linea da error al levantar la migracion
            services.AddDbContext<DataContext>(x =>x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddControllers();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
           // Start Registering and Initializing AutoMapper

   //services.AddAutoMapper(typeof(Startup));
            ///////IMPORTANTE//////////// VERSION .NET CORE 3.0/////////////
            //AGREGAR ESTA LINEA IMPORTANTISIMO////////
            ///Esto se agrega para resolver el problema al momento de devolver la data en 
            //el controlador de usuarios
            services.AddMvc().AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.PropertyNamingPolicy = null;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                });
            //agregando migracion
            services.AddTransient<Seed>();
            //agregando cabeceras cors para que permite que se consuma desde modo local
            services.AddCors();  
            //REGISTRANDO CLOUDINARY PARA CONECTARSE AL SERVER DE FOTOS
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            //REGISTRANDO EL AUTOMAPPER
           // Auto Mapper Configurations
            services.AddAutoMapper(typeof(DatingRepository).Assembly);
            //REgistrando servicio para injectar a los contralores el repositorio
            //Toma como parametero la interfaz y la clase de repositorio
            services.AddScoped<IAuthRepository, AuthRepository>();
            //Agregando el repositorio de user
            services.AddScoped<IDatingRepository, DatingRepository>();
            //agregando autenticacion 
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            // Se agrega porque se registra una nueva instanacia para actualizar la fecha de usuario ultima actividad
            services.AddScoped<LogUserActivity>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else{
                //Setting  up  the global exception handler
                app.UseExceptionHandler(builder =>{
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                      
                        if(error != null){
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }
            app.UseHttpsRedirection();
 
            app.UseRouting();
            //agregando migracion
            //seeder.SeedUser(); //comentado para que no se este haciendo la migracion en cada momento
            //Agregando core , en este orden porque si no no funciona
            //VERSION ANTERIOR 
            app.UseCors(x=>x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            //VERSION NUEVA  PARA TRABAJAR CON LA GALERIA DE IMAGENES ES NECESARIO HABILITAR QUE PERMITA CREDENCIALES
            //ESTO PODRIA DAR PROBLEMAS DE SEGURIDAD
            //app.UseCors(x=>x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            //Agregando la autenticacion de json token
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            }); 
        }
    }
}
