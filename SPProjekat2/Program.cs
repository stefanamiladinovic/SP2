using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json.Serialization;

namespace SPProjekat2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
                Server server = new Server();
                await server.Start();   
        }
    }
}