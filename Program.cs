using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SkiaSharp;
using QRCoder;
using System.Net;
using System.Collections;

namespace SimpleQRGenerator
{

    public class Program
    {
        public static void Main(string[] args)
        {
            const string C_TEST = "/test";
            //const string C_QRGENERATOR = "/qrgenerator/{inputString}";
            const string C_QRGENENDPOINT = "QRGENERATOR_ENDPOINT";
            string qrGeneratorEndPointValue = Environment.GetEnvironmentVariable(C_QRGENENDPOINT) + "/{inputString}";
            int httpPort=80;
            int httpsPort=443;

            
            if (args.Length == 3)
            {
                Console.WriteLine("command arguments list");
                httpPort = Convert.ToInt32(args[0]);
                httpsPort = Convert.ToInt32(args[1]);
                for (int i = 0; i < args.Length; i++)
                {
                    Console.WriteLine(args[i]);
                }
            }

            if (Convert.ToInt32(args[2])==1) //muestro o no las variables de entorno
            {
                Console.WriteLine("Environment variable list");
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                foreach (DictionaryEntry entry in environmentVariables)
                {
                    Console.WriteLine($"{entry.Key} = {entry.Value}");
                }
            }

            Console.WriteLine("Service SimpleQRGenerator start");
            Console.WriteLine("http port " + httpPort);

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost
            .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True")


            .ConfigureKestrel((context, serverOptions) =>
            {

                serverOptions.Listen(IPAddress.Any, httpPort);
                serverOptions.Listen(IPAddress.Any, httpsPort, listenOptions =>
                {
                    listenOptions.UseHttps();//nada de certificados.... por ahora-
                });
            });

            //Configuración de registro para limitar los mensajes de registro
            //builder.Logging.ClearProviders(); // Limpia todos los proveedores de registro existentes
            //builder.Logging.AddConsole(); // Agrega el proveedor de registro de consola
            //builder.Logging.SetMinimumLevel(LogLevel.Warning); // Establece el nivel de registro deseado (en este caso, Warning o superior)


             var app = builder.Build();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(C_TEST, context => context.Response.WriteAsync("ready online"));
                endpoints.MapGet(qrGeneratorEndPointValue, GenerateQRCode);
            });

            app.Run();


        }

        private static async Task GenerateQRCode(HttpContext context)
        {

            var inputString = context.Request.RouteValues["inputString"].ToString();

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(inputString, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                context.Response.ContentType = "image/png";
                await context.Response.Body.WriteAsync(qrCodeImage, 0, qrCodeImage.Length);
            }
        }
    }



}