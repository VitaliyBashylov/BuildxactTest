using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.ResolveAnything;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;

namespace SuppliesPriceLister
{
    public sealed class SupplierDataMap : ClassMap<SupplierData>
    {
        public SupplierDataMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.Price).Convert(d => d.Value.Price.ToString("C2"));
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var scope = Configure();
            if (args.Length == 0)
                Console.WriteLine("params <partner>:<file>");
            
            var data = new List<SupplierData>();
            foreach (var file in args)
            {
                var (partner, path) = GetInput(file);

                var reader = GetReader(partner, scope);

                await using var fr = File.OpenRead(path);
                data.AddRange(await reader.ReadAsync(fr));
            }

            data.Sort((l, r) => r.Price.CompareTo(l.Price));
                
            var writer = new CsvWriter(Console.Out, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ", "
            });
            writer.Context.RegisterClassMap<SupplierDataMap>();
            await writer.WriteRecordsAsync(data);
        }

        private static ISupplierDataReader GetReader(string partner, ILifetimeScope scope)
        {
            return partner switch
            {
                "megacorp" => scope.Resolve<MegacorpDataReader>(),
                "humphries" => scope.Resolve<HumphriesDataReader>(),
                _ => throw new ApplicationException($"invalid supplier {partner}")
            };
        }

        private static ILifetimeScope Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.Register(c =>
            {
                var cb = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);

                return cb.Build();
            }).As<IConfiguration>();
            
            builder.RegisterType<SimpleCurrencyConverter>().As<ICurrencyConverter>();

            return builder.Build();
        }

        private static (string partner, string path) GetInput(string file)
        {
            var split = file.Split(':');
            return (split[0].ToLowerInvariant(), split[1]);
        }
    }
}
