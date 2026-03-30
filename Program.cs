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
using System.Text;

namespace SimpleQRGenerator
{

    public class Program
    {
        public static void Main(string[] args)
        {
            const string C_TEST = "/test";
            
            const string C_QRGENENDPOINT = "QRGENERATOR_ENDPOINT";
            const string C_TEXTQRGENERATOR_ENDPOINT = "TEXTQRGENERATOR_ENDPOINT";
            //permite poner desde hola mundo hasta urls
            string qrGeneratorEndPointValue = Environment.GetEnvironmentVariable(C_QRGENENDPOINT) + "/{**inputString}";
            string textqrGeneratorEndPointValue = Environment.GetEnvironmentVariable(C_TEXTQRGENERATOR_ENDPOINT) + "/{**inputString}";
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
                //serverOptions.Listen(IPAddress.Any, httpsPort, listenOptions =>
                //{
                //    listenOptions.UseHttps();//nada de certificados.... por ahora-
                //});
            });

            var app = builder.Build();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(C_TEST, context => context.Response.WriteAsync("ready online"));
                endpoints.MapGet(qrGeneratorEndPointValue, GenerateQRCode);
                endpoints.MapGet(textqrGeneratorEndPointValue, GenerateQRCodeText);
            });

            app.Run();


        }

        private static async Task GenerateQRCode(HttpContext context)
        {
            var rawInput = context.Request.RouteValues["inputString"]?.ToString();
            var inputString = Uri.UnescapeDataString(rawInput ?? string.Empty);

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(inputString, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                context.Response.ContentType = "image/png";
                await context.Response.Body.WriteAsync(qrCodeImage, 0, qrCodeImage.Length);
            }
        }

        private static async Task GenerateQRCodeTextOld(HttpContext context)
        {
            var rawInput = context.Request.RouteValues["inputString"]?.ToString();
            var inputString = Uri.UnescapeDataString(rawInput ?? string.Empty);

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(inputString, QRCodeGenerator.ECCLevel.Q);

                int padding = 4; //espacios en blanco que rodean el qr
                int size = qrCodeData.ModuleMatrix.Count;

                var sb = new StringBuilder();

                //filas y padding arriba
                for (int y = 0; y < padding; y++)
                {
                    sb.AppendLine(new string(' ', (size + 2 * padding) * 2));
                }

                //contenido del QR con padding del costado
                for (int y = 0; y < size; y++)
                {
                    //padding izq
                    sb.Append(new string(' ', padding * 2));

                    for (int x = 0; x < size; x++)
                    {
                        bool pixel = qrCodeData.ModuleMatrix[y][x];
                        sb.Append(pixel ? "█" : " ");
                    }

                    //padding derecho
                    sb.Append(new string(' ', padding * 2));
                    sb.AppendLine();
                }

                //filas del padding inferior
                for (int y = 0; y < padding; y++)
                {
                    sb.AppendLine(new string(' ', (size + 2 * padding) * 2));
                }

                var qrText = sb.ToString();
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(qrText);
            }
        }

        private static async Task GenerateQRCodeText(HttpContext context)
        {
            var rawInput = context.Request.RouteValues["inputString"]?.ToString();
            var inputString = Uri.UnescapeDataString(rawInput ?? string.Empty);

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(inputString, QRCodeGenerator.ECCLevel.Q);

                int padding = 4; // espacios en blanco que rodean el qr
                int size = qrCodeData.ModuleMatrix.Count;

                var sb = new StringBuilder();

                string black = "\u001b[40m  "; // fondo negro, dos espacios
                string white = "\u001b[47m  "; // fondo blanco, dos espacios
                string reset = "\u001b[0m";

                //filas de padding arriba
                for (int y = 0; y < padding; y++)
                {
                    for (int x = 0; x < size + 2 * padding; x++)
                        sb.Append(white);
                    sb.Append(reset);
                    sb.AppendLine();
                }

                //contenido del QR con padding lateral
                for (int y = 0; y < size; y++)
                {
                    //padding izquierdo
                    for (int p = 0; p < padding; p++)
                        sb.Append(white);

                    for (int x = 0; x < size; x++)
                    {
                        bool pixel = qrCodeData.ModuleMatrix[y][x];
                        sb.Append(pixel ? black : white);
                    }

                    //padding derecho
                    for (int p = 0; p < padding; p++)
                        sb.Append(white);

                    sb.Append(reset);
                    sb.AppendLine();
                }

                //filas de padding abajo
                for (int y = 0; y < padding; y++)
                {
                    for (int x = 0; x < size + 2 * padding; x++)
                        sb.Append(white);
                    sb.Append(reset);
                    sb.AppendLine();
                }

                var qrText = sb.ToString();
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(qrText);
            }
        }



    }



}