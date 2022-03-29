using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SSEConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                IConfiguration Configuration = new ConfigurationBuilder()
								.AddJsonFile("appConfig.json", optional: true, reloadOnChange: true)
								.AddEnvironmentVariables()
								.AddCommandLine(args)
								.Build();

                var value = Configuration.GetSection("Connection").GetSection("CIF").Value;
                Console.WriteLine();
                await Consumer(value);

                Console.WriteLine("Connection closed");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
		}


        public static async Task Consumer(string CIF)
        {
			HttpClientHandler clientHandler = new HttpClientHandler();
			clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

			// Pass the handler to httpclient(from you are calling api)
			HttpClient client = new HttpClient(clientHandler);
			client.Timeout = TimeSpan.FromSeconds(10);
			string cif = CIF;

			string url = $"https://localhost:5003/messages/subscribe/{cif}";

			while (true)
			{
				try
				{
					Console.WriteLine("Establishing connection");
					using (var streamReader = new StreamReader(await client.GetStreamAsync(url)))
					{
						while (!streamReader.EndOfStream)
						{
							var message = await streamReader.ReadLineAsync();
							Console.WriteLine($"Received notifcation for CIF : {cif} Message is: {message}");
						}
					}
                    Console.ReadKey();

				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
					Console.WriteLine("Retrying in 5 seconds");
					await Task.Delay(TimeSpan.FromSeconds(5));
				}
			}
		}

        
    }
}
