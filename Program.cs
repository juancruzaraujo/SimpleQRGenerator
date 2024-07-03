using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using SkiaSharp;
using QRCoder;
using System.Net;


namespace SimpleQRGenerator
{

    public class Program
    {
        public static void Main(string[] args)
        {
            const string C_TEST = "/test";
            const string C_QRGENERATOR = "/qrgenerator/{inputString}";

            int httpPort = Convert.ToInt32(args[0]);
            //int httpsPort = Convert.ToInt32(args[1]);
            

            Console.WriteLine("Service SimpleQRGenerator start");
            Console.WriteLine("http port " + httpPort);
            //Console.WriteLine("https port " + httpsPort);
            Console.WriteLine(args[1]);

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost
            .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True")


            .ConfigureKestrel((context, serverOptions) =>
            {

                serverOptions.Listen(IPAddress.Any, httpPort);
                //serverOptions.Listen(IPAddress.Loopback, httpsPort, listenOptions =>
                //{
                //listenOptions.UseHttps();//nada de certificados.... por ahora-

                //});
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
                endpoints.MapGet(C_QRGENERATOR, GenerateQRCode);
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