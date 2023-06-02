using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Runtime.Caching;
using System.IO.Compression;

namespace SPProjekat2
{
    internal class Server
    {
        private MemoryCache cache;
        CacheItemPolicy policy;
        private static string url = "http://localhost:5050/";
        private HttpListener listener;
        private string root = @"C:\Users\korisnik\source\repos\SPProjekat2\root";
        public Server() {
            cache = new MemoryCache("Cache");
            policy = new CacheItemPolicy();
            policy.AbsoluteExpiration =
                DateTimeOffset.Now.AddMinutes(5);
        }
        public async Task Start() {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(url);
                listener.Start();
                Console.WriteLine("Listening...");

                while (listener.IsListening)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    ProcessRequest(context);
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerContext localContext = context;
                string fullUrl = localContext.Request.Url.ToString();
                Console.WriteLine(fullUrl);
                string fotoname=Path.GetFileName(fullUrl);
                string fotoex=Path.GetExtension(fullUrl);
                HttpListenerResponse response= localContext.Response;

                if (string.IsNullOrEmpty(fotoname))
                {
                    Console.WriteLine("Naziv slike nije naveden!");
                    string prazno = "Naziv slike nije naveden";
                    byte[] buffer = Encoding.UTF8.GetBytes(prazno);
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusDescription = "BadRequest";
                    response.Headers.Set("Content.Type", "text/html");
                    using Stream ros = response.OutputStream;
                    response.ContentLength64 = buffer.Length;
                    await ros.WriteAsync(buffer, 0, buffer.Length);
                    ros.Close();
                }
                else
                {
                    if (fotoex == ".png" || fotoex == ".jpg" || fotoex == ".jpeg")
                    {
                       await SendResponse(response, fotoname);
                        
                    }
                    else
                    {
                        Console.WriteLine("Pogresna ekstenzija fajla!");
                        string prazno = "Pogesna ekstenzija";
                        byte[] buffer = Encoding.UTF8.GetBytes(prazno);
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.StatusDescription = "BadRequest";
                        response.Headers.Set("Content.Type", "text/html");
                        using Stream ros = response.OutputStream;
                        response.ContentLength64 = buffer.Length;
                        await ros.WriteAsync(buffer, 0, buffer.Length);
                        ros.Close();

                    }
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        private async Task SendResponse(HttpListenerResponse response, string fotoname) { 
            string path= Directory.GetFiles(root, fotoname,SearchOption.AllDirectories).FirstOrDefault();
            if (path != null)
            {
                byte[] buffer = File.ReadAllBytes(path);
                
                    CacheItem cachedItem = cache.GetCacheItem(fotoname);
                    if (cachedItem != null)
                    {
                        Console.WriteLine("Slika je u kesu!");
                    }
                    else
                    {

                        cachedItem = new CacheItem(fotoname, buffer);
                        cache.Add(cachedItem, policy);
                        Console.WriteLine("Dodavanje slike u kes!");

                    }
                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.Headers.Set("Content.Type", "image/jpeg");
                using Stream ros = response.OutputStream;
                response.ContentLength64 = buffer.Length;
                await ros.WriteAsync(buffer, 0, buffer.Length);
                ros.Close();

            }

            else
            {
                string prazno = "Slika ne postoji na serveru";
                byte[] buffer = Encoding.UTF8.GetBytes(prazno);
                Console.WriteLine("Slika ne postoji na serveru!");
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = "BadRequest";
                response.Headers.Set("Content.Type", "text/html");
                using Stream ros = response.OutputStream;
                response.ContentLength64 = buffer.Length;
                await ros.WriteAsync(buffer, 0, buffer.Length);
                ros.Close();
            }
        }
    } 
}
